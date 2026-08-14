# 통합 제작 TODO

## 2026-08-13 원본/현재 Unity 직접 실행 UX 비교 후속 작업

> 비교 기준: 원본 `D:\\codex-project-mystery`의 `UI Basic Scene` Play Mode와 현재
> `Bootstrap -> Game` Play Mode를 동일한 16:9 화면 흐름에서 직접 조작했다.
> 아래 항목은 기존 `ScreenRouter`, `ModalRouter`, `TransitionDirector`, UI Prefab,
> `CharacterStage`를 확장하며, 별도 UI/전환 프레임워크를 만들지 않는다.

### 우선순위 0 — 화면 기준과 가독성

- [x] 저장된 16:9 해상도/전체 화면 설정을 앱 시작 시 실제 디스플레이에 적용한다.
  - 현재 `DisplaySettingsService.Load()`는 선택값만 복원하고 `Screen.SetResolution`을 호출하지 않는다.
  - 1920x1080을 권장 기본값으로 유지하고 1280x720, 1600x900, 2560x1440, 3840x2160을 설정에서 선택 가능하게 유지한다.
  - Bootstrap과 Game Canvas가 같은 1920x1080 기준과 0.5 width/height match를 사용하는지 검증한다.
- [ ] 원본과 현재 화면을 비교할 때 Unity Game View를 16:9 Fit 상태로 맞추는 개발용 검증 절차를 문서화한다.
- [ ] 공통 타이포그래피 기준을 확장해 제목/섹션/본문/버튼/보조 문구의 크기, 굵기, 자간, 행간, 대비를 원본 수준으로 맞춘다.
  - Dialogue 본문, `NARRATION` 이름표, Map/Save Slot의 작은 텍스트를 1080p 실기 기준으로 우선 개선한다.
  - 한글/영문 fallback, 깨진 글리프, `LOC_PORT` 같은 내부 ID 노출과 언어 혼용을 검증한다.
  - [x] Dialogue 본문 최소 크기, 패널 점유율, 이름표와 진행 버튼 크기를 1080p 가독성 기준으로 상향했다.

### 우선순위 1 — 핵심 화면 구성

- [x] Save Slot을 장식 프레임 안에서 축소된 카드가 아니라 원본처럼 화면 전체를 활용하는 3열 레이아웃으로 조정한다.
  - 루트의 장식 프레임 중첩을 제거하고 카드 높이·폭, 제목과 상태 글자 크기를 1080p 기준으로 확대했다.
- [x] 저장 슬롯 확인 UX를 작은 중앙 팝업에서 원본의 전체 화면 dim + 큰 선택 영역에 가까운 구성으로 조정하되 `ModalRouter`/`ConfirmDialog`를 재사용한다.
  - 기존 `ConfirmModal`의 전체 화면 입력 차단과 `ModalRouter` 흐름을 유지하면서 dim을 강화하고, 중앙 선택 패널·메시지·버튼의 1080p 점유율을 확대했다.
- [x] Persistent HUD의 불투명 상단 패널을 원본의 투명 금색 라인/아이콘 중심 오버레이로 재구성한다.
  - 기존 `PersistentHud`의 상태·목표 갱신과 Map/Record 라우팅은 유지하고, 상단 높이와 남색 면 채움을 줄인 뒤 금색 하단 라인과 저채도 정보 영역으로 정리했다.
- [ ] Map 화면의 덱 선택, 지도, 선택 장소 정보, 이동 행동의 정보 계층을 원본과 맞춘다.
  - 실제 `MapDefinition`, Location/Condition 연결은 유지한다.
  - Deck/장소명 표기와 한글/영문 혼용, 작은 글자, 겹치는 노드 라벨을 수정한다.
  - [x] 지도 뷰포트 점유율과 현재 위치·덱 제목 가독성을 높이고, `MAP_Deck07`·`Deck07` 같은 내부형 표기 대신 현지화된 갑판명을 표시하며 동적 장소 노드 폭을 확대했다.
- [x] Dialogue 프레임의 화면 점유율, 본문 여백, 이름표, 다음 버튼 크기를 원본과 맞춘다.
  - 앞서 확대한 1080p 본문 크기는 유지하면서 프레임을 원본처럼 좌측 하단 약 60% 폭으로 재배치하고, 본문 안전 여백·이름표 상단 정렬·프레임 안쪽 진행 버튼 위치를 함께 조정했다.

### 우선순위 2 — 전환과 살아 있는 화면

- [x] 공통 화면 전환 프로필에 원본의 UI exit, cover, hold, reveal, UI enter 리듬을 재현한다.
  - 타이틀→슬롯과 슬롯→게임에서 즉시 교체처럼 보이지 않도록 한다.
  - `TransitionDirector`와 기존 `TransitionProfile`만 확장한다.
  - 기존 프로필의 미사용 `uiExitDuration`·`holdDuration`·`uiEnterDuration`을 `TransitionDirector` 단계에 연결하고 표준 Fade의 cover/reveal 시간을 1080p 실제 전환에 맞게 조정했다.
