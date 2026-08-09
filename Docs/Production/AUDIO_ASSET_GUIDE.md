# 오디오 자산 정리 기준

## 역할별 폴더

`Assets/_Project/Audio/`에는 원본 음원만 둔다. 런타임 의미와 장면 연결은
`Content/Audio/`의 `AudioCueProfile`과 Sequence 데이터가 담당한다.

```text
Audio/
├── Music/              MUS_  배경 음악과 인물 테마
├── Ambience/           AMB_  반복 가능한 장소 환경음
│   ├── Interior/
│   ├── Ocean/
│   └── Ship/
├── SFX/                SFX_  짧은 효과음
│   ├── Doors/
│   ├── Evidence/
│   ├── Footsteps/
│   ├── Puzzle/
│   ├── StoryEvents/
│   └── UI/
├── VoiceBarks/         VO_   인물별 짧은 반응음
└── StoryRecordings/    REC_  장면 안에서 재생하는 녹음·메시지·긴 서사 음성
    └── <Story Scene ID>/
```

Voice Bark와 Story Recording은 같은 음성 파일이라도 역할이 다르다. 감정 반응처럼 여러 장면에서
재사용하는 짧은 소리는 `VoiceBarks/<Character>/VO_...`에 둔다. 녹음기, 익명 채팅, 음성 메시지,
복원 오디오처럼 서사 진행과 line ID에 연결되는 음원은
`StoryRecordings/<Story Scene ID>/REC_<Story Scene ID>_...`에 둔다.

## 현재 Story Recording 분류

| 폴더 | 내용 |
|---|---|
| `D1_06` | Daniel의 dying message |
| `D2_06` | Daniel cabin의 익명·Daniel 채팅 |
| `D4_01` | Evelyn 메시지 |
| `D5_03` | 익명·Daniel 채팅 |
| `D7_03` | Orpheus 녹음 복원 조각 |

파일 위치와 이름은 음원의 역할만 나타낸다. 어느 Dialogue line 또는 Sequence에서 재생할지는
콘텐츠 참조로 결정하며, 런타임 코드에서 Story Scene ID나 파일 경로를 분기하지 않는다.

## 이동과 이름 변경

Unity Project 창이나 `AssetDatabase.MoveAsset`으로 이동해 `.meta` GUID를 보존한다. 운영체제에서
옮겨야 한다면 음원과 같은 이름의 `.meta`를 반드시 함께 이동한다. GUID를 유지하면 기존
`AudioCueProfile`, Sequence와 Prefab 참조는 경로 변경 뒤에도 유지된다.

메뉴 `Under the Horizon > Audio > 역할별 폴더 정리`는 알려진 레거시 경로를 현재 구조로
옮긴다. 이미 정리된 자산은 건너뛰므로 반복 실행할 수 있다.

## 별도 문서와 원본 데이터

녹음 계획표와 큐 시트는 음원이 아니므로 `Audio/`에 두지 않는다.

- Voice Bark 제작 계획: `Docs/Narrative/Source/Under_the_Horizon_Voice_Bark_Master_Plan_KR.xlsx`
- 전체 오디오 큐: `Docs/Narrative/Source/Under_the_Horizon_Audio_Cue_Sheet_KR_v2.xlsx`

Story Recording과 Dialogue line ID의 최종 대응은 위 원본을 검토한 뒤 `AudioCueProfile` 또는
Sequence에 명시한다. 대응표가 확정되지 않은 상태에서 파일명 추측으로 자동 연결하지 않는다.
