# Unity 콘텐츠 제작 및 수정 가이드

> 대상 Unity 버전: `6000.3.20f1`
>
> 프로젝트 루트: `Assets/_Project/`
>
> 이 문서는 아트·오디오 교체, 캐릭터와 핫스팟 배치, 기존 Story Scene 수정, 새 Story Scene 추가를 현재 데이터 기반 아키텍처 안에서 수행하는 실무 절차다.

작업 전 `AGENTS.md`와 `Docs/Architecture/ARCHITECTURE.md`를 먼저 읽는다. 이 문서와 충돌하면 두 아키텍처 문서가 우선한다.

## 1. 가장 중요한 구조

이 프로젝트에서 Unity Scene과 Story Scene은 다르다.

- Unity Scene: `Bootstrap.unity`, `Game.unity` 같은 실행 셸
- Story Scene: `P-01`, `D2-04`, `D8-03` 같은 이야기 단위의 ScriptableObject

Story Scene을 추가할 때 `.unity` 파일이나 장면 전용 MonoBehaviour를 만들지 않는다. 다음 연결을 만든다.

```text
StorySceneDefinition
├── LocationDefinition + LocationStateDefinition
├── CharacterPlacementSet
├── InteractionSet
├── DialogueSequence
├── optional PuzzleDefinition
├── AudioCueProfile
├── optional SceneSequenceDefinition
├── TransitionProfile
├── Condition / GameEffect
└── StorySceneRoute
```

폴더의 역할은 다음과 같다.

| 폴더 | 넣는 것 | 넣지 않는 것 |
|---|---|---|
| `Runtime/` | 재사용 가능한 동작과 스키마 | 장면별 좌표·대사·ID 분기 |
| `Content/` | 게임 의미와 자산 참조 | 원본 PNG/WAV |
| `Art/` | PNG, PSD 등 시각 원본 | Story Scene 규칙 |
| `Audio/` | WAV, MP3 등 음원 원본 | 오디오 재생 관리자 |
| `Prefabs/` | 재사용 View 조립 | 논리 진행 상태 |
| `Scenes/` | Bootstrap/Game/Dev 셸 | Story Scene별 Unity Scene |
| `Editor/` | 편집·가져오기·미리보기·검증 | 빌드 런타임 동작 |
| `Tests/` | 자동 검증 | 제품 기능 소유권 |

## 2. 안전한 작업 순서

모든 콘텐츠 작업은 다음 순서로 한다.

1. 변경할 Story Scene 또는 콘텐츠 ID를 확정한다.
2. Project 창에서 현재 참조 자산을 찾는다.
3. 같은 역할의 기존 Definition/Profile/Set/Action을 재사용할 수 있는지 확인한다.
4. 원본 미디어는 `Art/` 또는 `Audio/`에 넣는다.
5. 게임 의미와 연결은 `Content/` 자산에서 수정한다.
6. Preview 창으로 원본 참조를 빠르게 확인한다.
7. `Bootstrap.unity` Play Mode에서 실제 화면과 입력을 확인한다.
8. `Under The Horizon > Validate > Build Preflight`를 실행한다.
9. 관련 EditMode/PlayMode 테스트를 실행한다.
10. `Docs/Production/TODO.md`를 갱신하고 한 기능 단위로 커밋한다.

기존 `.asset`, `.prefab`, 이미지, 음원의 파일명이나 위치를 바꿔야 한다면 Windows 탐색기가 아니라 Unity Project 창에서 이동한다. Unity가 `.meta` GUID를 함께 보존하기 때문이다.

## 3. 프로젝트 실행과 특정 장면 확인

### 정상 실행

1. `Assets/_Project/Scenes/Bootstrap.unity`를 연다.
2. Play를 누른다.
3. Title → Save Slot → Game 흐름을 확인한다.

`Bootstrap.unity`는 서비스 초기화와 `Game.unity` 로드만 담당한다. 특정 배경, 인물, 증거 또는 Story Scene 로직을 Bootstrap 계층에 넣지 않는다.

### 최초 Story Scene 변경

실제 우선순위는 다음과 같다.

1. `GameDefinition.firstStorySceneId`
2. 서비스에 GameDefinition이 없을 때만 `GameStartup.firstStorySceneId` fallback

기본 자산은 `Assets/_Project/Content/Game/GAME_UnderTheHorizon.asset`이다. 개발 중 특정 Story Scene부터 확인하려면 이 자산의 `First Story Scene Id`를 임시 변경한다. 작업 후 반드시 `P-01`로 복원한다.

