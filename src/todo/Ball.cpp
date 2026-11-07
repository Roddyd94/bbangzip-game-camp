#include "Ball.hpp"
#include "Paddle.hpp"
#include "PongGameRule.hpp"
#include "core/World.hpp"
#include "base/Utility.hpp"
#include <SFML/Graphics/RenderTarget.hpp>
#include <algorithm>
#include <iomanip>
#include <limits>
#include <sstream>

Ball::Ball(World& world, const BallSettings& settings)
	: PongBody(world), m_shape(std::make_unique<sf::CircleShape>()),
	m_settings(settings), m_previousPosition(settings.startPosition),
	m_movement(std::make_unique<MovementComponent>(this)),
	m_isBallBounced(false), m_readyToResetBounceFlag(false)
{
	m_position = settings.startPosition;
	m_movement->SetSpeed(m_settings.speed);
	m_movement->SetDirection({ 1.f, 1.f });
}

const BallSettings& Ball::getSettings() const
{
	return m_settings;
}

const MovementComponent* Ball::getMovement() const
{
	return m_movement.get();
}

sf::Vector2f Ball::getPreviousPosition() const
{
	return m_previousPosition;
}

void Ball::render(sf::RenderTarget& target) const
{
	if (!m_shouldRender)
		return;

	sf::RenderStates states;
	states.transform.translate(m_position);
	target.draw(*m_shape, states);
}

void Ball::serialize(std::ostream& output) const
{
	PongBody::serialize(output);
}

sf::FloatRect Ball::getCollisionBounds() const
{
	// 충돌 후보 영역에는 프레임 끝의 원뿐 아니라 전체 이동 경로가 포함되어야 합니다.
	const auto radius = m_size / 2.f;
	const sf::Vector2f minimum{ std::min(m_previousPosition.x, m_position.x),
							 std::min(m_previousPosition.y, m_position.y) };
	const sf::Vector2f maximum{ std::max(m_previousPosition.x, m_position.x),
							 std::max(m_previousPosition.y, m_position.y) };
	return { minimum - radius, maximum - minimum + m_size };
}

MovementComponent* Ball::getMovement()
{
	return m_movement.get();
}

void Ball::tick(float deltaTime)
{
	if (m_isBallBounced)
	{
		if (!m_readyToResetBounceFlag)
			m_readyToResetBounceFlag = true;
		else
		{
			m_isBallBounced = false;
			m_readyToResetBounceFlag = false;
		}
	}

	m_previousPosition = m_position;

	if (m_waitForRespawn >= 0.f)
	{
		m_waitForRespawn -= deltaTime;

		if (m_waitForRespawn < 0.f)
		{
			PongGameRule* gameRule = dynamic_cast<PongGameRule*>(m_world.getGameRule());
			if (!gameRule)
				return;

			m_shouldRender = true;
			m_movement->SetSpeed(m_settings.speed);
			m_position.x = gameRule->getBoundary().getCenter().x;
		}

		return;
	}

	// TODO: MovementComponent로 위치를 갱신하고 레벨 벽 충돌·득점을 처리하세요.
	m_movement->Move(deltaTime);

	PongGameRule* gameRule = dynamic_cast<PongGameRule*>(m_world.getGameRule());
	if (!gameRule)
		return;

	sf::FloatRect boundary = gameRule->getBoundary();

	// 벽 충돌
	if (m_position.y < boundary.position.y)
	{
		auto direction = m_movement->GetDirection();
		direction.y = std::abs(direction.y);
		m_movement->SetDirection(direction);

		m_position.y = boundary.position.y * 2 - m_position.y;
	}
	else if (m_position.y > boundary.position.y + boundary.size.y - m_size.y)
	{
		auto direction = m_movement->GetDirection();
		direction.y = -std::abs(direction.y);
		m_movement->SetDirection(direction);

		m_position.y = (boundary.position.y + boundary.size.y - m_size.y) * 2 - m_position.y;
	}

	// 득점
	if (m_position.x < boundary.position.x
		|| m_position.x > boundary.position.x + boundary.size.x - m_size.x)
	{
		m_movement->SetSpeed(0.f);
		m_waitForRespawn = gameRule->getRespawnDelay();
		m_shouldRender = false;

		if (m_position.x < boundary.position.x)
			gameRule->recordScore(false);
		else
			gameRule->recordScore(true);
	}
}

