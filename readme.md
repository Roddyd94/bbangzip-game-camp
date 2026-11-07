# 02주차 볼링 — 프레임 진행과 점수 계산

제공된 볼링장에서 **투구 결과를 기록하고, 10프레임의 점수와 다음 투구 상태를 계산**합니다. 공을 던지는 장면은 준비되어 있습니다. 경기 규칙 담당으로서 핀 결과를 점수판과 다음 투구 준비에 연결하는 훈련입니다.

## 1. 프로젝트 세팅

1. Starter Kit가 배포되면 원본 저장소를 Fork하고, 개인 Fork를 복제합니다. 이미 복제했다면 `upstream` 등록 여부를 확인한 뒤 `fetch`부터 진행합니다.

   ```powershell
   git clone <개인-Fork-URL> bbangzip-game-camp
   cd bbangzip-game-camp
   git remote add upstream https://github.com/Roddyd94/bbangzip-game-camp.git
   git fetch upstream
   git switch -c work/02 upstream/week/02-bowling
   ```

2. Unity Hub에서 `bowling-game/`을 엽니다. 에디터는 `ProjectSettings/ProjectVersion.txt`에 지정된 **Unity 6000.6.7f1**을 사용합니다.
3. 패키지 복원이 끝나면 [BowlingHallPractice 씬](bowling-game/Assets/BowlingGame/Scenes/BowlingHallPractice.unity)을 열고 Play합니다. 1번 레인에서 방향키로 값을 조절하고 Enter 또는 Space로 다음 단계로 넘어갑니다. 준비 화면에서 한 번 더 누르면 투구합니다. [전체 조작법](bowling-game/Assets/BowlingGame/Scripts/Practice/README.md)

## 2. 빌드

1. **File → Build Profiles**에서 Windows를 선택합니다.
2. Scene List에 `Assets/BowlingGame/Scenes/BowlingHallPractice.unity`를 추가하고 활성화합니다.
3. **Build And Run**을 눌러 `bowling-game/Builds/Windows/`에 출력합니다.
4. 실행 파일을 열어 투구와 점수판을 확인합니다. 실행 파일을 공유할 때는 함께 생성된 데이터 폴더도 포함합니다.

## 3. 과제

| 구분 | 내용 |
| --- | --- |
| 제공 | 8레인 씬, 투구 조작과 카메라, 공·핀 물리, 핀 감지와 재배치, 점수판 UI |
| 구현 | 투구 결과 검증과 기록, 프레임별 점수·누적 점수, 다음 투구와 핀 배치 결정, 경기 종료·초기화 |

레인 관련 코드는 [BowlingLane 폴더](bowling-game/Assets/BowlingGame/Scripts/Game/BowlingLane)에 있습니다. 현재 데모에서는 `BowlingLane`이 투구를 진행하고, `BowlingReferee`가 판정하며, `BowlingLaneBlackboard`가 기록을 보관합니다. 배포된 과제 브랜치에서 채울 범위는 해당 브랜치의 코드 안내를 따릅니다.

1. **투구 기록** — 이번 투구에 새로 쓰러진 핀만 기록합니다. 남은 핀보다 큰 결과, 중복 통지, 경기 종료 후 입력을 거부합니다.
2. **점수 계산** — 스트라이크는 다음 두 투구, 스페어는 다음 한 투구를 더합니다. 보너스가 모이기 전에는 미확정으로 표시하고, 누적 점수는 앞에서부터 연속으로 확정된 프레임까지 합산합니다.
3. **경기 진행** — 1~9프레임은 최대 두 번 투구하고, 첫 투구가 스트라이크면 바로 다음 프레임으로 넘어갑니다. 남은 핀 유지, 전체 재배치, 경기 종료를 구분합니다.
4. **10프레임과 초기화** — 스트라이크는 두 번, 스페어는 한 번 더 던집니다. 둘째 투구가 새 랙에서 스트라이크에 실패했다면 셋째는 남은 핀만 사용합니다. 새 경기는 기록과 점수를 초기화합니다.

| 확인할 투구 | 기대 결과 |
| --- | --- |
| 20회 모두 0핀 | 0점 |
| 프레임마다 9핀·0핀 | 90점 |
| 매 프레임 5핀·5핀, 마지막 추가 투구 5핀 | 150점 |
| 12회 연속 스트라이크 | 300점으로 종료 |
| 첫 프레임 10핀, 다음 프레임 3핀·4핀 | 첫 프레임은 처음에 미확정, 이후 17점으로 확정되고 총점 24점 |
| 첫 투구 8핀 뒤 5핀, 또는 10프레임 10핀·7핀 뒤 4핀 | 마지막 입력을 거부하고 이전 기록 유지 |

구현 결과를 개인 Fork에 올리고 Discord에 공유합니다. 발표에는 짧은 실행 시연과 핵심 구현 설명 3~5줄이면 충분합니다.

## 4. 선수 지식

| 개념 | 먼저 확인할 내용 |
| --- | --- |
| 볼링 점수 규칙 | 일반 프레임과 10프레임의 투구 횟수, 스트라이크·스페어 보너스 |
| 리스트와 인덱스 | 프레임 번호와 투구 번호의 차이, 다음 투구가 아직 없을 때의 처리 |
| 상태 전이 | 투구 대기 → 결과 확정 → 핀 재배치 → 다음 투구 또는 경기 종료 |
| Unity 이벤트와 생명주기 | 핀 결과가 규칙 코드와 점수판으로 전달되는 흐름, 씬 재시작 시 구독 해제 |
