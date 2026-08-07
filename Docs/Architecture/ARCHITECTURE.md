# Under the Horizon — Canonical Project Architecture

> **Status: Architecture source of truth**  
> This document defines the intended completed-state architecture of **Under the Horizon**.  
> AI agents and contributors must also follow the repository-root `AGENTS.md`.

---

# 1. Architectural goals

The project is designed around five goals:

1. **Story content must be editable without writing new runtime code for every scene.**
2. **Repeated physical locations must be reusable across days, story states, and investigations.**
3. **UI, audio, transitions, character placement, interactions, and story progression must remain independently maintainable.**
4. **Save data must store logical game state, not fragile presentation state.**
5. **AI or future contributors must be able to add content without silently restructuring the project.**

The core model is therefore **data-driven composition over Story Scene-specific scripting**.

---

# 2. Canonical project tree

The canonical top-level project structure is:

```text
Assets/
└── _Project/
    ├── Runtime/
    │   ├── App/
    │   ├── Flow/
    │   ├── State/
    │   ├── Save/
    │   ├── Content/
    │   ├── Locations/
    │   ├── Characters/
    │   ├── Interaction/
    │   ├── Narrative/
    │   ├── Audio/
    │   ├── Transitions/
    │   ├── Sequences/
    │   ├── UI/
    │   ├── Evidence/
    │   ├── Puzzles/
    │   └── Common/
    │
    ├── Editor/
    │   ├── ContentTools/
    │   ├── Importers/
    │   ├── Preview/
    │   └── Validators/
    │
    ├── Content/
    │   ├── Game/
    │   ├── StoryScenes/
    │   ├── Locations/
    │   ├── Characters/
    │   ├── Dialogue/
    │   ├── Evidence/
    │   ├── Theories/
    │   ├── Puzzles/
    │   ├── Audio/
    │   ├── Transitions/
    │   ├── Sequences/
    │   └── UI/
    │
    ├── Art/
    │   ├── Branding/
    │   ├── Backgrounds/
    │   ├── Characters/
    │   ├── Evidence/
    │   ├── Investigation/
    │   ├── Props/
    │   ├── Maps/
    │   ├── Cinematics/
    │   └── UI/
    │
    ├── Audio/
    │   ├── Music/
    │   ├── Ambience/
    │   ├── SFX/
    │   ├── VoiceBarks/
    │   └── StoryRecordings/
    │
    ├── Prefabs/
    │   ├── App/
    │   ├── Locations/
    │   ├── Characters/
    │   ├── Interaction/
    │   ├── UI/
    │   ├── FX/
    │   └── Puzzles/
    │
    ├── Scenes/
    │   ├── Bootstrap.unity
    │   ├── Game.unity
    │   └── Dev/
    │
    ├── Settings/
    │   ├── Audio/
    │   ├── Input/
    │   ├── Rendering/
    │   └── Addressables/
    │
    └── Tests/
        ├── EditMode/
        └── PlayMode/
```

This structure is intentional. Folder boundaries represent ownership boundaries.

---

# 3. Runtime shell model

## 3.1 Bootstrap.unity

`Bootstrap.unity` is the application entry point.

Responsibilities:

- initialize services
- initialize content databases
- initialize save system
- initialize audio
- configure app lifetime
- load `Game.unity`

Not responsibilities:

- Story Scene content
- world backgrounds
- characters
- investigation hotspots
- story-specific cinematic staging

## 3.2 Game.unity

`Game.unity` is the single persistent gameplay shell.

Representative hierarchy:

```text
GameRoot
├── WorldCanvas
│   ├── BackgroundLayer
│   ├── MidgroundLayer
│   ├── CharacterLayer
│   ├── ForegroundLayer
│   ├── HotspotLayer
│   └── WorldFxLayer
│
├── UICanvas
│   ├── PersistentHUD
│   ├── ScreenHost
│   ├── ModalHost
│   ├── TooltipHost
│   └── TransitionOverlay
│
├── Directors
│   ├── GameFlowController
│   ├── StorySceneDirector
│   ├── LocationPresenter
│   ├── CharacterStage
│   ├── InteractionDirector
│   ├── NarrativeDirector
│   ├── AudioDirector
│   ├── ScreenRouter
│   ├── TransitionDirector
│   ├── SequenceDirector
│   └── PuzzleDirector
│
└── EventSystem
```

Content is loaded into this shell.

---

# 4. Story Scene model

The narrative contains Story Scenes such as:

```text
P-01
P-02
P-03
D1-01 ... D1-07
D2-01 ... D2-06
...
D8-01 ... D8-03
```

