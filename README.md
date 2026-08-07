# Under the Horizon

2D 데이터 주도형 추리 내러티브 어드벤처 게임입니다. 41개의 Story Scene을 개별 Unity Scene으로 만들지 않고 `StorySceneDefinition` 콘텐츠가 하나의 `Game.unity` 셸을 구성합니다.

## 개발 환경

- Unity `6000.3.20f1`
- Unity Hub
- Windows 10/11
- Git LFS가 필요한 원격 저장소 설정

정확한 Unity 버전은 `ProjectSettings/ProjectVersion.txt`에서 확인할 수 있습니다. 다른 Unity 버전으로 열면 직렬화 자산과 `.meta`가 대량 변경될 수 있으므로 업그레이드 목적이 아니라면 사용하지 마세요.

## Unity에서 실행하기

1. Unity Hub에서 **Add project from disk**를 선택합니다.
2. 이 저장소의 루트 폴더 `Under-the-Horizon`을 선택합니다.
3. Editor 버전으로 `6000.3.20f1`을 지정해 프로젝트를 엽니다.
4. 최초 패키지 복원과 Asset Import가 끝나고 Console 오류가 없는지 확인합니다.
5. `Assets/_Project/Scenes/Bootstrap.unity`를 엽니다.
6. Play 버튼을 누릅니다.

`Bootstrap.unity`는 앱 서비스를 초기화한 뒤 `Game.unity`를 로드합니다. `Game.unity`를 직접 열어 화면·퍼즐을 개발할 수 있지만, 새 게임의 전체 초기화 흐름을 확인할 때는 반드시 Bootstrap부터 실행하세요.

## P0 콘텐츠 다시 생성하기

현재 프로젝트 구조의 ScriptableObject, 공통 Prefab과 Scene 셸은 다음 메뉴로 재생성할 수 있습니다.

```text
Under The Horizon > Build > P0 Project Content
```

이 도구는 다음 원본을 사용합니다.

- `Content/StoryScenes/StoryScene_Index_KR.csv`
- `Content/Dialogue/Source/Dialogue_Master_KR.csv`
- `Content/Evidence/Evidence_Master_KR.csv`

기존 콘텐츠 ID와 `.meta` GUID를 유지해야 하므로 자산을 수동 삭제한 뒤 다시 만들지 마세요. 생성 전 변경 사항을 커밋하거나 백업하고, 생성 후 `Under The Horizon > Validate > Build Preflight`를 실행하세요.

## Unity Test Runner로 테스트하기

1. Unity 메뉴에서 **Window > General > Test Runner**를 엽니다.
2. **EditMode** 탭에서 **Run All**을 실행합니다.
3. **PlayMode** 탭에서 **Run All**을 실행합니다.
4. 실패 항목을 선택해 Console과 Stack Trace를 확인합니다.

EditMode 테스트는 상태·조건·효과·세이브·콘텐츠 경로를 빠르게 검증합니다. PlayMode 테스트는 Bootstrap, Story Scene 전환, 장소 이동, 대화, 퍼즐과 저장 복원을 검증합니다.

## 명령줄에서 테스트하기

Unity가 같은 프로젝트를 열고 있으면 배치 실행이 실패하므로 Editor를 먼저 닫으세요. PowerShell 예시는 다음과 같습니다.

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'

& $unity `
  -batchmode `
  -nographics `
  -projectPath $PWD `
  -runTests `
  -testPlatform EditMode `
  -testResults 'Logs\EditMode-results.xml' `
  -logFile 'Logs\EditMode-tests.log'
```

PlayMode는 `-testPlatform PlayMode`와 별도 결과 파일을 사용합니다.

```powershell
& $unity `
  -batchmode `
  -nographics `
  -projectPath $PWD `
  -runTests `
  -testPlatform PlayMode `
  -testResults 'Logs\PlayMode-results.xml' `
  -logFile 'Logs\PlayMode-tests.log'
```

프로세스 종료 코드가 `0`인지 확인하고 XML 결과와 로그를 함께 검토하세요. Unity Hub 로그인이 풀렸다면 배치 모드가 라이선스를 얻지 못할 수 있으므로 Hub에서 로그인과 Unity Personal 라이선스를 먼저 확인합니다.

## 콘텐츠 검증

Unity 메뉴에서 다음을 실행합니다.

```text
Under The Horizon > Validate > Build Preflight
```

검증기는 중복 Story Scene ID, 누락 Location, 깨진 다음 장면 경로를 실패로 처리합니다. 검증을 통과시키기 위해 규칙을 약화하지 말고 콘텐츠나 참조를 수정하세요.

## 주요 문서

- `AGENTS.md`: 필수 아키텍처 가드레일
- `Docs/Architecture/ARCHITECTURE.md`: 표준 프로젝트 구조와 책임
- `Docs/Production/TODO.md`: 단일 제작 TODO
- `Docs/QA/ReleaseChecklist.md`: 릴리스 점검표
- `Docs/Migration/SOURCE_MIGRATION.md`: 원본 프로젝트 이관 기록

## 개발 규칙 요약

- Story Scene을 Unity Scene 또는 장면 전용 컨트롤러로 만들지 않습니다.
- 상태 변경은 `GameStateStore`와 공통 GameEffect를 사용합니다.
- 화면은 `ScreenRouter`, 전환은 `TransitionDirector`, 오디오는 `AudioDirector`를 통합니다.
- 새 인스턴스나 장면 차이는 `Content/`, 재사용 동작만 `Runtime/`에 둡니다.
- 기능 단위로 커밋하고 검증 후 원격 저장소에 푸시합니다.
