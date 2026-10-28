#pragma once
#include "PongBody.hpp"
#include <memory>
#include "MovementComponent.hpp"
#include <SFML/Graphics/CircleShape.hpp>

struct BallSettings
{
    sf::Vector2f startPosition{};
    float speed{0.f};
    float collisionSpeedIncrease{0.f};
};

class Ball : public PongBody
{
public:
    Ball(World& world, const BallSettings& settings);

private:
    std::unique_ptr<sf::CircleShape> m_shape;
    BallSettings m_settings;
    MovementComponent m_movement;
    sf::Vector2f m_previousPosition{};
    // TODO: 필요한 멤버 변수를 추가하세요.

public:
    const BallSettings& getSettings() const;
    const MovementComponent& getMovement() const;
    sf::Vector2f getPreviousPosition() const;
    void render(sf::RenderTarget& target) const override;
    void serialize(std::ostream& output) const override;
    sf::FloatRect getCollisionBounds() const override;
    MovementComponent& getMovement();
    void tick(float deltaTime) override;
    void onCollision(Object& other) override;
    void deserialize(std::istream& input) override;
};
