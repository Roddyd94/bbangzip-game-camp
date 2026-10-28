#include "PongGameRule.hpp"

PongGameRule::PongGameRule(const sf::FloatRect& boundary) : m_boundary(boundary)
{
}

const sf::FloatRect& PongGameRule::getBoundary() const
{
    return m_boundary;
}

float PongGameRule::getRespawnDelay() const
{
    return m_respawnDelay;
}

uint32 PongGameRule::getLeftScore() const
{
    return m_leftScore;
}

uint32 PongGameRule::getRightScore() const
{
    return m_rightScore;
}

void PongGameRule::recordScore(bool leftPlayer)
{
    if (leftPlayer)
        ++m_leftScore;
    else
        ++m_rightScore;
}

void PongGameRule::reset()
{
    m_leftScore = 0;
    m_rightScore = 0;
}
