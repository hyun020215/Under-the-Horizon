using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UiButtonFeedback : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    private SfxController sfx;
    private AudioClip hoverClip;
    private AudioClip clickClip;
    private Button button;
    private Graphic graphic;
    private Vector3 baseScale;
    private Color baseColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        graphic = GetComponent<Graphic>();
        baseScale = transform.localScale;
        if (graphic != null)
            baseColor = graphic.color;
    }

    public void Configure(SfxController controller, AudioClip hover, AudioClip click)
    {
        sfx = controller;
        hoverClip = hover;
        clickClip = click;
        button ??= GetComponent<Button>();
        graphic ??= GetComponent<Graphic>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;
        Apply(1.035f, 1.10f);
        sfx?.Play(hoverClip, 0.45f);
    }

    public void OnPointerExit(PointerEventData eventData) => Apply(1f, 1f);

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable())
            Apply(0.98f, 0.94f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsInteractable())
            Apply(1.035f, 1.10f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInteractable())
            sfx?.Play(clickClip, 0.55f);
    }

    private bool IsInteractable() => button == null || button.IsInteractable();

    private void Apply(float scale, float brightness)
    {
        transform.localScale = baseScale * scale;
        if (graphic == null)
            return;
        graphic.color = new Color(
            Mathf.Min(1f, baseColor.r * brightness),
            Mathf.Min(1f, baseColor.g * brightness),
            Mathf.Min(1f, baseColor.b * brightness),
            baseColor.a);
    }

    private void OnDisable() => Apply(1f, 1f);
}
