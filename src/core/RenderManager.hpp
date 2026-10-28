#pragma once
#include "Subsystem.hpp"
#include <SFML/Graphics/Color.hpp>

namespace sf
{
    class RenderTarget;
}

class World;

class RenderManager : public Subsystem
{
private:
    sf::Color m_clear{0, 0, 0};

public:
    void render(const World& world, sf::RenderTarget& target) const;
    void initialize() override;
    void deinitialize() noexcept override;
    void setClearColor(sf::Color color)
    {
        m_clear = color;
    }
};
