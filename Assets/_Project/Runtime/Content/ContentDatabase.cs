using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DATABASE_Content", menuName = "Under The Horizon/Content/Database")]
public sealed class ContentDatabase : ScriptableObject
{
    [SerializeField]
    private StorySceneDefinition[] storyScenes;

    [SerializeField]
    private LocationDefinition[] locations;

    [SerializeField]
    private EvidenceDefinition[] evidence;
    private Dictionary<string, StorySceneDefinition> sceneIndex;
    private Dictionary<string, LocationDefinition> locationIndex;
    public IReadOnlyList<StorySceneDefinition> StoryScenes => storyScenes;
    public IReadOnlyList<LocationDefinition> Locations => locations;
    public IReadOnlyList<EvidenceDefinition> Evidence => evidence;

    public bool TryGetStoryScene(string id, out StorySceneDefinition value)
    {
        EnsureIndex();
        return sceneIndex.TryGetValue(id ?? string.Empty, out value);
    }

    public bool TryGetLocation(string id, out LocationDefinition value)
    {
        EnsureIndex();
        return locationIndex.TryGetValue(id ?? string.Empty, out value);
    }

    private void EnsureIndex()
    {
        if (sceneIndex != null)
            return;
        sceneIndex = new Dictionary<string, StorySceneDefinition>(System.StringComparer.Ordinal);
        locationIndex = new Dictionary<string, LocationDefinition>(System.StringComparer.Ordinal);
        if (storyScenes != null)
            foreach (StorySceneDefinition item in storyScenes)
                if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                    sceneIndex[item.Id] = item;
        if (locations != null)
            foreach (LocationDefinition item in locations)
                if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                    locationIndex[item.Id] = item;
    }

    private void OnEnable()
    {
        sceneIndex = null;
        locationIndex = null;
    }
}
