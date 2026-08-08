# Unity 콘텐츠 제작 및 수정 매뉴얼

이 문서는 개발자가 타이틀 화면, Story Scene, 대사, 배치, 상호작용, 트랜지션, 오디오, 퍼즐과 UI를 현재 아키텍처 안에서 수정하는 방법을 설명한다.

## 1. 기본 원칙

- `Bootstrap.unity`와 `Game.unity`는 애플리케이션 셸이다. Story Scene마다 `.unity` 파일을 만들지 않는다.
- P-01부터 D8-03까지의 장면 차이는 `StorySceneDefinition` 자산과 연결된 콘텐츠 자산으로 표현한다.
- 게임 상태 변경은 `Condition`과 `GameEffect`를 사용한다.
- 화면 이동은 `ScreenRouter`, 연출은 `TransitionDirector`와 `SequenceDirector`, 오디오는 `AudioDirector`를 통한다.
- `Assets/_Project/Runtime/`은 동작, `Content/`는 게임 데이터, `Art/`와 `Audio/`는 원본 미디어다.

## 2. 실행 시작점

### Bootstrap 수정

- 씬: `Assets/_Project/Scenes/Bootstrap.unity`
- 런타임: `Assets/_Project/Runtime/App/AppBootstrap.cs`
- 역할: 서비스 초기화, 로딩 화면, `Game.unity` 로드만 담당한다.

배경, 등장인물이나 특정 Story Scene 로직을 Bootstrap에 넣지 않는다.

### 최초 Story Scene 변경

- 파일: `Assets/_Project/Runtime/Flow/GameStartup.cs`
- Inspector 필드: `First Story Scene Id`
- 기본값: `P-01`

개발 중 특정 장면부터 확인하려면 Game 씬의 `GameRoot > GameStartup`에서 이 값을 바꾼다. 확인 후 반드시 `P-01`로 되돌린다.

## 3. 타이틀 화면

- 프리팹: `Assets/_Project/Prefabs/UI/PF_TitleScreen.prefab`
- 화면 클래스: `Assets/_Project/Runtime/UI/Screens/TitleScreen.cs`
- 로고 원본: `Assets/_Project/Art/UI/Overhaul/UI_logo_transparent.png`
- 테마: `Assets/_Project/Content/UI/UITheme.asset`

레이아웃과 이미지 교체는 프리팹에서 수행한다. 시작·불러오기·설정 같은 동작은 `TitleScreen`이 서비스를 호출하도록 구현하고, 다른 패널을 직접 `SetActive`하지 않는다. 화면 전환은 `ScreenRouter.OpenAsync`를 사용한다.

## 4. Story Scene 수정

### 권장 편집 방법

Unity 메뉴에서 `Under The Horizon > Content > Story Scenes`를 연다. 왼쪽에서 장면을 선택하고 오른쪽 Inspector에서 수정한다.

### 자산 위치

- 프롤로그: `Assets/_Project/Content/StoryScenes/Prologue/`
- 본편: `Assets/_Project/Content/StoryScenes/Day01/`부터 `Day08/`
- 스키마: `Assets/_Project/Runtime/Flow/StorySceneDefinition.cs`

주요 필드는 다음과 같다.

| 필드 | 용도 |
|---|---|
| Id | 안정적인 장면 ID. 기존 값은 변경하지 않는다. |
| Location / Location State | 배경과 장소 상태 |
| Initial Screen | 장면 진입 화면 |
| Character Set | 등장인물 배치 |
| Interaction Set | 클릭 가능한 상호작용 |
| Entry Dialogue | 진입 대사 |
| Puzzle | 선택적 퍼즐 |
| Audio Profile | 장면 오디오 덮어쓰기 |
| Entry/Exit Sequence | 순서가 있는 연출 |
| Entry/Exit Transition | 화면 전환 |
| On Enter/Complete Effects | 상태 변화 |
| Routes | 다음 Story Scene과 조건 |

경로는 Unity 메뉴 `Under The Horizon > Content > Story Graph`에서 한 번에 확인한다.

## 5. 대사와 선택지

