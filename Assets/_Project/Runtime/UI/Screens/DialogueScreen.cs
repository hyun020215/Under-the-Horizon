using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialogueScreen : ScreenBase
{
    [SerializeField] private NarrativeDirector narrative;
    [SerializeField] private Text sceneLabel;
    [SerializeField] private Text speakerLabel;
    [SerializeField] private Text bodyLabel;
    [SerializeField] private Button advanceButton;
    [SerializeField] private Text advanceLabel;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private Text[] choiceLabels;
    [SerializeField] private SfxController sfx;
    [SerializeField] private AudioClip typewriterClip;
    [SerializeField, Min(1f)] private float charactersPerSecond = 45f;
    [SerializeField, Min(0f)] private float choiceRevealDuration = .18f;
    [SerializeField, Min(0f)] private float choiceRevealDistance = 18f;

    private TaskCompletionSource<DialogueChoice> pendingLine;
    private Coroutine revealRoutine;
    private string fullText = string.Empty;
    private bool revealing;
    private readonly List<DialogueChoice> availableChoices = new();
    private AccessibilitySettingsService accessibility;
    private int defaultBodyFontSize;
    public bool IsRevealing => revealing;

    private void Awake()
    {
        AppContext.Services?.TryGet(out accessibility);
        if (bodyLabel != null)
            defaultBodyFontSize = bodyLabel.fontSize;
        if (advanceButton != null)
            advanceButton.onClick.AddListener(Advance);

        for (var i = 0; i < choiceButtons?.Length; i++)
        {
            var choiceIndex = i;
            choiceButtons[i].onClick.AddListener(() => SelectChoice(choiceIndex));
        }
    }

    private void OnEnable()
    {
        if (narrative != null)
            narrative.LinePresented += PresentLineAsync;
    }

    private void OnDisable()
    {
        if (narrative != null)
            narrative.LinePresented -= PresentLineAsync;

        pendingLine?.TrySetResult(null);
        pendingLine = null;
        StopReveal();
    }

    public override Task OpenAsync(ScreenContext context)
    {
        if (sceneLabel != null)
            sceneLabel.text = string.Empty;

        return base.OpenAsync(context);
    }

    private Task<DialogueChoice> PresentLineAsync(DialogueLine line)
    {
        pendingLine?.TrySetResult(null);
        pendingLine = new TaskCompletionSource<DialogueChoice>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        bool narration = line.speaker == null;
        if (speakerLabel != null)
            speakerLabel.text = narration ? "NARRATION" : line.speaker.DisplayName;
        if (bodyLabel != null)
            bodyLabel.alignment = narration
                ? TextAnchor.UpperCenter
                : TextAnchor.UpperLeft;
        fullText = line.text ?? string.Empty;
        if (bodyLabel != null)
        {
            bodyLabel.fontSize = DialogueTypography.ResolveFontSize(
                defaultBodyFontSize, fullText.Length, narration);
            bodyLabel.text = string.Empty;
        }

        availableChoices.Clear();
        if (line.choices != null)
        {
            foreach (var choice in line.choices)
                if (choice != null && choice.IsAvailable(narrative.State))
                    availableChoices.Add(choice);
        }

        if (advanceLabel != null)
            advanceLabel.text = "계속";
        SetChoiceVisibility(false);
        if (advanceButton != null)
            advanceButton.gameObject.SetActive(true);
        revealRoutine = StartCoroutine(RevealText());

        return pendingLine.Task;
    }

    private void Advance()
    {
        if (revealing)
        {
            FinishReveal();
            return;
        }
        TaskCompletionSource<DialogueChoice> completedLine = pendingLine;
        pendingLine = null;
        completedLine?.TrySetResult(null);
    }

    private void SelectChoice(int index)
    {
        if (choiceButtons == null || index >= choiceButtons.Length)
            return;

        var binding = choiceButtons[index].GetComponent<DialogueChoiceBinding>();
        TaskCompletionSource<DialogueChoice> completedLine = pendingLine;
        pendingLine = null;
        completedLine?.TrySetResult(binding != null ? binding.Choice : null);
    }

    private IEnumerator RevealText()
    {
        revealing = true;
        sfx?.PlayLoop(typewriterClip, 0.35f);

        float speed = accessibility?.CharactersPerSecond ?? charactersPerSecond;
        if (float.IsPositiveInfinity(speed))
        {
            FinishReveal();
            yield break;
        }
        float visibleCharacters = 0f;
        while (visibleCharacters < fullText.Length)
        {
            visibleCharacters += speed * Time.unscaledDeltaTime;
            if (bodyLabel != null)
                bodyLabel.text = fullText.Substring(
                    0,
                    Mathf.Min(fullText.Length, Mathf.FloorToInt(visibleCharacters))
                );
            yield return null;
        }
        FinishReveal();
    }

    private void FinishReveal()
    {
        StopReveal();
        if (bodyLabel != null)
            bodyLabel.text = fullText;
        bool hasChoices = availableChoices.Count > 0;
        if (advanceButton != null)
            advanceButton.gameObject.SetActive(!hasChoices);
        SetChoiceVisibility(hasChoices);
    }

    private void StopReveal()
    {
        revealing = false;
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }
        sfx?.StopLoop(typewriterClip);
    }

    private void SetChoiceVisibility(bool showAvailable)
    {
        for (var i = 0; i < choiceButtons?.Length; i++)
        {
            bool visible = showAvailable && i < availableChoices.Count;
            choiceButtons[i].gameObject.SetActive(visible);
            if (!visible)
                continue;

            DialogueChoice choice = availableChoices[i];
            choiceButtons[i].name = choice.Id;
            choiceButtons[i].GetComponent<DialogueChoiceBinding>().Choice = choice;
            if (choiceLabels != null && i < choiceLabels.Length)
                choiceLabels[i].text = choice.Text;
            StartCoroutine(AnimateChoice(choiceButtons[i], i));
        }
    }

    private IEnumerator AnimateChoice(Button button, int index)
    {
        CanvasGroup group = button.GetComponent<CanvasGroup>();
        if (group == null)
            yield break;
        RectTransform rect = (RectTransform)button.transform;
        Vector2 rest = rect.anchoredPosition;
        if (accessibility?.ReducedMotion == true)
        {
            group.alpha = 1f;
            rect.anchoredPosition = rest;
            yield break;
        }
        group.alpha = 0f;
        rect.anchoredPosition = rest + Vector2.down * choiceRevealDistance;
        float delay = index * .045f;
        while (delay > 0f)
        {
            delay -= Time.unscaledDeltaTime;
            yield return null;
        }
        float elapsed = 0f;
        while (elapsed < choiceRevealDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = choiceRevealDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / choiceRevealDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            group.alpha = t;
            rect.anchoredPosition = Vector2.LerpUnclamped(
                rest + Vector2.down * choiceRevealDistance, rest, t);
            yield return null;
        }
        group.alpha = 1f;
        rect.anchoredPosition = rest;
    }
}
