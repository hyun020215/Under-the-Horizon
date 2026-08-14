# Under the Horizon 표준 아키텍처

> 상태: 필수 아키텍처 기준 문서
> 저장소 루트의 `AGENTS.md`와 함께 적용한다. 두 문서가 다르면 더 엄격한 규칙을 따르며, 명시적인 사용자 지시만 이 설계를 변경할 수 있다.

## 1. 설계 목표

Under the Horizon은 데이터 주도형 내러티브 수사 게임이다. 핵심 원칙은 **41개의 스토리 장면을 41개의 Unity Scene으로 만들지 않는 것**이다.

- 스토리 내용은 장면별 C# 코드 없이 수정할 수 있어야 한다.
- 같은 장소는 날짜와 사건 상태가 달라도 재사용한다.
- UI, 오디오, 전환, 인물 배치, 상호작용과 진행 상태의 책임을 분리한다.
- 세이브에는 논리 상태만 저장하고 화면 연출 상태는 저장하지 않는다.
- 새 콘텐츠를 추가해도 런타임 구조가 바뀌지 않아야 한다.

## 2. 폴더별 책임

```text
Assets/_Project/
├── Runtime/   재사용 가능한 런타임 동작
├── Editor/    저작·가져오기·미리보기·검증 도구
├── Content/   게임 의미와 시스템 간 참조를 가진 데이터
├── Art/       원본 시각 리소스
├── Audio/     원본 음원 리소스
├── Prefabs/   런타임 GameObject 조립 구조
├── Scenes/    애플리케이션 셸과 개발용 플레이그라운드
├── Settings/  입력·렌더링·오디오·Addressables 설정
└── Tests/     EditMode 및 PlayMode 자동 검증
```

새 최상위 폴더나 같은 역할의 두 번째 프레임워크를 만들지 않는다.

## 3. 엄격한 용어

| 용어 | 의미 |
|---|---|
| Unity Scene | `Bootstrap.unity`, `Game.unity` 같은 실행 셸 |
| Story Scene | `P-01`, `D1-06` 같은 서사·게임플레이 단위 |
| Location | Horizon Room, Medbay 같은 물리 장소 |
| Location State | 같은 장소의 일반·범죄 현장·조사 상태 |
| Screen | 탐색·대화·지도·조사 같은 UI 모드 |
| Sequence | 짧은 연출을 구성하는 순서 있는 명령 |
| Transition | 화면·장소·스토리 상태 사이의 시각 전환 |
| Puzzle | 공통 셸 안에서 실행되는 독립 규칙 집합 |

## 4. Unity Scene 셸

출시 런타임은 다음 두 Scene을 중심으로 한다.

```text
Scenes/
├── Bootstrap.unity
├── Game.unity
└── Dev/
    ├── ContentPreview.unity
    ├── UIPlayground.unity
    ├── PuzzlePlayground.unity
    ├── AudioPlayground.unity
    └── TransitionPlayground.unity
```

`Bootstrap.unity`는 서비스, 콘텐츠 데이터베이스, 세이브, 오디오를 초기화하고 `Game.unity`를 로드한다. 게임별 배경·인물·증거·스토리 로직을 포함하지 않는다.

`Game.unity`는 다음과 같은 영속 셸이다.

```text
GameRoot
├── WorldCanvas
├── UICanvas
├── Directors
└── EventSystem
```

스토리 콘텐츠는 데이터에서 이 셸에 주입한다.

## 5. Story Scene 구성

`StorySceneDefinition`은 장면을 구성하는 링크 허브다.

```text
StorySceneDefinition
├── ID, 표시명, 장·일·시간대
├── 진입 조건
├── Location + LocationState
├── 초기 Screen
├── CharacterPlacementSet
├── InteractionSet
├── DialogueSequence
├── 선택적 PuzzleDefinition
├── AudioCueProfile
├── 진입·종료 Sequence와 Transition
├── 진입·완료 GameEffect
└── 다음 Story Scene 경로와 진입 방식(Immediate / MapTravel)
```

`StorySceneDirector`는 이를 해석해 전문 시스템에 명령만 전달한다. 이미지 로딩, AudioSource 제어, UI 텍스트, 좌표 계산, 세이브 파일 쓰기를 직접 담당하지 않는다.

