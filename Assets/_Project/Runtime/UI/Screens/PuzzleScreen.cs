using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PuzzleScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private Text titleLabel;
    [SerializeField] private Text instructionLabel;
    [SerializeField] private Text hintLabel;
    [SerializeField] private Text resultLabel;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button returnButton;
    private Action cancel;
    private int hintStep;

    private void Awake()
    {
        hintButton?.onClick.AddListener(ShowHint);
        cancelButton?.onClick.AddListener(() => cancel?.Invoke());
        returnButton?.onClick.AddListener(ReturnToExploration);
    }

    public void Present(PuzzleDefinition definition, Action cancelAction)
    {
        cancel = cancelAction;
        hintStep = 0;
        if (titleLabel != null) titleLabel.text = FormatTitle(definition?.Id);
        if (instructionLabel != null)
            instructionLabel.text = "확보한 단서와 현장의 규칙을 비교해 결론을 완성하세요.";
        if (hintLabel != null) { hintLabel.text = string.Empty; hintLabel.gameObject.SetActive(false); }
        if (resultLabel != null) resultLabel.gameObject.SetActive(false);
        if (returnButton != null) returnButton.gameObject.SetActive(false);
        if (hintButton != null) hintButton.gameObject.SetActive(true);
        if (cancelButton != null) cancelButton.gameObject.SetActive(true);
    }

    public void ShowResult(PuzzleResult result)
    {
        if (resultLabel != null)
        {
            resultLabel.text = result.Completed ? "추론이 기록되었습니다." : "퍼즐을 중단했습니다.";
            resultLabel.gameObject.SetActive(true);
        }
        if (hintButton != null) hintButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        if (returnButton != null) returnButton.gameObject.SetActive(true);
    }

    private void ShowHint()
    {
        hintStep++;
        if (hintLabel == null) return;
        hintLabel.gameObject.SetActive(true);
        hintLabel.text = hintStep == 1
            ? "먼저 서로 모순되는 정보와 공통되는 정보를 분리해 보세요."
            : "아직 사용하지 않은 증거와 조작 가능한 요소를 다시 확인하세요.";
    }

    private async void ReturnToExploration()
    {
        if (screens != null) await screens.OpenAsync(ScreenId.Exploration);
    }

    private static string FormatTitle(string id) => string.IsNullOrWhiteSpace(id)
        ? "조사 퍼즐"
        : id.Replace("PUZ_", string.Empty).Replace('_', ' ');
}
