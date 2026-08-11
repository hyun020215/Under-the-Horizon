using System.Collections.Generic;
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
    private readonly List<Button> locationNodes = new();

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
        BuildLocationNodes(map);
        RefreshLayers();
    }

    private void BuildLocationNodes(MapDefinition map)
    {
        foreach (Button node in locationNodes)
            if (node != null)
                Destroy(node.gameObject);
        locationNodes.Clear();
        if (map?.Locations == null || baseLayer == null || deckButtons == null || deckButtons.Length == 0)
            return;

        Transform parent = baseLayer.transform.parent;
        foreach (LocationDefinition location in map.Locations)
        {
            MapNodeDefinition definition = location?.MapNode;
            if (definition == null)
                continue;
            Button node = Instantiate(deckButtons[0], parent);
            node.name = $"LocationNode_{location.Id}";
            RectTransform rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = definition.NormalizedPosition;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(180f, 58f);
            bool current = state?.State.currentLocationId == location.Id;
            bool objective = IsObjectiveDestination(location);
            bool unlocked = current || state?.State.unlockedLocations.Contains(location.Id) == true;
            Text label = node.GetComponentInChildren<Text>(true);
            if (label != null)
                label.text = current
                    ? $"현재 · {location.DisplayName}"
                    : objective
                        ? $"◆ {location.DisplayName}"
                        : unlocked ? location.DisplayName : $"잠김 · {location.DisplayName}";
            node.interactable = unlocked && !current;
            LocationDefinition destination = location;
            node.onClick.RemoveAllListeners();
            node.onClick.AddListener(() => Travel(destination));
            node.transform.SetAsLastSibling();
            locationNodes.Add(node);
        }
    }

    private bool IsObjectiveDestination(LocationDefinition location)
    {
        if (location == null || state == null)
            return false;
        if (AppContext.Services == null ||
            !AppContext.Services.TryGet(out ContentDatabase content) ||
            !content.TryGetStoryScene(state.State.currentStorySceneId, out StorySceneDefinition scene))
            return false;
        return scene.Location == location;
    }

    private async void Travel(LocationDefinition destination)
    {
        if (destination == null || state == null)
            return;
        state.SetCurrentLocation(destination.Id);
        if (screens != null)
            await screens.OpenAsync(ScreenId.Exploration);
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