- [x] 원본의 원형 부유 파티클과 밝은/남색 cover를 전환 플레이어로 재현하고 Reduced Motion에서는 정적으로 대체한다.
  - 기존 `FadeTransitionPlayer`가 프로필의 남색 cover와 금색 원형 glow를 생성·표시하도록 확장했으며, Reduced Motion에서는 이동 없이 즉시 정적 cover 상태만 적용한다. 원형 glow sprite는 `AmbientParticleOverlay`와 공용 `UiGlowSprite`를 사용해 중복 생성을 피한다.
- [ ] 탐색 화면에 기존 `AmbientParticleOverlay`를 활용해 먼지/광점의 밀도와 속도를 원본 수준으로 조정한다.
- [ ] `CharacterStage`/`CharacterView`의 기존 프레젠테이션 기능을 활용해 접지 그림자, 환경광, 미세 idle 이동을 보강한다.
  - 배치 좌표와 캐릭터별 차이는 `CharacterPlacementSet`/프로필 데이터에 둔다.
- [ ] 버튼 hover/press, 커서 glow, 화면 진입 후 focus 반응을 원본과 같은 강도로 조정한다.

### 직접 실행 완료 기준

- [ ] 1920x1080과 2560x1440에서 Title→Save Slot→Gameplay→Map 흐름을 각각 캡처한다.
- [ ] 원본과 현재의 정지 화면뿐 아니라 전환 중간 프레임, hover/press, 대화 진행 반응을 나란히 비교한다.
- [ ] 배경/캐릭터 원본 텍스처가 적절한 Game View Fit 상태에서도 흐릴 때만 Import max size/compression을 변경한다.
- [ ] 각 기능 단위로 TODO, 관련 테스트/검증, Unity Play Mode 결과를 같은 커밋에 포함한다.

> 이 문서는 프로젝트의 유일한 TODO 목록이다. 자산 폴더 안에 TODO 파일을 다시 만들지 않는다.  
> 갱신일: 2026-08-14 (지도 선택·확정 이동 UI 증분)

## 완료

- [x] `UNITY_CONTENT_AUTHORING_GUIDE.md`를 현재 Runtime/Content/Editor 구조에 맞게 갱신하고,
  개발자가 GUID를 보존하며 아트·오디오를 교체하고 배경·캐릭터·핫스팟을 조정하며 승인된
  Story Scene을 추가할 수 있도록 검증·실행·문제 해결 절차를 문서화했다.

- [x] 원본 저장소의 비생성 파일과 최신 버전을 비교하고 필요한 코드·미디어를 이식했다.
- [x] 41개 `StorySceneDefinition`과 `C-01`~`C-18`의 canonical ID를 구성했다.
- [x] P0 범위의 0바이트 Content ScriptableObject 212개와 Prefab 43개를 현재 스키마로
  복구하고 빈 중복 콘텐츠 자산을 제거했다.
- [x] `Bootstrap.unity`를 composition root로, `Game.unity`를 지속 런타임 셸로 연결했다.
- [x] Title → Save Slot → Game 흐름과 화면 라우팅을 연결했다.
- [x] 선택한 Save Slot을 활성 체크포인트에 바인딩하고 Story Scene 진입 시 같은 슬롯에 자동 저장한다.
  - `GameStartup`이 선택 슬롯을 Load/New 분기 전에 `SaveCheckpoint`에 바인딩하며, 바인딩 전이거나
    등록된 `SaveService`가 없으면 체크포인트는 저장하지 않는다.
  - `StorySceneDirector.Entered` 시점의 논리 상태를 저장한다. `SaveData` 필드와
    `SaveVersion.Current`는 변경하지 않아 저장 마이그레이션은 추가하지 않았다.
- [x] 등록되지 않은 Trust는 런타임 기본값 2로 해석하고, 최초 증감도 2에서 시작하도록 했다.
  - 저장에 명시된 0을 포함한 기존 Trust 값은 그대로 보존한다.
- [x] 한국어 마스터 CSV의 전체 대화와 100개 선택지를 실행 가능한 그래프로 가져왔다.
- [x] 41개 CharacterPlacementSet의 구조와 정규화 좌표 검증을 구성했다.
- [x] Story Scene·Location·Character Placement·Interaction·Audio Cue·Evidence·Sequence 편집 도구를 구성했다.
- [x] ID, 필수 참조, 대화 그래프, 배치 좌표, Interaction Action, Puzzle controller,
  완료 Effect, Location 배경·오디오를 검사하는 빌드 사전 검증을 구성했다.
- [x] D1-06 발견 장면을 첫 조사 vertical slice로 완성했다.
  - 문, 혈흔·시신, 세면대, 녹음기, 천장 패널의 월드 핫스팟
  - `Character`/`Context`/`Investigation` capability와 증거 획득 검증
  - 다섯 조사 완료 후 Richard 대화와 공식 선택지 개방
  - Input Lock, Audio, 4프레임 Image Montage, Dialogue로 구성한 entry Sequence
