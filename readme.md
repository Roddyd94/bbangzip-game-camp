# 01주차 — Pong — 공·패들 충돌과 점수

## 1. 이번 주차의 주제

준비된 창·입력·렌더러 위에서 공의 연속 이동·충돌·득점 시스템을 구현합니다. 공과 패들의 움직임을 실수 좌표로 계산하고 픽셀 기반 화면에서 바로 확인합니다.

### 훈련 상황

창·입력·렌더링·월드·설정 로딩이 준비된 작은 게임 프로젝트에 합류한 상황입니다. 패들과 공은 화면에 표시되지만 움직임과 경기 진행은 비어 있습니다. 이동 컴포넌트의 데이터와 API를 설계하고, 입력·deltaTime을 이동으로 연결한 뒤 빠른 공의 충돌·반사·득점·리스폰을 구현하는 훈련입니다. 이동·충돌에 필요한 계산을 직접 구현하고, 제공 엔진의 실행 순서 안에 연결하는 경험을 목표로 합니다.

## 2. 사전 지식

아래 개념·검색어를 따라 개인적으로 학습하고, 확인 질문에 작은 계산·그림·코드로 답할 수 있는지 살펴봅니다. 학습 노트 제출은 필요하지 않습니다. 설계 패턴은 이해를 돕는 수단이며 특정 구조를 강제하지 않습니다.

| 분야 | 학습할 개념·검색어 | 구현과의 연결 | 스스로 확인할 질문 |
|---|---|---|---|
| 수학·물리 | 2D 벡터, 속도, 위치 += 속도 × dt, 반사 | 실수 위치를 유지하고 벽 충돌 때 해당 축 속도의 부호를 바꿉니다. | 초당 10칸인 공의 0.1초 이동량은? X축 반사 후 vx와 vy는? |
| 컴퓨터 과학 | 고정·가변 시간 간격, 샘플링 오차 | 프레임 수와 이동량을 분리하고 큰 dt로 패들을 건너뛰는 문제를 이해합니다. | 매 프레임 1칸 이동하면 PC마다 속도가 달라지는 이유는? |
| 게임 엔진 원리 | 입력 → Update → 충돌 → render | 제공 루프에서 코어를 갱신하고 실수 좌표 오브젝트을 제공 SFML 렌더러에 전달합니다. 창·입력 처리는 제공됩니다. | 렌더링 빈도가 달라도 공의 속도를 유지하려면 무엇이 필요할까요? |
| 알고리즘·자료구조 | 구간 겹침·AABB, 상태 구조체 | 공·패들 정보를 구조체에 두고 충돌과 화면 이탈을 구분합니다. | 패들 끝에 닿은 공을 포함하는 비교 조건은? |
| 설계 패턴 | 게임 루프·상태 분리 | 작은 함수로 이동·충돌·득점·초기화를 나눕니다. 패턴 클래스는 필수가 아닙니다. | 득점이 다음 프레임에도 반복되지 않으려면 무엇을 바꿔야 할까요? |
| 운영체제 | 키 상태·이벤트, 프레임 시계, 대기 | 제공 입력·시계를 이해하되 직접 콘솔 입력기를 작성하지 않습니다. | 키를 누른 상태와 한 번 누른 이벤트는 무엇이 다를까요? |

## 3. 구현 환경 세팅

Windows x64 / MSVC / C++17 / SFML 3.1.0 환경입니다. SFML 헤더·DLL·링크 설정, 엔진 Subsystem과 파일 로더는 제공됩니다.

### 제공 구조와 구현 대상

#### 제공 구조

| 파일·폴더 | 제공 내용 |
|---|---|
| `src/base/` | 정수·SFML 타입 별칭·패턴 템플릿·Utility.hpp의 수학 편의 함수 |
| `src/core/` | 엔진 루프·월드·창·렌더·충돌 후보 검사·시간·입력·파일 시스템 |
| `src/todo/GameEngine.cpp` | Pong 전용 설정 로딩·오브젝트 생성·입력 매핑 |
| `src/todo/GameObject.hpp/.cpp` | 인터페이스 기본 구현과 World 참조 |
| `src/todo/PongBody.hpp/.cpp` | 공통 위치·크기·색·직렬화·중앙선 표시 |
| `src/todo/ScoreUI.hpp/.cpp` | PongGameRule의 좌·우 점수를 표시하는 폰트 없는 숫자 UI |
| `src/todo/PongGameRule.hpp/.cpp` | World가 소유하는 레벨 경계·점수·리스폰 대기 시간 |
| `content/game.ini` | 창 크기·프레임 제한·월드 및 설정 파일 경로 |
| `content/input.ini` | global / player1 / player2 키 바인딩 |
| `content/paddle.ini` | 패들 속력 |
| `content/ball.ini` | 공 초기 속력·패들 명중 가속량·시작 위치 |
| `content/pong.world` | 중앙선·Paddle 2개·Ball 1개·ScoreUI 배치 |

