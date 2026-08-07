# 오디오 시스템

## 책임

`AudioDirector`는 음악, 환경음, 효과음, 보이스 재생과 더킹의 단일 진입점이다. Story Scene이나 UI는 AudioSource를 직접 조작하지 않는다.

## 버스

| 버스 | 용도 |
|---|---|
| Music A/B | 끊김 없는 음악 교차 전환 |
| Ambience A/B | 장소 환경음 교차 전환 |
| SFX | UI·상호작용·사건 효과음 |
| Voice Bark | 짧은 인물 반응음 |
| Story Voice | 녹음·긴 서사 음성 |

## 데이터 해결 순서

`이벤트/Sequence 재정의 > Story Scene 프로필 > Location 기본 프로필 > 현재 적합 상태 유지` 순으로 `AudioCueProfile`을 결정한다. 특정 Story Scene ID를 오디오 코드에 넣지 않는다.

## 더킹

대화·심문·녹음 시작 알림을 받은 `AudioDuckingController`가 프로필에 따라 음악과 환경음을 낮춘다. 화면별로 볼륨 값을 복제하지 않는다.

## 리소스 정책

대형 음원은 Addressables 호환 참조를 사용한다. 파일명은 `MUS_`, `AMB_`, `SFX_`, `VO_`, `REC_` 접두사를 사용하고, 레거시 Resources 경로는 새 코드에 확산하지 않는다.

## 검증

- 프로필의 필수 클립과 버스 확인
- 잘못된 장면·장소 프로필 참조 확인
- Addressables 등록 및 레이블 확인
- 루프 구간과 음량 범위 확인
- 대화 종료 후 더킹 복구 확인
