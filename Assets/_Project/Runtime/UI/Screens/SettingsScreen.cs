using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class SettingsScreen : ScreenBase
{
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private Button backButton;
    [SerializeField] private AudioDirector audioDirector;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button applyDisplayButton;

    private DisplaySettingsService displaySettings;

    private void Awake()
    {
        backButton?.onClick.AddListener(Back);
        masterSlider?.onValueChanged.AddListener(value => audioDirector?.SetMasterVolume(value));
        musicSlider?.onValueChanged.AddListener(value => audioDirector?.SetMusicVolume(value));
        sfxSlider?.onValueChanged.AddListener(value => audioDirector?.SetSfxVolume(value));
        applyDisplayButton?.onClick.AddListener(ApplyDisplaySettings);
        if (AppContext.Services != null &&
            AppContext.Services.TryGet(out DisplaySettingsService settings))
        {
            displaySettings = settings;
            PopulateDisplaySettings();
        }
    }

    private async void Back()
    {
        if (screens != null)
            await screens.OpenAsync(ScreenId.Title);
    }

    private void PopulateDisplaySettings()
    {
        if (resolutionDropdown != null)
        {
            var options = new List<string>();
            foreach (DisplaySettingsService.DisplayResolution resolution in
                     displaySettings.Resolutions)
                options.Add(resolution.Label);
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(displaySettings.SelectedIndex);
        }
        fullscreenToggle?.SetIsOnWithoutNotify(displaySettings.Fullscreen);
    }

    private void ApplyDisplaySettings()
    {
        if (displaySettings == null)
            return;
        int index = resolutionDropdown != null
            ? resolutionDropdown.value
            : displaySettings.SelectedIndex;
        bool fullscreen = fullscreenToggle == null || fullscreenToggle.isOn;
        displaySettings.Apply(index, fullscreen);
    }
}