- [x] P-01을 실제 상호작용으로 완주하고 지도에서 P-02로 이동하는 첫 프롤로그 흐름을 연결했다.
  - 진입 Sequence는 `P-01_001`~`002`만 재생하고, 초대장 조사 → Daniel 부착 메신저 확인 →
    Daniel 본체 대화·선택을 `InteractionCompletedCondition`으로 순서화했다.
  - C-01 획득과 `anonymous_tip_preview`는 기존 GameEffect 계약으로 적용한다.
  - 마지막 `DialogueInteractionAction`의 재사용 가능한 advance 옵션이 대화 성공 뒤 route 해석을 요청한다.
    P-01은 Port에서 완료·저장되고 P-02를 pending으로 유지하며, HUD 지도에서 M.V. 엘리시움의
    Gangway 노드를 선택하고 별도 확인해야 P-02/`LOC_GANGWAY`에 진입한다.
  - 중앙 `INT_P_01_CONTINUE` Exit는 활성 InteractionSet에서 제외했다. 기존 Definition/Action 자산은
    직렬화 GUID와 개발 저장 호환성을 위해 삭제하지 않았다.
  - P-02는 entry Dialogue 자동 재생을 미루고, 배치된 Daniel의 실제 `Character` Interaction을 클릭해야
    `P-02_001`부터 대화가 시작된다. Evelyn·Daniel·Richard 배치는 같은 데이터 세트로 구성한다.
  - Validator는 `MapTravel` 출발 장면의 월드 Exit 의존과, deferred entry Dialogue의 클릭 가능한 실행 대상
    누락을 거부한다.
- [x] Story Scene 완료와 지도 이동을 분리하는 공통 pending travel 플로우를 추가했다.
  - `StorySceneRoute`의 `Immediate` 기본값을 유지하면서 재사용 가능한 `MapTravel` 진입 방식을 추가해
    기존 41개 Story Scene route 동작을 바꾸지 않았다.
  - `GameFlowController`가 대상 Story Scene·Location·진입 조건을 사전 검증하고, 완료된 출발 장면과
    `pendingStorySceneId`를 안정 체크포인트로 남긴 뒤 정확한 목적지 요청만 진입시킨다.
  - `GameStartup.ResumeAsync` 경로는 pending 저장에서 출발 장면 프레젠테이션만 복원하며 진입 Effect·Sequence·Dialogue를
    재실행하지 않는다. 완료됐지만 pending 필드가 없는 v1 저장도 route 데이터로 일반 복구한다.
  - `SaveVersion`을 2로 올리고 v1→v2 내장 마이그레이션, pending 체크포인트, 상위 버전 거부 검증을 추가했다.
  - advance 요청 Interaction이 사전 검증에 실패하면 Action Effect와 완료 기록을 함께 되돌려 재시도 가능한 상태를 보존한다.
  - 이 증분은 공통 Runtime·Save 계약을 제공했고, P-01 route 활성화와 지도 선택·확정 UI는 아래 후속
    증분에서 연결을 완료했다.
  - Unity 6000.3.20f1 기준 EditMode 96/96, PlayMode 24/24와 Build Preflight를 통과했다.
- [x] pending Story Scene 목적지를 지도에서 선택하고 확인해 이동하는 공통 UI를 연결했다.
  - `MapScreen`은 노드 클릭을 임시 선택으로만 처리하고, `GameFlowController`가 정확한 pending Location을
    승인한 경우에만 별도 `목표 경로로 이동` 버튼으로 Story Scene 진입을 요청한다.
  - Base/Restricted/Technical/장소 노드를 하나의 4:3 `Map Surface` 좌표계로 통일하고, 전용 노드
    템플릿과 선택 장소 이름·상태·설명·이동 피드백 영역을 추가했다.
  - M.V. 엘리시움 지도에 기존 `MAP_Port_Base`를 연결하고, 없는 제한/기술 Overlay와 토글은 숨긴다.
    Port와 Gangway에는 사용자 표시명·설명·검수 좌표를 저작했으며 Gangway는 `RouteOnly`로 분류했다.
  - HUD는 pending 중 `승선 통로로 향하기 / 지도에서 목적지를 선택해 이동하기`를 우선 표시하고,
    지도와 HUD 모두 `MAP_`·`LOC_` 내부 ID를 fallback 문구로 노출하지 않는다.
  - 자동 검증 대상은 지도 Prefab 직렬화 계약, 5개 지도 표시명/Base, M.V. 엘리시움 레이어,
    선택 시 상태 불변·pending 목적지 확인 이동, HUD 표시명 fallback 및 Build Preflight다.
    Unity 6000.3.20f1 기준 EditMode 101/101, PlayMode 25/25, Build Preflight와
    3개 해상도 × 14개 화면(42장) 반응형 캡처를 통과했다.
