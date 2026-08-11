# 통합 제작 TODO

> 이 문서는 프로젝트의 유일한 TODO 목록이다. 자산 폴더 안에 TODO 파일을 다시 만들지 않는다.  
> 갱신일: 2026-08-11 (`772736e` 이후 로컬 `main` 기준)

## 완료

- [x] 원본 저장소의 비생성 파일과 최신 버전을 비교하고 필요한 코드·미디어를 이식했다.
- [x] 41개 `StorySceneDefinition`과 `C-01`~`C-18`의 canonical ID를 구성했다.
- [x] 0바이트 ScriptableObject와 Prefab을 현재 스키마로 복구하고 빈 중복 자산을 제거했다.
- [x] `Bootstrap.unity`를 composition root로, `Game.unity`를 지속 런타임 셸로 연결했다.
- [x] Title → Save Slot → Game 흐름과 화면 라우팅을 연결했다.
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
- [ ] Dialogue UI의 화자/내레이션/선택지 모드, 초상화 포커스와 타이포그래피를 완성한다.
  - 기존 `DialogueScreen`, `NarrativeDirector`, `DialogueLine` 데이터를 확장한다.
  - 화자 없는 line의 NARRATION 이름표·중앙 정렬과 내부 Dialogue/Story ID 비노출은 완료했다.
  - 기존 `NarrativeDirector` 알림과 `CharacterStage`를 연결한 화자 포커스/비화자 dim,
    선택지 stagger 등장 연출은 완료했다.
  - [x] `DialogueLine.expression`을 현재 화자의 기존 `CharacterView`에 적용해 장면 데이터가 표정 전환을 소유한다.
  - [x] 짧은 대사는 강조하고 긴 대사·내레이션은 단계적으로 축소하는 반응형 본문 타이포그래피를 적용했다.
  - 별도 대화 초상화 슬롯은 남아 있다.
- [ ] Map 화면을 실제 Location/Condition 데이터와 연결한다.
  - 기존 `MapScreen`, `MapDefinition`, `ScreenRouter`를 사용한다.
  - Deck별 Base/Restricted/Technical 레이어, 덱 탭, 현재 위치 표시와 뒤로가기는 이식 완료했다.
  - [x] `MapDefinition`이 덱별 `LocationDefinition`을 참조하고 기존 `MapNodeDefinition` 좌표로 이동 노드를 구성한다.
  - [x] `GameStateStore.unlockedLocations`를 잠금 조건으로 사용하고 현재 Story Scene의 Location을 목표 목적지로 강조한다.
  - 각 장면의 위치 해금 Effect와 지도 이동 가능 범위 최종 조정은 콘텐츠 완성 단계에 남아 있다.
- [ ] 조사 기록·증거 노트 UI를 완성한다.
  - 기존 EvidenceDefinition/Inventory/Director와 Investigation Record 화면을 사용한다.
  - 발견한 증거만 표시하는 카드 목록과 이미지·명칭·설명 상세 보기는 완료했다.
  - 인물 탭, 증거 분류/필터와 새 증거 획득 표시 애니메이션은 남아 있다.
- [ ] Evidence Board의 노드·연결·이론 슬롯 UX를 완성한다.
  - 기존 `EvidenceBoardDirector`, `EvidenceBoardGraph`, `TheoryResolver`를 사용한다.
  - [x] 이전 프로젝트의 6개 canonical 추론 ID·설명·필요 증거 조합을 `TheoryDefinition` 콘텐츠로 이식했다.
  - [x] `EvidenceInventory`를 읽기만 하는 순수 `TheoryResolver` 판정 계층을 복구했다.
  - [ ] 기존 `ScreenRouter`와 보드 스텁을 확장해 발견 증거 노드·연결·추론 상태를 표시한다.
    - [x] 조사 기록에서 라우터로 진입하고 18개 증거 노드, 6개 추론 슬롯, 연결 증거 상세와 부족 상태를 표시한다.
    - [ ] 실제 선을 직접 배치·편집하는 상호작용과 추론 완료 효과는 후속 퍼즐 규칙 이식과 함께 연결한다.
  - [ ] 추론 완료의 논리 상태와 효과 적용은 기존 `GameStateStore`/`GameEffect` 계약에 맞춰 연결한다.
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

