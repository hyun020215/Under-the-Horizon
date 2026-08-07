using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class StorySceneDirector : MonoBehaviour
{
    [SerializeField] private GameStateStore state;
    [SerializeField] private LocationPresenter locations;
    [SerializeField] private CharacterStage characters;
    [SerializeField] private InteractionDirector interactions;
    [SerializeField] private NarrativeDirector narrative;
    [SerializeField] private AudioDirector audioDirector;
    [SerializeField] private ScreenRouter screens;
    [SerializeField] private TransitionDirector transitions;
    [SerializeField] private SequenceDirector sequences;
    public StorySceneDefinition Current { get; private set; }
    public event Action<StorySceneDefinition> Entered;
    public event Action<StorySceneDefinition> Completed;

    public async Task EnterAsync(StorySceneDefinition scene)
    {
        if (scene == null) throw new ArgumentNullException(nameof(scene));
        if (!ConditionResolver.All(scene.EntryConditions, state)) throw new InvalidOperationException($"Entry conditions failed for '{scene.Id}'.");
        await transitions.BeginAsync(scene.EntryTransition);
        Current = scene; state.SetCurrentScene(scene.Id); state.State.day = (int)scene.Day; state.State.timeBlock = scene.TimeBlock;
        await locations.ApplyAsync(scene.Location, scene.LocationState);
        await characters.ApplyAsync(scene.CharacterSet);
        interactions.Apply(scene.InteractionSet);
        audioDirector.Apply(scene.AudioProfile ?? scene.Location?.DefaultAudio);
        await screens.OpenAsync(scene.InitialScreen);
        Apply(scene.OnEnterEffects);
        await transitions.EndAsync(scene.EntryTransition);
        if (scene.EntrySequence != null) await sequences.PlayAsync(scene.EntrySequence);
        if (scene.EntryDialogue != null) await narrative.PlayAsync(scene.EntryDialogue);
    }

    public async Task<StorySceneResult> CompleteAsync()
    {
        if (Current == null) return new StorySceneResult(false);
        if (Current.ExitSequence != null) await sequences.PlayAsync(Current.ExitSequence);
        await transitions.BeginAsync(Current.ExitTransition);
        Apply(Current.OnCompleteEffects); state.CompleteScene(Current.Id);
        string next = Current.ResolveNext(state); Completed?.Invoke(Current);
        await transitions.EndAsync(Current.ExitTransition);
        return new StorySceneResult(true, next);
    }
    private void Apply(GameEffect[] effects) { if (effects != null) foreach (GameEffect effect in effects) effect?.Apply(state); }
}
