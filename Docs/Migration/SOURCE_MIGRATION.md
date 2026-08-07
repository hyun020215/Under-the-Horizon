# codex-project-mystery migration

Source: `D:\codex-project-mystery`

The destination architecture in `AGENTS.md` and
`Docs/Architecture/ARCHITECTURE.md` takes precedence over the source layout.

## Activated content

- Source artwork and former `Resources` images were moved under `Art/`.
- BGM, sound effects, and voice barks were moved under `Audio/`.
- Dialogue, Story Scene index, and evidence CSV sources were moved under
  `Content/`.
- Source Unity package and project settings were used to restore zero-byte
  project configuration files. The development-only Unity MCP package was
  excluded.
- Shipping build scenes remain limited to `Bootstrap.unity` and `Game.unity`.

Asset filenames use the destination prefixes (`BG_`, `CHR_`, `EVD_`, `MUS_`,
`SFX_`, `PUZ_`, and related role-specific prefixes). Imported media retain
their source `.meta` GUIDs.

## Preserved reference implementation

The source runtime, editor code, and tests are preserved under
`Docs/Migration/LegacySource/`. They are intentionally outside `Assets/` and
therefore do not compile into the game.

The old runtime cannot be activated unchanged because it introduces competing
owners for flow, state, UI routing, transitions, and content loading. Examples
include `GameStateManager`, `UIManager`, and `ProductionSceneDirector`. Feature
ports must translate those behaviors into `GameStateStore`, `ScreenRouter`,
`StorySceneDirector`, shared Conditions/GameEffects, and content definitions.

## Deliberately not activated

- One-off or monolithic source runtime systems.
- Source Unity scenes that implement the old UI/game shell.
- Source ScriptableObjects and prefabs whose script GUIDs refer to the old
  runtime.
- Source `Resources` placement and string-based loading paths.

These exclusions prevent broken serialized references and a parallel runtime
architecture while retaining the original implementation for incremental
porting.
