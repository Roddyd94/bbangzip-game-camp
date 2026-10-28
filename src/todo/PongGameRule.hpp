#pragma once
#include "core/GameRule.hpp"
#include "base/Types.hpp"
#include <SFML/Graphics/Rect.hpp>

class PongGameRule : public GameRule
{
public:
    explicit PongGameRule(const sf::FloatRect& boundary);

private:
    sf::FloatRect m_boundary;
    float m_respawnDelay{3.f};
    uint32 m_leftScore{0};
    uint32 m_rightScore{0};

public:
    const sf::FloatRect& getBoundary() const;
    float getRespawnDelay() const;
    uint32 getLeftScore() const;
    uint32 getRightScore() const;
    void recordScore(bool leftPlayer);
    void reset();
};
