using UnityEngine;

public sealed class UIInputBlocker : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup group;

    public void SetBlocked(bool blocked)
    {
        if (group == null)
            return;
        group.blocksRaycasts = blocked;
        group.interactable = blocked;
        group.alpha = blocked ? 1 : 0;
    }
}
