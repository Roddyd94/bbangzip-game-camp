#include "Paddle.hpp"
#include "PongGameRule.hpp"
#include "core/World.hpp"
#include <SFML/Graphics/RenderTarget.hpp>
#include <istream>
#include <ostream>

Paddle::Paddle(World& world, const PaddleSettings& settings, const InputSettings& bindings)
	: PongBody(world), m_shape(std::make_unique<sf::RectangleShape>()),
	m_input(std::make_unique<InputComponent>()), m_bindings(bindings)
{
	// TODO: m_movement를 초기화하세요.
}

const MovementComponent* Paddle::getMovement() const
{
	return m_movement.get();
}

const InputComponent* Paddle::getInputComponent() const
{
	return m_input.get();
}

void Paddle::render(sf::RenderTarget& target) const
{
	if (!m_shouldRender)
		return;

	sf::RenderStates states;
	states.transform.translate(m_position);
	target.draw(*m_shape, states);
}

void Paddle::serialize(std::ostream& output) const
{
	output << uint32(m_player) << ' ';
	PongBody::serialize(output);
}

sf::FloatRect Paddle::getCollisionBounds() const
{
	return { m_position, m_size };
}

MovementComponent* Paddle::getMovement()
{
	return m_movement.get();
}

InputComponent* Paddle::getInputComponent()
{
	return m_input.get();
}

void Paddle::tick(float deltaTime)
{
	m_input->update();
	const float moveInput = m_input->getKeyState(static_cast<uint8>(PaddleKey::Move));
	// TODO: moveInput(-1: 위, +1: 아래)으로 이동 방향을 정하고 변위를 적용하세요.
	// TODO: 기준점뿐 아니라 패들 전체가 World의 PongGameRule 경계 안에 있도록 위치를 제한하세요.
}

void Paddle::deserialize(std::istream& input)
{
	uint32 player;
	if (!(input >> player)
		|| (player != 1
			&& player != 2))
	{
		input.setstate(std::ios::failbit);
		return;
	}

	PongBody::deserialize(input);
	if (input.fail())
		return;
	const auto& keys = player == 1 ? m_bindings.player1 : m_bindings.player2;
	InputComponent mappings;
	if (!mappings.registerFloat(static_cast<uint8>(PaddleKey::Move), keys.up, keys.down)
		|| !mappings.registerBool(static_cast<uint8>(PaddleKey::Fire), keys.fire)
		|| !mappings.registerBool(static_cast<uint8>(PaddleKey::Reset), m_bindings.global.reset))
	{
		input.setstate(std::ios::failbit);
		return;
	}
	m_player = static_cast<uint8>(player);
	*m_input = std::move(mappings);
	m_shape->setSize(m_size);
	m_shape->setFillColor(m_color);
}
