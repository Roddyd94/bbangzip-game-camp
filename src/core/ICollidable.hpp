#pragma once
#include <SFML/Graphics/Rect.hpp>

class Object;

class ICollidable
{
public:
    virtual ~ICollidable() = default;

public:
    virtual sf::FloatRect getCollisionBounds() const = 0;
    virtual void onCollision(Object& other) = 0;
};
