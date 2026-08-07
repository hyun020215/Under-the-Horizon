# 콘텐츠 아키텍처

## 기본 모델

런타임 코드는 동작을 정의하고 `Content/`의 ScriptableObject 인스턴스가 실제 게임 내용을 정의한다. Story Scene은 Unity Scene이 아니라 `StorySceneDefinition`이다.

## Story Scene 링크

각 Story Scene은 안정적인 ID, 장소와 상태, 초기 화면, 인물 배치, 상호작용, 대화, 선택적 퍼즐, 오디오, Sequence, Transition, 효과와 다음 경로를 참조한다. 장면별 차이는 이 데이터에 두며 공유 코드에서 ID를 분기하지 않는다.

## 안정적인 계약

- Story Scene ID: `P-01`~`D8-03`
- 증거 ID: `C-01`~`C-18`
- Location·Character·Puzzle ID
- 저장 필드와 ScriptableObject 참조

ID나 직렬화 필드를 바꾸려면 참조·저장 데이터 마이그레이션 계획이 필요하다.

## 저작 흐름

1. 기존 정의·프로필로 표현 가능한지 확인한다.
2. 새 인스턴스라면 `Content/`에 데이터를 만든다.
3. 새 재사용 동작일 때만 `Runtime/`을 확장한다.
4. 반복 Inspector 작업은 `Editor/` 도구로 자동화한다.
5. 콘텐츠 검증과 진행 경로 검증을 통과시킨다.

## 현재 마이그레이션 상태

원본 미디어와 CSV는 활성 폴더에 배치되어 있다. 0바이트 `.asset` 및 Prefab 파일은 유효한 Unity 직렬화 자산이 아니라 자리표시자이므로, 단일 TODO 문서에 남은 저작 작업으로 추적한다. 이를 완성된 콘텐츠로 간주하거나 검증기에서 예외 처리하지 않는다.
