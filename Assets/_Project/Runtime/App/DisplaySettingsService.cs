using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DisplaySettingsService
{
    public readonly struct DisplayResolution : IEquatable<DisplayResolution>
    {
        public DisplayResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public string Label => $"{Width} × {Height}";

        public bool Equals(DisplayResolution other) =>
            Width == other.Width && Height == other.Height;

        public override bool Equals(object obj) =>
            obj is DisplayResolution other && Equals(other);

        public override int GetHashCode() => (Width * 397) ^ Height;
    }

    private const string WidthKey = "display.width";
    private const string HeightKey = "display.height";
    private const string FullscreenKey = "display.fullscreen";
    public const int RecommendedWidth = 1920;
    public const int RecommendedHeight = 1080;

    private static readonly DisplayResolution[] Supported =
    {
        new(1280, 720),
        new(1600, 900),
        new(1920, 1080),
        new(2560, 1440),
        new(3840, 2160),
    };

    public IReadOnlyList<DisplayResolution> Resolutions => Supported;
    public int SelectedIndex { get; private set; } = 2;
    public bool Fullscreen { get; private set; } = true;

    public void Load()
    {
        var saved = new DisplayResolution(
            PlayerPrefs.GetInt(WidthKey, RecommendedWidth),
            PlayerPrefs.GetInt(HeightKey, RecommendedHeight));
        SelectedIndex = FindClosestIndex(saved.Width, saved.Height);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) != 0;
    }

    public void Apply(int resolutionIndex, bool fullscreen)
    {
        SelectedIndex = Mathf.Clamp(resolutionIndex, 0, Supported.Length - 1);
        Fullscreen = fullscreen;
        DisplayResolution resolution = Supported[SelectedIndex];
        FullScreenMode mode = fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
        Screen.SetResolution(resolution.Width, resolution.Height, mode);
        PlayerPrefs.SetInt(WidthKey, resolution.Width);
        PlayerPrefs.SetInt(HeightKey, resolution.Height);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public int FindClosestIndex(int width, int height)
    {
        int bestIndex = 0;
        long bestDistance = long.MaxValue;
        for (var index = 0; index < Supported.Length; index++)
        {
            long widthDelta = Supported[index].Width - width;
            long heightDelta = Supported[index].Height - height;
            long distance = widthDelta * widthDelta + heightDelta * heightDelta;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = index;
            }
        }
        return bestIndex;
    }
}
