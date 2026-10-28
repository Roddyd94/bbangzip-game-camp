#pragma once
#include "core/Object.hpp"
#include "core/ITickable.hpp"
#include "core/ICollidable.hpp"
#include "core/IRenderable.hpp"
#include "core/ISerializable.hpp"

class World;

class GameObject : public Object, public ITickable, public IRenderable, public ICollidable, public ISerializable
{
public:
    explicit GameObject(World& world);

protected:
    World& m_world;

public:
    void render(sf::RenderTarget& target) const override;
    void serialize(std::ostream& output) const override;
    sf::FloatRect getCollisionBounds() const override;
    void tick(float deltaTime) override;
    void onCollision(Object& other) override;
    void deserialize(std::istream& input) override;
};