#### 구현 대상

| 파일 (`src/todo/`) | 이미 제공된 부분 | 참여자가 구현할 부분 |
|---|---|---|
| `MovementComponent.hpp/.cpp` | 클래스 골격과 TODO 안내 | 속력·방향 데이터, 초기화·조회·변경 함수 설계 및 deltaTime을 이용한 변위 계산 |
| `Paddle.hpp/.cpp` | 사각형 셰이프·플레이어별 입력 등록·이동 입력 조회·PaddleSettings 전달 | 설정 속력으로 MovementComponent를 초기화하고 입력을 이동에 반영하며 패들 전체를 레벨 경계 안으로 제한 |
| `Ball.hpp/.cpp` | 원형 셰이프·BallSettings 보관·이전 위치·이동 경로의 AABB 후보 영역·직렬화 | 설정 속력·방향으로 MovementComponent 초기화·이동·정확한 선분 충돌·각도 반사·가속·득점·3초 대기·리스폰 |

현재 실행 화면은 정지 상태입니다. 제공된 충돌 후보 검사는 정확한 선분 판정이나 반사 처리를 대신하지 않습니다. 세부 구현 규칙은 4절을 따릅니다.

1. 개인 Fork를 복제하고 Visual Studio의 C++ 데스크톱 개발·Windows SDK·CMake 도구를 설치합니다.
2. 저장소 루트에서 5절 명령으로 빌드합니다. main은 g_engine.initialize()와 run()만 호출합니다.
3. GameEngine이 설정을 읽고 World에 종류별 생성 함수를 전달합니다. tick은 deltaTime만 받고 InputState는 사용하지 않습니다. World의 파일 생성 타입은 GameObject(중앙선용 PongBody), Paddle, Ball입니다. 모든 GameObject는 자신이 속한 World를 참조하며 경계 등 공통 규칙은 World의 PongGameRule에서 조회합니다.
4. Paddle은 RectangleShape, Ball은 CircleShape를 소유합니다. Paddle 위치는 좌상단, Ball 위치는 중심입니다. 렌더러는 계산 결과를 그리기만 합니다.
5. getSubsystem은 포인터를 반환하며 실패 시 nullptr입니다. TimeManager의 deltaTime은 초, 위치는 픽셀, 속력은 픽셀/초입니다. 파일·초기화 실패는 bool과 스트림 상태로 알립니다.

### 수학 편의 함수

`#include "base/Utility.hpp"`로 `utility` 네임스페이스의 함수를 사용할 수 있습니다. MovementComponent와 충돌 반응의 설계·구현은 과제로 유지합니다.

| 함수 | 용도와 경계 조건 |
|---|---|
| `clamp(value, minimum, maximum)` | 실수를 범위 안으로 제한. minimum <= maximum 필요 |
| `saturate(value)` | 실수를 0~1로 제한 |
| `lerp(from, to, t)` | 실수 또는 float2 선형 보간. t는 자동 제한하지 않음 |
| `inverseLerp(from, to, value)` | 구간 내 비율 계산. 동일한 양 끝이면 0, 결과는 자동 제한하지 않음 |
| `toRadians(degrees)`, `toDegrees(radians)` | 도·라디안 변환 |
| `dot(first, second)` | float2 내적 |
| `lengthSquared(value)`, `length(value)` | float2 길이의 제곱과 길이 |
| `normalize(value, epsilon)` | 단위 벡터. 길이가 epsilon 이하이거나 유한하지 않으면 영벡터. 기본 epsilon은 0.000001이며 0 이상 유한 값 필요 |

