#pragma once
#include "World.hpp"
#include "Subsystem.hpp"
#include "base/Types.hpp"
#include "InputComponent.hpp"
#include <functional>
#include <typeindex>
#include <type_traits>
#include <unordered_map>
#include <string>

namespace sf
{
    class RenderTarget;
}

struct EngineSettings
{
    std::string title{"Study Game"};
    uint32 width{800};
    uint32 height{600};
    uint32 frameLimit{60};
};

enum class GlobalInputKey : uint8
{
    Reset,
    Close
};

class GameEngine
{
public:
    GameEngine();
    ~GameEngine();
    GameEngine(const GameEngine&) = delete;
    GameEngine& operator=(const GameEngine&) = delete;
    GameEngine(GameEngine&&) = delete;
    GameEngine& operator=(GameEngine&&) = delete;

private:
    std::vector<std::unique_ptr<Subsystem>> m_subsystems;
    std::unordered_map<std::type_index, Subsystem*> m_subsystemTypes;

    std::unique_ptr<World> m_world;
    std::unique_ptr<InputComponent> m_input;
    EngineSettings m_settings;
    uint64 m_initializedCount{0};
    uint8 m_initializing : 1;
    uint8 m_deinitializing : 1;
    uint8 m_ready : 1;

public:
    template<class T>
    const T* getSubsystem() const
    {
        static_assert(std::is_base_of_v<Subsystem, T>);

        const auto found = m_subsystemTypes.find(std::type_index(typeid(T)));
        if (found == m_subsystemTypes.end())
            return nullptr;

        return static_cast<const T*>(found->second);
    }

    template<class T>
    T* getSubsystem()
    {
        const GameEngine* engine = this;
        return const_cast<T*>(engine->getSubsystem<T>());
    }

    template<class T, class... Args>
    T* addSubsystem(Args&&... args)
    {
        static_assert(std::is_base_of_v<Subsystem, T>);
        if (m_initializing || m_deinitializing)
            return nullptr;

        const std::type_index type(typeid(T));
        if (m_subsystemTypes.count(type))
            return nullptr;

        auto subsystem = std::make_unique<T>(std::forward<Args>(args)...);
        auto* result = subsystem.get();

        if (m_subsystems.size() == m_subsystems.capacity())
        {
            const uint64 capacity = m_subsystems.capacity();
            const uint64 maximum = m_subsystems.max_size();
            if (capacity == maximum)
                return nullptr;
            const uint64 nextCapacity = capacity == 0 ? 1
                : (capacity > maximum / 2 ? maximum : capacity * 2);
            m_subsystems.reserve(nextCapacity);
        }

        m_subsystemTypes.emplace(type, result);
        m_subsystems.push_back(std::move(subsystem));

        return result;
    }

    const InputComponent* getInputComponent() const
    {
        return m_input.get();
    }
    World& getWorld()
    {
        return *m_world;
    }

    bool initialize();
    void deinitialize() noexcept;
    bool step(float dt, sf::RenderTarget& target);
    int32 run();

private:
    bool initializeWorld();
};

extern GameEngine g_engine;
