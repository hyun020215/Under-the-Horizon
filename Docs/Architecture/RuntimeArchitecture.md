# 런타임 아키텍처

## 구성

`Bootstrap.unity`가 애플리케이션 서비스를 초기화하고 `Game.unity`의 영속 셸을 연다. 이후 `GameFlowController`와 `StorySceneDirector`가 콘텐츠 정의를 전문 Director에 전달한다.

## 책임 경계

| 구성 요소 | 책임 |
|---|---|
| `GameStateStore` | 변경 가능한 논리 상태의 유일한 소유자 |
| `StorySceneDirector` | Story Scene 진입·완료 조율 |
| `LocationPresenter` | 장소와 장소 상태 표현 |
| `CharacterStage` | 인물 View 생성과 배치 |
| `InteractionDirector` | 상호작용 가용성·실행 조율 |
| `NarrativeDirector` | 대화 진행과 결과 반환 |
| `AudioDirector` | 오디오 상태와 라우팅 |
| `ScreenRouter` | Screen 전환 |
| `TransitionDirector` | 시각 전환 |
| `SequenceDirector` | 순서 있는 연출 명령 실행 |
| `PuzzleDirector` | 퍼즐 열기·종료·결과 전달 |
| `SaveService` | 논리 상태 영속화 |

## 명령과 이벤트

의도적인 단일 작업은 직접 서비스 호출을 사용한다. 대화 시작처럼 오디오 더킹, UI, 텔레메트리가 함께 알아야 하는 사실은 이벤트로 알린다. 이벤트를 명령 흐름의 대체물로 사용하지 않는다.

## 금지 사항

- 저수준 View에서 Story Scene 진행
- UI에서 저장 파일 직접 쓰기
- 퍼즐에서 장면 ID 분기
- 두 번째 상태·UI·오디오·흐름 관리자
- 프레젠테이션 상태를 권위 있는 게임 상태로 사용