```cpp
const float midpoint = utility::lerp(10.f, 30.f, 0.5f); // 20
const float ratio = utility::saturate(utility::inverseLerp(10.f, 30.f, 25.f)); // 0.75
const float2 direction = utility::normalize(float2{3.f, 4.f}); // (0.6, 0.8)
const float radians = utility::toRadians(90.f); // pi / 2
```

### 점수 UI

ScoreUI는 매 렌더링 시 World의 PongGameRule에서 점수를 읽어 좌측·우측 필드 상단에 표시합니다. 득점 처리에서 `recordScore(true)`는 왼쪽 플레이어, `recordScore(false)`는 오른쪽 플레이어 점수를 올립니다. `reset()`으로 두 점수를 지우면 UI도 다음 렌더링에 반영됩니다. 득점 판정과 전체 경기 Reset 연결은 구현 대상입니다.

`pong.world`의 `ScoreUI 0 20 20 36 255 255 255 255`는 가로 오프셋·상단 여백, 숫자 한 자리의 너비·높이, RGBA입니다. 여러 자리 점수도 표시하며 별도 폰트 파일은 필요하지 않습니다. UI는 충돌 영역을 갖지 않습니다.

### InputComponent 사용

입력은 오브젝트별 InputComponent에 등록하고 update 후 조회합니다. keyType은 해당 클래스에서 uint8 기반 enum으로 정의합니다. 같은 번호라도 서로 다른 컴포넌트에서는 독립적입니다.

```cpp
input.registerBool(fireKey, SfKey::Space);
input.registerFloat(moveKey, SfKey::W, SfKey::S);
input.registerVector2(directionKey, SfKey::A, SfKey::D, SfKey::W, SfKey::S);
input.update();
const float move = input.getKeyState(moveKey);
const bool fire = input.getBoolState(fireKey);
const float2 direction = input.getVector2State(directionKey);
```

- Bool은 0/1, Float은 keyInc−keyDec로 -1/0/1을 반환합니다. 동시에 누르면 0입니다.
- Vector2는 x축 감소·증가, y축 감소·증가 순서로 등록합니다. 각 축은 -1/0/1이고 대각선 정규화는 호출 측에서 결정합니다.
- getKeyState는 Bool·Float용 float 조회입니다. 벡터는 getVector2State로 조회합니다. 미등록·조회 타입 불일치는 0/false/영벡터를 반환합니다.
- 중복 액션 번호·유효하지 않은 키의 등록은 false이며 기존 매핑을 바꾸지 않습니다. bool 입력은 누름 전환 이벤트가 아니라 현재 눌림 상태입니다.
- Paddle은 파일에 저장된 플레이어 번호(1 또는 2)에 따라 1P/2P 키를 선택합니다. tick에서 update한 뒤 `const float moveInput = m_input->getKeyState(static_cast<uint8>(PaddleKey::Move));`로 조회합니다. 실제 이동 적용은 기존 TODO로 유지합니다.
- 엔진의 global Close·Reset도 InputComponent로 관리합니다. 전체 Reset을 구현할 때는 g_engine.getInputComponent()에서 GlobalInputKey::Reset을 조회할 수 있습니다.

## 4. 기획안과 구현 목표

### 설정과 초기화

```ini
# content/paddle.ini
speed = 300
```

```ini
# content/ball.ini
speed = 250
collisionSpeedIncrease = 25
start.x = 400
start.y = 300
```

속력은 양수, 증가치는 0 이상인 유한 값입니다. 시작 좌표도 유한 값이어야 하며 초기 공의 전체 원이 레벨 안에 있어야 합니다. 기본 레벨은 창과 같은 논리 사각형 `(0, 0) ~ (800, 600)`이고 중심선 x는 `(left + right) / 2`입니다.

