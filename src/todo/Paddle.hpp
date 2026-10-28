#pragma once
#include "PongBody.hpp"
#include <memory>
#include "MovementComponent.hpp"
#include "core/InputComponent.hpp"
#include <SFML/Graphics/RectangleShape.hpp>

enum class PaddleKey : uint8
{
	Move,
	Fire,
	Reset
};

struct PaddleSettings
{
	float speed{ 0.f };
};

class Paddle : public PongBody
{
public:
	Paddle(World& world, const PaddleSettings& settings, const InputSettings& bindings);

private:
	std::unique_ptr<sf::RectangleShape> m_shape;
	std::unique_ptr<InputComponent> m_input;
	std::unique_ptr<MovementComponent> m_movement;
	InputSettings m_bindings;
	uint8 m_player{ 1 };

public:
	const MovementComponent* getMovement() const;
	const InputComponent* getInputComponent() const;
	void render(sf::RenderTarget& target) const override;
	void serialize(std::ostream& output) const override;
	sf::FloatRect getCollisionBounds() const override;
	MovementComponent* getMovement();
	InputComponent* getInputComponent();
	void tick(float deltaTime) override;
	void deserialize(std::istream& input) override;
};
