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

    public void Configure(SfxController controller, AudioClip hover, AudioClip click)
    {
        sfx = controller;
        hoverClip = hover;
        clickClip = click;
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;
        transform.localScale = Vector3.one * 1.035f;
        sfx?.Play(hoverClip, 0.45f);
    }

    public void OnPointerExit(PointerEventData eventData) =>
        transform.localScale = Vector3.one;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable())
            transform.localScale = Vector3.one * 0.98f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsInteractable())
            transform.localScale = Vector3.one * 1.035f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInteractable())
            sfx?.Play(clickClip, 0.55f);
    }

    private bool IsInteractable() => button == null || button.IsInteractable();
}
