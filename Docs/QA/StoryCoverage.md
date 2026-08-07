# 스토리 콘텐츠 커버리지

정규 Story Scene은 총 41개다.

| 구간 | 장면 수 | ID 범위 |
|---|---:|---|
| Prologue | 3 | P-01~P-03 |
| Day 1 | 7 | D1-01~D1-07 |
| Day 2 | 6 | D2-01~D2-06 |
| Day 3 | 5 | D3-01~D3-05 |
| Day 4 | 4 | D4-01~D4-04 |
| Day 5 | 4 | D5-01~D5-04 |
| Day 6 | 5 | D6-01~D6-05 |
| Day 7 | 4 | D7-01~D7-04 |
| Day 8 | 3 | D8-01~D8-03 |

## 완성 판정

파일명만 있거나 0바이트인 `.asset`은 구현된 장면으로 세지 않는다. 각 장면은 다음 조건을 만족해야 한다.

- 유효한 `StorySceneDefinition` 직렬화 자산
- 정규 ID와 표시명
- Location과 Location State
- 초기 Screen
- 필요한 CharacterPlacementSet과 InteractionSet
- 진입 대화 또는 명시적인 무대사 설정
- AudioCueProfile 또는 의도적인 Location 기본 오디오 사용
- 필요한 퍼즐·Sequence·Transition
- 최소 하나의 유효한 종료 경로 또는 엔딩 처리
- 콘텐츠 Validator 통과

현재 상세 제작 현황과 남은 작업은 `Docs/Production/TODO.md`에서 단일 관리한다.
