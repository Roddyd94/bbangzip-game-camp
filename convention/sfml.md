# SFML / C++ 코딩 컨벤션

이 문서는 스터디에서 합의한 규칙과 직접 수정한 C++ 코드의 작성 방식을 정리합니다. Windows 콘솔 C++ 주차에도 공통 C++ 규칙을 동일하게 적용합니다. 규칙을 추가할 때는 목적과 작성 예시를 함께 기록합니다.

현재 01주차 Pong의 C++ / SFML 코드가 적용 기준입니다. GameObject·World·Subsystem 구조에서도 아래 규칙을 동일하게 사용합니다. 예시 클래스와 함수는 배치를 설명하기 위한 것이며 MovementComponent 등 과제 API를 고정하지 않습니다.

## 원본과 적용 방법

- 원본은 `main` 브랜치의 `convention/sfml.md`입니다.
- C++ 주차의 `convention.md`는 원본의 복사본입니다. 규칙을 바꿀 때는 main 원본을 먼저 수정한 뒤 해당 주차 복사본을 갱신합니다.
- Unity 주차에는 `convention/unity.md`를 복사합니다. 컨벤션 원본은 환경별로 따로 관리합니다.
- 기존 코드가 문서와 다르면 이후 코드 수정 시 맞춥니다. 이 문서 추가 자체가 기존 코드를 자동으로 변경하지는 않습니다.

## 함수·변수 이름

SFML의 함수·변수 명명 규칙을 유지합니다.

| 대상 | 규칙 | 예시 |
|---|---|---|
| 함수·멤버 함수 | lowerCamelCase | `tick()`, `getId()`, `onCollision()`, `createWindow()` |
| 매개변수·지역 변수 | lowerCamelCase | `deltaTime`, `windowSize`, `objectId` |
| 공개 데이터 멤버 | lowerCamelCase | `width`, `position`, `leftScore` |
| 비공개·보호 데이터 멤버 | `m_` + lowerCamelCase | `m_window`, `m_objectId`, `m_collisionManager` |

생성자·소멸자와 연산자 함수는 C++ 문법에 따른 이름을 사용합니다. 이 명명 규칙은 함수·변수에 관한 것이며 클래스 이름의 표기 변경을 요구하지 않습니다.

확인 근거: 포함된 SFML 3.1.0의 `SFML/System/Clock.hpp`는 `getElapsedTime()`과 비공개 멤버 `m_refPoint`를 사용합니다. SFML 라이브러리 원본은 수정하지 않습니다.

## cpp 작성 방식

### 처리 단계와 빈 줄

함수 안에서는 서로 이어지는 작업을 한 묶음으로 작성하고, 역할이 바뀌는 지점에 빈 줄 하나를 둡니다.

- 조회·준비, 유효성 검사, 실제 처리, 최종 반환을 구분합니다.
- 조기 반환·continue·break 뒤에 다른 처리가 이어지면 빈 줄을 둡니다.
- 같은 설정 객체의 필드를 연속으로 채우는 코드는 한 묶음으로 유지합니다.
- 선언과 사용이 긴밀하게 이어지는 경우에는 붙여도 됩니다. 모든 문장 사이에 빈 줄을 넣지는 않습니다.
- 함수 정의 사이에는 빈 줄 하나를 둡니다. 빈 줄에는 공백이나 탭을 넣지 않습니다.

```cpp
bool InputComponent::registerFloat(uint8 keyType, SfKey keyDec, SfKey keyInc)
{
    if (!validKey(keyDec)
        || !validKey(keyInc))
        return false;

    Binding binding;
    binding.type = ValueType::Float;
    binding.keyDecX = keyDec;
    binding.keyIncX = keyInc;

    return m_bindings.emplace(keyType, binding).second;
}
```

### 복합 조건 줄바꿈

수정하는 조건문에 `&&` 또는 `||`로 연결된 조건이 있으면 각 조건을 별도 줄에 씁니다. 첫 조건은 if와 같은 줄에 두고, 이후 조건은 한 단계 들여 쓴 줄의 **앞에 연산자**를 둡니다. 조건이 짧더라도 같은 형식을 사용합니다. 여러 조건으로 값을 계산하는 대입식에도 같은 방식을 적용합니다.

