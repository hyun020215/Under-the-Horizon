using UnityEngine;
using UnityEngine.UI;

public sealed class CreditsScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private Button backButton;

    private void Awake() => backButton?.onClick.AddListener(Back);

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Title);
    }
}
