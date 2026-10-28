#pragma once
#include "Object.hpp"
#include "GameRule.hpp"
#include <filesystem>
#include <functional>
#include <string>
#include <memory>
#include <utility>
#include <vector>

class World
{
private:
    // 객체 소멸 중에도 규칙을 참조할 수 있도록 객체 컨테이너보다 먼저 선언합니다.
    std::unique_ptr<GameRule> m_gameRule;
    std::vector<std::unique_ptr<Object>> m_objects;
    std::vector<std::unique_ptr<Object>> m_pending;

public:
    template<class T, class... Args>
    T& spawn(Args&&... args)
    {
        auto object = std::make_unique<T>(std::forward<Args>(args)...);
        T& result = *object;
        m_pending.push_back(std::move(object));
        return result;
    }

    const GameRule* getGameRule() const
    {
        return m_gameRule.get();
    }

    GameRule* getGameRule()
    {
        return m_gameRule.get();
    }

    void setGameRule(std::unique_ptr<GameRule> gameRule)
    {
        m_gameRule = std::move(gameRule);
    }

    bool isActive(Object::Id id) const;
    Object* find(Object::Id id) const;
    const std::vector<std::unique_ptr<Object>>& objects() const
    {
        return m_objects;
    }

    bool initialize(const std::filesystem::path& file,
        const std::function<std::unique_ptr<Object>(const std::string&)>& createObject);
    void destroy(Object::Id id);
    void tick(float dt);

    void flush();
};