```cpp
if (nullptr == collider
    || !bounds.findIntersection(collider->getCollisionBounds()))
    continue;

if (first->isActive()
    && second->isActive())
    secondCollider->onCollision(*first);
```

### 조기 종료와 블록

유효하지 않거나 처리 대상이 아닌 경우에는 먼저 return·continue·break로 빠져나옵니다. 정상 처리를 불필요하게 중첩하지 않습니다. 종료 문장이 하나면 중괄호를 생략할 수 있지만, 본문은 반드시 다음 줄에 씁니다. 여러 문장을 수행하거나 내부에 조건문이 있으면 블록을 명시합니다.

```cpp
for (const auto& object : world.objects())
{
    if (!object->isActive())
        continue;

    if (const auto* drawable = dynamic_cast<const IRenderable*>(object.get()))
        drawable->render(target);
}
```

### 디버깅하기 쉬운 지역 변수

- 여러 조건으로 계산한 반환값은 `result` 같은 지역 변수로 빼서 중단점에서 확인할 수 있게 합니다. 단순 멤버 getter까지 기계적으로 분리하지 않습니다.
- 조회한 Subsystem은 `windowManager`, `timeManager`처럼 역할이 드러나는 이름으로 받습니다.
- auto는 초기화식에서 타입을 파악할 수 있을 때 사용합니다. 포인터는 `auto*`, 참조는 `auto&` 또는 `const auto&`로 표현합니다.
- const 오버로드로 위임할 때는 const 객체 포인터를 지역 변수로 분리해 호출 대상을 확인할 수 있게 합니다.

```cpp
bool InputComponent::getBoolState(uint8 keyType) const
{
    const auto found = m_bindings.find(keyType);
    auto result = found != m_bindings.end()
        && found->second.type == ValueType::Bool
        && found->second.state.x != 0.f;

    return result;
}
```

```cpp
auto* windowManager = getSubsystem<WindowManager>();
auto* timeManager = getSubsystem<TimeManager>();

if (!windowManager
    || !timeManager)
    return false;
```

### null 검사

캐스팅 결과처럼 포인터와 nullptr를 명시적으로 비교할 때는 수정된 CollisionManager 코드처럼 `nullptr == pointer` 순서를 사용합니다. 포인터의 존재 여부만 확인하는 `if (!pointer)`도 허용합니다. 두 표현이 실제 코드에 함께 있으므로 모든 null 검사를 한 표현으로 강제하지 않습니다.

### 반환값 무시와 주석

- 반환값이 필요 없는 일반 호출은 `m_clock.restart();`처럼 호출문만 씁니다. 의도 표시만을 위한 `(void)` 캐스트는 붙이지 않습니다. 반환값 사용을 요구하는 API의 진단이 발생하면 해당 계약을 확인하고 처리합니다.
- 코드 그대로를 반복하는 주석이나 비어 있는 함수의 상태를 설명하는 주석은 생략합니다. 빈 생명주기 함수는 빈 블록으로 둡니다.
- 구현 의도·제약·주의할 점이 필요한 곳에 주석을 씁니다. `src/todo/`의 과제 설명과 TODO는 한글로 작성하고 식별자·API 이름은 유지합니다.

```cpp
void TimeManager::reset()
{
    m_clock.restart();
    m_deltaTime = 0.f;
}

void WindowManager::initialize()
{
}
```

### 들여쓰기와 기본 공백

현재 cpp에는 탭과 공백 들여쓰기가 혼재합니다. 수정 범위에서는 해당 파일의 들여쓰기를 따르며, 문서 예시는 공백 네 칸으로 표시합니다. 혼재 자체를 새 규칙으로 삼거나 문서 변경을 이유로 전체 소스를 재정렬하지 않습니다. 대입 연산자 양옆과 인수 구분 쉼표 뒤에는 공백을 둡니다.

## 멤버 선언 순서

클래스·구조체의 멤버는 다음 **종류 순서**로 배치합니다.

