# 원본 프로젝트 마이그레이션 기록

## 원본 확인

2026-08-08 기준 다음 두 경로를 파일별 SHA-256으로 비교했다.

- `D:\codex-project-mystery`
- `D:\우현 데이타\대학교\카이스트\생활\몰입캠프\project-mystery`

두 경로의 비생성 파일 2,466개는 모두 동일하며 차이는 0건이다. 양쪽 Git HEAD는 `1128581f48933a695b8ab61095f4a2b1f42d4a60`이다. 원본 작업 트리에 있던 퍼즐 이미지와 보이스 기획표도 현재 프로젝트에 반영되어 있다.

## 적용 원칙

현재 저장소의 `AGENTS.md`와 `Docs/Architecture/ARCHITECTURE.md`가 원본 폴더 구조보다 우선한다.

- 원본 이미지와 이전 Resources 이미지는 `Art/`로 분류했다.
- BGM, 효과음, 보이스는 `Audio/`로 분류했다.
- 대화, Story Scene 인덱스와 증거 CSV는 `Content/`로 분류했다.
- Unity 패키지와 프로젝트 설정은 원본을 기준으로 복원하되 개발 전용 Unity MCP 패키지는 제외했다.
- 출시 Build Settings의 Scene은 `Bootstrap.unity`와 `Game.unity`만 유지한다.
- 활성 미디어의 원본 `.meta` GUID를 유지했다.

## 레거시 구현 보관

원본 런타임·에디터·테스트 코드와 직렬화 자산은 `Docs/Migration/LegacySource/`에 보관한다. 이 폴더는 `Assets/` 밖이므로 게임 어셈블리에 포함되지 않는다.

원본의 `GameStateManager`, `UIManager`, `ProductionSceneDirector` 등을 그대로 활성화하면 현재의 상태·UI·흐름 소유자와 충돌한다. 필요한 동작은 `GameStateStore`, `ScreenRouter`, `StorySceneDirector`, 공통 Condition/GameEffect와 콘텐츠 정의로 옮긴다.

## 의도적으로 활성화하지 않은 항목

- 단일 목적 또는 거대한 레거시 런타임 관리자
- 이전 UI·게임 셸을 구현한 Unity Scene
- 이전 스크립트 GUID를 참조하는 Prefab과 ScriptableObject
- 광범위한 문자열 기반 Resources 경로

이 항목은 병렬 아키텍처와 깨진 직렬화 참조를 막기 위한 것이다. 필요한 기능은 보관본을 근거로 현재 구조 안에서 점진적으로 이식한다.