- 설정 파일 로딩과 설정 구조체 전달은 제공됩니다. 참여자는 Paddle 생성자의 PaddleSettings::speed와 Ball의 BallSettings::speed를 사용해 MovementComponent 초기화를 구현합니다.
- 초기 패들 방향은 정지, 초기 공 방향은 오른쪽 `(1, 0)`으로 구현합니다. 방향은 단위 벡터로 관리하고, 정지 상태는 영벡터로 표현합니다.
- 공의 배치는 `Ball start ...`로 기록합니다. start는 ball.ini의 위치를 사용한다는 뜻이며 좌표를 두 파일에 중복 저장하지 않습니다. serialize로 저장한 일반 Ball 레코드는 숫자 x/y를 사용해 현재 위치를 복원합니다.
- 월드 행의 첫 토큰으로 클래스를 선택하고 별도의 PongKind는 저장하지 않습니다. Paddle 데이터의 첫 값은 플레이어 번호(1 또는 2)입니다. 중앙선은 PongBody로 표시하고 충돌하지 않습니다.
- 1P는 W/S, 2P는 Up/Down으로 조작합니다. input.ini의 player1.up/down, player2.up/down을 패들별 InputComponent의 PaddleKey::Move 액션으로 등록합니다. fire는 설정만 제공하며 이번 Pong 목표에는 사용하지 않습니다. R은 전체 경기 초기화, Escape는 종료입니다.

### 구현 순서와 규칙

1. **MovementComponent**: 방향과 속력을 분리하고 deltaTime으로 프레임 변위를 계산합니다. 현재 클래스 골격만 제공되므로 필요한 멤버 변수와 초기화·조회·변경·변위 계산 함수를 직접 설계하고 구현합니다. 함수명은 고정되어 있지 않으며 Paddle과 Ball에서 공통으로 재사용합니다.
2. **Paddle**: 자신에게 해당하는 플레이어 입력으로 위·아래 이동합니다. 전체 패들이 레벨 안에 있어야 하므로 y는 `[top, bottom - paddleHeight]` 범위이고 x는 레벨 안의 고정 위치를 유지합니다. 위·아래 동시 입력은 정지입니다.
3. **레벨 벽**: 공의 반지름까지 고려해 위·아래 경계에서 반사합니다. 큰 deltaTime에 남은 이동 시간이 있으면 이를 어떻게 처리할지도 정합니다.
4. **패들 선분 충돌**: 공의 이전 중심에서 이동 예정 중심까지를 line segment로 검사합니다. 원의 반지름만큼 패들 사각형을 확장한 뒤 최초 접촉 시점·면·접촉 위치를 구하는 접근을 사용할 수 있습니다. 끝 위치만 검사하거나 AABB가 겹친다는 이유로 즉시 반사하지 않습니다. 이미 패들 뒤로 지나간 공을 뒤에서 다시 튕겨내지 않습니다.
5. **반사각·가속**: 패들 중심에서 충돌 지점까지의 거리를 반높이로 정규화합니다. 위쪽 끝은 45°, 중심은 90°, 아래쪽 끝은 135°로 매핑합니다. 이 각도는 **패들 면의 위쪽 접선(-Y)을 0°, 필드 안쪽 법선을 90°, 아래쪽 접선(+Y)을 180°**로 둔 로컬 각도입니다. 오른쪽 패들은 x방향을 거울 대칭합니다. 같은 접촉을 중복 반사·가속하지 않고 실제 패들 명중마다 collisionSpeedIncrease를 한 번 더합니다.
6. **득점·대기**: 공이 패들 뒤로 지나 왼쪽·오른쪽 레벨 경계에 접촉하면 반대편 플레이어의 점수를 한 번 기록합니다. 공 중심의 경계 접촉 위치는 반지름을 고려해 결정합니다. 접촉점은 프레임 종료 위치가 아니라 이동 선분에서 최초 접촉한 시점의 위치입니다. 이후 3초간 공의 이동·충돌·중복 득점을 멈춥니다.
7. **리스폰**: 경계 접촉점의 y를 유지하고 x만 중심선으로 바꿉니다. 득점 직전의 운동 방향은 유지하며 속력만 ball.ini의 초기 speed로 되돌립니다. start.x/y는 최초 시작·전체 Reset에 사용하며 매 득점 리스폰 위치로 사용하지 않습니다.
8. **전체 Reset**: R 요청에서 양쪽 점수·대기 시간·공·패들 위치와 속력을 초기화합니다. 리스폰과 전체 Reset의 책임을 구분합니다.

CollisionManager는 Ball의 이전·현재 위치를 덮는 swept AABB로 충돌 **후보**를 통지합니다. 정확한 선분 판정과 가장 빠른 접촉 선택은 Ball::onCollision에서 구현해야 합니다. Ball의 이동·경계 접촉·패들 충돌 사이의 순서도 정해야 합니다. 위치를 바로 득점 상태로 확정한 뒤 더 이른 패들 접촉을 검사하는 방식은 피합니다. 대기 중에는 Ball의 충돌 영역과 표시를 비활성화해야 합니다.

