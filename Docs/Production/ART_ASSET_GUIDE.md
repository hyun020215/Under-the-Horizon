# 아트 자산 정리 및 화질 기준

## 역할별 폴더

`Assets/_Project/Art/`에는 원본 시각 자산만 둔다. 최상위 분류는 다음과 같다.

```text
Art/
├── Backgrounds/  장소 배경과 장소 상태 변형
├── Branding/     로고와 앱 아이콘
├── Characters/   인물별 전신, 초상, 표정, 콘셉트 시트
├── Evidence/     증거 카드 이미지
├── Investigation/ 장소별 조사 확대 이미지와 퍼즐 재료
├── Props/        조사 오브젝트
├── Maps/         덱 도면, 지도 화면 배경, 컷어웨이
├── Cinematics/   순서 연출용 이미지
└── UI/           화면, 패널, 버튼, 아이콘, HUD
```

이전 프로젝트의 `Resources` 폴더명은 런타임 로딩 방식의 흔적이므로 아트 자산의 역할로 사용하지 않는다. 현재 대응은 다음과 같다.

| 이전 임시 경로 | 현재 역할 경로 |
|---|---|
| `Characters/World` | `Characters/<인물>/FullBody` |
| `Characters/Expressions` | `Characters/<인물>/Expressions` |
| `Characters/Runtime` | `Characters/<인물>/Concept` |
| `Characters/Ambient` | `Characters/AmbientNPC` 또는 `Characters/ProfessionalNPC` |
| `Investigation/BodyDiscovery` | `Investigation/Horizon/BodyDiscovery` |
| `Investigation/Puzzles` | `Investigation/Horizon/Puzzles` |
| `UI/Overhaul` | `UI/Screens` |
| `UI/Runtime/Icons` | `UI/Icons` |
| `UI/Maps` | `UI/Map` |

파일을 옮길 때는 Unity Project 창이나 `AssetDatabase.MoveAsset`을 사용한다. 이미지와 `.meta`를 따로 이동하거나 새로 가져오면 GUID가 바뀌어 `Content`와 Prefab의 참조가 끊길 수 있다.

## 화면용 원본 해상도

기준 화면은 16:9, UI 기준 해상도는 1920×1080이다.

- 전체 화면 배경과 타이틀 배경: 최소 1920×1080, 권장 2560×1440
- 전신 캐릭터: 실제 최대 표시 높이의 1.25배 이상
- UI 패널: 실제 최대 표시 크기 이상. 반복 확대가 필요한 장식은 9-slice 사용
- 작은 아이콘: 정수배에 가까운 크기로 표시하고 비정수 확대를 피한다

현재 원본 중 다수의 장소 배경은 1448×1086 또는 1536×1024이고, 타이틀 배경은 1672×941이다. 1920×1080 출력에서 가로로 확대되므로 임포트 압축을 끄더라도 원본에 없는 세부 묘사는 복원되지 않는다. 특히 4:3 또는 3:2 장소 배경은 16:9 화면과 종횡비도 다르다.

## Unity 임포트 기준

아트 이미지에는 다음 기준을 사용한다.

- Texture Type: Sprite (2D and UI)
- Compression: None
- Max Size: 4096
- Generate Mip Maps: Off
- Non Power of 2: None
- Filter Mode: Bilinear

메뉴 `Under the Horizon > Art > 정리 및 고화질 임포트 적용`으로 역할별 폴더 정리와 임포트 기준을 다시 적용할 수 있다.

## Game View에서 흐리게 보일 때

Unity의 `Free Aspect`는 Game 창의 실제 패널 크기로 렌더링한다. 작은 Game 창을 1배보다 크게 확대하면 최종 빌드와 무관하게 이미지와 글자가 흐리거나 계단져 보인다. 비교 검수 시 Game View 해상도를 `1920×1080` 또는 `2560×1440`으로 고정하고 Scale을 `Fit` 또는 1배 이하로 둔다.

실제 빌드는 1920×1080 기준으로 확인하되, 원본이 이보다 작은 배경은 고해상도 원본으로 교체해야 근본적으로 선명해진다. 런타임에서 인위적인 샤픈 필터를 적용하는 방식은 UI 글자와 인물 외곽선에 링잉을 만들기 때문에 사용하지 않는다.
