using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private Button backButton;
    [SerializeField] private AudioDirector audioDirector;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        backButton?.onClick.AddListener(Back);
        masterSlider?.onValueChanged.AddListener(value => audioDirector?.SetMasterVolume(value));
        musicSlider?.onValueChanged.AddListener(value => audioDirector?.SetMusicVolume(value));
        sfxSlider?.onValueChanged.AddListener(value => audioDirector?.SetSfxVolume(value));
    }

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Title);
    }
}
