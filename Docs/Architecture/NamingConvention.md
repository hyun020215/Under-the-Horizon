# 이름 규칙

## 공통 원칙

- 내부 ID는 안정적인 계약이며 표시명과 분리한다.
- 영문 대문자 접두사와 역할 중심 이름을 사용한다.
- Story Scene 파일명에는 정규 장면 ID를 유지한다.
- 단순 미관 개선을 위해 대규모 자산 이름을 바꾸지 않는다.

| 접두사 | 역할 | 예시 |
|---|---|---|
| `GAME_` | 게임 정의 | `GAME_UnderTheHorizon.asset` |
| `DATABASE_` | 콘텐츠 레지스트리 | `DATABASE_Content.asset` |
| `LOC_` | 장소 | `LOC_HORIZON.asset` |
| `CHR_` | 인물 | `CHR_EVELYN.asset` |
| `INT_` | 상호작용 집합 | `INT_D1_06_HORIZON.asset` |
| `DIA_` | 대화 | `DIA_D1_06.asset` |
| `C01_`~`C18_` | 증거 정의 | `C01_DanielInvitation.asset` |
| `EVD_` | 증거 이미지 | `EVD_C01_DanielInvitation.png` |
| `PUZ_` | 퍼즐 | `PUZ_D2_02_BloodPattern.asset` |
| `AUDIO_` | 오디오 프로필 | `AUDIO_D1_06_DISCOVERY.asset` |
| `TRANS_` | 전환 | `TRANS_DISCOVERY.asset` |
| `SEQ_` | Sequence | `SEQ_D1_06_BodyReveal.asset` |
| `BG_` | 배경 | `BG_HORIZON_NIGHT_CRIME.png` |
| `MUS_`/`AMB_`/`SFX_` | 음원 | `MUS_Horizon.mp3` |
| `PF_` | Prefab | `PF_CharacterView.prefab` |

Story Scene 예시는 `P01_PortJournalist.asset`, `D1_06_BodyDiscovery.asset`, `D8_03_ReturnToPort.asset`이다. 코드 타입은 PascalCase, 비공개 직렬화 필드는 camelCase를 사용한다.