- 자산: `Assets/_Project/Content/Dialogue/`
- 한국어 원본: `Assets/_Project/Content/Dialogue/Source/Dialogue_Master_KR.csv`
- 스키마: `DialogueSequence.cs`, `DialogueLine.cs`, `DialogueChoice.cs`
- 화면: `Assets/_Project/Prefabs/UI/PF_DialogueScreen.prefab`
- 화면 로직: `Assets/_Project/Runtime/UI/Screens/DialogueScreen.cs`

대사 한 줄은 line ID, 화자, 본문, 표정, 음성, 조건, 효과와 선택지를 가진다. 선택지는 자체 조건·효과와 다음 line ID를 가진다. 상태 변경을 UI 클릭 코드에 직접 작성하지 말고 `GameEffect` 자산으로 연결한다.

CSV를 바꾼 뒤 자동 생성 메뉴를 실행하면 생성 대상 DialogueSequence가 덮어써질 수 있다. 생성 데이터에 수동 수정이 필요하면 먼저 원본 CSV와 `P0ProjectBuilder.BuildDialogue`의 매핑을 함께 고친다.

## 6. 등장인물과 배치

- 인물 정의: `Assets/_Project/Content/Characters/Definitions/`
- 장면 배치: `Assets/_Project/Content/Characters/PlacementSets/`
- 편집 메뉴: `Under The Horizon > Content > Character Placements`
- 미리보기: `Under The Horizon > Preview > Character`
- 런타임: `CharacterStage.cs`, `CharacterView.cs`

`normalizedX`와 `normalizedY`는 0~1 범위로 입력한다. `scale`은 0보다 커야 한다. 장면별 좌표를 MonoBehaviour나 Unity Scene 계층에 하드코딩하지 않는다.

## 7. 장소와 배경

- 장소: `Assets/_Project/Content/Locations/Definitions/`
- 상태: `Assets/_Project/Content/Locations/States/`
- 배경 원본: `Assets/_Project/Art/Backgrounds/`
- 편집 메뉴: `Under The Horizon > Content > Locations`
- 미리보기: `Under The Horizon > Preview > Location`

같은 물리 장소의 시간·사건 차이는 새 Location이 아니라 `LocationStateDefinition`으로 만든다. State에서 배경, 색조와 Audio Override를 지정한다.

## 8. 상호작용과 상태 변화

- 세트: `Assets/_Project/Content/Locations/InteractionSets/`
- 정의와 Action: `Assets/_Project/Content/Locations/InteractionDefinitions/`
- 편집 메뉴: `Under The Horizon > Content > Interactions`
- 스키마: `Assets/_Project/Runtime/Interaction/`
- Condition: `Assets/_Project/Runtime/Common/Conditions/`
- GameEffect: `Assets/_Project/Runtime/Common/Effects/`

일반적인 작업 순서는 다음과 같다.

1. 재사용 가능한 `InteractionAction` 유형을 선택한다.
2. Action 자산에 Dialogue, Evidence, Puzzle 또는 Location을 연결한다.
3. `InteractionDefinition`에 ID, Type, Condition, Action, 반복 여부를 설정한다.
4. 장면의 `InteractionSet`에 Definition을 넣는다.
5. 결과 상태 변화는 Action 내부 임의 변경보다 `GameEffect`를 사용한다.

Story Scene ID를 검사하는 `if`나 `switch`를 공용 런타임에 추가하지 않는다.

## 9. 트랜지션과 Sequence

- 트랜지션 자산: `Assets/_Project/Content/Transitions/`
- 트랜지션 런타임: `Assets/_Project/Runtime/Transitions/`
- Sequence 자산: `Assets/_Project/Content/Sequences/`
- Sequence 편집 메뉴: `Under The Horizon > Content > Sequences`
- Sequence 런타임: `Assets/_Project/Runtime/Sequences/`

페이드 시간이나 입력 차단을 장면 스크립트에 직접 넣지 않는다. 짧은 연출의 순서는 `SceneSequenceDefinition` 명령 배열로 작성한다.

## 10. 오디오와 녹음

- 원본 오디오: `Assets/_Project/Audio/`
- Cue 자산: `Assets/_Project/Content/Audio/`
- 편집 메뉴: `Under The Horizon > Content > Audio Cues`
- 런타임: `Assets/_Project/Runtime/Audio/AudioDirector.cs`