- [x] P-01 메신저가 Daniel에게 부착된 Context임을 목표 문구와 비음성 조사 내레이션으로 명확히 했다.
  - 캐릭터 부착형 비월드 `Context`는 target Character ID, 현재 `CharacterPlacementSet`, HUD용 display name을 갖도록
    Validator와 저작 지침을 보강했다. target이 없는 기존 일반 Context는 이 부착 계약에 포함하지 않는다.
  - 공통 Character Context 배지와 tooltip을 추가하고, 배지는 정확한 `Context` 정의만 실행하며
    Daniel 본체는 정확한 `Character` 정의가 열렸을 때만 클릭되도록 입력을 분리했다.
  - Unity Game View의 16:9 Aspect, Full HD, QHD에서 배지와 tooltip이 화면 안에 유지되는지 확인했다.
- [x] `P-01_018`의 예약 기사·태블릿 경고를 선택 전 필수 복선으로 복구했다.
  - 활성 CSV의 beat를 `warning`으로 바로잡고 초기 `trust_daniel>=3` 조건만 제거했으며,
    line ID·순서·본문·화자·음성 필수 계약은 유지했다.
  - 새 저장의 Trust 2에서도 경고를 들은 뒤 C1은 Trust 3으로 올라 P-02/D1-03 보너스를 열고,
    C2는 Trust 1로 내려 보너스를 숨긴다. 후속 Trust Condition과 선택 GameEffect는 변경하지 않았다.
  - SaveData·SaveVersion 변경은 없다. 이미 P-01 대화를 완료한 개발 저장에는 해당 줄을 소급 재생하지 않는다.
- [x] 필수 Sequence가 `WaitCommand`만 포함하면 실패하도록 Validator를 강화했다.
- [x] `AudioDirector` 아래 Music A/B, Voice Bark, Story Voice, crossfade와 대화 더킹을 연결했다.
- [x] Audio 원본을 Music·Ambience·SFX·VoiceBarks·StoryRecordings 역할로 정리하고,
  Story Recording 16개를 Story Scene별 `REC_` 자산으로 분리했다.
- [x] Content, 대화 선택지, Save/Load, 대표 Puzzle, Transition, 오디오 회귀 테스트를 추가했다.
- [x] 고정 16:9 프레임을 제거하고 타이틀·장소 배경이 화면 비율을 유지하며 뷰포트를 덮도록 했다.
- [x] 13개 PuzzleDefinition을 장면별 월드 Interaction과 PuzzleInteractionAction에 연결했다.
  - 퍼즐 규칙과 장면별 핫스팟 최종 좌표는 별도 미완료 항목이다.

## UI/UX — 이전 프로젝트 이식 현황

### 완료

- [x] 데스크톱 기준 해상도를 1920×1080으로 설정하고 16:9 해상도 프리셋,
  전체화면/창 모드와 설정 화면 적용 기능을 추가했다.
- [x] 타이틀 배경·로고·메뉴 배치, 버튼 hover/press 피드백과 공통 UI 효과음을 이식했다.
- [x] 화면 전환을 `ScreenRouter` → `TransitionDirector` → `TransitionProfile` 단일 경로로 정리했다.
- [x] 타이틀·장소 파티클과 인물 호흡·흔들림·실루엣·바닥 그림자 값을 콘텐츠 프로필로 분리했다.
- [x] 저장 슬롯을 3개 가로 카드로 구성하고 실제 저장 상태의 DAY·장면·이어하기 상태를 표시한다.
- [x] Unity Play Mode에서 Title → Save Slot → Gameplay 실제 버튼 흐름과 16:9 캡처를 검증했다.

### 부분 이식 — 기존 시스템을 확장한다

- [x] 저장 슬롯 삭제와 시작/이어하기 확인 UX를 완성했다.
  - 기존 `ModalRouter`와 `ConfirmDialog`를 확장한 공통 확인 모달을 사용한다.
  - `SaveService`가 기본·백업·임시 저장 파일 삭제를 소유하며 EditMode/Bootstrap PlayMode로 검증했다.
- [x] Persistent HUD를 이전 프로젝트의 목표 중심 HUD로 완성한다.
  - 기존 `PersistentHud`, `InteractionSet`, `GameStateStore`를 사용한다.
  - 내부 수치는 제품 디자인이 승인한 정성 표현으로 바꾸기 전까지 추가 노출하지 않는다.
  - 시간·Location 표시명·현재 Story Scene 표시명을 목표로 렌더하고 시스템 화면에서 숨기는 작업은 완료했다.
  - [x] 장면의 기존 `InteractionSet`과 완료 상태를 목표 단계/guidance 데이터로 해석해 중복 목표 시스템을 만들지 않았다.
  - [x] 목표 진행 변경 시 위로 사라지고 새 guidance가 들어오는 전환을 적용하며 Reduced Motion에서는 즉시 갱신한다.
