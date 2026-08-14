public readonly struct ObjectiveGuidance
{
    public ObjectiveGuidance(string objective, string guidance, int completedSteps, int totalSteps)
    {
        Objective = objective ?? string.Empty;
        Guidance = guidance ?? string.Empty;
        CompletedSteps = completedSteps;
        TotalSteps = totalSteps;
    }

    public string Objective { get; }
    public string Guidance { get; }
    public int CompletedSteps { get; }
    public int TotalSteps { get; }

    public string HudText => TotalSteps > 0
        ? $"◆ {Objective}\n{UnityEngine.Mathf.Min(CompletedSteps + 1, TotalSteps)}/{TotalSteps} · {Guidance}"
        : $"◆ {Objective}\n{Guidance}";
}

public static class ObjectiveGuidanceResolver
{
    public static ObjectiveGuidance Resolve(PendingStorySceneTravel travel)
    {
        string destinationName = travel.Destination == null
            || string.IsNullOrWhiteSpace(travel.Destination.DisplayName)
                ? "목적지"
                : travel.Destination.DisplayName.Trim();
        return new ObjectiveGuidance(
            $"{destinationName}로 향하기",
            "지도에서 목적지를 선택해 이동하기",
            0,
            0);
    }

    public static ObjectiveGuidance Resolve(StorySceneDefinition scene, GameStateStore state)
    {
        string objective = string.IsNullOrWhiteSpace(scene?.DisplayName)
            ? "자유 조사"
            : scene.DisplayName.Trim();
        InteractionDefinition[] interactions = scene?.InteractionSet?.Interactions;
        if (interactions == null || interactions.Length == 0)
            return new ObjectiveGuidance(objective, "주변을 조사하세요", 0, 0);

        int total = 0;
        int completed = 0;
        InteractionDefinition next = null;
        foreach (InteractionDefinition interaction in interactions)
        {
            if (interaction == null || interaction.Repeatable)
                continue;
            total++;
            if (state != null && state.IsInteractionCompleted(interaction.Id))
            {
                completed++;
                continue;
            }
            if (next == null && interaction.IsAvailable(state))
                next = interaction;
        }

        if (next != null)
        {
            string guidance = string.IsNullOrWhiteSpace(next.DisplayName)
                ? "표시된 지점을 확인하세요"
                : next.DisplayName.Trim();
            return new ObjectiveGuidance(objective, guidance, completed, total);
        }

        string fallback = total > 0 && completed >= total
            ? "현재 목표를 완료했습니다"
            : "주변을 더 조사하세요";
        return new ObjectiveGuidance(objective, fallback, completed, total);
    }
}
