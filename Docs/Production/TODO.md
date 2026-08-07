# 통합 제작 TODO

> 이 문서는 프로젝트의 유일한 TODO 목록이다. 자산 폴더 안에 TODO 파일을 다시 만들지 않는다.  
> 갱신일: 2026-08-08

## 완료된 원본 반영

- [x] 최신 원본 두 경로의 Git HEAD와 2,466개 비생성 파일 SHA-256 비교
- [x] 장소 배경 59종과 Horizon 주요 상태 배경 반영
- [x] 핵심 증거 `C-01`~`C-18` 이미지 반영
- [x] Deck 06~10 및 Port 지도 레이어 반영
- [x] 주연·환경·전문직 NPC 이미지 반영
- [x] BGM 8종, 환경·사건 효과음 21종, 인물별 Voice Bark와 Story Recording 반영
- [x] 혈흔 방향 및 CCTV 퍼즐 원본 이미지 반영
- [x] 시나리오, 제작 매뉴얼, 전체 대화, 오디오 큐, 크루즈 구조도 원본을 `Docs/Narrative/Source/`에 반영
- [x] 대화·선택·장면 인덱스·증거 CSV 원본 반영
- [x] Unity 패키지와 핵심 ProjectSettings 기준선 반영

기존 TODO에 적힌 예시 파일명과 실제 원본 이름이 다른 경우 새 파일을 복제하지 않고 실제 원본을 유지한다. 예를 들어 Horizon 상태는 `BG_horizon_d1_discovery.png`, `BG_horizon_cleared_day.png`, `BG_horizon_d8_finale.png`로 존재하며, 음악은 실제 곡명 기반 `MUS_*.mp3`를 사용한다. 동일 미디어를 TODO 예시 이름으로 복제하면 GUID와 콘텐츠 참조만 늘어나므로 금지한다.

## P0 — Unity에서 먼저 복구할 항목

- [ ] 0바이트 `.asset` 212개를 실제 ScriptableObject로 저작한다.
  - Story Scene 41개
  - Location과 Location State
  - CharacterDefinition과 CharacterPlacementSet
  - InteractionSet
  - DialogueSequence와 데이터베이스
  - EvidenceDefinition `C-01`~`C-18`
  - PuzzleDefinition, AudioCueProfile, TransitionProfile, Sequence
  - `GAME_` 및 `DATABASE_` 루트 자산
- [ ] 0바이트 Prefab을 실제 GameObject 계층과 현재 스크립트 참조로 다시 만든다.
  - App, Character, Interaction, Location, UI, FX, Puzzle Prefab
- [ ] `Bootstrap.unity`에서 App 서비스와 `Game.unity` 로드를 연결한다.
- [ ] `Game.unity`에서 WorldCanvas, UICanvas, Directors, EventSystem을 실제 Prefab·컴포넌트에 연결한다.
- [ ] `ContentDatabase`에 41개 장면과 장소·증거 데이터베이스를 등록한다.

0바이트 자리표시자는 완성 자산으로 계산하지 않는다. 이전 스크립트 GUID를 가진 레거시 ScriptableObject를 그대로 활성화하지 말고 현재 스키마로 변환한다.

## P1 — 콘텐츠 완성

- [ ] 41개 Story Scene별 CharacterPlacementSet을 제작하고 정규화 좌표를 검수한다.
- [ ] 41개 Story Scene별 InteractionSet과 Condition/GameEffect를 제작한다.
- [ ] 모든 Location State에 배경·오디오·선택적 효과를 연결한다.
- [ ] 전체 대화 XLSX/CSV를 DialogueSequence로 가져오고 선택지·효과·다음 대사를 검증한다.
- [ ] Story Recording을 `Audio/StoryRecordings/` 역할로 정리하고 AudioCueProfile에 연결한다.
- [ ] 실제 원본에 없는 UI 공통 효과음(클릭·뒤로·확인·탭, 증거 발견·이론 해금)을 제작 또는 라이선스 확보한다.
- [ ] 게임 로고와 앱 아이콘의 최종 승인본을 제작한다. 현재 `UI_logo_transparent.png`는 임시 UI 로고로만 취급한다.

## P1 — Editor 및 Validator

- [ ] StoryScene·Location·CharacterPlacement·Interaction·AudioCue·Evidence·Sequence 편집 창을 구현한다.
- [ ] Story Scene·Location·Puzzle 직접 미리보기를 구현한다.
- [ ] 대화·오디오·증거 가져오기 도구를 실제 원본 형식에 연결한다.
- [ ] 다음 검증을 완성한다.
  - 중복 ID
  - 깨진 장면 경로와 진행 불가능 경로
  - Location·State·Character·Dialogue·Evidence·Puzzle 누락
  - Audio·Transition·Sequence 누락
  - 깨진 직렬화 참조
  - 필요한 Addressables 등록·레이블 누락

## P2 — 설정과 품질

- [ ] Audio Mixer 그룹, 스냅샷과 더킹 곡선을 최종 음원으로 튜닝한다.
- [ ] Input Actions, Render Pipeline, Renderer, Global Volume을 타깃 플랫폼에서 검증한다.
- [ ] 앱 이름, 회사명, 아이콘, 해상도, 품질, 플랫폼별 Player Settings를 확정한다.
- [ ] 대형 배경·캐릭터·증거·BGM·환경음·녹음을 Addressables 그룹과 레이블로 구성한다.
- [ ] EditMode 콘텐츠 검증과 PlayMode 대표 진행·저장·퍼즐 테스트를 통과시킨다.
- [ ] `Docs/QA/ReleaseChecklist.md`를 모두 확인한다.

## 작업 규칙

- 완료 항목은 근거 파일이나 테스트를 확인한 뒤 체크한다.
- 새 할 일은 우선순위와 완료 조건을 함께 적는다.
- 아키텍처 변경이 필요하면 구현 전에 `AGENTS.md`의 변경 절차를 따른다.
- 기능 단위로 커밋하고 검증 후 원격 `main`에 푸시한다.
