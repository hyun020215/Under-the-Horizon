# 세이브 시스템

## 저장 대상

- 현재 Story Scene과 Location, 날짜와 시간대
- 완료된 Story Scene에서 지도 이동을 기다리는 대상 Story Scene ID
- 플래그, 신뢰도, 불안도, 증거 무결성
- 발견 증거, 완료 상호작용·퍼즐·목표
- 선택 기록, 이론, 장소 해금과 엔딩 상태

## 저장하지 않는 대상

- AudioSource 재생 위치
- 트윈·전환 진행률
- UI Transform과 임시 모달
- 생성된 CharacterView 같은 GameObject 참조

## 동작

`SaveData`는 `GameState`의 직렬화 DTO다. `SaveService`는 임시 파일에 쓴 뒤 원본을 교체하고, 기존 파일을 백업한다. 불러오기 후에는 상태와 콘텐츠 정의로 프레젠테이션을 다시 구성한다.

### 활성 슬롯과 Story Scene 체크포인트

`GameStartup`은 Save Slot 화면에서 사용자가 선택한 `SaveSlot`을 Load/New 분기 전에 `SaveCheckpoint.Bind`으로 전달한다. `SaveCheckpoint`는 슬롯이 바인딩되지 않았거나 서비스 레지스트리에 `SaveService`가 없으면 저장하지 않으며, 임의의 기본 슬롯이나 별도 `SaveService`를 만들지 않는다.

`SaveCheckpoint`는 `StorySceneDirector.Entered`와 `GameFlowController.ProgressCheckpointReached`를 구독한다. `Entered`는 Story Scene의 논리 컨텍스트·Location·Interaction·화면·On Enter Effect와 진입 Transition이 적용된 뒤, entry Sequence와 자동 entry Dialogue가 시작되기 전에 발생한다. `ProgressCheckpointReached`는 현재 Story Scene 완료 후 다음 Story Scene을 지도 이동 대상으로 확정한 안정 경계에서 발생한다. 체크포인트는 두 시점의 논리 상태를 활성 슬롯에 저장하며 UI·전환·대사 재생 위치는 저장하지 않는다.

### 지도 이동 대기 상태와 복원

`pendingStorySceneId`는 임시 UI 선택이 아니라 Story Scene 완료와 다음 Story Scene 진입 사이를 나타내는 논리 상태다. 지도 이동 route가 확정되면 현재 Story Scene과 Location은 출발지에 그대로 두고, 완료 기록과 `pendingStorySceneId`를 함께 저장한다. 지도 노드 선택 자체는 이 값을 바꾸지 않으며, `GameFlowController.TravelAsync`가 정확한 목적지 Location과 진입 조건을 다시 검증한 뒤에만 대상 Story Scene으로 진입하고 pending 값을 비운다.

기존 저장을 불러오면 `GameStartup`은 새 게임용 `StartAsync` 대신 `ResumeAsync`를 호출한다. pending 값이 있으면 진입 Effect·Sequence·Dialogue를 다시 실행하지 않고 출발 Story Scene의 Location·Character·Interaction·Audio·Exploration 화면만 재구성한다. 이전 저장처럼 현재 Story Scene은 완료됐지만 pending 값이 비어 있는 경우에도 route 데이터가 `MapTravel`이면 같은 대기 상태를 일반 규칙으로 복구한다.

체크포인트 파일 쓰기가 실패하면 예외를 로그로 남기되 Story Scene 진입 또는 플로우 진행 알림 밖으로 전파하지 않는다. 저장 장치 오류가 이미 적용된 Story Scene 진입 명령이나 이후 entry Sequence·Dialogue, 지도 이동 대기 상태를 중단시키지 않도록 하기 위함이다.

### Trust 기본값과 기존 저장 호환성

Trust 항목이 상태에 등록되지 않았으면 `GameStateStore`는 기본값 2로 해석하고 최초 증감도 2에서 시작한다. 저장 파일에 명시된 값은 0을 포함해 그대로 읽고 이후 증감의 기준으로 사용한다. 기본값은 런타임 조회 규칙이며 누락된 Trust 항목을 저장 데이터에 임의로 추가하지 않는다.

## 버전과 마이그레이션

저장 스키마를 바꿀 때는 `SaveVersion`을 올리고 이전 버전에서 새 버전으로 가는 마이그레이션을 `SaveMigrationRegistry`에 등록한다. 필드 삭제·의미 변경은 기본값만 추가하는 변경과 달리 별도 호환성 검증이 필요하다. 절차와 이력은 `Docs/QA/SaveMigration.md`에 기록한다.

지도 이동 대기 상태를 저장하기 위해 `SaveData.pendingStorySceneId`를 추가하고 `SaveVersion.Current`를 2로 올렸다. 내장 `SaveMigrationV1ToV2`는 v1 저장에 빈 pending 값을 보충하므로 기존 즉시 진입 저장의 의미는 유지된다. v2보다 높은 버전은 알 수 없는 필드를 손실한 채 덮어쓰지 않도록 명시적으로 거부한다.
