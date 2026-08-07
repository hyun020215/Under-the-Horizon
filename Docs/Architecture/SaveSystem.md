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

## 버전과 마이그레이션

저장 스키마를 바꿀 때는 `SaveVersion`을 올리고 이전 버전에서 새 버전으로 가는 마이그레이션을 `SaveMigrationRegistry`에 등록한다. 필드 삭제·의미 변경은 기본값만 추가하는 변경과 달리 별도 호환성 검증이 필요하다. 절차와 이력은 `Docs/QA/SaveMigration.md`에 기록한다.