- [x] Dialogue UI의 화자/내레이션/선택지 모드, 초상화 포커스와 타이포그래피를 완성한다.
  - 기존 `DialogueScreen`, `NarrativeDirector`, `DialogueLine` 데이터를 확장한다.
  - 화자 없는 line의 NARRATION 이름표·중앙 정렬과 내부 Dialogue/Story ID 비노출은 완료했다.
  - 기존 `NarrativeDirector` 알림과 `CharacterStage`를 연결한 화자 포커스/비화자 dim,
    선택지 stagger 등장 연출은 완료했다.
  - [x] `DialogueLine.expression`을 현재 화자의 기존 `CharacterView`에 적용해 장면 데이터가 표정 전환을 소유한다.
  - [x] 짧은 대사는 강조하고 긴 대사·내레이션은 단계적으로 축소하는 반응형 본문 타이포그래피를 적용했다.
  - [x] `CharacterDefinition.Portrait`를 사용하는 별도 화자 초상화 슬롯과 Reduced Motion 대응 등장 연출을 추가했다.
- [ ] Map 화면을 실제 Location/Condition 데이터와 연결한다.
  - 기존 `MapScreen`, `MapDefinition`, `ScreenRouter`를 사용한다.
  - Deck별 Base/Restricted/Technical 레이어, 덱 탭, 현재 위치 표시와 뒤로가기는 이식 완료했다.
  - [x] `MapDefinition`이 덱별 `LocationDefinition`을 참조하고 기존 `MapNodeDefinition` 좌표로 이동 노드를 구성한다.
  - [x] `GameStateStore.unlockedLocations`를 잠금 조건으로 사용하고 현재 Story Scene의 Location을 목표 목적지로 강조한다.
  - [x] pending Story Scene 목적지가 속한 지도를 자동 선택하고, 장소 노드 선택과 실제 이동 확인을 분리했다.
  - [x] `RouteOnly` 노드는 현재 위치 또는 pending 목적지일 때만 표시하며 지도 View의 직접 상태 변경을 제거했다.
  - [x] 공통 4:3 Map Surface와 전용 노드 템플릿·선택 상세 패널을 구성하고 없는 Overlay 토글을 숨겼다.
  - 각 장면의 위치 해금 Effect와 지도 이동 가능 범위 최종 조정은 콘텐츠 완성 단계에 남아 있다.
- [ ] 조사 기록·증거 노트 UI를 완성한다.
  - 기존 EvidenceDefinition/Inventory/Director와 Investigation Record 화면을 사용한다.
  - 발견한 증거만 표시하는 카드 목록과 이미지·명칭·설명 상세 보기는 완료했다.
  - [x] 아직 직접 열지 않은 증거 카드에 `NEW`와 짧은 강조 애니메이션을 표시하며 Reduced Motion에서는 정적으로 표시한다.
  - [x] 이전 canonical category·직접/정황 분류를 18개 `EvidenceDefinition`에 이식하고 전체/직접/정황 필터를 추가했다.
  - 인물 탭은 증거–Character 관계 원본 데이터가 없어 남아 있다.
- [x] Evidence Board의 노드·연결·이론 슬롯 UX를 완성한다.
  - 기존 `EvidenceBoardDirector`, `EvidenceBoardGraph`, `TheoryResolver`를 사용한다.
  - [x] 이전 프로젝트의 6개 canonical 추론 ID·설명·필요 증거 조합을 `TheoryDefinition` 콘텐츠로 이식했다.
  - [x] `EvidenceInventory`를 읽기만 하는 순수 `TheoryResolver` 판정 계층을 복구했다.
  - [x] 기존 `ScreenRouter`와 보드 스텁을 확장해 발견 증거 노드·연결·추론 상태를 표시한다.
    - [x] 조사 기록에서 라우터로 진입하고 18개 증거 노드, 6개 추론 슬롯, 연결 증거 상세와 부족 상태를 표시한다.
    - [x] 증거 노드를 선택해 이론 슬롯에 연결선을 직접 구성하고 정확한 필요 증거 집합일 때만 추론 완료를 허용한다.
  - [x] 추론 완료는 `ResolveTheoryEffect`가 기존 `GameStateStore.flags`의 안정 키로 기록하고 이론별 후속 효과 배열을 적용한다.
  - [x] 보드 화면은 Director에 완료를 요청할 뿐 상태를 직접 변경하지 않으며 완료·논증 가능·부족 상태를 구분한다.
- [ ] Investigation/Puzzle 공통 셸의 열기·닫기·힌트·결과 표시를 이전 UX에 맞춘다.
  - 기존 `PuzzleDirector`와 각 전용 Puzzle controller의 규칙 코드는 재사용한다.
  - [x] `PuzzleScreen` 공통 프레임에 제목·안내·단계형 힌트·취소·결과·복귀 동작을 연결했다.
  - [x] 완료 판정과 `GameEffect` 적용은 계속 `PuzzleDirector`/`PuzzleDefinition`이 소유한다.
  - [ ] 각 전용 컨트롤러의 실제 조작 뷰와 중간 진행 데이터는 퍼즐별 콘텐츠 작업에서 연결한다.

