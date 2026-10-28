#include "GameObject.hpp"

GameObject::GameObject(World& world) : m_world(world)
{
}

void GameObject::render(sf::RenderTarget&) const
{
}

void GameObject::serialize(std::ostream&) const
{
}

sf::FloatRect GameObject::getCollisionBounds() const
{
    return {{0.f, 0.f}, {0.f, 0.f}};
}

void GameObject::tick(float)
{
}

void GameObject::onCollision(Object&)
{
}

void GameObject::deserialize(std::istream&)
{
}
