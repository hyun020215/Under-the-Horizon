# 전환 시스템

`TransitionDirector`는 Screen, Location, Story Scene 사이의 시각 전환을 소유한다. `StorySceneDirector`는 프로필을 전달할 뿐 애니메이션 구현을 알지 못한다.

## 표준 파이프라인

```text
입력 차단 → 기존 UI 퇴장 → 화면 덮기 → 콘텐츠 교체
→ 화면 공개 → 새 UI 진입 → 입력 해제
```

`TransitionProfile`은 퇴장·덮기·유지·공개·진입 시간과 easing, 입력 차단, stinger를 정의한다. 같은 시간을 장면 코드에 복제하지 않는다.

새 프로필은 기존 Player로 표현 가능한 시각 변형일 때 사용한다. 새로운 코드는 재사용 가능한 전환 알고리즘이 정말 필요한 경우에만 추가한다. 전환 중 상태 저장이나 Story Scene 완료 처리를 하지 않는다.
