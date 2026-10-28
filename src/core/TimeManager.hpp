#pragma once
#include "Subsystem.hpp"
#include "base/Types.hpp"
#include <SFML/System/Clock.hpp>

class TimeManager : public Subsystem
{
public:
    TimeManager();

private:
    sf::Clock m_clock;
    float m_deltaTime{0.f};
    uint8 m_initialized : 1;

public:
    float getDeltaTime() const;
    void initialize() override;
    void deinitialize() noexcept override;
    void reset();
    void update();
};