1. 생성자·소멸자·할당 연산자 (`operator=`, 복사·이동 할당 포함)
2. 데이터 멤버
3. 나머지 연산자 (`operator==`, `operator[]` 등)
4. 일반 멤버 함수

각 종류 안에서는 **public → protected → private** 순서로 배치합니다. 비어 있는 구간은 생략합니다. 종류가 바뀌면 접근 지정자를 다시 선언할 수 있습니다. 전체 클래스를 public 하나, protected 하나, private 하나로만 묶어 종류 순서를 깨뜨리지 않습니다.

함수 배치의 우선순위는 **함수 종류 → 접근 제한자 → 템플릿 여부 → const 여부 → virtual/override 여부**입니다.

1. 앞서 정한 종류 순서(생성·소멸·할당 → 데이터 → 나머지 연산자 → 일반 함수)를 유지합니다.
2. 각 종류 안에서 `public → protected → private` 순서를 적용합니다.
3. 같은 접근 수준 안에서는 **템플릿 멤버 함수 → 비템플릿 멤버 함수** 순서로 배치합니다.
4. 각 템플릿·비템플릿 구간 안에서는 **const 멤버 함수 → 비const 멤버 함수** 순서로 배치합니다.
5. 같은 const 구간 안에서는 **일반 함수 → 새로 선언하는 virtual 함수 → override 함수** 순서로 배치합니다. virtual 여부 때문에 const·비const 구간을 섞지 않습니다. override와 함께 final을 쓴 함수도 override 구간에 둡니다. 멤버 함수 템플릿은 virtual로 선언할 수 없으므로 이 구분은 적용 가능한 함수에만 사용합니다.

따라서 같은 접근 수준의 일반 함수는 `템플릿 const → 템플릿 비const → 비템플릿 const → 비템플릿 비const` 순서입니다. 템플릿 비const 함수가 비템플릿 const 함수보다 먼저 옵니다. 템플릿·const 함수를 앞으로 모으려고 함수 종류나 접근 수준을 섞지 않습니다. 비어 있는 구간은 생략합니다.

여기서 const는 `getId() const`처럼 함수 선언 뒤에 붙는 한정자입니다. 반환형이나 매개변수의 const 여부를 뜻하지 않습니다. 같은 템플릿 구간의 const·비const 오버로드도 const 버전을 먼저 둡니다. 정적 함수는 const 멤버 함수가 될 수 없으므로 해당 템플릿 여부의 비const 구간에 둡니다. `.cpp`의 멤버 함수 정의도 헤더의 선언 순서에 맞춥니다.

`operator=`는 연산자이지만 3번이 아니라 1번에 둡니다. 명시적으로 `= default` 또는 `= delete`한 특별 멤버 함수도 같은 위치에 둡니다.

## 빈 줄

- 클래스·구조체 정의의 **첫 접근 지정자가 여는 중괄호 바로 다음에 오면 빈 줄을 두지 않습니다.**
- 첫 접근 지정자보다 먼저 `friend` 선언이 있다면, `friend` 선언과 첫 접근 지정자 사이에는 **빈 줄 하나를 허용**합니다. 필수는 아닙니다.
- 두 번째 이후 접근 지정자 `public:`, `protected:`, `private:` **바로 앞에는 빈 줄 하나**를 둡니다.
- 클래스·구조체 정의가 이어질 때 `};`와 다음 정의 사이에 **빈 줄 하나**를 둡니다.
- 첫 접근 지정자는 앞에 `friend` 선언이 있는지도 함께 확인합니다.
- `.cpp` 파일의 함수 정의와 다음 함수 정의 사이에는 **빈 줄 하나**를 둡니다. 생성자·소멸자·연산자·일반 함수 모두 적용하며, 한 줄로 작성한 정의도 동일합니다.

## 배치 예시

예시의 멤버는 배치 설명용이며 실제 엔진 API를 정의하지 않습니다.

