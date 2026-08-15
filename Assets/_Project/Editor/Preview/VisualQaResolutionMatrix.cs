using System.Collections.Generic;
using UnityEngine;

public static class VisualQaResolutionMatrix
{
    private static readonly Vector2Int[] Values =
    {
        new(1920, 1080),
        new(2560, 1440),
        new(1920, 1200),
        new(2560, 1080),
        new(3440, 1440),
    };

    public static IReadOnlyList<Vector2Int> Resolutions => Values;
}
