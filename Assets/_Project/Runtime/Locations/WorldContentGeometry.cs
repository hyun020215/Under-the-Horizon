using System;
using UnityEngine;

public static class WorldContentGeometry
{
    public static Rect CalculateCoverRect(
        Vector2 viewportSize,
        float backgroundAspectRatio)
    {
        Validate(viewportSize, backgroundAspectRatio);

        float viewportAspectRatio = viewportSize.x / viewportSize.y;
        if (viewportAspectRatio >= backgroundAspectRatio)
        {
            float height = viewportSize.x / backgroundAspectRatio;
            return new Rect(
                0f,
                (viewportSize.y - height) * 0.5f,
                viewportSize.x,
                height);
        }

        float width = viewportSize.y * backgroundAspectRatio;
        return new Rect(
            (viewportSize.x - width) * 0.5f,
            0f,
            width,
            viewportSize.y);
    }

    public static Vector2 BackgroundToViewportNormalized(
        Vector2 backgroundNormalized,
        Vector2 viewportSize,
        float backgroundAspectRatio)
    {
        Rect cover = CalculateCoverRect(viewportSize, backgroundAspectRatio);
        Vector2 viewportPoint = cover.min + Vector2.Scale(
            backgroundNormalized,
            cover.size);
        return new Vector2(
            viewportPoint.x / viewportSize.x,
            viewportPoint.y / viewportSize.y);
    }

    public static Vector2 ViewportToBackgroundNormalized(
        Vector2 viewportNormalized,
        Vector2 viewportSize,
        float backgroundAspectRatio)
    {
        Rect cover = CalculateCoverRect(viewportSize, backgroundAspectRatio);
        Vector2 viewportPoint = Vector2.Scale(viewportNormalized, viewportSize);
        return new Vector2(
            (viewportPoint.x - cover.xMin) / cover.width,
            (viewportPoint.y - cover.yMin) / cover.height);
    }

    private static void Validate(
        Vector2 viewportSize,
        float backgroundAspectRatio)
    {
        if (!IsFinitePositive(viewportSize.x)
            || !IsFinitePositive(viewportSize.y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(viewportSize),
                viewportSize,
                "Viewport dimensions must be finite and positive.");
        }

        if (!IsFinitePositive(backgroundAspectRatio))
        {
            throw new ArgumentOutOfRangeException(
                nameof(backgroundAspectRatio),
                backgroundAspectRatio,
                "Background aspect ratio must be finite and positive.");
        }
    }

    private static bool IsFinitePositive(float value) =>
        value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
}
