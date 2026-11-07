#pragma once
#include "base/SfTypes.hpp"

class PongBody;

class MovementComponent
{
public:
	MovementComponent(PongBody* parent);

private:
	PongBody* parent;
	float2 m_direction;
	float m_speed;

	inline static float g_epsilon = 0.0001f;

public:
	float2 GetDirection() const;
	float GetSpeed() const;

	void Move(float deltaTime);
	void SetVelocity(float2 velocity);
	void SetDirection(float2 direction);
	void SetSpeed(float speed);
};
