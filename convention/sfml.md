# SFML / C++ 코딩 컨벤션

이 문서는 스터디에서 합의한 규칙을 기억날 때마다 추가·수정하는 문서입니다. Windows 콘솔 C++ 주차에도 공통 C++ 규칙을 동일하게 적용합니다.

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

## 멤버 선언 순서

클래스·구조체의 멤버는 다음 **종류 순서**로 배치합니다.

1. 생성자·소멸자·할당 연산자 (`operator=`, 복사·이동 할당 포함)
2. 데이터 멤버
3. 나머지 연산자 (`operator==`, `operator[]` 등)
4. 일반 멤버 함수

각 종류 안에서는 **public → protected → private** 순서로 배치합니다. 비어 있는 구간은 생략합니다. 종류가 바뀌면 접근 지정자를 다시 선언할 수 있습니다. 전체 클래스를 public 하나, protected 하나, private 하나로만 묶어 종류 순서를 깨뜨리지 않습니다.

`operator=`는 연산자이지만 3번이 아니라 1번에 둡니다. 명시적으로 `= default` 또는 `= delete`한 특별 멤버 함수도 같은 위치에 둡니다.

## 빈 줄

- 모든 접근 지정자 `public:`, `protected:`, `private:` **바로 앞에 빈 줄 하나**를 둡니다. 정의의 첫 접근 지정자 앞도 같습니다.
- 클래스·구조체 정의가 이어질 때 `};`와 다음 정의 사이에 **빈 줄 하나**를 둡니다.
- 아래 예시처럼 별도 주석이 필요하다면 접근 지정자 앞의 빈 줄 규칙을 유지합니다.

## 배치 예시

예시의 멤버는 배치 설명용이며 실제 엔진 API를 정의하지 않습니다.

```cpp
class Object
{

public:
    Object() = default;
    virtual ~Object() = default;
    Object(const Object&) = default;
    Object& operator=(const Object&) = default;

protected:
    explicit Object(unsigned id) : m_id(id) {}

public:
    bool visible{true};

protected:
    bool m_active{true};

private:
    unsigned m_id{0};

public:
    bool operator==(const Object& other) const
    {
        return m_id == other.m_id;
    }

public:
    unsigned getId() const { return m_id; }

protected:
    void setActive(bool active) { m_active = active; }

private:
    bool hasId() const { return m_id != 0; }
};

struct WindowSettings
{

public:
    unsigned width{800};
    unsigned height{600};
};
```

위 예시에서 public 구간이 반복되는 것은 의도한 배치입니다. 생성·소멸·할당 → 데이터 → 나머지 연산자 → 함수 순서를 먼저 지키고 각 구간에서 접근 수준을 정렬합니다.
