# 03주차 레이싱 게임 — 랩·구간 기록

제공된 카트와 트랙에서 **체크포인트 통과 순서를 검증하고 랩·구간 시간을 기록**합니다. 레이스 기록 시스템을 담당해 차량의 통과 이벤트를 HUD와 기록으로 연결하는 훈련입니다.

## 1. 프로젝트 세팅

1. 원본 저장소를 Fork하고 아래 명령으로 작업 브랜치를 만듭니다. 이미 복제했다면 `upstream` 등록을 확인하고 `fetch`부터 진행합니다.

   ```powershell
   git clone <개인-Fork-URL> bbangzip-game-camp
   cd bbangzip-game-camp
   git remote add upstream https://github.com/Roddyd94/bbangzip-game-camp.git
   git fetch upstream
   git switch -c work/03 upstream/week/03-lap-timing
   ```

2. Unity Hub에서 `laptime/`을 **Unity 6000.6.7f1**로 엽니다.
3. [LapTimingArena 씬](laptime/Assets/LapTiming/Scenes/LapTimingArena.unity)을 열고 Play합니다. 카운트다운의 `GO!` 신호에 AI 카트와 함께 출발합니다. **W/S 또는 ↑/↓**로 전후 가속, **A/D 또는 ←/→**로 조향합니다.

## 2. 빌드

1. **File → Build Profiles**에서 Windows를 선택합니다.
2. Scene List에 `Assets/LapTiming/Scenes/LapTimingArena.unity`를 추가하고 활성화합니다. `SampleScene`이 활성화돼 있으면 목록에서 해제합니다.
3. **Build And Run**으로 `laptime/Builds/Windows/`에 출력합니다.

## 3. 과제

카트 조작·물리, 트랙, 카메라, AI 주행, 카운트다운, 통과 이벤트, HUD와 완주 후 리플레이·다시하기가 제공됩니다. [LapTimingSession.cs](laptime/Assets/LapTiming/Scripts/TODO/LapTimingSession.cs)의 두 TODO를 구현합니다. 제공 상태에서도 주행과 첫 랩 시작은 가능하며, 구간 기록과 다음 랩 진행은 구현 후 동작합니다.

1. **`HandleCheckpoint`**에서 CP1 → CP2 → CP3 순서로 유효한 통과만 처리하고, 인접한 통과 시각의 차이를 구간 기록으로 전달합니다.
2. **`HandleLapCrossing`**에서 체크포인트가 누락됐다면 같은 랩을 다시 시작합니다. 유효한 완주라면 랩 시간·마지막 구간 시간·최고 기록을 갱신하고, 다음 랩 또는 경기 종료로 진행합니다.

**검증**은 정상 순서, 중복 통과, 누락 후 같은 랩 재시작을 확인합니다. 시작선·CP1·CP2·CP3·완주 시각이 0·3·6·9·12초라면 각 구간은 3초, 랩은 12초입니다.

**제출**은 개인 Fork의 작업 브랜치 링크와 핵심 구현 설명 3~5줄을 Discord에 공유하고 짧게 시연합니다.

## 4. 선수 지식

- Unity의 트리거 이벤트와 `Rigidbody`를 이용한 차량 구분
- 다음 체크포인트 인덱스로 순서와 중복을 판정하는 방법
- 같은 시간 기준으로 랩·구간 시간을 계산하는 방법
