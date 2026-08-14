# 세이브 마이그레이션 운영 기록

## 규칙

저장 DTO의 필드명·타입·의미를 바꾸거나 필드를 제거할 때는 저장 버전을 올린다. 각 버전 변환은 순차적으로 적용할 수 있어야 하며 원본 파일은 백업한다.

## 변경 절차

1. 기존 스키마와 새 스키마의 차이를 기록한다.
2. 누락 필드의 기본값과 변환 불가능한 값의 처리 방식을 정한다.
3. `SaveMigrationRegistry`에 변환을 등록한다.
4. 이전 버전 fixture를 불러와 논리 상태를 비교한다.
5. 최신 버전으로 다시 저장하고 재로드한다.
6. 실패 시 백업 복구와 사용자 메시지를 확인한다.

## 현재 상태

현재 구현은 논리 `GameState`를 `SaveData`로 변환하며, 컬렉션을 Unity 직렬화 가능한 목록 형태로 저장한다. `SaveMigrationRegistry`는 내장 마이그레이션을 순서대로 적용하고 현재 런타임보다 높은 버전은 명시적으로 거부한다.

| 원본 버전 | 대상 버전 | 변경 내용 | 검증 fixture |
|---|---|---|---|
| - | 1 | 초기 데이터 주도형 세이브 기준선 | EditMode round-trip 테스트 |
| 1 | 2 | 지도 이동 대기용 `pendingStorySceneId` 추가. 기존 저장은 빈 값으로 이행 | raw v1 JSON migration, v2 pending round-trip, newer-version rejection |
