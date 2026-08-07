using UnityEngine;

[CreateAssetMenu(fileName = "UITheme", menuName = "Under The Horizon/UI/Theme Profile")]
public sealed class UIThemeProfile : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private Sprite panel;

    [SerializeField]
    private Sprite primaryButton;

    [SerializeField]
    private Color textColor = new(0.93f, 0.9f, 0.82f, 1f);

    [SerializeField]
    private Color accentColor = new(0.55f, 0.42f, 0.68f, 1f);

    public string Id => id;
    public Sprite Panel => panel;
    public Sprite PrimaryButton => primaryButton;
    public Color TextColor => textColor;
    public Color AccentColor => accentColor;
}
