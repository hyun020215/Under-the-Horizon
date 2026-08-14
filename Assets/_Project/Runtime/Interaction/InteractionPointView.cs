using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InteractionPointView : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TooltipView tooltip;

    public InteractionDefinition Definition { get; private set; }
    public TooltipView Tooltip => tooltip;
    public event Action<InteractionPointView> Clicked;
    private InteractionFeedbackService feedback;

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
        }

        tooltip?.Hide();
        gameObject.SetActive(definition != null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Definition != null)
        {
            ResolveFeedback()?.Click();
            tooltip?.Hide();
            Clicked?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Definition != null)
        {
            ResolveFeedback()?.Enter();
            tooltip?.Show(Definition.DisplayName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        feedback?.Exit();
        tooltip?.Hide();
    }

    private void OnDisable()
    {
        feedback?.Exit();
        tooltip?.Hide();
    }

    private InteractionFeedbackService ResolveFeedback()
    {
        if (feedback == null)
            AppContext.Services?.TryGet(out feedback);
        return feedback;
    }
}