### 미이식 — 후속 순서

- [ ] Reduced Motion과 타이핑 속도 등 접근성 설정을 설정 서비스와 연출 프로필에 연결한다.
  - [x] `AccessibilitySettingsService`가 움직임 줄이기와 4단계 대화 표시 속도를 저장·공급한다.
  - [x] 캐릭터 유휴 움직임, 선택지 등장, 버튼 크기 피드백과 대화 타이핑이 공통 설정을 따른다.
  - [ ] 자막 크기·고대비·색각 보조는 최종 접근성 범위 확정 후 같은 서비스에 추가한다.
- [ ] 게임 커서, 클릭 가능 지점 피드백, alpha/polygon hit 영역을 공통 Interaction View로 이식한다.
  - [x] `InteractionFeedbackService`가 클릭 가능 커서와 hover/click 오디오를 공통 소유한다.
  - [x] 기존 `InteractionPointView`는 피드백을 위임하고 실행 명령은 계속 `InteractionDirector`에 전달한다.
  - [ ] 이미지별 alpha/polygon hit shape는 최종 hotspot 아트가 확정되면 동일 View의 Raycast 계약으로 추가한다.
- [ ] 타이틀 수면/광선, 발견·증거 획득·이론 해금 화면 효과를 Sequence/Transition 데이터로 이식한다.
  - [x] `ScreenRouter`가 화면별 `TransitionProfile` 매핑을 사용하도록 확장했다.
  - [x] 조사·발견/기록·증거 보드·퍼즐·지도·엔딩에 기존 TRANS 자산을 연결했다.
  - [x] Reduced Motion에서는 동일 `TransitionDirector` 경로에서 화면 연출을 생략한다.
  - [ ] 개별 증거 획득·이론 해금 순간의 오버레이와 타이틀 수면/광선은 기존 이벤트·Sequence 명령에 연결한다.
    - [x] 모든 `GameStateStore.AddEvidence` 성공 알림을 `EvidenceDirector`가 콘텐츠 정의로 변환한다.
    - [x] `TRANS_DISCOVERY` 타이밍을 사용하는 공통 증거 카드 오버레이를 Game UI 셸에 연결했다.
    - [x] 새 증거로 `TheoryResolver` 조건이 처음 충족되면 같은 오버레이가 이론 논증 가능 상태를 표시한다.
    - [x] 기존 `AmbientParticleProfile`에 선택적 광선·수면 레이어 값을 추가하고 타이틀 프로필에만 적용했다.
    - [x] 타이틀 광선 drift와 수면 shimmer는 Reduced Motion에서 정적인 저강도 표현으로 바뀐다.
- [x] 지도·증거·퍼즐·모달 화면을 포함한 1280×720, 1920×1080, 2560×1440 시각 회귀 캡처를 구축한다.
  - [x] 라우팅되는 14개 화면 프리팹을 임시 씬의 1920×1080 기준 Canvas에 올려 실제 RenderTexture로 캡처한다.
  - [x] 세 해상도에서 타이틀·저장 슬롯·기록·증거 보드·퍼즐·설정 화면의 잘림과 비율을 직접 비교했다.
  - [x] 캡처 대상 화면과 16:9 검증 해상도가 빠지지 않도록 EditMode 회귀 테스트를 추가했다.

> UI/UX 작업 시 이 목록을 매 기능 커밋마다 갱신한다. 기존 Runtime/Content/Prefab이 있는 기능은
> 새 프레임워크를 만들지 않고 해당 시스템을 확장한다.

## P1 — 플레이 가능한 콘텐츠 완성

- [ ] P-01과 D1-06을 제외한 39개 Story Scene의 일반 대화 Interaction을 실제 NPC·맥거핀·조사·증거·출구·퍼즐
  Interaction으로 교체한다.
  - 완료 조건: 각 장면의 원본 행동과 `requiredInteractionTypes`, 최소 상호작용 수,
    Condition/GameEffect가 일치한다.
  - P-02의 첫 Daniel `Character` 대화와 직접 PlayMode 진입 검증은 완료했다. P-02의 나머지 원본 행동,
    장면 완료 조건과 P-03으로 이어지는 완주 흐름은 아직 남아 있다.
- [ ] 41개 CharacterPlacementSet을 실제 플레이 화면에서 시각 검수하고 좌표·스케일·sorting order를
  최종 조정한다.
- [ ] 모든 Story Scene을 원본의 전용 Location State와 배경·오디오에 연결한다.
  - 현재 다수 장면은 생성된 기본 State를 공유하므로 구조적 유효성과 장면별 시각 재현을 구분한다.
- [ ] D4-02, D5-01, D7-01, D8-02, D8-03 Sequence를 실제 연출 명령으로 작성한다.
  - 현재 Audio + Wait는 placeholder 탈출을 위한 최소 구성일 뿐 완성 시네마틱이 아니다.
