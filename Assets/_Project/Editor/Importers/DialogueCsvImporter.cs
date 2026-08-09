using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class DialogueCsvImporter
{
    private const string DialogueRoot = "Assets/_Project/Content/Dialogue";
    private const string MasterPath = DialogueRoot + "/Source/Dialogue_Master_KR.csv";
    private const string ChoicesPath = DialogueRoot + "/Source/Dialogue_Choices_KR.csv";
    private const string EffectRoot =
        "Assets/_Project/Content/Effects/Generated/Dialogue";
    private const string ConditionRoot =
        "Assets/_Project/Content/Conditions/Generated/Dialogue";
    private const string ChoiceFlagPrefix = "DIALOGUE_CHOICE_";

    private sealed class MasterRow
    {
        public string Id;
        public string SceneId;
        public int Order;
        public string Type;
        public string Speaker;
        public string Text;
        public string Condition;
        public string ChoiceId;
        public string NextOrEffect;
        public bool VoiceRequired;
        public string BranchGroup;
    }

    private sealed class ChoiceRow
    {
        public string Id;
        public string Text;
        public string Condition;
        public string Effect;
    }

    [MenuItem("Under The Horizon/Import/Dialogue Graphs")]
    public static void ImportAll()
    {
        var master = new List<MasterRow>();
        var sequences = new Dictionary<string, DialogueSequence>();
        AssetDatabase.StartAssetEditing();
        try
        {
            EnsureFolder(EffectRoot);
            EnsureFolder(ConditionRoot);

            master = ReadCsv(MasterPath)
                .Skip(1)
                .Where(row => row.Count >= 15 && !string.IsNullOrWhiteSpace(row[1]))
                .Select(ParseMasterRow)
                .ToList();
            Dictionary<string, ChoiceRow> choices = ReadCsv(ChoicesPath)
                .Skip(1)
                .Where(row => row.Count >= 7 && !string.IsNullOrWhiteSpace(row[0]))
                .Select(ParseChoiceRow)
                .ToDictionary(row => row.Id, StringComparer.Ordinal);
            sequences = LoadSequences();

            foreach (IGrouping<string, MasterRow> group in master.GroupBy(row => row.SceneId))
            {
                string sequenceId = "DIA_" + NormalizeSceneId(group.Key);
                if (!sequences.TryGetValue(sequenceId, out DialogueSequence sequence))
                {
                    throw new InvalidOperationException(
                        $"Dialogue asset {sequenceId} is missing.");
                }

                ImportSequence(
                    sequence,
                    group.OrderBy(row => row.Order).ToList(),
                    choices);
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.Refresh();
        Debug.Log($"Imported {master.Count} dialogue rows into {sequences.Count} assets.");
    }

    public static void ImportFromCommandLine()
    {
        ImportAll();
        EditorApplication.Exit(0);
    }

    private static void ImportSequence(
        DialogueSequence sequence,
        List<MasterRow> source,
        IReadOnlyDictionary<string, ChoiceRow> choiceCatalog)
    {
        Dictionary<string, DialogueLine> existing = (sequence.Lines
                ?? Array.Empty<DialogueLine>())
            .Where(line => !string.IsNullOrWhiteSpace(line.id))
            .ToDictionary(line => line.id, StringComparer.Ordinal);
        Dictionary<string, List<MasterRow>> choicesByOwner =
            BuildChoicesByOwner(source);
        List<MasterRow> retained = source
            .Where(row => !IsChoiceRow(row))
            .ToList();

        var serialized = new SerializedObject(sequence);
        SerializedProperty lines = serialized.FindProperty("lines");
        lines.arraySize = retained.Count;

        for (var index = 0; index < retained.Count; index++)
        {
            MasterRow row = retained[index];
            existing.TryGetValue(row.Id, out DialogueLine oldLine);
            SerializedProperty line = lines.GetArrayElementAtIndex(index);
            line.FindPropertyRelative("id").stringValue = row.Id;
            line.FindPropertyRelative("text").stringValue = row.Text;
            line.FindPropertyRelative("voiceRequired").boolValue = row.VoiceRequired;
            line.FindPropertyRelative("voiceClip").objectReferenceValue = oldLine.voiceClip;

            SerializedProperty speaker = line.FindPropertyRelative("speaker");
            speaker.FindPropertyRelative("overrideName").stringValue = row.Speaker;
            speaker.FindPropertyRelative("character").objectReferenceValue =
                oldLine.speaker?.Character ?? FindCharacter(row.Speaker);

            SetObjectArray(
                line.FindPropertyRelative("conditions"),
                BuildConditions(row.Condition));
            SetObjectArray(
                line.FindPropertyRelative("effects"),
                oldLine.effects?.Cast<UnityEngine.Object>());

            choicesByOwner.TryGetValue(row.Id, out List<MasterRow> authoredChoices);
            ConfigureChoices(
                line.FindPropertyRelative("choices"),
                authoredChoices,
                source,
                choiceCatalog);
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(sequence);
    }

    private static Dictionary<string, List<MasterRow>> BuildChoicesByOwner(
        IReadOnlyList<MasterRow> rows)
    {
        var result = new Dictionary<string, List<MasterRow>>(StringComparer.Ordinal);
        for (var index = 0; index < rows.Count; index++)
        {
            if (!IsChoiceRow(rows[index]))
                continue;

            var ownerIndex = index - 1;
            while (ownerIndex >= 0 && IsChoiceRow(rows[ownerIndex]))
                ownerIndex--;
            if (ownerIndex < 0)
                throw new InvalidOperationException(
                    $"Choice {rows[index].ChoiceId} has no preceding dialogue line.");

            string ownerId = rows[ownerIndex].Id;
            if (!result.TryGetValue(ownerId, out List<MasterRow> choices))
            {
                choices = new List<MasterRow>();
                result.Add(ownerId, choices);
            }
            choices.Add(rows[index]);
        }
        return result;
    }

    private static void ConfigureChoices(
        SerializedProperty property,
        IReadOnlyList<MasterRow> authoredChoices,
        IReadOnlyList<MasterRow> source,
        IReadOnlyDictionary<string, ChoiceRow> choiceCatalog)
    {
        property.arraySize = authoredChoices?.Count ?? 0;
        if (authoredChoices == null)
            return;

        string[] siblingIds = authoredChoices
            .Select(row => row.ChoiceId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToArray();

        for (var index = 0; index < authoredChoices.Count; index++)
        {
            MasterRow authored = authoredChoices[index];
            choiceCatalog.TryGetValue(authored.ChoiceId, out ChoiceRow catalog);
            SerializedProperty choice = property.GetArrayElementAtIndex(index);
            choice.FindPropertyRelative("id").stringValue = authored.ChoiceId;
            choice.FindPropertyRelative("text").stringValue =
                catalog?.Text ?? authored.Text;
            choice.FindPropertyRelative("nextLineId").stringValue =
                FindBranchTarget(authored.ChoiceId, source);
            SetObjectArray(
                choice.FindPropertyRelative("conditions"),
                BuildConditions(catalog?.Condition));
            SetObjectArray(
                choice.FindPropertyRelative("effects"),
                new UnityEngine.Object[]
                {
                    BuildChoiceEffect(
                        authored.ChoiceId,
                        siblingIds,
                        catalog?.Effect ?? authored.NextOrEffect),
                });
        }
    }

    private static string FindBranchTarget(
        string choiceId,
        IReadOnlyList<MasterRow> source)
    {
        string marker = $"choice({choiceId})";
        MasterRow response = source.FirstOrDefault(row =>
            !IsChoiceRow(row)
            && !string.IsNullOrWhiteSpace(row.Condition)
            && row.Condition.IndexOf(marker, StringComparison.Ordinal) >= 0);
        return response?.Id ?? string.Empty;
    }

    private static CompositeEffect BuildChoiceEffect(
        string choiceId,
        IEnumerable<string> siblingIds,
        string expression)
    {
        string token = FileToken(choiceId);
        string path = $"{EffectRoot}/FX_CHOICE_{token}.asset";
        CompositeEffect composite = AssetDatabase.LoadAssetAtPath<CompositeEffect>(path);
        if (composite == null)
        {
            composite = ScriptableObject.CreateInstance<CompositeEffect>();
            composite.name = "FX_CHOICE_" + token;
            AssetDatabase.CreateAsset(composite, path);
        }

        foreach (UnityEngine.Object child in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (child != composite)
                UnityEngine.Object.DestroyImmediate(child, true);
        }

        var effects = new List<GameEffect>();
        foreach (string siblingId in siblingIds)
        {
            effects.Add(CreateFlagEffect(
                ChoiceFlagPrefix + siblingId,
                siblingId == choiceId));
        }

        foreach (string item in SplitExpression(expression))
            effects.Add(CreateEffect(item));

        for (var index = 0; index < effects.Count; index++)
        {
            effects[index].name = $"{composite.name}_{index:00}";
            AssetDatabase.AddObjectToAsset(effects[index], composite);
        }

        var serialized = new SerializedObject(composite);
        SetObjectArray(
            serialized.FindProperty("effects"),
            effects.Cast<UnityEngine.Object>());
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(composite);
        return composite;
    }

    private static GameEffect CreateEffect(string expression)
    {
        string[] pair = expression.Split(':', 2);
        string key = pair[0].Trim();
        string value = pair.Length > 1 ? pair[1].Trim() : "true";

        if (key.StartsWith("trust_", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value, out int trust))
        {
            var effect = ScriptableObject.CreateInstance<ModifyTrustEffect>();
            SetString(effect, "characterId", "CHR_" + key.Substring(6).ToUpperInvariant());
            SetInt(effect, "amount", trust);
            return effect;
        }

        if (key.Equals("publicAnxiety", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value, out int anxiety))
        {
            var effect = ScriptableObject.CreateInstance<ChangeAnxietyEffect>();
            SetInt(effect, "amount", anxiety);
            return effect;
        }

        if (key.Equals("evidenceIntegrity", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value, out int integrity))
        {
            var effect = ScriptableObject.CreateInstance<ChangeIntegrityEffect>();
            SetInt(effect, "amount", integrity);
            return effect;
        }

        if (key.Equals("flag", StringComparison.OrdinalIgnoreCase))
            return CreateFlagEffect(value, true);

        return CreateFlagEffect("DIALOGUE_EFFECT_" + expression, true);
    }

    private static SetFlagEffect CreateFlagEffect(string flagId, bool value)
    {
        var effect = ScriptableObject.CreateInstance<SetFlagEffect>();
        SetString(effect, "flagId", flagId);
        SetBool(effect, "value", value);
        return effect;
    }

    private static IEnumerable<UnityEngine.Object> BuildConditions(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return Array.Empty<UnityEngine.Object>();

        Match choice = Regex.Match(expression, @"choice\(([^)]+)\)");
        if (choice.Success)
            return new UnityEngine.Object[] { GetChoiceCondition(choice.Groups[1].Value) };

        Match flag = Regex.Match(expression, @"^flag[:(]([^)]+)\)?$");
        if (flag.Success)
            return new UnityEngine.Object[] { GetFlagCondition(flag.Groups[1].Value) };

        Match trust = Regex.Match(expression, @"^trust_([a-z_]+)>=(\-?\d+)$");
        if (trust.Success)
        {
            string id = "TRUST_" + trust.Groups[1].Value + "_GTE_" + trust.Groups[2].Value;
            string path = $"{ConditionRoot}/COND_{FileToken(id)}.asset";
            TrustCondition condition = GetOrCreate<TrustCondition>(path);
            SetString(
                condition,
                "characterId",
                "CHR_" + trust.Groups[1].Value.ToUpperInvariant());
            SetInt(condition, "minimum", int.Parse(trust.Groups[2].Value));
            SetInt(condition, "maximum", 100);
            return new UnityEngine.Object[] { condition };
        }

        return Array.Empty<UnityEngine.Object>();
    }

    private static HasFlagCondition GetChoiceCondition(string choiceId) =>
        GetFlagCondition(ChoiceFlagPrefix + choiceId);

    private static HasFlagCondition GetFlagCondition(string flagId)
    {
        string path = $"{ConditionRoot}/COND_{FileToken(flagId)}.asset";
        HasFlagCondition condition = GetOrCreate<HasFlagCondition>(path);
        SetString(condition, "flagId", flagId);
        SetBool(condition, "expected", true);
        return condition;
    }

    private static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        asset.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static Dictionary<string, DialogueSequence> LoadSequences() =>
        AssetDatabase.FindAssets("t:DialogueSequence", new[] { DialogueRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<DialogueSequence>)
            .Where(sequence => sequence != null && sequence.Id.StartsWith("DIA_"))
            .ToDictionary(sequence => sequence.Id, StringComparer.Ordinal);

    private static CharacterDefinition FindCharacter(string speaker)
    {
        string expectedId = "CHR_" + (speaker ?? string.Empty).Trim().ToUpperInvariant();
        return AssetDatabase.FindAssets("t:CharacterDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CharacterDefinition>)
            .FirstOrDefault(character => character != null && character.Id == expectedId);
    }

    private static MasterRow ParseMasterRow(List<string> row) => new()
    {
        Id = row[0],
        SceneId = row[1],
        Order = int.TryParse(row[2], out int order) ? order : 0,
        Type = row[4],
        Speaker = row[5],
        Text = row[6],
        Condition = row[8],
        ChoiceId = row[9],
        NextOrEffect = row[10],
        VoiceRequired = row[12].Equals("Y", StringComparison.OrdinalIgnoreCase),
        BranchGroup = row[13],
    };

    private static ChoiceRow ParseChoiceRow(List<string> row) => new()
    {
        Id = row[0],
        Text = row[2],
        Condition = row[3],
        Effect = row[4],
    };

    private static bool IsChoiceRow(MasterRow row) =>
        row.Type.Equals("choice", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(row.ChoiceId);

    private static IEnumerable<string> SplitExpression(string expression) =>
        (expression ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => item.Length > 0);

    private static string NormalizeSceneId(string id) =>
        id.Replace("-", "_");

    private static string FileToken(string value) =>
        Regex.Replace(value ?? string.Empty, @"[^A-Za-z0-9_]+", "_")
            .Trim('_')
            .ToUpperInvariant();

    private static void SetObjectArray(
        SerializedProperty property,
        IEnumerable<UnityEngine.Object> values)
    {
        UnityEngine.Object[] array = values?.Where(value => value != null).ToArray()
            ?? Array.Empty<UnityEngine.Object>();
        property.arraySize = array.Length;
        for (var index = 0; index < array.Length; index++)
            property.GetArrayElementAtIndex(index).objectReferenceValue = array[index];
    }

    private static void SetString(UnityEngine.Object target, string property, string value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).stringValue = value ?? string.Empty;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetInt(UnityEngine.Object target, string property, int value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetBool(UnityEngine.Object target, string property, bool value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void EnsureFolder(string path)
    {
        string current = "Assets";
        foreach (string segment in path.Split('/').Skip(1))
        {
            string next = current + "/" + segment;
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, segment);
            current = next;
        }
    }

    private static List<List<string>> ReadCsv(string assetPath)
    {
        string text = File.ReadAllText(Path.GetFullPath(assetPath), Encoding.UTF8);
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        for (var index = 0; index < text.Length; index++)
        {
            char character = text[index];
            if (character == '"')
            {
                if (quoted && index + 1 < text.Length && text[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (character == ',' && !quoted)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if ((character == '\n' || character == '\r') && !quoted)
            {
                if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    index++;
                row.Add(field.ToString());
                field.Clear();
                if (row.Any(value => value.Length > 0))
                    rows.Add(row);
                row = new List<string>();
            }
            else
            {
                field.Append(character);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }
}
