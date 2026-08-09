using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InteractionPointView : MonoBehaviour, IPointerClickHandler
{
    public InteractionDefinition Definition { get; private set; }
    public event Action<InteractionPointView> Clicked;

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
            Clicked?.Invoke(this);
    }
}
