using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InteractionPointView : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public InteractionDefinition Definition { get; private set; }
    public event Action<InteractionPointView> Clicked;
    private InteractionFeedbackService feedback;

    private void Awake() => AppContext.Services?.TryGet(out feedback);

    public void Apply(InteractionDefinition definition)
    {
        Definition = definition;
        gameObject.name = definition == null
            ? "Hotspot"
            : $"Hotspot_{definition.Id}";

        if (transform is RectTransform rect && definition != null)
        {
            Rect normalized = definition.NormalizedRect;
            rect.anchorMin = normalized.min;
            rect.anchorMax = normalized.max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        gameObject.SetActive(definition != null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Definition != null)
        {
            ResolveFeedback()?.Click();
            Clicked?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Definition != null)
            ResolveFeedback()?.Enter();
    }

    public void OnPointerExit(PointerEventData eventData) => feedback?.Exit();

    private void OnDisable() => feedback?.Exit();

    private InteractionFeedbackService ResolveFeedback()
    {
        if (feedback == null)
            AppContext.Services?.TryGet(out feedback);
        return feedback;
    }
}
