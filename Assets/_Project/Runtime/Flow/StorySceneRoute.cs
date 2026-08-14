using System;
using UnityEngine;

[Serializable]
public sealed class StorySceneRoute
{
    [SerializeField]
    private string targetSceneId;

    [SerializeField]
    private Condition[] conditions;

    [SerializeField]
    private StorySceneAdvanceMode advanceMode;

    public string TargetSceneId => targetSceneId;
    public StorySceneAdvanceMode AdvanceMode => advanceMode;

    public bool IsAvailable(GameStateStore state) =>
        !string.IsNullOrWhiteSpace(targetSceneId) && ConditionResolver.All(conditions, state);
}
