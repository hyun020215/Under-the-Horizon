# 릴리스 점검표

## 프로젝트

- [ ] Unity 버전이 `ProjectVersion.txt`와 일치한다.
- [ ] Build Settings에는 `Bootstrap.unity`, `Game.unity`만 출시 Scene으로 등록되어 있다.
- [ ] 콘솔 컴파일 오류와 누락 스크립트가 없다.
- [ ] 작업 트리에 의도하지 않은 생성 파일이 없다.
- [ ] Graphics와 모든 Quality 단계가 `Assets/_Project/Settings/Rendering/`의 canonical URP Pipeline을 참조한다.
- [ ] canonical 2D Renderer, URP Pipeline, URP Global Settings와 Volume Profile이 유효하며 서로 연결되어 있다.
- [ ] Editor 재시작 뒤 `Assets/UniversalRenderPipelineGlobalSettings.asset` 또는 `Assets/DefaultVolumeProfile.asset` fallback이 다시 생성되지 않는다.

## 콘텐츠

- [ ] 41개 Story Scene 정의가 유효하고 ID가 중복되지 않는다.
- [ ] 모든 필수 장소·상태·인물·대화·상호작용 참조가 연결되어 있다.
- [ ] 장면 경로가 존재하며 필수 진행 경로가 막히지 않는다.
- [ ] `C-01`~`C-18` 증거 정의와 표시 데이터가 유효하다.
- [ ] 퍼즐·오디오·Sequence·Transition 참조가 유효하다.
- [ ] 필요한 대형 미디어가 Addressables에 등록되어 있다.

## 상태와 세이브

- [ ] 새 게임, 슬롯 저장, 불러오기, 덮어쓰기와 백업 복구를 확인했다.
- [ ] 이전 저장 버전 마이그레이션 테스트가 통과한다.
- [ ] 로드 후 현재 Story Scene의 화면·장소·인물·오디오가 재구성된다.

## 플레이

- [ ] Bootstrap부터 엔딩까지 대표 정상 경로를 실행했다.
- [ ] 각 Screen과 Modal의 입력 차단·복귀가 정상이다.
- [ ] 퍼즐 완료·취소 결과가 Story 흐름에 한 번만 반영된다.
- [ ] 대화 더킹과 장소·장면 오디오 우선순위가 정상이다.
- [ ] 개발용 로그·메뉴·Scene이 출시 빌드에 노출되지 않는다.

## 자동 검증과 빌드

- [ ] `Under The Horizon > Validate > Build Preflight`가 통과한다.
- [ ] 전체 EditMode와 PlayMode 테스트가 실패·건너뜀·불확정 없이 통과한다.
- [ ] `Under The Horizon > Build > Windows 64-bit Player Smoke`가 성공하고 실행 파일이 Title 화면에 진입한다.
- [ ] `Library`가 없는 clean checkout의 최초 import에서도 Build Preflight와 Player 빌드가 통과한다.
- [ ] cold import, 테스트와 Player 빌드 전후 `git status --porcelain`이 비어 있고 root fallback 렌더링 자산이 없다.
