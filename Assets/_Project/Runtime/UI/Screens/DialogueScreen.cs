using System;
using System.Collections.Generic;
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

    private TaskCompletionSource<DialogueChoice> pendingLine;

    private void Awake()
    {
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

        pendingLine?.TrySetCanceled();
        pendingLine = null;
    }

    public override Task OpenAsync(ScreenContext context)
    {
        if (context.Payload is DialogueSequence sequence && sceneLabel != null)
            sceneLabel.text = sequence.Id;

        return base.OpenAsync(context);
    }

    private Task<DialogueChoice> PresentLineAsync(DialogueLine line)
    {
        pendingLine?.TrySetCanceled();
        pendingLine = new TaskCompletionSource<DialogueChoice>();

        if (speakerLabel != null)
            speakerLabel.text = line.speaker?.DisplayName ?? string.Empty;
        if (bodyLabel != null)
            bodyLabel.text = line.text ?? string.Empty;

        var available = new List<DialogueChoice>();
        if (line.choices != null)
        {
            foreach (var choice in line.choices)
                if (choice != null && choice.IsAvailable(narrative.State))
                    available.Add(choice);
        }

        var hasChoices = available.Count > 0;
        if (advanceButton != null)
            advanceButton.gameObject.SetActive(!hasChoices);
        if (advanceLabel != null)
            advanceLabel.text = "계속";

        for (var i = 0; i < choiceButtons?.Length; i++)
        {
            var visible = i < available.Count;
            choiceButtons[i].gameObject.SetActive(visible);
            if (!visible)
                continue;

            choiceButtons[i].name = available[i].Id;
            choiceButtons[i].GetComponent<DialogueChoiceBinding>().Choice = available[i];
            if (choiceLabels != null && i < choiceLabels.Length)
                choiceLabels[i].text = available[i].Text;
        }

        return pendingLine.Task;
    }

    private void Advance()
    {
        pendingLine?.TrySetResult(null);
        pendingLine = null;
    }

    private void SelectChoice(int index)
    {
        if (choiceButtons == null || index >= choiceButtons.Length)
            return;

        var binding = choiceButtons[index].GetComponent<DialogueChoiceBinding>();
        pendingLine?.TrySetResult(binding != null ? binding.Choice : null);
        pendingLine = null;
    }
}

public sealed class DialogueChoiceBinding : MonoBehaviour
{
    public DialogueChoice Choice { get; set; }
}
