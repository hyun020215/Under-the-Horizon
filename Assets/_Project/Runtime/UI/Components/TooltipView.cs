using UnityEngine;
using UnityEngine.UI;

public sealed class TooltipView : MonoBehaviour
{
    [SerializeField]
    private Text label;

    public bool IsVisible => gameObject.activeSelf;
    public string Text => label != null ? label.text : string.Empty;

    public void Show(string value)
    {
        string next = value?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(next))
        {
            Hide();
            return;
        }

        if (label != null)
            label.text = next;
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