기존 저장 슬롯을 선택하면 저장된 `currentStorySceneId`가 우선하므로, 새 게임용 빈 슬롯을 사용해야 시작 ID 변경을 확인할 수 있다.

## 4. 아트 에셋을 갈아끼우는 방법

### 참조를 유지하는 가장 안전한 교체

같은 역할의 이미지 내용만 교체할 때는 다음 방식이 가장 안전하다.

1. Unity를 닫거나 해당 파일 import가 끝난 상태인지 확인한다.
2. 기존 PNG와 동일한 경로·파일명으로 새 PNG 내용을 덮어쓴다.
3. 기존 `.meta` 파일은 삭제하거나 교체하지 않는다.
4. Unity로 돌아와 reimport를 기다린다.
5. Inspector의 Sprite 참조가 유지되는지 확인한다.

이 방식은 GUID가 유지되어 CharacterDefinition, Location State, Prefab 참조를 다시 연결할 필요가 없다.

### 새 파일로 교체

새 이름을 써야 한다면:

1. 역할에 맞는 `Assets/_Project/Art/` 하위 폴더에 넣는다.
2. Texture Type을 `Sprite (2D and UI)`로 설정한다.
3. Alpha가 있으면 `Alpha Is Transparency`를 켠다.
4. 배경과 UI는 일반적으로 Mip Maps를 끈다.
5. 압축으로 경계나 글자가 깨지면 Compression을 `None`으로 검토한다.
6. 해당 Content 자산 또는 Prefab의 Sprite 필드에 새 Sprite를 연결한다.
7. 이전 파일은 참조 검색 후에만 제거한다.

프로젝트 정리 메뉴 `Under the Horizon > Art > 정리 및 고화질 임포트 적용`은 `ArtAssetMaintenance`가 관리하는 폴더 이동과 import 설정을 일괄 적용한다. 이 메뉴는 대규모 이동을 포함하므로 일반적인 한 장 교체에 반복 실행하지 않는다.

세부 명명과 권장 크기는 `Docs/Production/ART_ASSET_GUIDE.md`를 따른다.

## 5. 배경 교체와 Location State

관련 경로:

- 원본: `Assets/_Project/Art/Backgrounds/`
- 장소: `Assets/_Project/Content/Locations/Definitions/`
- 상태: `Assets/_Project/Content/Locations/States/`
- 편집: `Under The Horizon > Content > Locations`
- 빠른 미리보기: `Under The Horizon > Preview > Location`

`LocationDefinition.defaultBackground`는 장소의 fallback 배경이다. Story Scene이 `LocationStateDefinition`을 참조하고 그 State에 Background가 있으면 State 배경이 우선한다.

같은 방의 낮/밤/범죄 현장/봉쇄 상태는 새 Location이 아니라 새 Location State로 만든다.

```text
LOC_HORIZON
├── HORIZON_NormalDay
├── HORIZON_NormalNight
├── HORIZON_CrimeScene
└── HORIZON_FinalInterrogation
```

Location State에서 조정할 수 있는 값:

- `Id`: 안정적인 상태 ID
- `Background`: 표시할 Sprite
- `Tint`: 전체 색조
- `Audio Override`: 장소 기본 오디오를 덮어쓸 Cue
- `Ambient Particles`: 선택적인 광선·부유 입자 프로필

배경이 잘릴 때 Sprite 자체를 임의 crop하기 전에 Canvas/Viewport가 16:9 기준으로 cover 표시하는지 확인한다. 중요한 인물이나 출입구가 화면 가장자리에 있다면 1280×720, 1920×1080, 2560×1440 캡처에서 모두 검수한다.

Location Preview는 현재 `defaultBackground` 한 장을 확인하는 간단한 창이다. 특정 Location State, 캐릭터, 핫스팟이 합성된 최종 화면은 Story Scene Preview와 실제 Play Mode로 확인한다.

## 6. 캐릭터 이미지 교체

관련 경로:

- 원본: `Assets/_Project/Art/Characters/<Character>/`
- 정의: `Assets/_Project/Content/Characters/Definitions/`
- 배치: `Assets/_Project/Content/Characters/PlacementSets/`
- 정의 미리보기: `Under The Horizon > Preview > Character`

`CharacterDefinition`의 주요 필드:

