#pragma once

namespace sf
{
    class RenderTarget;
}

class IRenderable
{
public:
    virtual ~IRenderable() = default;

public:
    virtual void render(sf::RenderTarget& target) const = 0;
};
