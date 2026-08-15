using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class WorldContentGeometryTests
{
    [TestCase(1920f, 1080f, 0f, -100f, 1920f, 1280f)]
    [TestCase(1920f, 1200f, 0f, -40f, 1920f, 1280f)]
    [TestCase(3440f, 1440f, 0f, -426.66667f, 3440f, 2293.3333f)]
    [TestCase(1000f, 1000f, -250f, 0f, 1500f, 1000f)]
    public void CalculateCoverRectMatchesEnvelopeParentGeometry(
        float width,
        float height,
        float expectedX,
        float expectedY,
        float expectedWidth,
        float expectedHeight)
    {
        Rect actual = WorldContentGeometry.CalculateCoverRect(
            new Vector2(width, height),
            1.5f);

        Assert.That(actual.x, Is.EqualTo(expectedX).Within(0.01f));
        Assert.That(actual.y, Is.EqualTo(expectedY).Within(0.01f));
        Assert.That(actual.width, Is.EqualTo(expectedWidth).Within(0.01f));
        Assert.That(actual.height, Is.EqualTo(expectedHeight).Within(0.01f));
    }

    [TestCase(1920f, 1080f)]
    [TestCase(1920f, 1200f)]
    [TestCase(3440f, 1440f)]
    [TestCase(1000f, 1000f)]
    public void BackgroundAndViewportConversionsRoundTrip(
        float width,
        float height)
    {
        var viewport = new Vector2(width, height);
        var authored = new Vector2(0.6f, 0.205f);

        Vector2 viewportPoint =
            WorldContentGeometry.BackgroundToViewportNormalized(
                authored,
                viewport,
                1.5f);
        Vector2 roundTrip =
            WorldContentGeometry.ViewportToBackgroundNormalized(
                viewportPoint,
                viewport,
                1.5f);

        Assert.That(roundTrip.x, Is.EqualTo(authored.x).Within(0.00001f));
        Assert.That(roundTrip.y, Is.EqualTo(authored.y).Within(0.00001f));
    }

    [Test]
    public void PlacementSpaceNumericValuesRemainBackwardCompatible()
    {
        Assert.That((int)CharacterPlacementSpace.ViewportNormalized, Is.Zero);
        Assert.That((int)CharacterPlacementSpace.BackgroundNormalized, Is.EqualTo(1));
    }

    [Test]
    public void ExistingPlacementSetsRemainViewportNormalizedByDefault()
    {
        CharacterPlacementSet[] sets = AssetDatabase
            .FindAssets("t:CharacterPlacementSet")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CharacterPlacementSet>)
            .ToArray();

        Assert.That(sets, Has.Length.EqualTo(41));
        Assert.That(
            sets.All(set =>
                set.PlacementSpace == CharacterPlacementSpace.ViewportNormalized),
            Is.True,
            "Foundation must not opt existing content into the new coordinate space.");
    }

    [Test]
    public void InvalidGeometryInputsFailLoudly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WorldContentGeometry.CalculateCoverRect(Vector2.zero, 1.5f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WorldContentGeometry.CalculateCoverRect(
                new Vector2(1920f, 1080f),
                float.NaN));
    }
}
