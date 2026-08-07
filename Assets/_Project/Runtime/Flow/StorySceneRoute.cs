using System;
using UnityEngine;
[Serializable] public sealed class StorySceneRoute { [SerializeField] private string targetSceneId; [SerializeField] private Condition[] conditions; public string TargetSceneId => targetSceneId; public bool IsAvailable(GameStateStore state) => !string.IsNullOrWhiteSpace(targetSceneId) && ConditionResolver.All(conditions, state); }
