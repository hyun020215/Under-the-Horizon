# Under the Horizon — AI / Agent Architecture Guardrails

> **Status: REQUIRED PROJECT POLICY**  
> This file exists primarily to prevent AI coding agents, automated refactors, and future contributors from silently changing the architecture of **Under the Horizon**.
>
> Before editing runtime architecture, folder structure, content schemas, scene loading, save data, UI routing, audio routing, or story flow, read this file and `Docs/Architecture/ARCHITECTURE.md` completely.

## 0. Authority and change policy

This document and `Docs/Architecture/ARCHITECTURE.md` are the architecture source of truth.

### MUST

- Preserve the architecture described here unless the user/maintainer explicitly approves an architectural change.
- Prefer the smallest implementation that fits the existing architecture.
- Extend an existing system before introducing a parallel replacement system.
- Treat existing content IDs, scene IDs, evidence IDs, character IDs, save fields, and ScriptableObject references as stable contracts.
- When a task seems to require an architectural change, stop and propose the change instead of silently implementing it.
- Explain migration impact before changing serialized data, save data, Addressable references, ScriptableObject schemas, or scene composition.

### MUST NOT

- Do not redesign the project because another pattern appears cleaner, more modern, or more familiar.
- Do not replace the data-driven architecture with scene-specific or monolithic code.
- Do not create a second competing flow, UI, audio, state, transition, save, or content-loading framework.
- Do not move large groups of files solely for stylistic consistency.
- Do not introduce a new top-level directory under `Assets/_Project/` without explicit approval.
- Do not delete or rename serialized fields, content IDs, or assets without a migration plan.
- Do not create one Unity Scene or one bespoke controller per Story Scene.

If a user request conflicts with this document, **the explicit user request wins**. Otherwise, preserve this architecture.

---

# 1. Core architectural intent

Under the Horizon is a **data-driven narrative investigation game**.

The fundamental separation is:

```text
Runtime/   = how systems behave
Content/   = what exists in the game
Art/       = how visual source assets look
Audio/     = how source audio sounds
Prefabs/   = how runtime GameObjects are assembled
Scenes/    = application shells and development playgrounds
Editor/    = authoring, import, preview, validation tools
Tests/     = automated verification
```

This separation is non-negotiable.

A content change should normally modify `Content/`, `Art/`, `Audio/`, or `Prefabs/` — **not add scene-specific runtime code**.

---

# 2. Terminology is strict

Do not use these terms interchangeably.

| Term | Meaning |
|---|---|
| **Unity Scene** | A `.unity` shell such as `Bootstrap.unity` or `Game.unity` |
| **Story Scene** | A narrative/gameplay unit such as `P-01`, `D1-06`, `D6-05` |
| **Location** | A physical place such as Horizon Room, Medbay, Ballroom |
| **Location State** | A visual/gameplay condition of a Location, e.g. normal/crime/investigation |
| **Screen** | A UI mode such as Exploration, Dialogue, Map, Investigation, Evidence Board |
| **Sequence** | Ordered presentation commands for a short cinematic/event |
| **Transition** | Presentation used when moving between screens/locations/story states |
| **Puzzle** | A distinct interactive rule set with reusable shell + specialized logic |

Do not implement Story Scenes as Unity Scenes.

---

# 3. Unity Scene policy

The shipping runtime should remain centered on:

```text
Assets/_Project/Scenes/
├── Bootstrap.unity
├── Game.unity
└── Dev/
    ├── ContentPreview.unity
    ├── UIPlayground.unity
    ├── PuzzlePlayground.unity
    ├── AudioPlayground.unity
    └── TransitionPlayground.unity
```

### Bootstrap.unity

Owns application initialization only:

- App bootstrap/lifetime
- service registration
- content database initialization
- save initialization
- audio initialization
- loading overlay
- loading `Game.unity`

It must not contain game-specific backgrounds, characters, evidence, or Story Scene logic.

### Game.unity

Owns the persistent runtime shell:

```text
GameRoot
├── WorldCanvas
├── UICanvas
├── Directors
└── EventSystem
```

Story content is injected into this shell through data.

### Forbidden

Do not add:

```text
D1_06.unity
D2_01.unity
D6_05.unity
...
```

unless an explicit architectural decision overrides this policy.

---

# 4. Story Scenes are data, not bespoke controllers

The 41 narrative Story Scenes from `P-01` through `D8-03` are represented by `StorySceneDefinition` assets.

Canonical relationship:

```text
Runtime/Flow/StorySceneDefinition.cs
            ↓ defines schema
Content/StoryScenes/.../*.asset
            ↓ instances
Runtime/Flow/StorySceneDirector.cs
            ↓ orchestrates shared systems
Location / Characters / Interaction / Narrative / Audio / UI / Sequence / Transition
```

### StorySceneDefinition is a link hub

A Story Scene asset may reference:

- ID / display name
- chapter/day/time block
- entry conditions
- Location
- Location State
- initial Screen
- CharacterPlacementSet
- InteractionSet
- DialogueSequence
- optional PuzzleDefinition
- AudioCueProfile
- entry/exit Sequence
- entry/exit Transition
- GameEffects
- routes to later Story Scenes

### Forbidden pattern

Do not create files such as:

```text
D1_06_BodyDiscoveryController.cs
D2_05_CeilingPanelController.cs
D7_04_EvelynOfferController.cs
```

for normal Story Scene differences.

Scene-specific differences belong in `.asset` data.

### Exception

A unique puzzle may have specialized code because the **interaction rules** are genuinely different:

```text
BloodPatternPuzzleController.cs
CargoRailPuzzleController.cs
TimelinePuzzleController.cs
```

Even then, the Story Scene should reference a `PuzzleDefinition`; it must not hard-code puzzle logic into the Story flow.

---

# 5. No Story Scene ID branching in shared runtime systems

Shared runtime systems must not contain growing chains such as:

```csharp
if (sceneId == "D1-06") { ... }
else if (sceneId == "D2-05") { ... }
else if (sceneId == "D7-01") { ... }
```

or:

```csharp
switch (sceneId)
{
    case "D1-06": ...
    case "D2-05": ...
}
```

Story-specific differences must be expressed as:

- definitions
- profiles
- conditions
- effects
- sequences
- routes
- content references

If a shared system needs a new behavior, add a reusable behavior primitive rather than a Story Scene ID special case.

---

# 6. Location policy

Locations are reusable physical spaces.

A Location must not be duplicated merely because it appears on another day.

Use:

```text
LocationDefinition
    +
LocationStateDefinition
```

Example:

```text
LOC_HORIZON.asset
├── HORIZON_NormalDay.asset
├── HORIZON_NormalNight.asset
├── HORIZON_CrimeScene.asset
├── HORIZON_Sealed.asset
├── HORIZON_Investigation.asset
└── HORIZON_FinalInterrogation.asset
```

Do not create separate `Horizon_Day1`, `Horizon_Day2`, `Horizon_Final` Location systems if they are the same physical place.

---

# 7. Character placement is content data

Character positions, pose, expression, scale, sorting order, and clickability are authored through `CharacterPlacementSet`.

### MUST

- Use normalized coordinates relative to the location/background where practical.
- Keep placement data out of Story Scene-specific MonoBehaviours.
- Let `CharacterStage` instantiate/apply placement.

### MUST NOT

Do not write:

```csharp
if (sceneId == "D1-01")
    claire.transform.position = ...;
```

Do not put important story placement only in a Unity Scene hierarchy.

---

# 8. Interaction / hotspot policy

Clickable characters, macguffins, context interactions, investigation points, exits, and puzzle triggers are represented through reusable `InteractionDefinition` / `InteractionSet` data.

Canonical interaction types include:

```text
Character
MacGuffin
Context
Investigation
Exit
Puzzle
```

Availability must be controlled by the common Condition system.

Consequences must be applied through reusable InteractionActions / GameEffects.

Do not write one-off hotspot scripts for ordinary story content unless a truly new interaction rule is required.

---

# 9. Conditions and effects are shared infrastructure

All systems should use the common condition/effect model.

Examples of Conditions:

- HasFlag
- HasEvidence
- Trust threshold
- Story Scene completed
- Puzzle completed
- Anxiety threshold
- Evidence Integrity threshold
- compound ALL / ANY / NOT

Examples of GameEffects:

- SetFlag
- ModifyTrust
- ChangeAnxiety
- ChangeIntegrity
- AddEvidence
- CompleteObjective
- CompleteScene
- UnlockLocation

### Forbidden

Do not mutate central game state directly from arbitrary views or dialogue UI:

```csharp
state.trust["Richard"]++;
```

when the change can be represented by a `GameEffect`.

Centralized effects are required for auditability, save correctness, debugging, and QA.

---

# 10. State ownership

`GameStateStore` is the authoritative mutable gameplay state.

Typical logical state includes:

- current Story Scene
- current Location
- day / time block
- Trust values
- Public Anxiety
- Evidence Integrity
- flags
- discovered evidence
- completed interactions
- completed puzzles
- theory state
- ending state

