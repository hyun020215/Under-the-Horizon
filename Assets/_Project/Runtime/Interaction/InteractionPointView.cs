using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InteractionPointView : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, ISubmitHandler
{
    private const float WorldTooltipEdgeThreshold = 0.25f;
    private const float WorldTooltipTopThreshold = 0.75f;
    private const float WorldTooltipGap = 12f;
    private const float WorldMarkerInset = 36f;

    [SerializeField]
    private TooltipView tooltip;

    [SerializeField]
    private RectTransform marker;

    public InteractionDefinition Definition { get; private set; }
    public TooltipView Tooltip => tooltip;
    public RectTransform Marker => ResolveMarker();
    public event Action<InteractionPointView> Clicked;
    private InteractionFeedbackService feedback;
    private bool pointerInside;
    private bool selected;
    private bool tooltipSuppressed;

    private void Awake()
    {
        AppContext.Services?.TryGet(out feedback);
        tooltip?.Hide();
    }

    public void Apply(InteractionDefinition definition) =>
        Apply(definition, applyNormalizedRect: true);

    public void ApplyAnchored(InteractionDefinition definition) =>
        Apply(definition, applyNormalizedRect: false);

    private void Apply(InteractionDefinition definition, bool applyNormalizedRect)
    {
        Definition = definition;
        gameObject.name = definition == null
            ? "Hotspot"
            : $"Hotspot_{definition.Id}";

        if (applyNormalizedRect
            && transform is RectTransform rect
            && definition != null)
        {
            Rect normalized = definition.NormalizedRect;
            rect.anchorMin = normalized.min;
            rect.anchorMax = normalized.max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            PositionWorldMarker(normalized.center);
            PositionWorldTooltip(normalized.center);
        }

        pointerInside = false;
        selected = false;
        tooltipSuppressed = false;
        RefreshMarkerVisibility();
        tooltip?.Hide();
        gameObject.SetActive(definition != null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Activate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Definition != null)
        {
            pointerInside = true;
            tooltipSuppressed = false;
            ResolveFeedback()?.Enter();
            RefreshMarkerVisibility();
            RefreshTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        feedback?.Exit();
        RefreshMarkerVisibility();
        RefreshTooltip();
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        tooltipSuppressed = false;
        RefreshMarkerVisibility();
        RefreshTooltip();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        tooltipSuppressed = false;
        RefreshMarkerVisibility();
        RefreshTooltip();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Activate();
    }

    private void OnDisable()
    {
        pointerInside = false;
        selected = false;
        tooltipSuppressed = false;
        RefreshMarkerVisibility();
        feedback?.Exit();
        tooltip?.Hide();
    }

    private void Activate()
    {
        if (Definition == null)
            return;

        ResolveFeedback()?.Click();
        tooltipSuppressed = true;
        tooltip?.Hide();
        Clicked?.Invoke(this);
    }

    private void RefreshTooltip()
    {
        if (!tooltipSuppressed
            && Definition != null
            && (pointerInside || selected))
            tooltip?.Show(Definition.DisplayName);
        else
            tooltip?.Hide();
    }

    private void PositionWorldMarker(Vector2 normalizedCenter)
    {
        RectTransform markerRect = ResolveMarker();
        if (markerRect == null)
            return;

        var anchor = new Vector2(0.5f, 0.5f);
        var offset = Vector2.zero;
        if (normalizedCenter.x <= WorldTooltipEdgeThreshold)
        {
            anchor.x = 1f;
            offset.x = -WorldMarkerInset;
        }
        else if (normalizedCenter.x >= 1f - WorldTooltipEdgeThreshold)
        {
            anchor.x = 0f;
            offset.x = WorldMarkerInset;
        }

        if (normalizedCenter.y <= WorldTooltipEdgeThreshold)
        {
            anchor.y = 1f;
            offset.y = -WorldMarkerInset;
        }
        else if (normalizedCenter.y >= WorldTooltipTopThreshold)
        {
            anchor.y = 0f;
            offset.y = WorldMarkerInset;
        }

        markerRect.anchorMin = markerRect.anchorMax = anchor;
        markerRect.anchoredPosition = offset;
    }

    private void PositionWorldTooltip(Vector2 normalizedCenter)
    {
        if (tooltip?.transform is not RectTransform tooltipRect
            || ResolveMarker() is not RectTransform markerRect)
            return;

        tooltipRect.anchorMin = tooltipRect.anchorMax = markerRect.anchorMin;
        Vector2 position = markerRect.anchoredPosition;
        float horizontalClearance = markerRect.sizeDelta.x * 0.5f + WorldTooltipGap;
        float verticalClearance = markerRect.sizeDelta.y * 0.5f + WorldTooltipGap;

        float pivotX = 0.5f;
        if (normalizedCenter.x <= WorldTooltipEdgeThreshold)
        {
            pivotX = 0f;
            position.x += horizontalClearance;
        }
        else if (normalizedCenter.x >= 1f - WorldTooltipEdgeThreshold)
        {
            pivotX = 1f;
            position.x -= horizontalClearance;
        }

        bool nearTop = normalizedCenter.y >= WorldTooltipTopThreshold;
        position.y += nearTop ? -verticalClearance : verticalClearance;
        tooltipRect.pivot = new Vector2(pivotX, nearTop ? 1f : 0f);
        tooltipRect.anchoredPosition = position;
    }

    private void RefreshMarkerVisibility()
    {
        RectTransform markerRect = ResolveMarker();
        if (markerRect == null)
            return;

        bool visible = Definition?.WorldMarkerVisibility switch
        {
            WorldMarkerVisibility.HoverOrFocus => pointerInside || selected,
            WorldMarkerVisibility.Hidden => false,
            _ => Definition != null,
        };
        markerRect.gameObject.SetActive(visible);
    }

    private RectTransform ResolveMarker()
    {
        if (marker == null)
            marker = transform.Find("Marker") as RectTransform;
        return marker;
    }

    private InteractionFeedbackService ResolveFeedback()
    {
        if (feedback == null)
            AppContext.Services?.TryGet(out feedback);
        return feedback;
    }
}
