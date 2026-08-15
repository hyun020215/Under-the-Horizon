using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class WorldContentFrame : MonoBehaviour
{
    [SerializeField]
    private AspectRatioFitter backgroundAspect;

    [SerializeField]
    private AspectRatioFitter backgroundCharacterAspect;

    public void ApplyAspectRatio(float aspectRatio)
    {
        if (aspectRatio <= 0f
            || float.IsNaN(aspectRatio)
            || float.IsInfinity(aspectRatio))
        {
            throw new ArgumentOutOfRangeException(
                nameof(aspectRatio),
                aspectRatio,
                "World content aspect ratio must be finite and positive.");
        }

        if (backgroundAspect == null || backgroundCharacterAspect == null)
        {
            throw new InvalidOperationException(
                "WorldContentFrame requires both background aspect fitters.");
        }

        backgroundAspect.aspectRatio = aspectRatio;
        backgroundCharacterAspect.aspectRatio = aspectRatio;
    }
}