void Ball::onCollision(Object& other)
{
	// TODO: m_previousPosition에서 m_position까지의 선분과 패들 면의 충돌을 검사하세요.
	// 이동 경로 AABB의 겹침은 후보일 뿐 실제 패들 명중을 의미하지 않습니다.
	// TODO: 최초 접촉을 선택하고 패들 중심과 접촉점의 거리로 45~135도 반사를 계산하세요.
	//       충돌 한 번당 속력을 한 번 증가시키고 패들 뒤로 넘어간 뒤에는 반사하지 마세요.

	if (m_readyToResetBounceFlag)
		m_readyToResetBounceFlag = false;

	if (m_isBallBounced)
		return;

	Paddle* paddle = dynamic_cast<Paddle*>(&other);
	if (!paddle)
		return;

	sf::FloatRect paddleBounds = paddle->getCollisionBounds();

	float2 ballCenterPos = m_position + m_size / 2.f;
	float ballRadius = m_size.x / 2.f;

	bool isBallTouchingPaddle
		= paddleBounds.position.x - ballCenterPos.x <= ballRadius
		&& paddleBounds.position.y - ballCenterPos.y <= ballRadius
		&& ballCenterPos.x - (paddleBounds.position.x + paddleBounds.size.x) <= ballRadius
		&& ballCenterPos.y - (paddleBounds.position.y + paddleBounds.size.y) <= ballRadius;

	if (!isBallTouchingPaddle)
		return;

	m_isBallBounced = true;

	PongGameRule* gameRule = dynamic_cast<PongGameRule*>(m_world.getGameRule());
	if (!gameRule)
		return;

	float t = (ballCenterPos.y - paddleBounds.getCenter().y) / (paddleBounds.getCenter().y - paddleBounds.position.y);
	float speedX = paddle->getPosition().x > gameRule->getBoundary().getCenter().x ? -1.f : 1.f;
	float speedY = utility::lerp(0.f, 1.f, t);
	m_movement->SetSpeed(m_movement->GetSpeed() + m_settings.collisionSpeedIncrease);
	m_movement->SetDirection({ speedX, speedY });
}

void Ball::deserialize(std::istream& input)
{
	// start <너비> <높이> <R> <G> <B> <A>는 ball.ini의 시작 위치를 사용합니다.
	// 숫자 x/y는 저장된 현재 위치이며 부모 클래스에서 읽습니다.
	std::string x;
	if (!(input >> x))
	{
		input.setstate(std::ios::failbit);
		return;
	}
	std::ostringstream expanded;
	expanded.imbue(std::locale::classic());
	expanded << std::setprecision(std::numeric_limits<float>::max_digits10);
	if (x == "start")
		expanded << m_settings.startPosition.x << ' ' << m_settings.startPosition.y << ' ';
	else
		expanded << x << ' ';
	expanded << input.rdbuf();
	std::istringstream record(expanded.str());
	record.imbue(std::locale::classic());
	// 잘못된 데이터가 현재 객체를 바꾸지 않도록 임시 Ball에서 먼저 검증합니다.
	Ball loaded(m_world, m_settings);
	loaded.PongBody::deserialize(record);
	if (record.fail()
		|| loaded.m_size.x != loaded.m_size.y)
	{
		input.setstate(std::ios::failbit);
		return;
	}
	record >> std::ws;
	if (!record.eof())
	{
		input.setstate(std::ios::failbit);
		return;
	}
	const auto* gameRule = dynamic_cast<const PongGameRule*>(m_world.getGameRule());

	if (nullptr == gameRule)
	{
		input.setstate(std::ios::failbit);
		return;
	}

	const auto& boundary = gameRule->getBoundary();
	const auto radiusSize = loaded.m_size / 2.f;
	if (x == "start")
	{
		const auto minimum = loaded.m_position - radiusSize;
		const auto maximum = loaded.m_position + radiusSize;
		const auto levelMaximum = boundary.position + boundary.size;
		if (minimum.x < boundary.position.x
			|| minimum.y < boundary.position.y
			|| maximum.x > levelMaximum.x
			|| maximum.y > levelMaximum.y)
		{
			input.setstate(std::ios::failbit);
			return;
		}
	}
	m_position = loaded.m_position;
	m_size = loaded.m_size;
	m_color = loaded.m_color;
	m_previousPosition = m_position;
	const float radius = m_size.x / 2.f;
	m_shape->setRadius(radius);
	m_shape->setOrigin({ radius, radius });
	m_shape->setFillColor(m_color);
}
