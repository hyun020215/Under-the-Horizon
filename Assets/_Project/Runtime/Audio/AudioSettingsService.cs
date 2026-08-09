using UnityEngine;

public sealed class AudioSettingsService
{
    private const string MasterVolumeKey = "audio.master";
    private const string MusicVolumeKey = "audio.music";
    private const string SfxVolumeKey = "audio.sfx";

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    public void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    }

    public void Set(float master, float music, float sfx)
    {
        MasterVolume = Mathf.Clamp01(master);
        MusicVolume = Mathf.Clamp01(music);
        SfxVolume = Mathf.Clamp01(sfx);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
    }

    public void Apply(AudioDirector audio)
    {
        if (audio == null)
            return;

        audio.SetMasterVolume(MasterVolume);
        audio.SetMusicVolume(MusicVolume);
        audio.SetSfxVolume(SfxVolume);
    }
}