장소 기본 오디오는 `LocationDefinition.DefaultAudio`, 장면별 덮어쓰기는 `StorySceneDefinition.AudioProfile`에 연결한다. Story Recording은 line ID 대응표를 확정한 뒤 Dialogue 또는 Sequence의 음성 명령에서 재생한다. Story Scene 스크립트에서 `AudioSource`를 직접 조작하지 않는다.

## 11. 증거와 퍼즐

- 증거: `Assets/_Project/Content/Evidence/`
- 증거 편집 메뉴: `Under The Horizon > Content > Evidence`
- 퍼즐 정의: `Assets/_Project/Content/Puzzles/`
- 퍼즐 런타임: `Assets/_Project/Runtime/Puzzles/`

핵심 증거 ID C-01~C-18은 변경하지 않는다. 증거 획득은 `AddEvidenceEffect` 또는 Evidence Interaction Action으로 처리한다. 퍼즐 컨트롤러는 퍼즐 규칙만 처리하고, 완료 후 스토리 진행은 `PuzzleResult`와 GameEffect가 담당한다.

## 12. UI 화면 수정

- 화면 프리팹: `Assets/_Project/Prefabs/UI/`
- 화면 클래스: `Assets/_Project/Runtime/UI/Screens/`
- 공통 컴포넌트: `Assets/_Project/Runtime/UI/Components/`
- 라우터: `Assets/_Project/Runtime/UI/Core/ScreenRouter.cs`, `ModalRouter.cs`

프리팹의 직렬화 필드를 바꿀 때는 대응 화면 클래스의 필드도 확인한다. 다른 화면을 열 때는 Router를 사용한다. 같은 화면을 다시 여는 요청은 현재 화면을 비활성화하지 않고 컨텍스트만 갱신한다.

## 13. 자동 생성 도구 사용 시 주의

메뉴 `Under The Horizon > Build > P0 Project Content`는 현재 이름과 달리 P0/P1 기본 콘텐츠와 씬을 함께 재생성한다.

이 도구가 생성 또는 갱신하는 범위에는 다음이 포함된다.

- 41개 Story Scene 연결
- 생성 DialogueSequence
- 생성 CharacterPlacementSet
- 생성 Interaction/Effect
- Location 기본 State와 Audio Profile
- UI 프리팹
- Bootstrap/Game 씬

따라서 생성 대상 자산을 Inspector에서만 수정하면 다음 실행 때 덮어써질 수 있다. 반복 보존해야 하는 변경은 `Assets/_Project/Editor/ContentTools/P0ProjectBuilder.cs` 또는 원본 CSV에도 반영한다.

## 14. 검증과 테스트

작업 완료 전 다음 순서로 확인한다.

1. `Under The Horizon > Validate > Build Preflight`
2. `Window > General > Test Runner`에서 EditMode 실행
3. Test Runner에서 PlayMode 실행
4. `Bootstrap.unity`를 열고 Play
5. P-01 대사 표시, 계속 버튼, 선택지, 다음 장면 전환 확인

Preflight 규칙은 `Assets/_Project/Editor/Validators/ContentValidator.cs`에 있다. 검증을 통과시키려고 규칙을 약화하지 말고 콘텐츠 참조를 수정한다.

## 15. 자주 발생하는 문제

### TaskCanceledException이 대사에서 발생함

현재 화면이 대사 대기 중 비활성화됐다는 뜻이다. `ScreenRouter`를 거치지 않은 패널 토글이나 같은 화면의 중복 Close/Open 호출을 확인한다. `DialogueScreen.OnDisable`은 안전하게 현재 줄을 종료하지만, 화면을 임의로 끄는 코드는 제거해야 한다.

### 배경만 보이고 입력이 안 됨

- Game 씬에 `EventSystem`과 `InputSystemUIInputModule`이 있는지 확인한다.
- `ScreenRouter.screens`에 DialogueScreen이 등록됐는지 확인한다.
- DialogueScreen의 `NarrativeDirector`, 버튼과 Text 참조가 연결됐는지 확인한다.
- Console의 첫 번째 예외부터 해결한다.

### 생성 후 수동 변경이 사라짐

생성 대상 자산을 직접 고친 경우다. 원본 CSV 또는 `P0ProjectBuilder`의 생성 규칙을 수정한 뒤 다시 생성한다.
