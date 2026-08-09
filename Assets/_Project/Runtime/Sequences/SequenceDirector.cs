using System.Threading.Tasks;
using UnityEngine;

public sealed class SequenceDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private NarrativeDirector narrative;

    [SerializeField]
    private AudioDirector audioDirector;

    [SerializeField]
    private TransitionDirector transitions;

    [SerializeField]
    private ScreenRouter screens;

    [SerializeField]
    private UIInputBlocker inputBlocker;

    public async Task PlayAsync(SceneSequenceDefinition definition)
    {
        if (definition?.Commands == null)
            return;
        var context = new SequenceContext(
            state,
            narrative,
            audioDirector,
            transitions,
            screens,
            inputBlocker);
        foreach (var command in definition.Commands)
            if (command != null)
                await command.ExecuteAsync(context);
    }
}