### 동작 확인 예시

| 상황 | 기대 결과 |
|---|---|
| 패들을 위·아래로 계속 이동 | 패들 전체가 경계 안에 남음 |
| 같은 방향으로 30FPS·120FPS 이동 | 같은 실제 시간 동안 거의 같은 이동 거리 |
| 패들 위쪽·중앙·아래쪽 명중 | 각각 45°·90°·135° 로컬 반사각, 필드 안쪽 방향 |
| 큰 deltaTime으로 패들 전면을 한 번에 통과 | 선분 충돌이 검출되어 뒤로 빠지지 않음 |
| 후보 AABB만 겹치고 실제 선분은 빗나감 | 반사·가속 없음 |
| 패들 접촉 한 번 | 속력 정확히 증가치 한 번만 증가 |
| 왼쪽 득점 경계 최초 접촉 | 2P 점수 1 증가, 3초 대기 중 추가 득점 없음 |
| (8, 123)에서 득점, 폭 800·반지름 8 | 약 3초 후 (400, 123)에 리스폰, 방향 유지·초기 속력 |
| start.y=250이고 득점 y=123 | 리스폰 y는 123; 전체 Reset에서는 250 |

별도 보고서는 필요하지 않습니다. 코드·짧은 시연·핵심 설명으로 결과를 공유하면 됩니다. Timer 관리 시스템·사운드·추가 이펙트는 필수가 아닙니다. 3초 대기는 우선 deltaTime 누적으로 구현해도 됩니다.

## 5. 프로젝트 Fork와 세팅 후 빌드

```powershell
git clone <개인-Fork-URL> game-system-lab
cd game-system-lab
git remote add upstream <원본-저장소-URL>
git fetch upstream
git switch -c work/01 upstream/week/01-pong
.\build.ps1
.\build\result\StudyGame.exe
```

이미 복제했다면 clone·remote add는 생략합니다. src/todo/와 필요한 설정 파일을 작업하며 공통 엔진은 학습 목표상 필요한 경우에만 수정합니다. 빌드 시 SFML DLL과 content/ 전체를 실행 파일 옆으로 복사합니다. 기본 데이터 경로는 실행 파일 기준이므로 현재 작업 폴더에 의존하지 않습니다.

컨벤션은 [convention.md](convention.md)를 따릅니다. 이동·반사·득점·리스폰은 아직 TODO이며, 정지한 셰이프가 정상적으로 보이는 것이 제공 기반의 시작 상태입니다.

### PowerShell 빌드

루트에서 `./build.ps1`을 실행하면 src/todo/의 모든 cpp를 자동 수집하고 Release로 빌드합니다. 새 cpp를 추가할 때 CMakeLists를 수정할 필요는 없습니다. 일반 PowerShell에서도 Visual Studio에 포함된 CMake를 자동으로 찾습니다. Visual Studio의 C++ 데스크톱 개발·Windows SDK·CMake 도구는 설치되어 있어야 합니다.

필요한 경우 todo 소스만 지정합니다. 파일명은 src/todo 기준이며 하위 경로·저장소 기준 src/todo 경로·절대 경로도 사용할 수 있습니다. main과 core는 자동으로 포함됩니다.

```powershell
./build.ps1 -TodoSources GameEngine.cpp,GameObject.cpp,PongBody.cpp,PongGameRule.cpp,MovementComponent.cpp,Paddle.cpp,Ball.cpp,ScoreUI.cpp
./build.ps1 -Configuration Debug
```

결과는 `build/result/StudyGame.exe`와 같은 폴더의 SFML DLL·content/입니다. 배포할 때 이 폴더 전체를 사용합니다. 중간 파일은 build/project/에 보관하며, 실행하는 PowerShell의 현재 폴더와 무관하게 스크립트 위치를 기준으로 동작합니다. 빌드 실패 시 실패 코드를 반환하고 기존 result는 덮어쓰지 않습니다. 지정한 소스 목록은 필요한 todo 의존 소스를 모두 포함해야 합니다.