```cpp
class Object
{
public:
    Object() : visible(1), m_active(1)
    {
    }
    virtual ~Object() = default;
    Object(const Object&) = default;
    Object& operator=(const Object&) = default;

protected:
    explicit Object(uint32 id) : visible(1), m_active(1), m_id(id)
    {
    }

public:
    uint8 visible : 1;

protected:
    uint8 m_active : 1;

private:
    uint32 m_id{0};

public:
    bool operator==(const Object& other) const
    {
        return m_id == other.m_id;
    }

public:
    uint32 getId() const
    {
        return m_id;
    }

protected:
    void setActive(bool active)
    {
        m_active = active;
    }

private:
    bool hasId() const
    {
        return m_id != 0;
    }
};

struct WindowSettings
{
public:
    uint32 width{800};
    uint32 height{600};
};
```

위 예시에서 public 구간이 반복되는 것은 의도한 배치입니다. 생성·소멸·할당 → 데이터 → 나머지 연산자 → 함수 순서를 먼저 지키고 각 구간에서 접근 수준을 정렬합니다.

### friend 선언이 먼저 있는 경우

`friend` 선언 뒤에서는 첫 접근 지정자 앞의 빈 줄을 허용합니다.

```cpp
class WindowManager
{
    friend class GameEngine;

public:
    WindowManager() = default;
};
```

### cpp 함수 정의 간격

```cpp
uint32 Object::getId() const
{
    return m_id;
}

void Object::tick(float deltaTime)
{
    // 이동과 상태를 갱신합니다.
}
```

### 템플릿·const 함수 우선 배치

같은 public 구간에서 템플릿 여부를 먼저 구분한 뒤, 각 구간에서 const 함수를 앞에 둡니다.

```cpp
class World
{
public:
    template <typename T>
    const T* findAs(uint32 id) const;
    template <typename T>
    T& spawn();
    uint32 getObjectCount() const;
    const Object* find(uint32 id) const;
    Object* find(uint32 id);
    void tick(float deltaTime);
    void clear();
};
```

## 멤버 변수 선언과 SFML 키 별칭

- 멤버 변수는 **한 줄에 한 개씩** 선언합니다. 같은 타입도 쉼표로 묶지 않습니다. 초기화 목록 안의 쉼표는 이 규칙의 대상이 아닙니다.
- 키 타입은 `using SfKey = sf::Keyboard::Key;` 별칭으로 간결하게 표기합니다.

```cpp
using SfKey = sf::Keyboard::Key;

struct PlayerKeyBindings
{
    SfKey up{};
    SfKey down{};
    SfKey fire{};
};
```

## 조건문·블록·예외 처리

- 한 문장짜리 if도 조건과 본문을 같은 줄에 쓰지 않습니다. 본문은 다음 줄에 한 단계 들여 씁니다. else·else if도 동일하게 적용합니다.
- 블록을 쓰는 경우 여는 중괄호와 닫는 중괄호는 각각 별도 줄에 둡니다. 블록 내용은 두 중괄호 사이 줄에 작성합니다. 함수·클래스·반복문·조건문·람다에 동일하게 적용하며 초기화 목록의 중괄호와는 구분합니다.
- try/catch는 최대한 사용하지 않습니다. 일반 분기는 조건·반환값으로 처리하고, 자원은 RAII와 소유권·연산 순서로 관리합니다. 오류를 그대로 재던지는 목적의 catch나 중첩 catch는 피합니다.
- 예외 복구·진단·최종 정리가 실제로 필요한 경계에서만 catch합니다. 초기화·실행 오류를 보고하고 Subsystem을 해제하는 엔진 경계의 catch는 유지합니다. catch를 줄이기 위해 오류 검증이나 실패 시 정리를 삭제하지 않습니다.

```cpp
if (!object)
    return;

if (object->isActive())
{
    object->tick(deltaTime);
}
else
{
    return;
}
```

## 멤버 크기·패딩·플래그

