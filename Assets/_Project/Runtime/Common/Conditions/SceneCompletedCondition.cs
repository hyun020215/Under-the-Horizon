using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Scene Completed")]
public sealed class SceneCompletedCondition : Condition
{
    [SerializeField] private string storySceneId;
    public override bool Evaluate(GameStateStore state) => state != null && state.IsSceneCompleted(storySceneId);
}