Views should render state; they should not become alternate sources of truth.

Do not use UI active states, GameObject existence, audio playback position, or scene hierarchy state as authoritative gameplay state.

---

# 11. Save system policy

Save **logical state**, not transient presentation state.

### Save

- Story progress
- current Location / Story Scene
- flags
- Trust / Anxiety / Integrity
- evidence
- interaction completion
- puzzle progress that must persist
- choices
- map/location unlock state
- ending state

### Do not save as authoritative state

- current `AudioSource.time`
- current tween progress
- current UI transform positions
- spawned CharacterView instance references
- temporary modal state
- temporary transition state

On load, presentation should be reconstructed from logical state + content definitions.

Serialized save schema changes require a migration strategy.

---

# 12. UI routing policy

UI screens are controlled by `ScreenRouter` / `ModalRouter`.

Typical Screens:

- Title
- Save Slot
- Exploration
- Dialogue
- Map
- Investigation
- Investigation Record
- Interrogation
- Evidence Board
- Reconstruction
- Puzzle
- Ending
- Credits
- Settings

### Forbidden

Cross-screen code must not manually toggle unrelated panels:

```csharp
mapPanel.SetActive(false);
dialoguePanel.SetActive(true);
hud.SetActive(false);
```

Use the router:

```csharp
await screenRouter.OpenAsync(ScreenId.Dialogue, context);
```

Persistent HUD and mode-specific screen UI must remain conceptually separate.

Internal numeric systems such as Trust, Public Anxiety, and Evidence Integrity should remain internal unless a UI design specifically exposes a qualitative representation.

---

# 13. Transition policy

Story logic does not implement visual transitions directly.

`TransitionDirector` owns transition playback.

A transition profile may define:

- UI exit animation
- cover/fade
- hold
- reveal
- UI enter animation
- input blocking
- stinger
- timing/easing

`StorySceneDirector` may request a transition; it must not contain tween-specific presentation code.

Do not duplicate transition timings across Story Scene scripts.

---

# 14. Sequence policy

Short cinematics/events are authored as `SceneSequenceDefinition` data and executed by `SequenceDirector`.

Reusable commands include:

- Wait
- Camera
- Audio
- Dialogue
- Character
- Location
- UI
- Transition
- State
- Input Lock

Do not bury ordered cinematic timing inside a Story Scene-specific coroutine when it can be represented as Sequence data.

---

# 15. Audio policy

Audio is owned by `AudioDirector` and specialized controllers.

Expected routing:

```text
Music A
Music B
Ambience A
Ambience B
SFX
Voice Bark
Story Voice / Recording
```

Scene/location audio differences belong in `AudioCueProfile` data.

Resolution priority should remain conceptually:

```text
Event / Sequence override
    > Story Scene override
    > Location default
    > keep current appropriate state
```

### MUST NOT

- Do not directly manipulate global AudioSources from Story Scene scripts.
- Do not encode Story Scene IDs inside `AudioDirector`.
- Do not create a second audio manager for a single feature.

Dialogue ducking belongs to the audio system, not individual dialogue screens.

---

# 16. Puzzle policy

Puzzles use:

```text
PuzzleDirector
+ common Puzzle UI shell
+ PuzzleDefinition data
+ specialized controller only for unique rule sets
```

Common concerns stay shared:

- open/close
- input blocking
- hints
- result reporting
- state persistence
- audio hooks
- transition hooks

Unique controllers should implement puzzle rules only.

A puzzle controller must not independently advance unrelated story progression; it returns a `PuzzleResult`, and Story/Game effects handle progression.

---

# 17. Evidence and theory policy

Evidence is defined as stable content assets with stable internal IDs.

Canonical core evidence IDs are `C-01` through `C-18`.

Do not expose internal clue IDs, total clue counts, or completion percentages to the player unless the product design is explicitly changed.

The Evidence Board operates on evidence/theory data, not Story Scene-specific hard-coded connections.

---

# 18. Asset role boundaries

Keep these meanings distinct.

## Art/

Raw visual source assets.

Example:

```text
BG_HORIZON_NIGHT_CRIME.png
CHR_EVELYN_FULL_NEUTRAL.png
EVD_C16_VisorDNA.png
```

## Audio/

Raw audio source assets.

Example:

```text
MUS_Horizon.mp3
AMB_Wind.mp3
SFX_EvidenceFound.wav
```

## Prefabs/

Runtime GameObject assembly / view structure.

Example:

```text
PF_CharacterView.prefab
PF_DialogueScreen.prefab
PF_Hotspot.prefab
```

## Content/

Game meaning and references.