- [ ] 나머지 40개 Story Scene의 일반 대화 Interaction을 실제 NPC·맥거핀·조사·증거·출구·퍼즐
  Interaction으로 교체한다.
  - 완료 조건: 각 장면의 원본 행동과 `requiredInteractionTypes`, 최소 상호작용 수,
    Condition/GameEffect가 일치한다.
- [ ] 41개 CharacterPlacementSet을 실제 플레이 화면에서 시각 검수하고 좌표·스케일·sorting order를
  최종 조정한다.
- [ ] 모든 Story Scene을 원본의 전용 Location State와 배경·오디오에 연결한다.
  - 현재 다수 장면은 생성된 기본 State를 공유하므로 구조적 유효성과 장면별 시각 재현을 구분한다.
- [ ] D4-02, D5-01, D7-01, D8-02, D8-03 Sequence를 실제 연출 명령으로 작성한다.
  - 현재 Audio + Wait는 placeholder 탈출을 위한 최소 구성일 뿐 완성 시네마틱이 아니다.
- [ ] 13개 PuzzleDefinition에 입력 항목, 정답·순서, 오답 판정, 힌트 단계와 저장할 중간 진행을
  데이터로 작성하고 실제 Puzzle Interaction에 연결한다.
  - Puzzle Interaction 진입 연결은 완료했다. 규칙 데이터와 최종 핫스팟 좌표가 남았다.
- [ ] D8-01 최종 심문의 6단계 논증과 A/B/C/Bad 엔딩 판정·라우팅을 현재 Condition/GameEffect와
  `GameStateStore.endingId`로 이식한다.
- [ ] Story Recording 파일과 Dialogue line ID의 승인된 대응표를 AudioCue/Sequence에 연결한다.
- [ ] 클릭·호버·확인·취소·증거 발견·이론 해금 공통 효과음을 선정하고 라이선스를 기록한다.
- [ ] 최종 게임 로고와 앱 아이콘 승인본을 적용한다.

## P1 — 검증 및 제작 도구

- [ ] P-01부터 모든 엔딩까지 도달 가능한지 검사하는 Story graph 회귀 검증을 추가한다.
  - D8-01 분기 규칙이 아직 이식되지 않아 현재 D8-02가 그래프에서 도달 불가능하다.
- [ ] 13개 퍼즐 각각에 대해 정답 → `PuzzleResult` → `GameEffect` → Save/Load 회귀 테스트를 추가한다.
- [ ] Puzzle 직접 미리보기와 공통 Preview 계약을 구현한다.
- [ ] 필요한 Addressables 등록·레이블과 깨진 직렬화 참조 검증을 추가한다.
- [ ] Unity Editor에서 EditMode/PlayMode 전체 테스트와 Bootstrap 대표 플레이스루를 통과시킨다.
  - 2026-08-11 기준 EditMode 28/28, Bootstrap PlayMode 1/1과 실제 화면 캡처를 통과했다.
  - 전체 PlayMode 모음과 전 Story Scene 대표 플레이스루는 계속 확장한다.

## P2 — 출시 설정과 품질

- [ ] Audio Mixer 그룹·스냅샷과 더킹 attack/release 및 버스별 최종 볼륨을 실제 음원으로 튜닝한다.
- [ ] 대형 배경·캐릭터·증거·BGM·환경음·녹음을 Addressables 그룹과 레이블로 구성한다.
- [ ] Input Actions, Render Pipeline, Renderer와 Global Volume을 타깃 플랫폼에서 검증한다.
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
