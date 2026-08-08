using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CharacterView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image image;
    public CharacterDefinition Definition { get; private set; }
    public event Action<CharacterView> Clicked;

    private bool clickable;

    public void Apply(CharacterPlacement placement)
    {
        Definition = placement.character;
        if (image != null)
        {
            image.sprite = Definition?.Resolve(placement.pose, placement.expression);
            image.raycastTarget = placement.clickable;
        }
        clickable = placement.clickable;
        RectTransform rect = transform as RectTransform;
        if (rect != null)
            rect.anchorMin = rect.anchorMax = new Vector2(
                placement.normalizedX,
                placement.normalizedY
            );
        transform.localScale = Vector3.one * (placement.scale <= 0 ? 1 : placement.scale);
        gameObject.SetActive(Definition != null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickable)
            Clicked?.Invoke(this);
    }
}
