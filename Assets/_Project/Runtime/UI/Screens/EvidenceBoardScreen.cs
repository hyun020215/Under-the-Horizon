using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class EvidenceBoardScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private EvidenceBoardDirector board;
    [SerializeField] private Button[] evidenceButtons;
    [SerializeField] private Image[] evidenceImages;
    [SerializeField] private Text[] evidenceLabels;
    [SerializeField] private Button[] theoryButtons;
    [SerializeField] private Text[] theoryLabels;
    [SerializeField] private Text detailTitle;
    [SerializeField] private Text detailBody;
    [SerializeField] private Text progressLabel;
    [SerializeField] private Text emptyLabel;
    [SerializeField] private Button backButton;
    private EvidenceDefinition[] discovered = Array.Empty<EvidenceDefinition>();
    private TheoryEvaluation[] theories = Array.Empty<TheoryEvaluation>();

    private void Awake()
    {
        for (var index = 0; index < evidenceButtons?.Length; index++)
        {
            int selected = index;
            evidenceButtons[index]?.onClick.AddListener(() => SelectEvidence(selected));
        }
        for (var index = 0; index < theoryButtons?.Length; index++)
        {
            int selected = index;
            theoryButtons[index]?.onClick.AddListener(() => SelectTheory(selected));
        }
        backButton?.onClick.AddListener(Back);
    }

    public override Task OpenAsync(ScreenContext context)
    {
        Refresh();
        return base.OpenAsync(context);
    }

    private void Refresh()
    {
        discovered = board?.Discovered ?? Array.Empty<EvidenceDefinition>();
        theories = board?.EvaluateTheories() ?? Array.Empty<TheoryEvaluation>();
        for (var index = 0; index < evidenceButtons?.Length; index++)
        {
            bool visible = index < discovered.Length;
            evidenceButtons[index].gameObject.SetActive(visible);
            if (!visible) continue;
            EvidenceDefinition item = discovered[index];
            if (index < evidenceImages.Length) evidenceImages[index].sprite = item.Image;
            if (index < evidenceLabels.Length) evidenceLabels[index].text = item.DisplayName;
        }
        for (var index = 0; index < theoryButtons?.Length; index++)
        {
            bool visible = index < theories.Length;
            theoryButtons[index].gameObject.SetActive(visible);
            if (!visible) continue;
            TheoryEvaluation evaluation = theories[index];
            string state = evaluation.CanResolve ? "논증 가능" : $"단서 {evaluation.MissingEvidence.Count}개 부족";
            if (index < theoryLabels.Length)
                theoryLabels[index].text = $"{evaluation.Theory.DisplayName}\n{state}";
            theoryButtons[index].image.color = evaluation.CanResolve
                ? new Color(0.42f, 0.31f, 0.12f, 1f)
                : new Color(0.09f, 0.11f, 0.15f, 1f);
        }
        if (progressLabel != null)
            progressLabel.text = $"수집 증거 {discovered.Length} · 논증 가능 {theories.Count(item => item.CanResolve)}/{theories.Length}";
        if (emptyLabel != null) emptyLabel.gameObject.SetActive(discovered.Length == 0);
        if (discovered.Length > 0) SelectEvidence(0); else if (theories.Length > 0) SelectTheory(0);
    }

    private void SelectEvidence(int index)
    {
        if (index < 0 || index >= discovered.Length) return;
        EvidenceDefinition item = discovered[index];
        detailTitle.text = item.DisplayName;
        detailBody.text = item.Description;
    }

    private void SelectTheory(int index)
    {
        if (index < 0 || index >= theories.Length) return;
        TheoryEvaluation evaluation = theories[index];
        detailTitle.text = evaluation.Theory.DisplayName;
        string required = string.Join(" · ", evaluation.Theory.RequiredEvidence.Select(item => item.DisplayName));
        string missing = evaluation.CanResolve ? "모든 연결 증거가 확보되었습니다." :
            "미확보: " + string.Join(" · ", evaluation.MissingEvidence.Select(item => item.DisplayName));
        detailBody.text = $"{evaluation.Theory.Description}\n\n연결 증거: {required}\n{missing}";
    }

    private async void Back()
    {
        if (screens != null) await screens.OpenAsync(ScreenId.InvestigationRecord);
    }
}