- [ ] 13개 PuzzleDefinition에 입력 항목, 정답·순서, 오답 판정, 힌트 단계와 저장할 중간 진행을
  데이터로 작성하고 실제 Puzzle Interaction에 연결한다.
  - Puzzle Interaction 진입 연결은 완료했다. 규칙 데이터와 최종 핫스팟 좌표가 남았다.
  - [x] D2-02 혈흔 배열과 D6-02 화물 레일의 허용 입력·정답·증거 gate·3단계 힌트를
    `PuzzleDefinition.rules`로 이식하고 기존 전용 컨트롤러가 해당 콘텐츠를 판정하도록 연결했다.
    - 2026-08-12 병합 감사에서 미이식 퍼즐의 빈 직렬화 객체를 authored rule로 오인하던 validator를
      수정했고, 기존 11개 퍼즐의 하위 호환을 유지한 상태로 Build Preflight를 다시 통과했다.
  - [x] D2-04 CCTV 로그의 CCTV 검토·출입문 로그·설비 로그 중첩·22:18 감지기 오류·위치 확인
    5단계 규칙과 3단계 힌트를 이식하고 허용되지 않은 관찰 ID를 거부하도록 연결했다.
  - [x] D6-05 참 타임라인의 원본 근거가 있는 12개 카드 순서와 3단계 힌트를 이식하고,
    `puzzleProgress`에 현재 배열을 저장하며 순서가 정확할 때만 완료하도록 연결했다.
  - [ ] 나머지 9개 퍼즐 규칙과 퍼즐별 조작 View·중간 진행 복원을 같은 계약으로 이식한다.
- [x] D8-01 최종 심문의 6단계 논증과 A/B/C/Bad 엔딩 판정·라우팅을 현재 Condition/GameEffect와
  `GameStateStore.endingId`로 이식했다.
  - 기존 대화의 6단계 선택 플래그와 이론 해금 상태를 재사용하며 별도 판정 UI/상태 관리자를 만들지 않았다.
  - Bad → A → B → C 우선순위, 최초 엔딩 확정, A/B → D8-02 및 C/Bad → D8-03 라우트를 콘텐츠로 구성했다.
- [ ] Story Recording 파일과 Dialogue line ID의 승인된 대응표를 AudioCue/Sequence에 연결한다.
- [ ] 클릭·호버·확인·취소·증거 발견·이론 해금 공통 효과음을 선정하고 라이선스를 기록한다.
- [ ] 최종 게임 로고와 앱 아이콘 승인본을 적용한다.

## P1 — 검증 및 제작 도구

- [ ] P-01부터 모든 엔딩까지 도달 가능한지 검사하는 Story graph 회귀 검증을 추가한다.
  - D8-01의 네 엔딩별 라우트 단위 검증은 완료했다. P-01부터의 전체 조건 충족 경로 탐색이 남았다.
- [ ] 13개 퍼즐 각각에 대해 정답 → `PuzzleResult` → `GameEffect` → Save/Load 회귀 테스트를 추가한다.
- [ ] Puzzle 직접 미리보기와 공통 Preview 계약을 구현한다.
- [ ] 필요한 Addressables 등록·레이블과 깨진 직렬화 참조 검증을 추가한다.
- [x] Unity Editor에서 현재 EditMode/PlayMode 전체 suite와 Bootstrap 자동 대표 흐름을 통과시킨다.
  - 2026-08-14 `codex/p01-map-travel` 증분은 Unity `6000.3.20f1`에서 EditMode 103/103,
    PlayMode 25/25와 Build Preflight를 실패·건너뜀 없이 통과했다.
    대표 PlayMode는 실제 EventSystem 클릭으로 새 Slot 3 → 초대장 → Daniel 메신저 배지 → Daniel 대화·선택 →
    Port/P-01 pending 체크포인트 재시작 → HUD 지도 → M.V. 엘리시움/Gangway 노드 → 이동 확인 →
    P-02 체크포인트 재시작 → Daniel 본체 클릭 대화를 검증한다.
  - 2026-08-14 `codex/p01-playable-to-p02` 증분을 Unity `6000.3.20f1`에서 검증했다.
    EditMode 91/91, PlayMode 17/17과 Build Preflight가 실패·건너뜀 없이 통과했다. PlayMode는 실제
    EventSystem 클릭으로 새 Slot 3 → P-01 초대장 → Daniel 부착 메신저 배지 → Daniel 본체 대화·선택 →
    Trust 2에서 예약 기사·태블릿 경고 확인 → 출구 → P-02 Trust 보너스 대사 → 앱 재시작 후
    같은 Slot 3의 P-02 복원을 확인하며, reduced-motion 전환의 입력 해제와 체크포인트 저장 실패 격리도 포함한다.
  - 2026-08-12 기준 커밋 `b53a09f1416cb2a3ad838d0d4ac9d7eef4d810c6`, Unity `6000.3.20f1`
    (`c9ba695d4f07`)에서 Build Preflight, EditMode 64/64, PlayMode 11/11을 통과했다. 실패·건너뜀은 0건이다.
  - `BootstrapTests.BootstrapLoadsPersistentGameShell`이 런타임 UI 버튼 이벤트와 `ScreenRouter` 경로로
    Bootstrap → Title → Save Slot 3 → Dialogue → Exploration → Map → Exploration → Record →
    Exploration → 캐릭터 상호작용을 통과했다.
  - 2026-08-12 `745363f` 기반 변경, Unity `6000.3.20f1`에서 Bootstrap 테스트마다 OS 임시 경로의
    고유 디렉터리를 `SaveService`에 주입해 실제 사용자 게임 저장 폴더
    `Application.persistentDataPath/Saves`와 겹치지 않는 빈 Slot 3에서 시작하도록 했다. 동일 Unity
    프로세스 2회 연속 실행과 PlayMode 11/11, EditMode 64/64, Build Preflight를 통과했고
    실패·건너뜀·불확정 결과와 남은 테스트별 고유 임시 Save 디렉터리는 0건이다.
  - 대화형 Editor에서는 Bootstrap → Title → Save Slot 3 → Dialogue까지 화면 표시를 직접 확인했다.
    Map과 Record의 자동 PlayMode 경로는 통과했지만 별도 수동 시각 확인은 완료하지 않았다.
  - 새 아키텍처 scaffold 단계에서 생성되어 P2로 유보된 설정용 0바이트 placeholder 8개가 아직
    유효한 Unity 자산으로 저작·연결되지 않아 cold import 오류가 발생한다. 최신 기능 변경의 회귀는
    아니지만 현재 Build Preflight와 테스트 범위 밖이므로 아래 P2 작업에서 별도로 해결한다.
