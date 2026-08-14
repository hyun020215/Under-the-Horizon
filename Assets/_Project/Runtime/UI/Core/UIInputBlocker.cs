using UnityEngine;
using UnityEngine.UI;

public sealed class UIInputBlocker : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup group;

    private void Awake()
    {
        if (group != null)
            SetBlocked(group.blocksRaycasts);
    }

    public void SetBlocked(bool blocked)
    {
        if (group == null)
            return;
        group.blocksRaycasts = blocked;
        group.interactable = blocked;

        // Keep the full-screen transition surface out of GraphicRaycaster
        // results once the transition releases input. This mirrors the
        // CanvasGroup state and prevents a transparent cover from swallowing
        // clicks on the newly opened screen.
        Graphic raycastSurface = group.GetComponent<Graphic>();
        if (raycastSurface != null)
            raycastSurface.raycastTarget = blocked;
    }
}