Example:

```text
CHR_EVELYN.asset
LOC_HORIZON.asset
D1_06_BodyDiscovery.asset
AUDIO_D1_06_DISCOVERY.asset
```

Do not mix these responsibilities simply to reduce file count.

---

# 19. Content loading policy

Large media should be compatible with Addressables-based loading.

Prefer Addressables/content references for:

- backgrounds
- character sprites
- cinematics
- BGM
- ambience
- large evidence images

Do not expand unrestricted `Resources.Load` usage as a shortcut.

If legacy Resources usage remains during migration, keep it isolated and do not spread new dependencies on it.

---

# 20. Runtime dependency direction

Prefer the dependency direction:

```text
Content / Definitions
        ↓
       Core
        ↓
     Gameplay
   ↙    ↓     ↘
Audio   UI   Puzzles
```

Views should not own global game flow.

Examples of forbidden dependencies:

- `CharacterView` directly advancing Story Scenes
- `EvidenceScreen` writing save files directly
- `AudioDirector` depending on `DialogueScreen`
- puzzle controller branching on Story Scene IDs
- UI components directly creating/deleting central state

Use services, contexts, events, results, effects, or directors according to existing patterns.

---

# 21. Events are notifications, not a replacement for command flow

Use direct service calls for intentional commands.

Use the event bus when multiple independent systems need notification.

Good example:

```text
DialogueStartedEvent
        ↓
AudioDuckingController
Telemetry / UI listeners
```

Do not convert the entire application into an opaque event chain.

---

# 22. Editor tooling is part of the architecture

Content-heavy work should be authorable without custom code per Story Scene.

Maintain and extend tools for:

- Story Scene editing
- Story graph visualization
- Location preview
- Character placement
- Interaction editing
- Audio cue editing
- Sequence preview/editing
- CSV import
- content validation

If repeated manual Inspector work becomes error-prone, prefer improving an Editor tool over adding runtime hacks.

---

# 23. Validation is mandatory

Before merge/build, validators should be able to detect at minimum:

- duplicate IDs
- missing Story Scene references
- broken Story Scene routes
- missing Locations / Location States
- invalid Character references
- missing Dialogue IDs/assets
- missing Evidence references
- invalid Puzzle references
- missing Audio profiles/assets
- missing Transition profiles
- missing Addressable entries/labels where required
- broken serialized references
- impossible required progression paths

Do not bypass validation by weakening validators to make a change pass.

Fix the content or obtain explicit approval to change the rule.

---

# 24. Naming conventions

Prefer stable prefixes for content assets:

```text
GAME_
DATABASE_
LOC_
CHR_
INT_
DIA_
C01_ / C02_ ...
THEORY_
PUZ_
AUDIO_
TRANS_
SEQ_
BG_
EVD_
MUS_
AMB_
SFX_
PF_
```

Story Scene files should preserve their canonical IDs in filenames:

```text
P01_PortJournalist.asset
D1_06_BodyDiscovery.asset
D8_03_ReturnToPort.asset
```

Do not casually rename canonical IDs for aesthetics.

---

# 25. Folder structure guardrail

Canonical top-level structure under `Assets/_Project/`:

```text
Runtime/
Editor/
Content/
Art/
Audio/
Prefabs/
Scenes/
Settings/
Tests/
```

Do not introduce parallel alternatives such as:

```text
Scripts2/
GameSystems/
ManagersNew/
NewArchitecture/
SceneControllers/
RuntimeNew/
```

without explicit approval.

If code appears misplaced, first determine whether moving it would break serialized GUID/reference stability or undermine an ongoing migration.

---

# 26. When implementing a feature, choose the correct layer

Ask these questions in order:

### Is this a new reusable behavior?

Put code in `Runtime/`.

### Is this a new instance/configuration of existing behavior?

Put data in `Content/`.

### Is this a visual source asset?

Put it in `Art/`.

### Is this an audio source asset?

Put it in `Audio/`.

### Is this a reusable GameObject/view hierarchy?

Put it in `Prefabs/`.

### Is this authoring/validation tooling?

Put it in `Editor/`.

### Is this only useful to automated verification?

Put it in `Tests/`.

Do not add runtime code when a content asset is sufficient.

---

# 27. AI-specific anti-refactor rules

AI agents must not perform the following unless explicitly asked:

