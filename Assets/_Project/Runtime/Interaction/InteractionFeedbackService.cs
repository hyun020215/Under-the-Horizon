using UnityEngine;

public sealed class InteractionFeedbackService : MonoBehaviour
{
    [SerializeField] private Texture2D interactiveCursor;
    [SerializeField] private Vector2 cursorHotspot;
    [SerializeField] private SfxController sfx;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    public void Enter()
    {
        if (interactiveCursor != null)
            Cursor.SetCursor(interactiveCursor, cursorHotspot, CursorMode.Auto);
        sfx?.Play(hoverClip, 0.35f);
    }

    public void Exit() => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

    public void Click() => sfx?.Play(clickClip, 0.5f);

    private void OnDisable() => Exit();
}