`StorySceneRoute`의 기본 진입 방식은 기존 콘텐츠와 호환되는 `Immediate`다. `MapTravel` route는 현재 Story Scene 완료 후 대상 Story Scene ID를 논리 pending 상태로 남기며, `GameFlowController`가 지도에서 요청한 Location을 검증한 뒤 대상 장면 진입을 수행한다. 지도 View는 Story Scene 완료나 현재 Location 변경을 직접 소유하지 않는다.

공유 런타임에서 `sceneId`를 비교하는 분기나 `D1_06_BodyDiscoveryController` 같은 일반 장면 전용 컨트롤러를 만들지 않는다.

## 6. 장소와 인물

물리 장소는 `LocationDefinition`, 그 장소의 시각·게임 상태는 `LocationStateDefinition`이다. 동일 장소를 날짜별로 복제하지 않는다.

인물의 위치, 크기, 정렬 순서, 자세, 표정, 클릭 가능 여부는 `CharacterPlacementSet`에 둔다. 가능한 경우 배경 기준 0~1 정규화 좌표를 사용한다. `CharacterStage`가 `CharacterView`를 생성하고 배치를 적용하며, View는 스토리 진행을 직접 변경하지 않는다.

## 7. 상호작용, 조건과 효과

캐릭터, 맥거핀, 맥락, 조사 지점, 출구, 퍼즐 트리거는 `InteractionDefinition`과 `InteractionSet`으로 표현한다.

모든 가용성 판단은 공통 Condition을 사용한다.

- 플래그·증거 보유
- 신뢰도·불안도·증거 무결성 임계값
- 장면·퍼즐 완료
- ALL·ANY·NOT 복합 조건

모든 결과는 가능한 한 공통 GameEffect를 사용한다.

- 플래그 설정
- 신뢰도·불안도·무결성 변경
- 증거 추가
- 목표·장면 완료
- 장소 해금

UI나 View가 `GameStateStore`를 임의로 직접 변경하지 않는다.

## 8. 상태와 세이브

`GameStateStore`가 변경 가능한 논리 상태의 유일한 소유자다. 주요 상태는 현재 장면·장소·시간, 지도 이동을 기다리는 대상 Story Scene, 신뢰도, 불안도, 증거 무결성, 플래그, 증거, 완료 상호작용·퍼즐·목표, 이론, 엔딩이다.

세이브에는 논리 상태만 저장한다. AudioSource 시간, 트윈 진행률, UI Transform, 생성된 View 참조와 임시 모달·전환 상태는 저장하지 않는다. 로드 시 논리 상태와 콘텐츠 정의로 화면을 재구성한다. 저장 스키마 변경에는 버전과 마이그레이션이 필요하다.

지도 이동 대기 상태를 복원할 때는 완료된 출발 Story Scene의 프레젠테이션만 재구성한다. 진입 Effect·Sequence·Dialogue를 재실행하거나 UI 선택 상태를 저장 데이터로 승격하지 않는다.

## 9. UI와 전환

Screen 변경 권한은 `ScreenRouter`, 모달 변경 권한은 `ModalRouter`에 있다. 다른 화면의 패널을 직접 켜고 끄지 않는다. 영속 HUD와 모드별 화면은 분리한다.

시각 전환은 `TransitionDirector`가 `TransitionProfile`에 따라 재생한다. 스토리 흐름은 전환을 요청할 뿐 트윈이나 페이드 구현을 알지 못한다.

## 10. Sequence

짧은 인게임 연출은 `SceneSequenceDefinition`과 `SequenceDirector`로 구성한다. Wait, Camera, Audio, Dialogue, Character, Location, UI, Transition, State, InputLock 같은 재사용 명령을 사용한다. 장면 전용 코루틴에 연출 시간을 숨기지 않는다.

## 11. 오디오

`AudioDirector`가 다음 버스를 소유한다.

```text
Music A / Music B
Ambience A / Ambience B
SFX
Voice Bark
Story Voice / Recording
```

