#include "Ball.hpp"
#include "PongGameRule.hpp"
#include "core/World.hpp"
#include <SFML/Graphics/RenderTarget.hpp>
#include <algorithm>
#include <iomanip>
#include <limits>
#include <sstream>

Ball::Ball(World& world, const BallSettings& settings)
	: PongBody(world), m_shape(std::make_unique<sf::CircleShape>()), m_settings(settings), m_previousPosition(settings.startPosition)
{
	m_position = settings.startPosition;
	// TODO: m_movement를 초기화하세요.
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
	m_previousPosition = m_position;
	// TODO: MovementComponent로 위치를 갱신하고 레벨 벽 충돌·득점을 처리하세요.
	// TODO: 3초 대기 후 득점 접촉점을 중심선에 사영하여 리스폰하세요.
	//       운동 방향을 유지하고 설정된 초기 속력으로 복원하세요.
	(void)deltaTime;
}

void Ball::onCollision(Object& other)
{
	// TODO: m_previousPosition에서 m_position까지의 선분과 패들 면의 충돌을 검사하세요.
	// 이동 경로 AABB의 겹침은 후보일 뿐 실제 패들 명중을 의미하지 않습니다.
	// TODO: 최초 접촉을 선택하고 패들 중심과 접촉점의 거리로 45~135도 반사를 계산하세요.
	//       충돌 한 번당 속력을 한 번 증가시키고 패들 뒤로 넘어간 뒤에는 반사하지 마세요.
	(void)other;
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
