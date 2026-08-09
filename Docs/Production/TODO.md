# 통합 제작 TODO

> 이 문서는 프로젝트의 유일한 TODO 목록이다. 자산 폴더 안에 TODO 파일을 다시 만들지 않는다.  
> 갱신일: 2026-08-09 (`d40b061` 이후 로컬 `main` 기준)

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
- [x] Content, 대화 선택지, Save/Load, 대표 Puzzle, Transition, 오디오 회귀 테스트를 추가했다.

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
  - 현재 배치 실행은 Unity Licensing Client IPC 실패로 완료하지 못했으며 C# 프로젝트 컴파일은 통과했다.

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
