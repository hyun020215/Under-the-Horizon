using UnityEngine;

[CreateAssetMenu(fileName = "PUZ_", menuName = "Under The Horizon/Puzzles/Definition")]
public sealed class PuzzleDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string controllerKey;

    [SerializeField]
    private GameEffect[] completionEffects;
    public string Id => id;
    public string ControllerKey => controllerKey;

    public void ApplyCompletion(GameStateStore state)
    {
        state?.CompletePuzzle(id);
        if (completionEffects != null)
            foreach (var effect in completionEffects)
                effect?.Apply(state);
    }
}