| 필드 | 의미 |
|---|---|
| `Id` | 저장·대사·상호작용에서 쓰는 안정 ID |
| `Display Name` | 플레이어 표시 이름 |
| `Portrait` | 대화 초상 및 visual fallback |
| `Visuals` | Pose + Expression별 전신/표정 Sprite |
| `Presentation Override` | 인물별 그림자·idle 등 표시 프로필 |

기존 ID는 바꾸지 않는다. Sprite만 교체한다면 기존 파일 내용을 같은 경로에 덮어써 GUID를 유지하는 방법을 우선한다.

`Resolve(pose, expression)`은 정확히 일치하는 Visual을 먼저 찾고, 없으면 Portrait를 반환한다. 특정 표정이 Portrait로 떨어진다면 Visual Set에 해당 Pose/Expression 조합이 있는지 확인한다.

## 7. 캐릭터 위치·크기·앞뒤 순서 조정

편집 메뉴: `Under The Horizon > Content > Character Placements`

1. Story Scene 자산의 `Character Set` 참조를 확인한다.
2. Placement 창에서 해당 `SET_<SCENE>_CHARACTERS` 자산을 검색한다.
3. Placements 배열에서 인물을 선택하거나 추가한다.
4. 아래 값을 조정한다.

| 필드 | 범위/의미 |
|---|---|
| `Character` | CharacterDefinition 참조 |
| `Normalized X` | 배경 왼쪽 0, 오른쪽 1 |
| `Normalized Y` | 배경 아래 0, 위 1 |
| `Scale` | 0보다 큰 배율 |
| `Sorting Order` | 값이 클수록 앞에 표시 |
| `Pose` | 전신 자세 |
| `Expression` | 표정 |
| `Clickable` | 캐릭터 클릭 상호작용 허용 여부 |

권장 조정 순서:

1. `normalizedX/Y`로 발 위치 또는 기준점을 맞춘다.
2. `scale`로 원근을 맞춘다.
3. `sortingOrder`로 겹침을 해결한다.
4. Pose/Expression을 선택한다.
5. Story Scene Play Mode에서 HUD·Dialogue UI와 겹치는지 확인한다.

위치를 `CharacterView.transform`이나 Story Scene ID 조건문에 하드코딩하지 않는다. 배치의 권위 있는 원본은 CharacterPlacementSet이다.

## 8. 핫스팟 위치와 상호작용 조정

관련 경로:

- 세트: `Assets/_Project/Content/Locations/InteractionSets/`
- 정의/Action: `Assets/_Project/Content/Locations/InteractionDefinitions/`
- 편집: `Under The Horizon > Content > Interactions`

`InteractionDefinition` 주요 필드:

| 필드 | 의미 |
|---|---|
| `Id` | 완료 상태에 저장되는 안정 ID |
| `Type` | Character, MacGuffin, Context, Investigation, Exit, Puzzle |
| `Display Name` | HUD/목표 안내용 이름 |
| `Target Id` | 캐릭터 등 대상 필터. 비어 있으면 모든 대상 허용 |
| `Has World Hotspot` | 배경 위 클릭 영역 생성 여부 |
| `Normalized Rect` | 0~1 배경 좌표의 x/y/width/height |
| `Conditions` | 노출·실행 조건 |
| `Action` | 실행할 재사용 Action |
| `Repeatable` | 완료 뒤에도 다시 실행 가능한지 |

`Normalized Rect` 조정 절차:

1. 1920×1080 기준 화면에서 대상의 좌상/우하 위치를 잰다.
2. x와 width는 픽셀 값을 1920으로, y와 height는 1080으로 나눈다.
3. Inspector에서 Rect를 입력한다.
4. 1280×720과 2560×1440에서도 클릭 영역이 대상을 따라가는지 확인한다.
5. UI가 위에 겹치는 영역은 의도치 않은 클릭이 발생하지 않는지 확인한다.

이미지 alpha/polygon hit shape는 아직 공통 최종 계약이 완성되지 않았다. 현 단계에서는 Rect가 권위 있는 영역이며, 장면 전용 Raycast 스크립트를 만들지 않는다.

기존 Action 유형:

- `CharacterInteractionAction`: 캐릭터 대화
- `DialogueInteractionAction`: 일반 대화
- `InvestigationInteractionAction`: 대사 구간 + GameEffect 배열
- `EvidenceInteractionAction`: 증거 획득
- `LocationExitAction`: Location 이동
- `PuzzleInteractionAction`: PuzzleDirector를 통한 퍼즐 실행

