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
    private CharacterPresentationProfile defaultPresentation;
    private CharacterPresentationProfile activePresentation;

    private void Awake()
    {
        idleMotion = GetComponent<CharacterIdleMotion>()
            ?? gameObject.AddComponent<CharacterIdleMotion>();
        if (image != null)
        {
            silhouette = image.GetComponent<Outline>()
                ?? image.gameObject.AddComponent<Outline>();
            silhouette.useGraphicAlpha = true;
        }
    }

    public void ConfigurePresentation(CharacterPresentationProfile profile) =>
        defaultPresentation = profile;

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
        CharacterPresentationProfile presentation =
            Definition?.PresentationOverride ?? defaultPresentation;
        activePresentation = presentation;
        if (silhouette != null && presentation != null)
        {
            silhouette.effectColor = presentation.SilhouetteColor;
            silhouette.effectDistance = presentation.SilhouetteDistance;
        }
        idleMotion?.Configure(StableHash(Definition?.Id), presentation);
        gameObject.SetActive(Definition != null);
    }

    public void SetDialogueFocus(bool dialogueActive, bool focused)
    {
        if (image == null)
            return;
        image.color = !dialogueActive || activePresentation == null
            ? Color.white
            : focused
                ? activePresentation.DialogueFocusTint
                : activePresentation.DialogueUnfocusedTint;
        if (dialogueActive && focused)
            transform.SetAsLastSibling();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickable)
            Clicked?.Invoke(this);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 17;
            if (value != null)
                foreach (char character in value)
                    hash = hash * 31 + character;
            return hash;
        }
    }
}
