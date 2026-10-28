#include "TimeManager.hpp"
#include <SFML/System/Time.hpp>

TimeManager::TimeManager()
    : m_initialized(0)
{
}

float TimeManager::getDeltaTime() const
{
    return m_deltaTime;
}

void TimeManager::initialize()
{
    reset();
    m_initialized = 1;
}

void TimeManager::deinitialize() noexcept
{
    m_deltaTime = 0.f;
    m_initialized = 0;
}

void TimeManager::reset()
{
    m_clock.restart();
    m_deltaTime = 0.f;
}

void TimeManager::update()
{
    if (!m_initialized)
        return;

    m_deltaTime = m_clock.restart().asSeconds();
}