These are represented by `StorySceneDefinition` ScriptableObjects, not Unity Scenes.

## 4.1 StorySceneDefinition

Canonical purpose:

> Describe what systems and content must be active for a particular narrative/gameplay unit.

Typical fields:

```text
Identity
- id
- displayName

Story
- chapter
- day
- timeBlock

Entry
- entryConditions

World
- location
- locationState
- characterPlacementSet
- interactionSet

Presentation
- initialScreen
- entryTransition
- exitTransition
- entrySequence
- exitSequence

Narrative
- entryDialogue

Puzzle
- optional puzzle

Audio
- audioProfile

State
- onEnterEffects
- onCompleteEffects

Flow
- routes
```

## 4.2 StorySceneDirector

`StorySceneDirector` is an orchestrator, not a God Object.

Conceptual flow:

```text
StorySceneDefinition
        ↓
Transition begin
        ↓
GameState current scene
        ↓
LocationPresenter
        ↓
CharacterStage
        ↓
InteractionDirector
        ↓
AudioDirector
        ↓
ScreenRouter
        ↓
On-enter GameEffects
        ↓
Transition reveal
        ↓
optional SequenceDirector
```

It delegates implementation to specialized systems.

---

# 5. Example Story Scene composition: D1-06

Conceptually:

```text
D1_06_BodyDiscovery.asset
│
├── ID: D1-06
├── Display Name: 발견
├── Location: LOC_HORIZON
├── Location State: HORIZON_CrimeScene
├── Screen: Exploration
├── Character Set: SET_D1_06_CHARACTERS
│   ├── Richard
│   └── Helena
├── Interaction Set: INT_D1_06_HORIZON
│   ├── body
│   ├── open door
│   ├── blood
│   ├── overflowing sink
│   ├── recorder
│   └── relevant context interactions
├── Dialogue: DIA_D1_06
├── Audio: AUDIO_D1_06_DISCOVERY
├── Sequence: SEQ_D1_06_BodyReveal
├── Transition: TRANS_DISCOVERY
├── State Effects
│   └── evidence integrity branch / scene completion
└── Route
    └── D1-07
```

No `D1_06_BodyDiscoveryController.cs` is necessary for ordinary scene differences.

---

# 6. Location architecture

A Location represents physical space.

A Location State represents how that space appears/behaves at a particular time or story condition.

Example:

```text
LOC_HORIZON.asset
    ↓
HORIZON_NormalDay.asset
HORIZON_NormalNight.asset
HORIZON_CrimeScene.asset
HORIZON_Sealed.asset
HORIZON_Investigation.asset
HORIZON_FinalInterrogation.asset
```

`LocationPresenter` composes the selected location state into `Game.unity`.

This allows one physical location to be reused across many Story Scenes without duplicate Unity Scenes.

---

# 7. Character architecture

## Definition

`CharacterDefinition` stores stable character identity and reusable references.

## Placement

`CharacterPlacementSet` stores Story Scene/location-specific presentation:

- normalized position
- scale
- sorting order
- pose
- expression
- clickability

## Runtime

`CharacterStage` creates and updates `CharacterView` instances.

The character view does not own story progression.

Interaction is passed to `InteractionDirector` / narrative systems.

---

# 8. Interaction architecture

Interactions are content-driven.

A scene/location may expose:

- character interaction
- macguffin observation
- contextual monologue
- investigation point
- exit
- puzzle trigger

An `InteractionSet` is applied by `InteractionDirector`.

Availability uses common Conditions.

Outcomes use actions / GameEffects.

This prevents hotspot-specific scripts from becoming the story logic layer.

---

# 9. Narrative architecture

Narrative data is separated from Story Scene flow.

Core pieces:

```text
NarrativeDirector
DialogueDatabase
DialogueSequence
DialogueLine
DialogueChoice
DialogueHistory
BarkDirector
BarkPool
```

Dialogue may request effects, evidence presentation, expression changes, or routes through structured data/results.

Dialogue UI should not directly own global story state.

Voice barks are resolved through speaker/emotion/state data rather than hard-coded file paths in every dialogue line.

---

# 10. State architecture

`GameStateStore` owns mutable logical game state.

Representative state:

```text
currentStorySceneId
currentLocationId
day
timeBlock
publicAnxiety
evidenceIntegrity
trust
flags
discoveredEvidence
completedInteractions
completedPuzzles
theories
map unlocks
ending
```

State changes should pass through shared effects where practical.

Views reflect state; views are not state.

---

# 11. Condition and GameEffect architecture

The project uses reusable declarative Conditions and GameEffects to keep story rules auditable.

