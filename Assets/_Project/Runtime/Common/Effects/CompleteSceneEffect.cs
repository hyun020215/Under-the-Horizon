using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Complete Scene")]
public sealed class CompleteSceneEffect : GameEffect
{
    [SerializeField] private string sceneId;
    public override void Apply(GameStateStore state) => state?.CompleteScene(sceneId);
}
