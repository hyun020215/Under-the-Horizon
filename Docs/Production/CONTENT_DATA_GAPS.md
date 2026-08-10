# 콘텐츠 데이터 누락 및 확인 요청

> 기준일: 2026-08-10  
> 목적: 원본에서 복구할 수 있는 정보와 제작자 결정이 필요한 정보를 분리한다.

## 1. 퍼즐 규칙

13개 `PuzzleDefinition`은 ID, controller key, 완료 Effect까지 존재하지만 실제 입력 목록과 정답은
직렬화되어 있지 않다. 현재 일부 Controller의 `Submit` 인자로 테스트 코드가 임의의 정답을 넘길 수
있어, 그런 테스트는 콘텐츠 정확성을 보장하지 못한다.

13개 장면 모두 퍼즐 진입 `InteractionDefinition`과 `PuzzleInteractionAction`은 연결했다. 따라서
플레이 중 퍼즐 화면을 여는 경로는 생겼지만, 아래 규칙 데이터가 확정되기 전까지 퍼즐 자체를
완성된 콘텐츠로 판정하지 않는다. 임시 핫스팟은 화면 중앙 30% 영역이며 장면별 최종 좌표는
플레이 화면 검수 후 확정해야 한다.

| Puzzle | 장면 | 원본에서 복구 가능한 핵심 | 추가 확인이 필요한 항목 |
|---|---|---|---|
| Blood Pattern | D2-02 | 혈흔 방향과 시신 이동 추론 | 타일 초기 배치, 회전 허용, 힌트·재시도 |
| CCTV Logs | D2-04 | 카메라 사각과 시설 로그 비교 | 관찰 대상 ID, 시간 범위, 정답 판정 조합 |
| Vault Authentication | D3-04 | 봉인 기록·인증 로그 | 입력 패턴, 실패 허용 횟수, 힌트 |
| Stair Reconstruction | D4-03 | 발자국·손자국 기반 추락 순서 | 조각 목록, 정확한 순서, 부분 정답 처리 |
| Claire Contradiction | D5-02 | 진술과 장치 기록 모순 | 연결 가능한 카드 전체와 필수 연결 |
| Stabilizer Log | D6-01 | 안정기 신호 복원 | 목표 주파수·위상, 허용 오차, 조작 범위 |
| Cargo Rail | D6-02 | 밸러스트 별실 → 수직 샤프트 → Horizon 천장 | 전체 노드·간선, 우회·오답 경로, 힌트 |
| Luminol | D6-03 | 세척된 혈흔 탐지 | 검사 대상 전체, 반응 강도, 필수 대상 |
| Cause of Death | D6-04 | 질소 노출에 의한 질식 | 카드 전체, cause/mechanism ID, 오답 피드백 |
| True Timeline | D6-05 | 원문에 사건 시각과 순서 존재 | 카드 ID 확정, 동시 사건 처리, 힌트 단계 |
| Visor DNA | D7-02 | 보호면 DNA 비교 | marker/allele 전체 값, 부분 일치 처리 |
| Audio Restoration | D7-03 | 15년 전 Orpheus 녹음 복원 | 파형 조각 ID·순서, 실제 재생 클립 구간 |
| Final Accusation | D8-01 | 6단계 공식 정답과 A/B/C/Bad 규칙 존재 | 재시도·오답 누적 UX, 증거 gate, 힌트 정책 |

우선 원문 CSV와 `Docs/Migration/LegacySource`에서 정답을 복구한 뒤, 위 표의 UX 정책만 제작자에게
확인받아야 한다. 규칙 데이터 스키마는 `PuzzleDefinition`이 참조하는 재사용 가능한 퍼즐 규칙
자산으로 확장하는 것이 현재 아키텍처에 부합한다.

## 2. 엔딩 흐름

공식 원문과 레거시 구현에는 다음 규칙이 있다.

- A `ending_a_complete`: 살인과 Orpheus 은폐를 모두 공개하며 D8-02를 연다.
- B `ending_b_convenient_culprit`: 살인은 해결하지만 과거 은폐는 남기며 D8-02를 연다.
- C `ending_c_wrong_person`: 핵심 논증 실패 후 D8-03으로 간다.
- Bad `ending_bad_panic`: 불안 100 또는 증거 무결성 0에서 D8-03으로 간다.

현재 프로젝트는 `GameState.endingId`를 저장하지만 이를 설정하는 Effect와 판정 규칙, 엔딩별 Route
Condition이 없다. 따라서 `D8-01.routes`를 임의로 연결하지 말고 다음을 함께 이식해야 한다.

1. 6단계 Final Accusation 규칙 데이터
2. 공통 `SetEndingEffect`와 `EndingCondition`
3. A/B는 D8-02, C/Bad는 D8-03으로 가는 조건부 Route
4. D8-02 완료 후 D8-03, D8-03 이후 종료 처리
5. 네 엔딩의 도달 가능성과 Save/Load 회귀 테스트

## 3. 오디오·아트·배포

다음 값은 저장소 원문만으로 승인 여부를 결정할 수 없다.

- Story Recording 파일별 Dialogue line ID 또는 Sequence 구간
- UI 공통 효과음의 최종 파일과 라이선스 출처
- Music/Ambience 더킹 목표 dB, attack/release, crossfade 기본 시간
- 로고와 앱 아이콘 최종 시안, Android/iOS/PC 등 대상 플랫폼 규격
- Addressables 원격 배포 여부와 그룹 정책

## 4. CI 확인 요청

CI 파일을 확정하려면 다음 운영 결정을 받아야 한다.

- GitHub-hosted runner와 self-hosted runner 중 어느 것을 사용할지
- Unity 라이선스 활성화 방식과 Repository Secrets 제공 가능 여부
- CI에서 테스트만 수행할지, Windows/Android 등 빌드까지 수행할지
- push와 pull request 중 어느 이벤트에서 실행할지

운영 선택 전에도 로컬 Build Preflight와 Test Runner는 유지한다. 자격 증명이나 라이선스 값을
저장소에 직접 커밋하지 않는다.

## 5. 제작자 확인 체크리스트

아래 항목은 원본만으로 최종 결정을 내릴 수 없으므로 확인 전에는 임시값을 출시값으로 간주하지 않는다.

- [ ] 13개 퍼즐의 장면별 핫스팟 위치와 크기를 실제 배경 위에서 승인한다.
- [ ] 위 표의 퍼즐별 재시도, 오답 피드백, 힌트, 허용 오차와 중간 저장 정책을 정한다.
- [ ] D8-01 오답 누적 방식과 증거 gate 정책을 정한다.
- [ ] Story Recording 16개의 Dialogue line ID 또는 Sequence 재생 구간을 승인한다.
- [ ] 41개 CharacterPlacement와 전용 Location State의 최종 화면 구도를 승인한다.
- [ ] D4-02, D5-01, D7-01, D8-02, D8-03 연출의 컷 구성과 타이밍을 승인한다.
- [ ] UI 공통 효과음과 라이선스 출처를 승인한다.
- [ ] Audio Mixer 목표 음량, 더킹 attack/release와 crossfade 시간을 승인한다.
- [ ] 로고, 앱 아이콘과 대상 플랫폼별 규격을 승인한다.
- [ ] Addressables 로컬/원격 배포 및 그룹·레이블 정책을 정한다.
- [ ] CI runner, Unity 라이선스 방식, 실행 이벤트와 빌드 플랫폼을 정한다.
