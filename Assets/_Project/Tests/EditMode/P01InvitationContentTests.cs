using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class P01InvitationContentTests
{
    private const string PortPath =
        "Assets/_Project/Content/Locations/Definitions/LOC_PORT.asset";
    private const string PortDefaultStatePath =
        "Assets/_Project/Content/Locations/States/Generated/PORT_Default.asset";
    private const string P01InvitationStatePath =
        "Assets/_Project/Content/Locations/States/Port/PORT_P01_Invitation.asset";
    private const string P01ScenePath =
        "Assets/_Project/Content/StoryScenes/Prologue/P01_PortJournalist.asset";
    private const string D803ScenePath =
        "Assets/_Project/Content/StoryScenes/Day08/D8_03_ReturnToPort.asset";
    private const string BasePortBackgroundPath =
        "Assets/_Project/Art/Backgrounds/Locations/BG_location_port.png";
    private const string InvitationPortBackgroundPath =
        "Assets/_Project/Art/Backgrounds/Locations/BG_location_port_evidence.png";
    private const string InvitationInteractionPath =
        "Assets/_Project/Content/Locations/InteractionDefinitions/Generated/"
        + "INT_P_01_INVITATION.asset";

    [Test]
    public void P01UsesApprovedInvitationBackgroundWithoutChangingD803()
    {
        LocationDefinition port = Load<LocationDefinition>(PortPath);
        LocationStateDefinition portDefault =
            Load<LocationStateDefinition>(PortDefaultStatePath);
        LocationStateDefinition p01Invitation =
            Load<LocationStateDefinition>(P01InvitationStatePath);
        StorySceneDefinition p01 = Load<StorySceneDefinition>(P01ScenePath);
        StorySceneDefinition d803 = Load<StorySceneDefinition>(D803ScenePath);

        Assert.That(p01.Location, Is.SameAs(port));
        Assert.That(d803.Location, Is.SameAs(port));
        Assert.That(p01Invitation.Id, Is.EqualTo("PORT_P01_INVITATION"));
        Assert.That(p01.LocationState, Is.SameAs(p01Invitation));
        Assert.That(p01.LocationState, Is.Not.SameAs(d803.LocationState));
        Assert.That(
            AssetDatabase.GetAssetPath(d803.LocationState.Background),
            Is.Not.EqualTo(InvitationPortBackgroundPath),
            "D8-03 may migrate to its own epilogue state, but must never reuse P-01 evidence art.");

        Assert.That(
            AssetDatabase.GetAssetPath(p01Invitation.Background),
            Is.EqualTo(InvitationPortBackgroundPath),
            "P-01 must use the approved Port composition with C-01 painted on the bench.");
        Assert.That(
            AssetDatabase.AssetPathToGUID(InvitationPortBackgroundPath),
            Is.EqualTo("fd85cdd6e6e64585af8ae2d616ecab6e"));
        Assert.That(
            AssetDatabase.GetAssetPath(portDefault.Background),
            Is.EqualTo(BasePortBackgroundPath),
            "The shared Port default must remain free of P-01-specific evidence.");
        Assert.That(
            AssetDatabase.GetAssetPath(port.DefaultBackground),
            Is.EqualTo(BasePortBackgroundPath));
        Assert.That(port.States, Does.Contain(portDefault));
        Assert.That(port.States, Does.Contain(p01Invitation));
        Assert.That(port.States.Distinct().Count(), Is.EqualTo(port.States.Length));
    }

    [Test]
    public void P01InvitationKeepsItsStableEvidenceAndHotspotContract()
    {
        StorySceneDefinition p01 = Load<StorySceneDefinition>(P01ScenePath);
        InteractionDefinition invitation =
            Load<InteractionDefinition>(InvitationInteractionPath);

        Assert.That(
            p01.InteractionSet.Interactions.Select(item => item.Id),
            Is.EqualTo(new[]
            {
                "INT_P_01_INVITATION",
                "INT_P_01_MESSENGER",
                "INT_P_01_DIALOGUE",
            }));
        Assert.That(p01.InteractionSet.Interactions[0], Is.SameAs(invitation));
        Assert.That(invitation.Id, Is.EqualTo("INT_P_01_INVITATION"));
        Assert.That(invitation.Type, Is.EqualTo(InteractionType.Investigation));
        Assert.That(invitation.TargetId, Is.EqualTo("C-01"));
        Assert.That(invitation.HasWorldHotspot, Is.True);
        Assert.That(invitation.Repeatable, Is.False);
        Assert.That(invitation.Action.GrantsEvidence, Is.True);
        Assert.That(
            invitation.NormalizedRect,
            Is.EqualTo(new Rect(0.012f, 0.182f, 0.066f, 0.086f)),
            "The hit area must remain aligned with the approved C-01 background polygon.");

        SerializedProperty markerVisibility = new SerializedObject(invitation)
            .FindProperty("worldMarkerVisibility");
        Assert.That(
            markerVisibility,
            Is.Not.Null,
            "World interactions require an explicit, reusable marker visibility policy.");
        Assert.That(
            markerVisibility.intValue,
            Is.EqualTo(1),
            "The baked invitation must be visible at rest; its marker appears on hover or focus.");
    }

    private static T Load<T>(string path)
        where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        Assert.That(asset, Is.Not.Null, path);
        return asset;
    }
}
