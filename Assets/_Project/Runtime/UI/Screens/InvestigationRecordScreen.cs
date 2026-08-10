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
    private EvidenceDefinition[] visibleEvidence = System.Array.Empty<EvidenceDefinition>();

    private void Awake()
    {
        for (var index = 0; index < cardButtons?.Length; index++)
        {
            int selected = index;
            cardButtons[index]?.onClick.AddListener(() => Select(selected));
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
        visibleEvidence = evidence?.Inventory?.Discovered
            .Where(item => item != null)
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
                cardLabels[index].text = item.DisplayName;
        }
        if (emptyLabel != null)
            emptyLabel.gameObject.SetActive(visibleEvidence.Length == 0);
        Select(visibleEvidence.Length > 0 ? 0 : -1);
    }

    private void Select(int index)
    {
        EvidenceDefinition item = index >= 0 && index < visibleEvidence.Length
            ? visibleEvidence[index]
            : null;
        if (detailImage != null)
        {
            detailImage.sprite = item?.Image;
            detailImage.gameObject.SetActive(item?.Image != null);
        }
        if (detailTitle != null)
            detailTitle.text = item?.DisplayName ?? "수집된 증거 없음";
        if (detailBody != null)
            detailBody.text = item?.Description ?? "현장을 조사해 증거를 확보하세요.";
    }

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Exploration);
    }
}
