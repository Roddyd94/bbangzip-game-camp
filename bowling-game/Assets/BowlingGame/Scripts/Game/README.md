# 레인 내부 구조

| 타입 | 책임 |
|---|---|
| BowlingLane | 소유권·직접 참조·로컬 이벤트를 연결하는 레인 허브 |
| BowlingLaneBlackboard | 레인별 10프레임, 투구별 전후 핀 마스크·쓰러진 핀 수·마크·확정 점수 기록 |
| BowlingReferee | 핀 결과 판정, 스플릿 조회, 프레임/10프레임 진행, 보너스와 누적 점수 계산 |
| BowlingLanePublisher | Lane의 로컬 이벤트를 외부 EventBus로 전달하는 어댑터 |
| LaneEnd | BowlingBall 진입을 C# BallReached 이벤트로 해당 Lane에 알림 |

블랙보드는 MonoBehaviour가 아닌 Serializable 클래스이며 Lane이 인스턴스를 소유합니다. 외부에서는 Frames / Rolls를 읽고, 기록과 점수 갱신은 레인·심판이 수행합니다. 과거 기록은 다음 투구가 와도 덮어쓰지 않습니다. 비트 0은 1번 핀, 비트 9는 10번 핀입니다.

## 흐름

1. Lane.BeginGame(playerName) → 기록 초기화 → 로컬 GameStart / FrameStart / RollStart
2. LaneEnd.BallReached → 해당 Lane이 3초 대기 → PinNumber 기준으로 기립 마스크 관측
3. Lane.ResolveRoll(mask) → 심판 판정·기록·점수 갱신 → 로컬 RollEnd / FrameEnd
4. 경기가 끝났으면 GameEnd. 계속된다면 PinSetupRequested(decision)
5. 핀세터가 실제 핀 보존·재배치를 완료한 뒤 Lane.CompletePinSetup() 호출
6. 필요한 경우 FrameStart, 이어서 RollStart. 준비가 끝나기 전 중복 투구 결과는 거절

프리팹은 Start 시 최초 게임을 시작합니다. GameManager가 그 전에 BeginGame을 호출했다면 중복 시작하지 않습니다. 재시작은 BeginGame으로 명시적으로 요청합니다.

## 내부 통신

LaneEnd → Lane: BallReached C# 이벤트.
Lane → Referee / Blackboard: 직접 메서드 호출.
Lane → PinSetter: PinSetupRequested 로컬 이벤트와 Lane이 보관하는 PinSetter 참조.
PinSetter → Lane: 실제 작업 완료 후 CompletePinSetup 메서드 호출.
Lane → Publisher: GameEvent / FrameEvent / RollEvent 로컬 이벤트.
Publisher → 외부 점수판: EventBus.

PinSetter의 실제 집게·스위퍼·핀 물리 이동은 기존 미구현 상태입니다. 해당 구현에서 PinSetupRequested를 구독하고, decision.RequiresFullRack에 따라 전체 재배치 또는 남은 핀 보존 작업을 마친 뒤 CompletePinSetup을 호출해야 합니다. 대기 시간만으로 핀 준비 완료를 가정하지 않습니다. 게임 재시작 시 실제 공·핀 원위치 복귀 역시 그 구현에서 함께 수행해야 합니다.

FrameEnd는 프레임 종료 시뿐 아니라 후속 투구로 보너스가 확정된 이전 프레임에도 발행됩니다. FrameData.Score는 점수판 칸에 표시할 해당 프레임까지의 누적 점수이고, Blackboard 프레임의 Score는 그 프레임 자체의 점수, TotalScore는 누적 점수입니다. RollData.PinCount는 이번 투구에서 쓰러뜨린 수이며 RemainingPinCount와 구분합니다.

## 검증

Unity 메뉴 Tools → Bowling → Validate Lane Architecture 또는 BowlingLaneArchitectureChecks.Run()을 실행합니다. 테스트는 임시 오브젝트로 논리 판정과 이벤트 경계를 검증하며 실제 물리·애니메이션 사이클의 통합 시연은 포함하지 않습니다.


## 참조와 연결

컴포넌트 참조는 프리팹 Inspector에서 명시적으로 설정합니다. 런타임 자동 탐색이나 누락 시 대체 연결은 하지 않습니다.
Lane.OnEnable에서 LaneEnd 이벤트를 구독하고 Publisher.Subscribe(this)를 호출합니다.
Lane.OnDisable에서 같은 연결을 해제합니다. Publisher는 별도의 OnEnable/Initialize/연결 상태 플래그를 사용하지 않습니다.
