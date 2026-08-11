using System;
using UnityEngine;

public sealed class AccessibilitySettingsService
{
    private const string ReducedMotionKey = "accessibility.reducedMotion";
    private const string TextSpeedKey = "accessibility.textSpeed";
    private static readonly float[] Speeds = { 30f, 45f, 75f, float.PositiveInfinity };

    public bool ReducedMotion { get; private set; }
    public int TextSpeedIndex { get; private set; } = 1;
    public float CharactersPerSecond => Speeds[Mathf.Clamp(TextSpeedIndex, 0, Speeds.Length - 1)];
    public event Action Changed;

    public void Load()
    {
        ReducedMotion = PlayerPrefs.GetInt(ReducedMotionKey, 0) != 0;
        TextSpeedIndex = Mathf.Clamp(PlayerPrefs.GetInt(TextSpeedKey, 1), 0, Speeds.Length - 1);
    }

    public void Apply(bool reducedMotion, int textSpeedIndex)
    {
        ReducedMotion = reducedMotion;
        TextSpeedIndex = Mathf.Clamp(textSpeedIndex, 0, Speeds.Length - 1);
        PlayerPrefs.SetInt(ReducedMotionKey, ReducedMotion ? 1 : 0);
        PlayerPrefs.SetInt(TextSpeedKey, TextSpeedIndex);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