## Conditions

Examples:

```text
HasFlag
HasEvidence
TrustCondition
SceneCompleted
PuzzleCompleted
AnxietyCondition
IntegrityCondition
CompoundCondition
```

## GameEffects

Examples:

```text
SetFlag
ModifyTrust
ChangeAnxiety
ChangeIntegrity
AddEvidence
CompleteObjective
CompleteScene
UnlockLocation
```

This model should be extended rather than bypassed with arbitrary scene-specific code.

---

# 12. UI architecture

The UI has one routing authority.

```text
ScreenRouter
ModalRouter
```

Typical screen IDs:

```text
Title
SaveSlot
Exploration
Dialogue
Map
Investigation
InvestigationRecord
Interrogation
EvidenceBoard
Reconstruction
Puzzle
Ending
Credits
Settings
```

Persistent HUD remains separate from mode-specific screens.

Screens communicate results to gameplay systems; they do not arbitrarily toggle each other as unrelated GameObjects.

---

# 13. Transition architecture

All visual screen/location/story transitions are played by `TransitionDirector` using `TransitionProfile` data.

Profiles may define:

```text
UI exit duration/ease
cover duration
hold duration
reveal duration
UI enter duration/ease
input blocking
stinger
```

Story logic requests transitions and remains unaware of specific tween implementation.

---

# 14. Sequence architecture

Short in-engine cinematics use `SceneSequenceDefinition` + `SequenceDirector`.

Reusable commands:

```text
Wait
Camera
Audio
Dialogue
Character
Location
UI
Transition
State
InputLock
```

This is used for events such as body reveal, stair fall, smoke reveal, alarms, confrontations, and epilogues.

---

# 15. Audio architecture

The intended audio architecture supports layered audio:

```text
Music A
Music B
Ambience A
Ambience B
SFX
Voice Bark
Story Voice / Recordings
```

`AudioDirector` owns routing and state.

Content controls audio through `AudioCueProfile`.

Conceptual resolution:

```text
event / sequence override
    > Story Scene override
    > Location default
    > appropriate current state
```

Dialogue/interrogation ducking is centralized.

Story scripts never directly control shared AudioSources.

---

# 16. Puzzle architecture

The project has a common puzzle shell plus specialized rule implementations.

Shared:

```text
PuzzleDirector
PuzzleDefinition
PuzzleContext
PuzzleResult
PuzzleHintSystem
PuzzleStateSerializer
PuzzleScreen
```

Specialized rule examples:

```text
BloodPattern
CCTV logs
Vault authentication
Stair reconstruction
Claire contradiction
Stabilizer log
Cargo rail
Luminol
Cause of death
Timeline
DNA
Audio restoration
Final accusation
```

Puzzle completion returns a result. Story progression remains controlled by shared flow/effects.

---

# 17. Evidence / theory architecture

Evidence assets represent stable investigative facts.

Core canonical evidence uses `C-01` through `C-18` internally.

Player-facing UI should not expose internal IDs or total clue counts unless product design changes.

Theory resolution is data-driven from evidence relationships and reusable theory definitions.

---

# 18. Save architecture

The save file stores logical state necessary to reconstruct the game.

Presentation is reconstructed from content definitions after load.

This protects save files from:

- UI hierarchy changes
- tween changes
- prefab changes
- audio implementation changes
- scene presentation refactors

Every serialized save schema change must consider migration.

---

# 19. Asset/content separation

The relationship between code, data, source media, and prefab views should remain explicit.

Example character:

```text
Runtime/Characters/CharacterDefinition.cs
        ↓ schema
Content/Characters/Definitions/CHR_EVELYN.asset
        ↓ game meaning
Art/Characters/Evelyn/...png
        ↓ source visual
Prefabs/Characters/PF_CharacterView.prefab
        ↓ runtime view
```

Example Story Scene:

```text
Runtime/Flow/StorySceneDefinition.cs
        ↓ schema
Content/StoryScenes/Day01/D1_06_BodyDiscovery.asset
        ↓ composed references
shared runtime systems
```

Do not collapse these layers merely to make local implementation shorter.

---

# 20. Addressables / Resources strategy

The target architecture is Addressables-compatible for large media.

Good Addressables candidates:

- backgrounds
- full-body character art
- large investigation images
- cinematic images
- BGM
- ambience
- large story recordings

Legacy Resources-based paths may remain during migration, but new systems should not expand broad Resources coupling.

---

# 21. Editor architecture

The content-heavy nature of the game requires first-class authoring tools.

Intended tools:

