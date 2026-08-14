using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class StorySceneDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private LocationPresenter locations;

    [SerializeField]
    private CharacterStage characters;

    [SerializeField]
    private InteractionDirector interactions;

    [SerializeField]
    private NarrativeDirector narrative;

    [SerializeField]
    private AudioDirector audioDirector;

    [SerializeField]
    private ScreenRouter screens;

    [SerializeField]
    private TransitionDirector transitions;

    [SerializeField]
    private SequenceDirector sequences;
    public StorySceneDefinition Current { get; private set; }
    public event Action<StorySceneDefinition> Entered;
    public event Action<StorySceneDefinition> Completed;

    public async Task EnterAsync(StorySceneDefinition scene)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        if (!ConditionResolver.All(scene.EntryConditions, state))
            throw new InvalidOperationException($"Entry conditions failed for '{scene.Id}'.");
        await transitions.BeginAsync(scene.EntryTransition);
        Current = scene;
        state.SetStoryContext(scene.Id, (int)scene.Day, scene.TimeBlock);
        await locations.ApplyAsync(scene.Location, scene.LocationState);
        await characters.ApplyAsync(scene.CharacterSet);
        interactions.Apply(scene.InteractionSet);
        audioDirector.Apply(scene.AudioProfile ?? scene.Location?.DefaultAudio);
        await screens.OpenAsync(scene.InitialScreen);
        Apply(scene.OnEnterEffects);
        await transitions.EndAsync(scene.EntryTransition);
        Entered?.Invoke(scene);
        if (scene.EntrySequence != null)
            await sequences.PlayAsync(scene.EntrySequence);
        if (scene.EntryDialogue != null && !scene.DeferEntryDialogue)
            await narrative.PlayAsync(scene.EntryDialogue);
    }

    public async Task RestorePresentationAsync(StorySceneDefinition scene)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        Current = scene;
        state.SetStoryContext(scene.Id, (int)scene.Day, scene.TimeBlock);
        await locations.ApplyAsync(scene.Location, scene.LocationState);
        await characters.ApplyAsync(scene.CharacterSet);
        interactions.Apply(scene.InteractionSet);
        audioDirector.Apply(scene.AudioProfile ?? scene.Location?.DefaultAudio);
        await screens.OpenAsync(
            ScreenId.Exploration,
            default,
            null,
            null);
    }

    public async Task<StorySceneResult> CompleteAsync()
    {
        if (Current == null)
            return new StorySceneResult(false);
        if (Current.ExitSequence != null)
            await sequences.PlayAsync(Current.ExitSequence);
        await transitions.BeginAsync(Current.ExitTransition);
        Apply(Current.OnCompleteEffects);
        state.CompleteScene(Current.Id);
        StorySceneRoute route = Current.ResolveRoute(state);
        Completed?.Invoke(Current);
        await transitions.EndAsync(Current.ExitTransition);
        return new StorySceneResult(
            true,
            route?.TargetSceneId,
            route?.AdvanceMode ?? StorySceneAdvanceMode.Immediate);
    }

    private void Apply(GameEffect[] effects)
    {
        if (effects != null)
            foreach (GameEffect effect in effects)
                effect?.Apply(state);
    }
}