상태 변화는 가능하면 Condition/GameEffect 자산으로 표현한다. View나 버튼이 `GameStateStore` 내부 컬렉션을 직접 수정하면 안 된다.

## 9. 대사와 선택지 수정

관련 경로:

- 생성된 대사: `Assets/_Project/Content/Dialogue/`
- 원본 CSV: `Assets/_Project/Content/Dialogue/Source/Dialogue_Master_KR.csv`
- 가져오기: `Under The Horizon > Import > Dialogue Graphs`

대사 한 줄은 안정적인 line ID, 화자, 본문, 표정, 음성, Conditions, Effects, Choices를 가진다. Choice는 ID, 텍스트, Conditions, Effects, Next Line ID를 가진다.

반복 보존할 대사 수정은 CSV에서 한다. 생성된 DialogueSequence만 Inspector에서 고치면 다음 import 또는 P0 builder 실행 때 사라질 수 있다.

체크 항목:

- line ID와 choice ID를 기존 저장/조건 계약 때문에 임의 변경하지 않았는가
- `nextLineId`가 실제 line을 가리키는가
- 분기 line에 도달 가능한 조건이 있는가
- 상태 변화가 GameEffect로 연결됐는가
- 화자 CharacterDefinition과 expression visual이 존재하는가
- 음성이 필수인 줄에 승인된 clip이 연결됐는가

## 10. 오디오 에셋 교체

관련 경로:

- 원본: `Assets/_Project/Audio/`
- Cue: `Assets/_Project/Content/Audio/`
- 편집: `Under The Horizon > Content > Audio Cues`
- 상세 규칙: `Docs/Production/AUDIO_ASSET_GUIDE.md`

같은 음원을 개선한 파일로 교체할 때는 기존 파일 경로와 `.meta`를 유지한다. 새 파일을 쓰면 AudioCueProfile 또는 Sequence의 AudioCommand 참조를 갱신한다.

우선순위는 다음과 같다.

```text
Sequence/Event override > Story Scene Audio Profile > Location default > 현재 적절한 상태 유지
```

Story Scene 코드나 UI에서 AudioSource를 직접 조작하지 않는다. 음악·환경음·효과음·Voice Bark·Story Recording은 AudioDirector가 소유한다.

## 11. Sequence와 Transition 작성

관련 경로:

- Sequence: `Assets/_Project/Content/Sequences/`
- 편집: `Under The Horizon > Content > Sequences`
- Transition: `Assets/_Project/Content/Transitions/`

현재 Sequence 명령 유형:

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
- Image Montage

짧은 연출은 명령 배열 순서로 작성한다. 장면 전용 Coroutine을 만들지 않는다.

몽타주는 `ImageMontageCommand`의 Frames, 프레임별 Hold Seconds, Fade, 시작 Scale, Stinger, Seen Flag를 설정한다. Seen Flag가 이미 있으면 몽타주만 건너뛰고 다음 명령은 계속된다. 예시는 `SEQ_D1_06_BodyReveal.asset`이다.

Transition 시간과 easing은 TransitionProfile에 둔다. StorySceneDirector는 프로필을 요청할 뿐 tween 값을 소유하지 않는다.

## 12. 퍼즐 콘텐츠 수정

관련 경로:

- 정의: `Assets/_Project/Content/Puzzles/`
- 공통 스키마: `Runtime/Puzzles/Core/PuzzleDefinition.cs`
- 전용 규칙: `Runtime/Puzzles/<Puzzle>/`

PuzzleDefinition은 다음을 소유한다.

- 안정 ID
- Controller Key
- Completion GameEffects
- 선택적인 Rules: Allowed Input IDs, Solution IDs, Required Evidence IDs, Hints, Order Matters

새 퍼즐 규칙을 콘텐츠로 이식할 때:

1. 원본에서 입력 ID와 정답 근거를 확인한다.
2. 기존 전용 controller가 있는지 찾는다.
3. PuzzleDefinition Rules에 허용 입력·정답·증거 gate·힌트를 작성한다.
4. controller가 `PuzzleContext.Definition.Rules`를 읽게 한다.
5. controller는 `PuzzleResult`만 반환한다.
6. 완료 상태·증거·스토리 진행은 PuzzleDefinition CompletionEffects와 Story flow가 처리한다.

퍼즐 controller가 Story Scene ID로 분기하거나 다음 장면을 직접 열면 안 된다.