```text
StorySceneEditorWindow
StorySceneGraphWindow
LocationEditorWindow
CharacterPlacementEditorWindow
InteractionEditorWindow
AudioCueEditorWindow
EvidenceEditorWindow
SequenceEditorWindow
```

Preview tools should allow jumping directly to a Story Scene/location/puzzle without playing the full game from the beginning.

Importers should support dialogue/audio/evidence source data where appropriate.

---

# 22. Validation architecture

Automated content validation is a project feature, not optional cleanup.

Build/preflight should validate:

- IDs are unique
- Story Scene routes resolve
- required references exist
- Locations / Location States resolve
- characters resolve
- dialogue resolves
- evidence resolves
- puzzle definitions resolve
- audio profiles resolve
- transition profiles resolve
- Addressable references are valid
- required progression is not broken

Validation rules must be maintained when schemas evolve.

---

# 23. Runtime dependency philosophy

Runtime classes should have focused ownership.

Examples:

```text
StorySceneDirector = orchestration
LocationPresenter = physical location presentation
CharacterStage = character presentation
InteractionDirector = click/interaction orchestration
NarrativeDirector = dialogue/narrative progression
AudioDirector = audio state/routing
ScreenRouter = screen navigation
TransitionDirector = transition presentation
SequenceDirector = ordered event playback
PuzzleDirector = puzzle lifecycle
GameStateStore = logical mutable state
SaveService = persistence
```

Do not centralize all responsibilities into `GameManager`.

Do not let low-level views own high-level flow.

---

# 24. Stable content naming

Recommended prefixes:

```text
GAME_       game definition
DATABASE_   registries/databases
LOC_        location
CHR_        character
INT_        interaction set
DIA_        dialogue
C01_...     evidence
THEORY_     theory
PUZ_        puzzle
AUDIO_      audio cue profile
TRANS_      transition
SEQ_        sequence
BG_         background art
EVD_        evidence art
MUS_        music
AMB_        ambience
SFX_        sound effect
PF_         prefab
```

Story Scene assets preserve canonical narrative identifiers in their filenames.

---

# 25. What should happen when adding content

## Add a normal new Story Scene

Usually create/edit:

```text
StorySceneDefinition asset
Location/LocationState reference
CharacterPlacementSet
InteractionSet
DialogueSequence
AudioCueProfile if needed
Sequence/Transition if needed
Conditions/GameEffects/routes
```

Usually **do not add a new C# Story Scene controller**.

## Add a new visual state of an existing location

Create a `LocationStateDefinition` and source art as needed.

Do not duplicate the Location.

## Add a new character placement

Edit/create a `CharacterPlacementSet`.

Do not add hard-coded coordinates to runtime systems.

## Add a new transition flavor

Add a reusable `TransitionProfile`, unless a genuinely new reusable transition algorithm is needed.

## Add a new puzzle type

Add a specialized puzzle controller only if the rules are genuinely new, then expose it through `PuzzleDefinition` and the shared `PuzzleDirector` lifecycle.

---

# 26. Architectural anti-patterns

The following patterns are considered architectural regressions unless explicitly approved:

```text
one Unity Scene per Story Scene
one controller per Story Scene
growing switch(sceneId) blocks
all-purpose GameManager
direct UI panel cross-toggle spaghetti
direct Story Scene → AudioSource manipulation
hard-coded character positions
hard-coded asset resource paths spread across views
duplicate location definitions by day
UI GameObject state used as save/game state
parallel new flow/audio/UI/state framework
large-scale renaming without migration need
```

---

# 27. Architecture change process

If a limitation appears, prefer extending the existing model first.

A true architectural proposal must document:

```text
Problem
Current limitation
Why existing extension points are insufficient
Proposed change
Affected systems
Affected assets
Save/serialization migration
Content migration
Testing impact
Validator impact
Rollout plan
Rollback plan
```

Do not merge architecture changes without explicit maintainer approval.

After approval, this document and root `AGENTS.md` must be updated in the same change.

---

# 28. Final architecture principle

Under the Horizon is not organized around individual Story Scene scripts.

It is organized around reusable systems composed by content data:

```text
StorySceneDefinition
        ↓
StorySceneDirector
   ├── LocationPresenter
   ├── CharacterStage
   ├── InteractionDirector
   ├── NarrativeDirector
   ├── AudioDirector
   ├── ScreenRouter
   ├── TransitionDirector
   ├── SequenceDirector
   └── PuzzleDirector
        ↓
GameStateStore
        ↓
SaveService
```

The architecture is successful when a designer or AI agent can change most story presentation and flow by editing content assets, while reusable runtime code remains stable.
