# 아트 자산 정리 및 화질 기준

실제 Sprite 교체, GUID를 보존하는 덮어쓰기, Location State 연결과 캐릭터 배치 조정 절차는
`Docs/Production/UNITY_CONTENT_AUTHORING_GUIDE.md`를 함께 따른다.

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

### 고정 해상도 Fit 비교 절차

1. Unity `6000.3.20f1`에서 `Bootstrap.unity`를 열고 Play Mode로 실제 Title → Save Slot → Story Scene 흐름에 진입한다. `StoryScenePreviewWindow`나 빈 씬의 UI Prefab 캡처는 배경 crop, 캐릭터, HUD, hotspot 합성 승인 근거로 사용하지 않는다.
2. Game View 왼쪽 해상도 메뉴에서 `Fixed Resolution` 프리셋 `1920×1080`, `2560×1440`, `1920×1200`, `2560×1080`, `3440×1440` 중 하나를 직접 선택한다. 프리셋이 없으면 같은 메뉴 아래 `+`에서 `Fixed Resolution`을 고르고 width/height를 입력해 추가한다. 메뉴 `Under The Horizon > Preview > Game View PNG Capture`에서 같은 목표를 고르고 `Verify Target Resolution`을 누른 뒤, Play loop가 실제 `Screen.width/height`와 서로 다른 3개 Play frame의 안정을 확인해 `READY`를 표시할 때까지 기다린다. 일시 정지 중에는 검증을 진행하지 않는다. 도구는 Unity 내부 Game View API를 반사 호출하거나 프리셋을 자동 변경하지 않는다.
3. Game View Scale은 `Fit` 또는 1배 이하로 유지한다. 확대된 Editor 미리보기의 흐림을 실제 렌더링 결함으로 판정하지 않는다.
4. 검수 상태를 직접 만든 뒤 `Session`, `Scope`, `State`를 확인하고 `Capture Exact PNG`를 누른다. 캡처 창은 Game View와 도킹하지 않고 별도 Utility 창으로 유지하며, `PASS`가 표시될 때까지 Game View 탭과 Play Mode를 그대로 둔다. hover는 Game View 탭에 입력 포커스를 준 뒤 포인터를 대상 위에 유지한 채 `Ctrl+Alt+Shift+G`를 눌러 `PointerExit` 없는 동일 경로로 캡처한다. focus는 Interaction ID를 입력하고 `Focus Interaction`으로 실제 EventSystem 선택과 marker·tooltip 표시 성공을 확인한 뒤 캡처한다. idle·hover·focus 증거를 먼저 확보한 다음 격리된 QA 슬롯에서만 `Ctrl+Alt+Shift+F`를 사용한다. 이 보조 명령은 검증·캡처 작업이 없는 unpaused Play Mode에서 현재 합성 화면의 같은 interaction을 선택하고 live `EventSystem.submitHandler`를 dispatch하며, 실제 interaction이므로 진행·효과·체크포인트를 변경할 수 있다. `PASS`는 handler dispatch 성공만 뜻하므로 대화·효과·hotspot 상태 변화도 별도로 관찰한다. 이 확인은 해당 interaction의 submit 경로만 승인하며 물리 키보드·게임패드 전역 navigation 승인으로 확대 해석하지 않는다.
5. 캡처 도구가 캡처 직전에도 목표 해상도가 서로 다른 3개 Play frame에서 유지되는지 다시 확인하고, 요청 시점 `Screen.width/height`와 저장된 PNG의 실제 pixel 크기가 일치할 때만 `PASS`를 출력해야 한다. `Session`에는 `{date}_{short-sha}` 또는 미커밋 검수라면 `{date}_working-tree`를 기록한다. 출력은 Git에서 제외되는 `Logs/Validation/{session}_{scope}/` 아래 `{width}x{height}_{scope}_{state}.png` 형식으로 남기며, 같은 이름의 기존 증거는 덮어쓰지 않고 순번을 붙여 보존한다. 캡처 도중 창을 닫거나 Play Mode를 종료하면 이 도구의 고유 `.uth-game-view-capture.<guid>.pending.png`만 최대 60초 동안 후속 정리하고 감시를 종료하며, 다른 도구의 임시 파일과 완료된 증거 PNG는 건드리지 않는다.
6. 같은 상태를 다섯 해상도에서 비교한다. 16:9끼리는 동일 crop을 유지하는지, 16:10과 두 21:9 해상도에서는 cover crop·HUD·캐릭터·상호작용 좌표가 함께 정렬되고 화면 가장자리에서 잘리지 않는지 확인한다. exact PNG에서도 원본 자체가 흐릴 때만 Import max size, compression 또는 원본 교체를 검토한다.
