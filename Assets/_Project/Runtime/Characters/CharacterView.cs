using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CharacterView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private InteractionPointView contextBadgePrefab;

    public CharacterDefinition Definition { get; private set; }
    public InteractionPointView ContextBadge => contextBadge;
    public bool BodyInteractionAvailable { get; private set; }
    public event Action<CharacterView> Clicked;
    public event Action<CharacterView, InteractionDefinition> ContextClicked;

    private bool clickable;
    private bool placementAllowsClick;
    private InteractionPointView contextBadge;
    private CharacterIdleMotion idleMotion;
    private Outline silhouette;
    private CharacterPresentationProfile defaultPresentation;
    private CharacterPresentationProfile activePresentation;
    private CharacterPose pose;
    private CharacterExpression expression;

    private void Awake()
    {
        if (contextBadgePrefab != null)
            contextBadge = Instantiate(contextBadgePrefab, transform);
        idleMotion = GetComponent<CharacterIdleMotion>()
            ?? gameObject.AddComponent<CharacterIdleMotion>();
        if (image != null)
        {
            silhouette = image.GetComponent<Outline>()
                ?? image.gameObject.AddComponent<Outline>();
            silhouette.useGraphicAlpha = true;
        }
        if (contextBadge != null)
            contextBadge.Clicked += OnContextBadgeClicked;
    }

    private void OnDestroy()
    {
        if (contextBadge != null)
            contextBadge.Clicked -= OnContextBadgeClicked;
    }

    public void ConfigurePresentation(CharacterPresentationProfile profile) =>
        defaultPresentation = profile;

    public void Apply(CharacterPlacement placement)
    {
        Definition = placement.character;
        pose = placement.pose;
        expression = placement.expression;
        placementAllowsClick = placement.clickable;
        if (image != null)
        {
            image.sprite = Definition?.Resolve(pose, expression);
            image.raycastTarget = false;
        }
        clickable = false;
        BodyInteractionAvailable = false;
        contextBadge?.ApplyAnchored(null);
        RectTransform rect = transform as RectTransform;
        if (rect != null)
            rect.anchorMin = rect.anchorMax = new Vector2(
                placement.normalizedX,
                placement.normalizedY
            );
        float placementScale = placement.scale <= 0 ? 1 : placement.scale;
        transform.localScale = Vector3.one * placementScale;
        if (contextBadge != null)
            contextBadge.transform.localScale = Vector3.one / placementScale;
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

    public void SetBodyInteractionAvailable(bool available)
    {
        BodyInteractionAvailable = placementAllowsClick && available;
        clickable = BodyInteractionAvailable;
        if (image != null)
            image.raycastTarget = BodyInteractionAvailable;
    }

    public void SetContextInteraction(InteractionDefinition definition) =>
        contextBadge?.ApplyAnchored(placementAllowsClick ? definition : null);

    public void ApplyExpression(CharacterExpression next)
    {
        expression = next;
        if (image == null || Definition == null)
            return;
        Sprite resolved = Definition.Resolve(pose, expression);
        if (resolved != null)
            image.sprite = resolved;
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

    private void OnContextBadgeClicked(InteractionPointView view)
    {
        if (view?.Definition != null)
            ContextClicked?.Invoke(this, view.Definition);
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