1. Convert ScriptableObject content into JSON/YAML/custom databases.
2. Convert the project from data-driven composition to one-scene-per-level.
3. Merge directors/managers into a single `GameManager`.
4. Split a stable shared system into multiple competing managers.
5. Introduce ECS/DOTS, dependency-injection frameworks, reactive frameworks, or a new event framework merely as cleanup.
6. Replace UI routing with direct panel toggling.
7. Replace Addressables-compatible references with widespread Resources paths.
8. Replace shared Conditions/GameEffects with arbitrary callbacks/lambdas embedded in content.
9. Hard-code character coordinates, audio paths, evidence IDs, or Story Scene routing in view classes.
10. Rename large asset trees without a concrete product requirement.
11. Rewrite working systems only because generated code would be shorter.
12. remove abstraction layers because they appear unused in one local task.
13. bypass content profiles by placing values directly into Story Scene scripts.
14. invent new canonical story facts, IDs, or scene structure while performing code work.

Local simplicity must not destroy global authorability.

---

# 28. Incremental migration and commit workflow

UI/UX ports, legacy migrations, and other multi-step feature work must proceed as a sequence of independently verifiable functional increments.

### Before each increment

- Inventory the existing Runtime, Content, Prefab, Editor, and Test implementation related to the feature.
- Search for an existing owner, director, router, profile, definition, or view before adding a new type or asset.
- Classify the change into the correct architecture layer and identify its authoritative state owner.
- If an implementation is partial, extend it instead of creating a parallel replacement.
- Record the increment as completed, partial, or remaining in the existing production TODO document.

### During each increment

- Keep the change centered on one coherent behavior or user-facing capability.
- Reuse existing systems and serialized assets wherever they satisfy the requirement.
- Route screen, transition, audio, state, and content work through their established owners.
- Do not duplicate a legacy implementation merely to reproduce its appearance; port the behavior through current architecture boundaries.
- Keep unrelated worktree changes out of the increment.

### Before starting the next increment

- Audit the diff for parallel systems, duplicated responsibilities, hard-coded content differences, and dependency-direction violations.
- Run the narrowest relevant tests plus any required content or architecture validation.
- Update `Docs/Production/TODO.md` in the same commit with what was completed, what remains partial, and the next migration target.
- Commit the verified increment immediately with a message describing that single capability.
- Do not accumulate several unrelated UI/UX features into a bulk migration commit.

If an increment cannot be committed independently, reduce its scope or explain the unavoidable coupling before proceeding.

---

# 29. Architecture change protocol

An architectural change is any change that modifies one or more of:

- top-level project structure
- Unity Scene model
- Story Scene representation
- state ownership
- save schema strategy
- UI routing model
- audio routing model
- transition ownership
- content loading model
- condition/effect model
- core dependency direction

Before making such a change:

1. Describe the current architecture.
2. Describe the exact limitation causing the proposed change.
3. Show why the limitation cannot be solved cleanly inside the current model.
4. Describe the proposed architecture.
5. List affected files/assets.
6. Explain serialization/save/content migration.
7. Explain backward compatibility.
8. Explain QA/validator changes.
9. Wait for explicit approval.
10. Update `Docs/Architecture/ARCHITECTURE.md` and this file if the change is approved.

Do not treat silence as approval.

---

# 30. Pre-commit architecture checklist for AI agents

Before finishing a task, verify:

- [ ] I did not create a Unity Scene for a Story Scene.
- [ ] I did not add a bespoke Story Scene controller where data would work.
- [ ] I did not hard-code Story Scene IDs into shared systems.
- [ ] I used existing Condition/GameEffect infrastructure for state rules where applicable.
- [ ] I did not make UI views authoritative game state.
- [ ] I routed screen changes through ScreenRouter/ModalRouter.
- [ ] I routed transitions through TransitionDirector.
- [ ] I routed audio through AudioDirector/profiles.
- [ ] I kept character placement and hotspots in content data where applicable.
- [ ] I did not spread new `Resources.Load` dependencies.
- [ ] I did not create a parallel framework.
- [ ] I inspected and reused the existing owner/system before adding new implementation.
- [ ] I did not duplicate an already migrated or partially migrated feature.
- [ ] I preserved stable IDs and serialized references.
- [ ] I considered save migration when serialized state changed.
- [ ] I added or updated validation/tests for new reusable behavior.
- [ ] I updated `Docs/Production/TODO.md` for incremental migration work.
- [ ] This commit contains one coherent feature and excludes unrelated worktree changes.
- [ ] I updated architecture documentation if an explicitly approved architectural change was made.

If any checked statement is false, fix the implementation or explicitly report the architectural conflict.

---

# 31. Final rule

When uncertain between:

> "change the architecture to fit this task"

and

> "fit this task into the established architecture"

choose the second option unless the maintainer explicitly approves the first.