해결 우선순위는 이벤트·Sequence 재정의, Story Scene 재정의, Location 기본값, 현재 적합 상태 순이다. 대화 더킹도 오디오 시스템이 담당한다. 장면 코드가 전역 AudioSource를 직접 조작하거나 AudioDirector가 장면 ID를 분기하지 않는다.

## 12. 퍼즐

퍼즐은 `PuzzleDirector + 공통 Puzzle Screen + PuzzleDefinition + 선택적 전문 컨트롤러` 구조다. 전문 컨트롤러는 고유 규칙만 처리하고 `PuzzleResult`를 반환한다. 스토리 진행과 상태 반영은 공통 효과·흐름 계층이 담당한다.

## 13. 증거와 이론

핵심 증거 ID `C-01`~`C-18`은 안정적인 계약이다. UI 표시명과 내부 ID를 분리하며, 제품 결정 없이 내부 ID·전체 단서 수·완료율을 플레이어에게 노출하지 않는다. 증거 보드는 장면별 하드코딩이 아니라 증거·이론 데이터로 동작한다.

## 14. 콘텐츠 로딩

배경, 전신 캐릭터, 대형 증거 이미지, 영상, BGM, 환경음과 긴 녹음은 Addressables 호환 참조를 우선한다. 레거시 `Resources.Load`는 마이그레이션 경계 안에 격리하고 새 의존성을 확산하지 않는다.

## 15. 런타임 의존 방향

```text
Content / Definitions
        ↓
       Core
        ↓
     Gameplay
   ↙    ↓     ↘
Audio   UI   Puzzles
```

의도적인 명령은 직접 서비스 호출로, 여러 독립 시스템에 대한 알림은 이벤트로 전달한다. 모든 흐름을 불투명한 이벤트 체인으로 만들지 않는다.

## 16. 에디터 도구와 검증

Story Scene 편집·그래프, 장소·인물 배치·상호작용·오디오·증거·Sequence 편집, CSV 가져오기와 직접 미리보기를 지원한다.

빌드 전 검증은 다음을 찾아야 한다.

- 중복 ID와 깨진 직렬화 참조
- 누락되거나 잘못된 장면 경로
- 장소·장소 상태·인물·대화·증거·퍼즐 참조 오류
- 오디오·전환 프로필 누락
- 필요한 Addressables 등록·레이블 누락
- 진행 불가능한 필수 경로

통과를 위해 검증 규칙을 약화하지 않고 콘텐츠를 고친다.

## 17. 이름 규칙

`GAME_`, `DATABASE_`, `LOC_`, `CHR_`, `INT_`, `DIA_`, `C01_`, `THEORY_`, `PUZ_`, `AUDIO_`, `TRANS_`, `SEQ_`, `BG_`, `EVD_`, `MUS_`, `AMB_`, `SFX_`, `PF_` 접두사를 역할에 맞게 사용한다. Story Scene 파일명에는 `P01`, `D1_06`, `D8_03` 같은 정규 ID를 유지한다.

## 18. 금지되는 회귀

- Story Scene마다 Unity Scene 또는 전용 컨트롤러 생성
- 공유 시스템의 장면 ID switch 증가
- 모든 책임을 가진 GameManager 도입
- UI 화면끼리 직접 패널 토글
- 장면 코드에서 AudioSource 직접 조작
- 인물 좌표·에셋 경로·장면 경로 하드코딩
- 날짜별 Location 복제
- UI GameObject 상태를 게임·세이브 상태로 사용
- 두 번째 흐름·상태·UI·오디오 프레임워크 도입
- 마이그레이션 이유 없는 대규모 이름 변경

## 19. 아키텍처 변경 절차

기존 확장 지점으로 해결할 수 없는 경우에만 문제, 현재 한계, 대안 검토, 제안 구조, 영향 파일, 직렬화·세이브·콘텐츠 마이그레이션, 호환성, 테스트·검증 변경, 배포·롤백 계획을 문서화한다. 명시적 승인을 받은 뒤 이 문서와 `AGENTS.md`를 같은 변경에서 갱신한다.

## 20. 최종 원칙

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

대부분의 스토리 표현과 흐름을 콘텐츠 데이터 수정만으로 바꿀 수 있고, 재사용 런타임 코드는 안정적으로 유지될 때 이 아키텍처가 성공한 것이다.
