# 볼링 모니터 점수판 UI

1280×720 기준의 Unity uGUI / TextMeshPro World Space Canvas입니다. `Prefabs/BowlingScoreboard.prefab`을 BowlingHall 씬의 모니터 8개에 각각 연결했습니다. 기존 검은 모니터 메시 위에 UI가 별도로 놓이며 모델 파일에 점수판 이미지를 구워 넣지 않습니다.

## 표시 항목과 상태 연결

레인 번호, 플레이어 이름, 프레임 1~10, 투구별 표기, 프레임별 점수, 총점, 현재 프레임·투구, 남은 핀, 경기 상태를 표시합니다. 1~9프레임은 두 칸, 10프레임은 세 칸입니다. 미확정 프레임 점수는 `–`로 표시합니다. 점수표는 프레임별 점수이며 누적 점수표가 아닙니다.

`BowlingScoreboardView.SetLaneNumber(int)`와 `ApplyState(BowlingScoreboardData)`로 갱신합니다. 데이터 클래스는 `Assets/Scripts/Presentation/Bowling/`에 있습니다. 점수 계산·중복 투구 검사·프레임 진행 로직은 포함하지 않습니다. 규칙 코어가 계산한 점수와 X·/·- 등의 투구 표기를 전달합니다.

```csharp
var data = new BowlingScoreboardData
{
    PlayerName = "플레이어 01",
    CurrentFrame = 1,
    CurrentRoll = 2,
    RemainingPins = 3,
    TotalScore = 0
};
data.Frames[0] = new BowlingFrameDisplay
{
    Rolls = new[] { "7", "" },
    IsConfirmed = false
};
view.SetLaneNumber(1);
view.ApplyState(data);
```

기본 화면은 총점 0, 1프레임 첫 투구, 핀 10개, 점수 미확정 상태입니다. 씬에는 예시 점수를 넣지 않았습니다. 기능 연결 시 숫자 갱신마다 UI 전체를 다시 만들지 않고 제공 View에 상태를 전달합니다.

## 폰트

- IBM Plex Sans KR Medium / SemiBold 원본 TTF 포함
- TextMeshPro SDF: 샘플링 90pt, padding 9, 2048×2048 아틀라스
- 한글·영문·숫자·현재 UI 문자를 사전 추가하고 Dynamic / Multi Atlas로 추가 문자 지원
- 기본 본문 19~27, 프레임 점수 42, 주요 숫자 48~87: 1280×720 기준
- 폰트는 비트맵 글자 PNG가 아니라 SDF로 확대·축소합니다. 아틀라스 픽셀 수는 지원 문자 수·품질을 위한 값이며 텍스트 크기와 별개입니다.

출처: [IBM Plex 공식 프로젝트](https://github.com/IBM/plex), [Google Fonts 배포 폴더](https://github.com/google/fonts/tree/main/ofl/ibmplexsanskr). SIL Open Font License 원문은 `Fonts/OFL.txt`에 포함했습니다. OS 설치 폰트에 의존하지 않습니다.

## 아이콘

직접 제작한 8종의 흰색 투명 RGBA PNG입니다. UI Image의 Color로 색을 바꿉니다. 실제 사용 크기는 약 23~54px이며, 원본은 각 256×256px입니다. Sprite / Single, 최대 256, 압축 없음, 밉맵 없음, Bilinear로 임포트했습니다.

| 파일 | 용도 |
|---|---|
| bowling_ball | 브랜드·볼링 |
| bowling_pin | 플레이어·핀 |
| trophy | 총점·결과 |
| target | 남은 핀·목표 |
| roll | 현재 투구·진행 |
| info | 안내 |
| reset | 초기화 UI용 예비 아이콘 |
| chevron_right | 다음 단계 UI용 예비 아이콘 |

SVG 원본은 저장소 루트의 `SourceAssets/UI/Bowling/Icons/`에 있습니다. 아이콘 폰트 패키지에 의존하지 않고 개별 Sprite로 사용합니다. reset·chevron은 에셋만 제공하며 동작 버튼을 연결하지 않았습니다.

## 미리보기

저장소 루트 `SourceAssets/UI/Bowling/`의 `ScoreboardReady.png`, `ScoreboardExample.png`, `MonitorCloseup.png`에서 확인할 수 있습니다. 예시 화면의 46점은 UI 검증을 위한 샘플이며 실제 경기 기록이 아닙니다. 모니터 화면 비율에 맞춘 월드 크기는 약 1.10m×0.62m이며 FBX 부모의 스케일을 보정했습니다.

## 씬과 레인 프리팹 구조

상위 `Assets/Prefabs/Bowling/Environment.prefab` 안의 Lanes에는 공통 `Lane.prefab` 인스턴스 8개가 들어 있습니다. 각 Lane의 정적 모델은 Environment, 핀은 Pins, 공은 Ball에 두고, 모니터 점수판은 별도 `UI/MonitorScoreboard`로 묶습니다. UI는 모니터 메시의 자식이 아닙니다. 레인 번호·번호판 메시·모니터 위치 차이는 인스턴스 오버라이드로 유지합니다. 건물과 2레인 공용 가구는 상위 Building에 남깁니다.

```text
Environment (Environment.prefab)
├─ Building
├─ Lanes
│  └─ Lane_01 ... Lane_08 (Lane.prefab)
│     ├─ Environment (레인 표면·백스톱·번호판·모니터 모델)
│     ├─ Pins (10개)
│     ├─ Ball
│     └─ UI
│        └─ MonitorScoreboard (BowlingScoreboard.prefab)
├─ RackBalls
└─ Lighting
```


## 결과·상태 아이콘

다음 아이콘은 512×512 투명 PNG이며 Unity Sprite로 가져옵니다. 흰색 원본이므로 `Image.color`로 색을 지정합니다. SVG 원본은 `SourceAssets/UI/Bowling/Icons/`, 모음 미리보기는 `SourceAssets/UI/Bowling/ResultIconsPreview.png`입니다. 기존 일반 UI 아이콘은 256×256을 유지합니다.

| 파일 | 목적 | 표시 예시 |
|---|---|---|
| strike | 스트라이크 결과 | X, 결과 알림에서 강조 |
| spare | 스페어 결과 | /, 결과 알림에서 강조 |
| miss | 쓰러뜨린 핀이 없는 투구 | 투구 칸의 - |
| foul | 파울 결과 | 투구 칸의 F |
| split | 스플릿 상태 보조 표시 | 해당 투구 기록 옆 배지 |
| waiting | 투구·결과 대기 상태 | 상태 문구 옆 모래시계 |
| game_complete | 경기 완료 | 경기 종료 문구 옆 체크 |
| player | 플레이어 정보 | 이름 왼쪽 |
| frame | 프레임 정보 | 현재 프레임 문구 왼쪽 |

결과와 상태는 외부 게임 로직이 결정합니다. 아이콘 에셋만 추가하며 기존 문자열 기반 투구 표시와 프리팹 연결은 유지합니다. 스플릿은 숫자 기록과 함께 사용하는 보조 아이콘이며 점수 기호를 대체하지 않습니다.