- [ ] P-01부터 모든 엔딩까지 실제 대표 플레이스루를 완료한다.
  - Story graph 도달 가능성 자동 검사와 별도로 장면별 Interaction·Puzzle·연출·저장·엔딩 흐름을 확인한다.

## P2 — 출시 설정과 품질

- [ ] P2로 유보한 설정용 0바이트 placeholder 8개를 현재 아키텍처에 맞는 유효한 Unity
  `6000.3.20f1` 자산으로 저작·교체·연결한다.
  - Input 1개, Rendering 3개, Audio 3개, Addressables settings 1개를 대상으로 한다.
  - 과거의 유효한 본체는 Git 이력에 없으므로 원본을 단순 복원하지 않고 현재 owner와 참조 계약을 따른다.
  - 사용하지 않는 orphan placeholder는 serialized reference와 GUID 영향을 확인한 뒤 제거한다.
  - Rendering은 `GraphicsSettings`·`QualitySettings`의 누락된 active URP 참조와 URP Global Settings를
    Renderer Data·URP Pipeline·Global Volume과 함께 정상화한다.
- [ ] Audio Mixer 그룹·스냅샷과 더킹 attack/release 및 버스별 최종 볼륨을 실제 음원으로 튜닝한다.
- [ ] Addressables 패키지와 settings를 구성하고 대형 배경·캐릭터·증거·BGM·환경음·녹음을
  그룹과 레이블로 구성한다.
- [ ] 프로젝트 전용 Input Actions 사용 범위와 정상화한 Rendering 설정을 타깃 플랫폼에서 검증한다.
- [ ] Library가 없는 clean checkout에서 cold import 설정 오류 0건을 확인하고 타깃 Player build를 검증한다.
- [ ] 앱 이름, 회사명, 아이콘, 해상도, 품질과 플랫폼별 Player Settings를 확정한다.
- [ ] CI에서 Unity Test Runner와 Build Preflight를 push/PR마다 실행한다.
- [ ] `Docs/QA/ReleaseChecklist.md`를 모두 확인한다.

## 외부 결정·데이터가 필요한 항목

세부 목록과 원문에서 복구 가능한 범위는 `Docs/Production/CONTENT_DATA_GAPS.md`를 기준으로 한다.

- 퍼즐별 최종 UX, 허용 오차, 재시도·힌트 정책과 중간 진행 저장 범위
- Story Recording ↔ Dialogue line ID 최종 대응표
- UI 공통 효과음의 음원·라이선스
- 로고·앱 아이콘 아트 디렉션과 배포 플랫폼 규격
- CI 실행 환경, Unity 라이선스 방식과 대상 빌드 플랫폼
- Audio Mixer의 승인된 목표 음량·더킹 값

## 작업 규칙

- 완료 항목은 근거 파일이나 테스트를 확인한 뒤 체크한다.
- 원본에 없는 서사 사실이나 정답을 임의로 만들지 않는다.
- 장면 차이는 Content 데이터로 표현하고 Story Scene 전용 컨트롤러를 만들지 않는다.
- 아키텍처 변경이 필요하면 `AGENTS.md` 변경 절차에 따라 승인받는다.
- 기능 단위로 커밋하고 검증한다. 원격 공개 저장소 push는 사용자의 명시적 승인 후 수행한다.
