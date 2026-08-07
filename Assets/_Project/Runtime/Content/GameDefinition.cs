using UnityEngine;

[CreateAssetMenu(fileName = "GAME_", menuName = "Under The Horizon/Content/Game Definition")]
public sealed class GameDefinition : ScriptableObject
{
    [SerializeField]
    private string id = "GAME_UNDER_THE_HORIZON";

    [SerializeField]
    private ContentDatabase content;

    [SerializeField]
    private string firstStorySceneId = "P-01";

    public string Id => id;
    public ContentDatabase Content => content;
    public string FirstStorySceneId => firstStorySceneId;
}
