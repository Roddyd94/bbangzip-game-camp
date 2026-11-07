#include "MovementComponent.hpp"
#include "PongBody.hpp"
#include <cmath>

MovementComponent::MovementComponent(PongBody* parent)
	: parent(parent)
{
}

float2 MovementComponent::GetDirection() const
{
	return m_direction;
}

float MovementComponent::GetSpeed() const
{
	return m_speed;
}

void MovementComponent::Move(float deltaTime)
{
	parent->m_position += m_direction * m_speed * deltaTime;
}

void MovementComponent::SetVelocity(float2 velocity)
{
	m_speed = velocity.length();

	if (m_speed < g_epsilon)
		m_direction = { 0.f, 1.f };
	else
		m_direction = velocity.normalized();
}

void MovementComponent::SetDirection(float2 direction)
{
	if (std::abs(direction.x) < g_epsilon && std::abs(direction.y) < g_epsilon)
		m_direction = { 0.f, 0.f };
	else
		m_direction = direction.normalized();
}

void MovementComponent::SetSpeed(float speed)
{
	m_speed = speed;
}
