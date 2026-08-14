using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapScreen : ScreenBase
{
    private enum NodeState
    {
        Current,
        Objective,
        Available,
        Locked,
    }

    [SerializeField] private ScreenRouter screens;
    [SerializeField] private GameStateStore state;
    [SerializeField] private GameFlowController flow;
    [SerializeField] private MapDefinition[] maps;
    [SerializeField] private RectTransform mapSurface;
    [SerializeField] private Image baseLayer;
    [SerializeField] private Image restrictedLayer;
    [SerializeField] private Image technicalLayer;
    [SerializeField] private RectTransform nodeRoot;
    [SerializeField] private Button nodeTemplate;
    [SerializeField] private Text deckLabel;
    [SerializeField] private Text locationLabel;
    [SerializeField] private Button[] deckButtons;
    [SerializeField] private Toggle restrictedToggle;
    [SerializeField] private Toggle technicalToggle;
    [SerializeField] private Text selectionNameLabel;
    [SerializeField] private Text selectionStatusLabel;
    [SerializeField] private Text selectionDescriptionLabel;
    [SerializeField] private Text feedbackLabel;
    [SerializeField] private Button travelButton;
    [SerializeField] private Text travelButtonLabel;
    [SerializeField] private Button backButton;

    private readonly List<Button> locationNodes = new();
    private int selectedMapIndex;
    private LocationDefinition selectedLocation;
    private bool isTraveling;

    public string SelectedLocationId => selectedLocation?.Id ?? string.Empty;
    public string SelectedMapId => CurrentMap?.Id ?? string.Empty;

    private MapDefinition CurrentMap => maps != null
        && selectedMapIndex >= 0
        && selectedMapIndex < maps.Length
            ? maps[selectedMapIndex]
            : null;

    private void Awake()
    {
        for (var index = 0; index < deckButtons?.Length; index++)
        {
            int mapIndex = index;
            Text label = deckButtons[index]?.GetComponentInChildren<Text>(true);
            if (label != null && maps != null && mapIndex < maps.Length)
                label.text = ResolveMapName(maps[mapIndex]);
            deckButtons[index]?.onClick.AddListener(() => SelectMap(mapIndex));
        }

        restrictedToggle?.onValueChanged.AddListener(_ => RefreshLayers());
        technicalToggle?.onValueChanged.AddListener(_ => RefreshLayers());
        travelButton?.onClick.AddListener(ConfirmTravel);
        backButton?.onClick.AddListener(Back);
        if (nodeTemplate != null)
            nodeTemplate.gameObject.SetActive(false);
    }

    public override Task OpenAsync(ScreenContext context)
    {
        isTraveling = false;
        SetFeedback(string.Empty);

        string focusLocationId = state?.State.currentLocationId;
        if (flow != null && flow.TryGetPendingTravel(out PendingStorySceneTravel pending))
            focusLocationId = pending.DestinationId;

        int focusMap = FindMapIndex(focusLocationId);
        int maximumIndex = maps != null ? Mathf.Max(0, maps.Length - 1) : 0;
        SelectMap(focusMap >= 0 ? focusMap : Mathf.Clamp(selectedMapIndex, 0, maximumIndex));

        if (!string.IsNullOrWhiteSpace(focusLocationId)
            && TryFindVisibleLocation(CurrentMap, focusLocationId, out LocationDefinition location))
        {
            SelectLocation(location);
        }
        else
        {
            SelectLocation(null);
        }

        RefreshCurrentLocation();
        return base.OpenAsync(context);
    }

    private void SelectMap(int index)
    {
        if (maps == null || maps.Length == 0)
            return;

        selectedMapIndex = Mathf.Clamp(index, 0, maps.Length - 1);
        MapDefinition map = CurrentMap;
        ApplyLayer(baseLayer, map?.BaseLayer);
        ApplyLayer(restrictedLayer, map?.RestrictedLayer);
        ApplyLayer(technicalLayer, map?.TechnicalLayer);
        ConfigureLayerToggle(restrictedToggle, map?.RestrictedLayer != null);
        ConfigureLayerToggle(technicalToggle, map?.TechnicalLayer != null);
        if (deckLabel != null)
            deckLabel.text = ResolveMapName(map);

        if (selectedLocation != null && !MapContains(map, selectedLocation.Id))
            selectedLocation = null;
        BuildLocationNodes(map);
        RefreshSelection();
        RefreshLayers();
    }

    private void BuildLocationNodes(MapDefinition map)
    {
        foreach (Button node in locationNodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        locationNodes.Clear();

        if (map?.Locations == null || nodeRoot == null || nodeTemplate == null)
            return;

        foreach (LocationDefinition location in map.Locations)
        {
            MapNodeDefinition definition = location?.MapNode;
            if (definition == null || !IsNodeVisible(location))
                continue;

            Button node = Instantiate(nodeTemplate, nodeRoot);
            node.gameObject.SetActive(true);
            node.name = $"LocationNode_{location.Id}";
            RectTransform rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = definition.NormalizedPosition;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;

            NodeState nodeState = ResolveNodeState(location);
            Text label = node.GetComponentInChildren<Text>(true);
            if (label != null)
                label.text = FormatNodeLabel(location, nodeState);
            ApplyNodeState(node, nodeState);

            LocationDefinition selection = location;
            node.onClick.RemoveAllListeners();
            node.onClick.AddListener(() => SelectLocation(selection));
            locationNodes.Add(node);
        }
    }

    private void SelectLocation(LocationDefinition location)
    {
        selectedLocation = location;
        SetFeedback(string.Empty);
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (selectionNameLabel != null)
            selectionNameLabel.text = selectedLocation != null
                ? ResolveNodeName(selectedLocation)
                : "목적지를 선택하세요";
        if (selectionDescriptionLabel != null)
            selectionDescriptionLabel.text = selectedLocation != null
                ? ResolveNodeDescription(selectedLocation)
                : "지도에서 장소를 선택하면 이동 가능 여부를 확인할 수 있습니다.";

        bool canTravel = selectedLocation != null
            && !isTraveling
            && flow != null
            && flow.CanTravelTo(selectedLocation.Id, out _);
        if (selectionStatusLabel != null)
            selectionStatusLabel.text = ResolveSelectionStatus(selectedLocation, canTravel);
        if (travelButton != null)
            travelButton.interactable = canTravel;
        if (travelButtonLabel != null)
            travelButtonLabel.text = isTraveling ? "이동 중..." : canTravel ? "목표 경로로 이동" : "이동 불가";
    }

    private async void ConfirmTravel()
    {
        if (isTraveling || selectedLocation == null || flow == null)
            return;
        if (!flow.CanTravelTo(selectedLocation.Id, out _))
        {
            SetFeedback("현재 목표로 지정된 장소만 이동할 수 있습니다.");
            RefreshSelection();
            return;
        }

        isTraveling = true;
        RefreshSelection();
        StorySceneTravelResult result = await flow.TravelAsync(selectedLocation.Id);
        if (!result.Success)
        {
            isTraveling = false;
            SetFeedback("이동을 완료하지 못했습니다. 현재 목표를 다시 확인해 주세요.");
            RefreshSelection();
        }
    }

    private bool IsNodeVisible(LocationDefinition location)
    {
        if (location?.MapNode == null)
            return false;
        if (location.MapNode.AccessMode != MapNodeAccessMode.RouteOnly)
            return true;

        string currentId = state?.State.currentLocationId;
        if (string.Equals(currentId, location.Id, StringComparison.Ordinal))
            return true;
        return flow != null
            && flow.TryGetPendingTravel(out PendingStorySceneTravel pending)
            && string.Equals(pending.DestinationId, location.Id, StringComparison.Ordinal);
    }

    private NodeState ResolveNodeState(LocationDefinition location)
    {
        if (string.Equals(state?.State.currentLocationId, location.Id, StringComparison.Ordinal))
            return NodeState.Current;
        if (flow != null
            && flow.TryGetPendingTravel(out PendingStorySceneTravel pending)
            && string.Equals(pending.DestinationId, location.Id, StringComparison.Ordinal))
        {
            return NodeState.Objective;
        }
        return state?.State.unlockedLocations.Contains(location.Id) == true
            ? NodeState.Available
            : NodeState.Locked;
    }

    private static void ApplyNodeState(Button node, NodeState state)
    {
        if (node == null)
            return;
        ColorBlock colors = node.colors;
        colors.normalColor = state switch
        {
            NodeState.Current => new Color(0.45f, 0.34f, 0.19f, 1f),
            NodeState.Objective => new Color(0.64f, 0.42f, 0.08f, 1f),
            NodeState.Available => new Color(0.20f, 0.16f, 0.13f, 1f),
            _ => new Color(0.12f, 0.13f, 0.15f, 0.78f),
        };
        colors.selectedColor = colors.normalColor;
        node.colors = colors;
        node.interactable = state != NodeState.Locked;
    }

    private static string FormatNodeLabel(LocationDefinition location, NodeState state)
    {
        string name = ResolveNodeName(location);
        return state switch
        {
            NodeState.Current => $"현재 · {name}",
            NodeState.Objective => $"목표 · {name}",
            NodeState.Locked => $"잠김 · {name}",
            _ => name,
        };
    }

    private string ResolveSelectionStatus(LocationDefinition location, bool canTravel)
    {
        if (location == null)
            return "선택 없음";
        if (string.Equals(state?.State.currentLocationId, location.Id, StringComparison.Ordinal))
            return "현재 위치";
        if (canTravel)
            return "이동 가능한 목표";
        if (ResolveNodeState(location) == NodeState.Locked)
            return "아직 이동할 수 없는 장소";
        return "현재 이동 경로에 포함되지 않음";
    }

    private void RefreshCurrentLocation()
    {
        if (locationLabel == null)
            return;
        string locationId = state?.State.currentLocationId;
        string displayName = ResolveLocationName(locationId);
        locationLabel.text = string.IsNullOrWhiteSpace(displayName)
            ? "현재 위치 · 알 수 없는 위치"
            : $"현재 위치 · {displayName}";
    }

    private string ResolveLocationName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;
        if (AppContext.Services != null
            && AppContext.Services.TryGet(out ContentDatabase content)
            && content.TryGetLocation(id, out LocationDefinition location))
        {
            return location.DisplayName;
        }
        if (maps != null)
        {
            foreach (MapDefinition map in maps)
            {
                if (map?.Locations == null)
                    continue;
                foreach (LocationDefinition candidate in map.Locations)
                    if (candidate != null && string.Equals(candidate.Id, id, StringComparison.Ordinal))
                        return candidate.DisplayName;
            }
        }
        return string.Empty;
    }

    private static string ResolveNodeName(LocationDefinition location)
    {
        if (!string.IsNullOrWhiteSpace(location?.MapNode?.DisplayName))
            return location.MapNode.DisplayName.Trim();
        if (!string.IsNullOrWhiteSpace(location?.DisplayName))
            return location.DisplayName.Trim();
        return "알 수 없는 장소";
    }

    private static string ResolveNodeDescription(LocationDefinition location) =>
        !string.IsNullOrWhiteSpace(location?.MapNode?.Description)
            ? location.MapNode.Description.Trim()
            : "이 장소에 대한 상세 정보가 아직 없습니다.";

    private static string ResolveMapName(MapDefinition map) =>
        !string.IsNullOrWhiteSpace(map?.DisplayName)
            ? map.DisplayName.Trim()
            : FormatDeckLabel(map?.Id);

    public static string FormatDeckLabel(string id) => id switch
    {
        "MAP_Deck07" => "7층 갑판",
        "MAP_Deck08" => "8층 갑판",
        "MAP_Deck09" => "9층 갑판",
        "MAP_Deck10" => "10층 갑판",
        "MAP_MVElysium" => "M.V. 엘리시움",
        _ => string.Empty,
    };

    private int FindMapIndex(string locationId)
    {
        if (maps == null || string.IsNullOrWhiteSpace(locationId))
            return -1;
        for (var index = 0; index < maps.Length; index++)
            if (MapContains(maps[index], locationId))
                return index;
        return -1;
    }

    private bool TryFindVisibleLocation(
        MapDefinition map,
        string locationId,
        out LocationDefinition result)
    {
        result = null;
        if (map?.Locations == null)
            return false;
        foreach (LocationDefinition location in map.Locations)
        {
            if (location != null
                && string.Equals(location.Id, locationId, StringComparison.Ordinal)
                && IsNodeVisible(location))
            {
                result = location;
                return true;
            }
        }
        return false;
    }

    private static bool MapContains(MapDefinition map, string locationId)
    {
        if (map?.Locations == null || string.IsNullOrWhiteSpace(locationId))
            return false;
        foreach (LocationDefinition location in map.Locations)
            if (location != null && string.Equals(location.Id, locationId, StringComparison.Ordinal))
                return true;
        return false;
    }

    private static void ApplyLayer(Image image, Sprite sprite)
    {
        if (image == null)
            return;
        image.sprite = sprite;
        image.gameObject.SetActive(sprite != null);
    }

    private static void ConfigureLayerToggle(Toggle toggle, bool available)
    {
        if (toggle == null)
            return;
        if (!available)
            toggle.SetIsOnWithoutNotify(false);
        toggle.gameObject.SetActive(available);
    }

    private void RefreshLayers()
    {
        if (restrictedLayer != null)
            restrictedLayer.gameObject.SetActive(
                restrictedLayer.sprite != null
                && restrictedToggle != null
                && restrictedToggle.gameObject.activeSelf
                && restrictedToggle.isOn);
        if (technicalLayer != null)
            technicalLayer.gameObject.SetActive(
                technicalLayer.sprite != null
                && technicalToggle != null
                && technicalToggle.gameObject.activeSelf
                && technicalToggle.isOn);
    }

    private void SetFeedback(string message)
    {
        if (feedbackLabel != null)
            feedbackLabel.text = message ?? string.Empty;
    }

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Exploration);
    }
}
