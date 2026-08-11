using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class InvestigationRecordScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private EvidenceDirector evidence;
    [SerializeField] private Button[] cardButtons;
    [SerializeField] private Image[] cardImages;
    [SerializeField] private Text[] cardLabels;
    [SerializeField] private Image detailImage;
    [SerializeField] private Text detailTitle;
    [SerializeField] private Text detailBody;
    [SerializeField] private Text emptyLabel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button boardButton;
    [SerializeField] private Button filterButton;
    [SerializeField] private Text filterLabel;
    private EvidenceDefinition[] visibleEvidence = System.Array.Empty<EvidenceDefinition>();
    private readonly HashSet<string> viewedEvidence = new(System.StringComparer.Ordinal);
    private AccessibilitySettingsService accessibility;
    private EvidenceRecordFilter filter;

    private void Awake()
    {
        AppContext.Services?.TryGet(out accessibility);
        for (var index = 0; index < cardButtons?.Length; index++)
        {
            int selected = index;
            cardButtons[index]?.onClick.AddListener(() => Select(selected, true));
        }
        backButton?.onClick.AddListener(Back);
        boardButton?.onClick.AddListener(OpenBoard);
        filterButton?.onClick.AddListener(CycleFilter);
    }

    public override Task OpenAsync(ScreenContext context)
    {
        Refresh();
        return base.OpenAsync(context);
    }

    private void Refresh()
    {
        visibleEvidence = evidence?.Inventory?.Discovered
            .Where(item => item != null)
            .Where(MatchesFilter)
            .OrderBy(item => item.Id, System.StringComparer.Ordinal)
            .ToArray() ?? System.Array.Empty<EvidenceDefinition>();
        for (var index = 0; index < cardButtons?.Length; index++)
        {
            bool visible = index < visibleEvidence.Length;
            cardButtons[index].gameObject.SetActive(visible);
            if (!visible)
                continue;
            EvidenceDefinition item = visibleEvidence[index];
            if (cardImages != null && index < cardImages.Length && cardImages[index] != null)
                cardImages[index].sprite = item.Image;
            if (cardLabels != null && index < cardLabels.Length && cardLabels[index] != null)
                cardLabels[index].text = viewedEvidence.Contains(item.Id)
                    ? item.DisplayName
                    : $"NEW · {item.DisplayName}";
            if (!viewedEvidence.Contains(item.Id))
                StartCoroutine(AnimateNewCard(cardButtons[index]));
        }
        if (emptyLabel != null)
            emptyLabel.gameObject.SetActive(visibleEvidence.Length == 0);
        Select(visibleEvidence.Length > 0 ? 0 : -1, false);
    }

    private void Select(int index, bool markViewed)
    {
        EvidenceDefinition item = index >= 0 && index < visibleEvidence.Length
            ? visibleEvidence[index]
            : null;
        if (markViewed && item != null && viewedEvidence.Add(item.Id)
            && cardLabels != null && index < cardLabels.Length && cardLabels[index] != null)
            cardLabels[index].text = item.DisplayName;
        if (detailImage != null)
        {
            detailImage.sprite = item?.Image;
            detailImage.gameObject.SetActive(item?.Image != null);
        }
        if (detailTitle != null)
            detailTitle.text = item?.DisplayName ?? "수집된 증거 없음";
        if (detailBody != null)
            detailBody.text = item == null
                ? "현장을 조사해 증거를 확보하세요."
                : $"{item.Category} · {(item.IsDirect ? "직접 증거" : "정황 증거")}\n\n{item.Description}";
    }

    private bool MatchesFilter(EvidenceDefinition item) => filter switch
    {
        EvidenceRecordFilter.Direct => item.IsDirect,
        EvidenceRecordFilter.Circumstantial => !item.IsDirect,
        _ => true,
    };

    private void CycleFilter()
    {
        filter = (EvidenceRecordFilter)(((int)filter + 1) % 3);
        if (filterLabel != null)
            filterLabel.text = filter switch
            {
                EvidenceRecordFilter.Direct => "직접 증거",
                EvidenceRecordFilter.Circumstantial => "정황 증거",
                _ => "전체 증거",
            };
        Refresh();
    }

    private IEnumerator AnimateNewCard(Button card)
    {
        if (card == null || accessibility?.ReducedMotion == true)
            yield break;
        Transform target = card.transform;
        Vector3 rest = target.localScale;
        const float duration = .28f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = Mathf.Sin(t * Mathf.PI) * .055f;
            target.localScale = rest * (1f + pulse);
            yield return null;
        }
        target.localScale = rest;
    }

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Exploration);
    }

    private async void OpenBoard()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.EvidenceBoard);
    }
}

public enum EvidenceRecordFilter
{
    All,
    Direct,
    Circumstantial,
}