## 13. 기존 Story Scene 수정

편집 메뉴: `Under The Horizon > Content > Story Scenes`

자산 위치:

- 프롤로그: `Content/StoryScenes/Prologue/`
- 본편: `Content/StoryScenes/Day01/` ~ `Day08/`

주요 필드:

| 그룹 | 필드 |
|---|---|
| Identity | Id, Display Name |
| Story | Chapter, Day, Time Block |
| Entry | Entry Conditions |
| Location | Location, Location State |
| Presentation | Initial Screen, Character Set, Interaction Set |
| Narrative | Entry Dialogue, Defer Entry Dialogue |
| Puzzle | optional PuzzleDefinition |
| Audio | Audio Profile |
| Sequence | Entry/Exit Sequence |
| Transition | Entry/Exit Transition |
| State | On Enter/Complete Effects |
| Flow | Routes |
| Validation | Authoring Requirements |

수정 뒤 `Under The Horizon > Content > Story Graph`에서 진입/종료 경로를 확인하고 `Under The Horizon > Preview > Story Scene`에서 주요 참조와 배경을 확인한다.

## 14. 새 Story Scene 추가 절차

새 Story Scene 추가는 콘텐츠 계약 변경이다. 기존 41개 canonical 장면을 대체하거나 ID 체계를 바꾸는 경우 먼저 명시적 승인을 받는다. 승인된 추가 장면은 다음 순서로 만든다.

1. ID와 이전/다음 Story Scene을 설계한다.
2. 기존 Location을 재사용할지 결정한다.
3. 시각 상태가 다르면 LocationStateDefinition을 추가한다.
4. DialogueSequence를 CSV 원본에 작성하고 import한다.
5. CharacterPlacementSet을 만든다.
6. InteractionDefinition/Action과 InteractionSet을 만든다.
7. 필요하면 PuzzleDefinition, AudioCueProfile, Sequence, Transition을 만든다.
8. StorySceneDefinition을 해당 Day 폴더에 만든다.
9. 앞 장면 Routes와 새 장면 Routes를 Condition과 함께 연결한다.
10. ContentDatabase의 Story Scenes 배열에 새 자산을 등록한다.
11. Authoring Requirements를 실제 구성과 일치시킨다.
12. Story Graph, Preview, Preflight, 테스트, Play Mode 순으로 검증한다.

명명 예:

```text
Story Scene: D3_05_NewInvestigation.asset
Dialogue:    DIA_D3_05.asset
Characters:  SET_D3_05_CHARACTERS.asset
Interaction: INT_D3_05.asset
Audio:       AUDIO_D3_05.asset
Sequence:    SEQ_D3_05_ENTRY.asset
```

새 Story Scene을 위해 `D3_05Controller.cs`나 `D3_05.unity`를 만들지 않는다. 고유 규칙이 있는 퍼즐만 전용 puzzle controller 예외가 가능하다.

## 15. ContentDatabase와 생성 도구

실행 콘텐츠는 `ContentDatabase`가 Story Scene, Location, Evidence를 인덱싱한다. 새 자산이 Project에 존재해도 Database 배열에 없으면 런타임에서 찾지 못할 수 있다.

`Under The Horizon > Build > P0 Project Content`는 이름과 달리 광범위한 생성기다. 다음을 생성·갱신할 수 있다.

- 41개 Story Scene 연결
- DialogueSequence
- CharacterPlacementSet
- Interaction/Effect
- Location State와 Audio Profile
- UI Prefab
- Bootstrap/Game Unity Scene
- Content database catalog

생성 대상 자산을 Inspector에서만 수정하면 덮어쓸 수 있다. 영구 변경은 원본 CSV 또는 `P0ProjectBuilder.cs`의 생성 규칙에도 반영해야 한다.

일반적인 에셋 한 장 교체나 Placement 수정을 위해 P0 builder를 실행하지 않는다.

## 16. Prefab과 UI 수정

관련 경로:

- UI Prefab: `Assets/_Project/Prefabs/UI/`
- Screen: `Assets/_Project/Runtime/UI/Screens/`
- 공통 View: `Assets/_Project/Runtime/UI/Components/`
- Router: `ScreenRouter`, `ModalRouter`

Prefab 수정 시:

