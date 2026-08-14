using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InteractionPointView : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, ISubmitHandler
{
    private const float WorldTooltipEdgeThreshold = 0.25f;
    private const float WorldTooltipTopThreshold = 0.75f;
    private const float WorldTooltipOffset = 48f;
    private const float WorldMarkerInset = 36f;

    [SerializeField]
    private TooltipView tooltip;

    public InteractionDefinition Definition { get; private set; }
    public TooltipView Tooltip => tooltip;
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
            RefreshTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        feedback?.Exit();
        RefreshTooltip();
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        tooltipSuppressed = false;
        RefreshTooltip();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        tooltipSuppressed = false;
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
        if (transform.Find("Marker") is not RectTransform markerRect)
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
        if (tooltip?.transform is not RectTransform tooltipRect)
            return;

        tooltipRect.anchorMin = tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);

        float pivotX = 0.5f;
        float offsetX = 0f;
        if (normalizedCenter.x <= WorldTooltipEdgeThreshold)
        {
            pivotX = 0f;
            offsetX = WorldTooltipOffset;
        }
        else if (normalizedCenter.x >= 1f - WorldTooltipEdgeThreshold)
        {
            pivotX = 1f;
            offsetX = -WorldTooltipOffset;
        }

        bool nearTop = normalizedCenter.y >= WorldTooltipTopThreshold;
        tooltipRect.pivot = new Vector2(pivotX, nearTop ? 1f : 0f);
        tooltipRect.anchoredPosition = new Vector2(
            offsetX,
            nearTop ? -WorldTooltipOffset : WorldTooltipOffset);
    }

    private InteractionFeedbackService ResolveFeedback()
    {
        if (feedback == null)
            AppContext.Services?.TryGet(out feedback);
        return feedback;
    }
}
