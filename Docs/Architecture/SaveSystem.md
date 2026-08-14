# 세이브 시스템

## 저장 대상

- 현재 Story Scene과 Location, 날짜와 시간대
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

`SaveCheckpoint`는 `StorySceneDirector.Entered`를 구독한다. 이 이벤트는 Story Scene의 논리 컨텍스트·Location·Interaction·화면·On Enter Effect와 진입 Transition이 적용된 뒤, entry Sequence와 자동 entry Dialogue가 시작되기 전에 발생한다. 체크포인트는 이 시점의 논리 상태를 활성 슬롯에 저장하며 UI·전환·대사 재생 위치는 저장하지 않는다.

체크포인트 파일 쓰기가 실패하면 예외를 로그로 남기되 `StorySceneDirector.Entered` 알림 밖으로 전파하지 않는다. 저장 장치 오류가 이미 적용된 Story Scene 진입 명령이나 이후 entry Sequence·Dialogue를 중단시키지 않도록 하기 위함이다.

### Trust 기본값과 기존 저장 호환성

Trust 항목이 상태에 등록되지 않았으면 `GameStateStore`는 기본값 2로 해석하고 최초 증감도 2에서 시작한다. 저장 파일에 명시된 값은 0을 포함해 그대로 읽고 이후 증감의 기준으로 사용한다. 기본값은 런타임 조회 규칙이며 누락된 Trust 항목을 저장 데이터에 임의로 추가하지 않는다.

## 버전과 마이그레이션

저장 스키마를 바꿀 때는 `SaveVersion`을 올리고 이전 버전에서 새 버전으로 가는 마이그레이션을 `SaveMigrationRegistry`에 등록한다. 필드 삭제·의미 변경은 기본값만 추가하는 변경과 달리 별도 호환성 검증이 필요하다. 절차와 이력은 `Docs/QA/SaveMigration.md`에 기록한다.

활성 슬롯 바인딩, Story Scene 진입 체크포인트와 미등록 Trust 기본값 2 적용은 `SaveData` 필드나 직렬화 형식을 바꾸지 않는다. `SaveVersion.Current`는 1로 유지하며 이 증분을 위한 마이그레이션은 없다.