1. Prefab Mode에서 연다.
2. 직렬화된 Text/Image/Button/Container 참조를 기록한다.
3. 오브젝트 이름을 바꾸거나 삭제하기 전에 테스트가 Transform path를 사용하는지 검색한다.
4. Anchor와 Pivot을 기준 해상도 1920×1080뿐 아니라 다른 16:9 해상도에서도 확인한다.
5. 화면 열기/닫기는 Router를 사용한다.

UI active 상태를 게임 진행 상태로 사용하지 않는다. HUD와 mode-specific Screen은 분리한다.

## 17. 검증 체크리스트

### 빠른 콘텐츠 검증

Unity 메뉴:

`Under The Horizon > Validate > Build Preflight`

검사 범위에는 ID 중복, 참조 누락, Story route, Location/State, Character, Dialogue, Evidence, Puzzle controller/rules, Audio, Transition, Placement, Interaction Action 등이 포함된다.

검증을 통과시키기 위해 validator를 약화하지 않는다. 규칙이 실제로 잘못되었을 때만 테스트와 근거를 포함해 validator를 수정한다.

### 테스트

1. `Window > General > Test Runner`
2. 관련 EditMode 테스트
3. 전체 EditMode
4. 관련 PlayMode 테스트
5. Bootstrap 대표 PlayMode

### 실제 화면

최소 확인 항목:

- 1280×720, 1920×1080, 2560×1440
- 배경 crop과 중요 피사체
- 캐릭터 발 위치, scale, sorting, 그림자
- HUD/Dialogue와 캐릭터 겹침
- 핫스팟 실제 클릭 범위
- 대사 선택지와 다음 line
- Sequence 입력 잠금 해제
- Puzzle 완료 Effect
- 다음 Story Scene route
- 저장 후 재진입

## 18. 자주 발생하는 문제

### 배경 또는 캐릭터가 흐리다

- Texture Type이 Sprite인지 확인한다.
- Max Size가 원본보다 지나치게 작지 않은지 확인한다.
- Compression과 Filter Mode를 확인한다.
- CanvasScaler와 RectTransform이 이미지를 비정상 확대하는지 확인한다.

### 이미지가 잘린다

- 배경 cover 정책 때문에 발생하는 정상 crop인지 확인한다.
- 중요한 피사체가 safe area 안에 있는지 확인한다.
- Sprite Editor crop과 RectTransform anchor를 함께 확인한다.

### 캐릭터가 다른 인물 뒤로 들어간다

- CharacterPlacement의 Sorting Order를 확인한다.
- scale과 normalizedY로 원근 관계도 함께 조정한다.

### 클릭이 안 된다

- InteractionDefinition이 현재 InteractionSet에 있는지 확인한다.
- Has World Hotspot과 Normalized Rect를 확인한다.
- Conditions와 Repeatable/완료 상태를 확인한다.
- 캐릭터 상호작용이면 Target Id와 CharacterPlacement Clickable을 확인한다.
- EventSystem과 InputSystemUIInputModule 오류를 확인한다.

### 대사가 중간에 멈춘다

- Next Line ID가 존재하는지 확인한다.
- Choice Condition 때문에 모든 선택지가 숨겨지지 않았는지 확인한다.
- Router 밖에서 Dialogue 패널을 SetActive하지 않았는지 확인한다.
- Console의 최초 예외부터 해결한다.

### 생성 후 수정이 사라진다

P0 builder 또는 CSV import 생성 대상일 가능성이 높다. 원본 CSV/생성 규칙을 수정한 뒤 다시 생성한다.

### ContentLoader game null 오류

`Bootstrap.unity`의 AppBootstrap에 GameDefinition 참조가 연결됐는지 확인한다. 기본 자산은 `GAME_UnderTheHorizon.asset`이며, 이 자산의 ContentDatabase 참조도 유효해야 한다.

## 19. 커밋 전 아키텍처 확인

- Story Scene용 Unity Scene이나 전용 controller를 만들지 않았는가
- 공용 Runtime에 Story Scene ID 분기를 넣지 않았는가
- 기존 Definition/Profile/Set/Action을 재사용했는가
- 화면은 Router, 전환은 TransitionDirector, 오디오는 AudioDirector를 통하는가
- 좌표와 규칙은 Content 자산에 있는가
- 안정 ID와 `.meta` GUID를 보존했는가
- 새 규칙에 validator/test가 있는가
- TODO와 관련 제작 문서를 갱신했는가
- 한 커밋이 한 기능만 포함하는가

이 체크를 통과한 뒤 기능 단위로 커밋한다.