- 같은 데이터·접근 수준 구간에서는 **컨테이너 멤버 → 비컨테이너 멤버 → 작은 플래그** 순서로 배치합니다. vector·map·unordered_map·set 등과 문자열 저장소인 std::string도 컨테이너 구간에 둡니다.
- 각 구간 안에서는 정렬 요구(alignof)가 큰 타입을 먼저 배치하고, 같은 정렬 요구에서는 크기(sizeof)가 큰 타입을 우선합니다. 컨테이너 우선 규칙을 지키면서 패딩을 고려합니다.
- 소멸 순서 등 수명 의존성이 필요한 경우 이를 우선하며 이유를 주석으로 남깁니다. 멤버는 선언 역순으로 소멸하므로 다른 멤버가 참조하는 소유자를 먼저 선언합니다.
- 저장용 참·거짓 멤버는 bool 대신 `uint8`(unsigned char)의 1비트 필드로 선언합니다. 반환형·지역 변수의 bool까지 바꾸지는 않습니다.
- `using uint8 = unsigned char;`는 `base/Types.hpp`에서 제공합니다. 비트필드도 한 줄에 하나씩 씁니다.
- C++17에서는 비트필드를 생성자 초기화 목록에서 초기화합니다. 접근 제한자·멤버 종류의 기존 우선순위는 유지합니다.
- 패딩 감소는 대상 빌드에서 sizeof·alignof로 확인합니다. 비트필드의 바이트 배치를 파일 형식으로 쓰지 않으며, 파일 직렬화는 명시적인 필드 값으로 처리합니다.

```cpp
#include "base/Types.hpp"

struct PlayerInputState
{
    PlayerInputState() : up(0), down(0), fire(0)
    {
    }

    uint8 up : 1;
    uint8 down : 1;
    uint8 fire : 1;
};
```

## 크기가 큰 소유 멤버

World, 렌더 윈도우, SFML 셰이프, 입력 컴포넌트처럼 크기가 큰 객체를 단독 소유할 때는 `std::unique_ptr<T>`로 보관합니다. 작은 좌표·색상·설정값·MovementComponent는 값으로 유지합니다. GameRule처럼 기반 클래스로 파생 객체를 소유하는 경우에도 unique_ptr를 사용하고 기반 클래스에 virtual 소멸자를 둡니다.

소유하지 않는 필수 World 연결은 `World&`로 표현합니다. 참조하는 객체보다 World가 오래 살아 있어야 하며, World와 Subsystem의 선언 순서도 이 수명 조건을 지킵니다. 이미 unique_ptr를 원소로 갖는 컨테이너는 직접 멤버로 유지합니다.

## 정수 타입 별칭

프로젝트 내부의 정수 선언과 캐스팅에는 `base/Types.hpp`의 고정 폭 별칭을 사용합니다.

| 비트 수 | 부호 있음 | 부호 없음 |
|---|---|---|
| 8 | int8 | uint8 |
| 16 | int16 | uint16 |
| 32 | int32 | uint32 |
| 64 | int64 | uint64 |

- uint8은 unsigned char이며 나머지는 표준 고정 폭 정수 타입의 별칭입니다. 정의 지점에서는 표준 타입을 사용하고 실제 폭은 static_assert로 검증합니다.
- 일반 정수·크기·좌표·ID의 의미와 필요한 범위에 맞는 폭을 선택합니다. 현재 Windows x64 구성의 컨테이너 인덱스와 개수에는 uint64를 사용합니다.
- 참·거짓 반환값과 지역 조건은 bool을 유지하고, 저장용 플래그 멤버는 앞서 정한 uint8 비트필드를 사용합니다. 문자·문자열 타입과 외부 라이브러리 원본은 바꾸지 않습니다.

## 전방 선언 namespace

namespace도 여는·닫는 중괄호를 각각 별도 줄에 둡니다. 여는 중괄호와 첫 선언 사이, 마지막 선언과 닫는 중괄호 사이에는 불필요한 빈 줄을 넣지 않습니다. 전방 선언만 있어도 한 줄로 축약하지 않습니다.

```cpp
namespace sf
{
    class RenderTarget;
}
```

## virtual·override 함수 순서 예시

아래는 Base에 render/update가 선언되어 있다고 가정한 함수 배치 예시입니다. const 판단을 먼저 하므로 const override가 비const 일반 함수보다 앞에 옵니다.

```cpp
class Example : public Base
{
public:
    uint32 getId() const;
    virtual bool isVisible() const;
    void render() const override;
    void reset();
    virtual void refresh();
    void update() override;
};
```
