using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private GameStateStore state;
    [SerializeField] private MapDefinition[] maps;
    [SerializeField] private Image baseLayer;
    [SerializeField] private Image restrictedLayer;
    [SerializeField] private Image technicalLayer;
    [SerializeField] private Text deckLabel;
    [SerializeField] private Text locationLabel;
    [SerializeField] private Button[] deckButtons;
    [SerializeField] private Toggle restrictedToggle;
    [SerializeField] private Toggle technicalToggle;
    [SerializeField] private Button backButton;
    private int selectedIndex;

    private void Awake()
    {
        for (var index = 0; index < deckButtons?.Length; index++)
        {
            int mapIndex = index;
            deckButtons[index]?.onClick.AddListener(() => SelectMap(mapIndex));
        }
        restrictedToggle?.onValueChanged.AddListener(_ => RefreshLayers());
        technicalToggle?.onValueChanged.AddListener(_ => RefreshLayers());
        backButton?.onClick.AddListener(Back);
    }

    public override Task OpenAsync(ScreenContext context)
    {
        int maximumIndex = maps != null ? Mathf.Max(0, maps.Length - 1) : 0;
        SelectMap(Mathf.Clamp(selectedIndex, 0, maximumIndex));
        if (locationLabel != null)
        {
            string location = state?.State.currentLocationId;
            locationLabel.text = string.IsNullOrWhiteSpace(location)
                ? "현재 위치 확인 중"
                : $"현재 위치 · {location}";
        }
        return base.OpenAsync(context);
    }

    private void SelectMap(int index)
    {
        if (maps == null || maps.Length == 0)
            return;
        selectedIndex = Mathf.Clamp(index, 0, maps.Length - 1);
        MapDefinition map = maps[selectedIndex];
        if (baseLayer != null)
            baseLayer.sprite = map?.BaseLayer;
        if (restrictedLayer != null)
            restrictedLayer.sprite = map?.RestrictedLayer;
        if (technicalLayer != null)
            technicalLayer.sprite = map?.TechnicalLayer;
        if (deckLabel != null)
            deckLabel.text = map?.Id?.Replace("MAP_", string.Empty) ?? string.Empty;
        RefreshLayers();
    }

    private void RefreshLayers()
    {
        if (restrictedLayer != null)
            restrictedLayer.gameObject.SetActive(restrictedToggle != null && restrictedToggle.isOn);
        if (technicalLayer != null)
            technicalLayer.gameObject.SetActive(technicalToggle != null && technicalToggle.isOn);
    }

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Exploration);
    }
}
