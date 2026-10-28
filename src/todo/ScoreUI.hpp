#pragma once
#include "PongBody.hpp"

class ScoreUI : public PongBody
{
public:
    explicit ScoreUI(World& world);

public:
    void render(sf::RenderTarget& target) const override;
};
