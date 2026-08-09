using System;
using UnityEngine;

[Serializable]
public sealed class StorySceneAuthoringRequirements
{
    [SerializeField, Min(0)]
    private int minimumInteractionCount;

    [SerializeField]
    private bool requiresPuzzle;

    [SerializeField]
    private bool requiresEntrySequence;

    [SerializeField]
    private bool requiresExitSequence;

    [SerializeField]
    private InteractionType[] requiredInteractionTypes;

    [SerializeField]
    private bool requiresEvidenceAcquisition;

    [SerializeField]
    private bool requiresSceneChoice;

    public int MinimumInteractionCount => minimumInteractionCount;
    public bool RequiresPuzzle => requiresPuzzle;
    public bool RequiresEntrySequence => requiresEntrySequence;
    public bool RequiresExitSequence => requiresExitSequence;
    public InteractionType[] RequiredInteractionTypes => requiredInteractionTypes;
    public bool RequiresEvidenceAcquisition => requiresEvidenceAcquisition;
    public bool RequiresSceneChoice => requiresSceneChoice;
}

[CreateAssetMenu(fileName = "StoryScene", menuName = "Under The Horizon/Story/Story Scene")]
public sealed class StorySceneDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string id;

    [SerializeField]
    private string displayName;

    [Header("Story")]
    [SerializeField]
    private StoryChapter chapter;

    [SerializeField]
    private StoryDay day;

    [SerializeField]
    private TimeBlock timeBlock;

    [Header("Entry")]
    [SerializeField]
    private Condition[] entryConditions;

    [Header("Location")]
    [SerializeField]
    private LocationDefinition location;

    [SerializeField]
    private LocationStateDefinition locationState;

    [Header("Presentation")]
    [SerializeField]
    private ScreenMode initialScreen;

    [SerializeField]
    private CharacterPlacementSet characterSet;

    [SerializeField]
    private InteractionSet interactionSet;

    [Header("Narrative")]
    [SerializeField]
    private DialogueSequence entryDialogue;

    [SerializeField]
    private bool deferEntryDialogue;

    [Header("Puzzle")]
    [SerializeField]
    private PuzzleDefinition puzzle;

    [Header("Audio")]
    [SerializeField]
    private AudioCueProfile audioProfile;

    [Header("Sequence")]
    [SerializeField]
    private SceneSequenceDefinition entrySequence;

    [SerializeField]
    private SceneSequenceDefinition exitSequence;

    [Header("Transition")]
    [SerializeField]
    private TransitionProfile entryTransition;

    [SerializeField]
    private TransitionProfile exitTransition;

    [Header("State")]
    [SerializeField]
    private GameEffect[] onEnterEffects;

    [SerializeField]
    private GameEffect[] onCompleteEffects;

    [Header("Flow")]
    [SerializeField]
    private StorySceneRoute[] routes;

    [Header("Authoring Validation")]
    [SerializeField]
    private StorySceneAuthoringRequirements authoringRequirements;

    public string Id => id;
    public string DisplayName => displayName;

    public StoryChapter Chapter => chapter;
    public StoryDay Day => day;
    public TimeBlock TimeBlock => timeBlock;

    public Condition[] EntryConditions => entryConditions;

    public LocationDefinition Location => location;
    public LocationStateDefinition LocationState => locationState;

    public ScreenMode InitialScreen => initialScreen;

    public CharacterPlacementSet CharacterSet => characterSet;
    public InteractionSet InteractionSet => interactionSet;

    public DialogueSequence EntryDialogue => entryDialogue;
    public bool DeferEntryDialogue => deferEntryDialogue;
    public PuzzleDefinition Puzzle => puzzle;

    public AudioCueProfile AudioProfile => audioProfile;

    public SceneSequenceDefinition EntrySequence => entrySequence;
    public SceneSequenceDefinition ExitSequence => exitSequence;

    public TransitionProfile EntryTransition => entryTransition;
    public TransitionProfile ExitTransition => exitTransition;

    public GameEffect[] OnEnterEffects => onEnterEffects;
    public GameEffect[] OnCompleteEffects => onCompleteEffects;

    public StorySceneRoute[] Routes => routes;
    public StorySceneAuthoringRequirements AuthoringRequirements => authoringRequirements;

    public string ResolveNext(GameStateStore state)
    {
        if (routes != null)
            foreach (StorySceneRoute route in routes)
                if (route != null && route.IsAvailable(state))
                    return route.TargetSceneId;
        return string.Empty;
    }
}
