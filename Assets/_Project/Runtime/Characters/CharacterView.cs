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
    private CharacterIdleMotion idleMotion;
    private Outline silhouette;

    private void Awake()
    {
        idleMotion = GetComponent<CharacterIdleMotion>()
            ?? gameObject.AddComponent<CharacterIdleMotion>();
        if (image != null)
        {
            silhouette = image.GetComponent<Outline>()
                ?? image.gameObject.AddComponent<Outline>();
            silhouette.effectColor = new Color(0.015f, 0.02f, 0.03f, 0.58f);
            silhouette.effectDistance = new Vector2(7f, -5f);
            silhouette.useGraphicAlpha = true;
        }
    }

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
        idleMotion?.Configure(Definition?.Id?.GetHashCode() ?? 0);
        gameObject.SetActive(Definition != null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickable)
            Clicked?.Invoke(this);
    }
}
