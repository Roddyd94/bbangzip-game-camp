#pragma once
#include "GameObject.hpp"
#include <SFML/Graphics/Color.hpp>

class PongBody : public GameObject
{
public:
	explicit PongBody(World& world) : GameObject(world), m_shouldRender(true)
	{
	}

protected:
	sf::Vector2f m_position{};
	sf::Vector2f m_size{ 16.f, 16.f };
	sf::Color m_color;
	uint8 m_shouldRender : 1;

public:
	sf::Vector2f getPosition() const;
	sf::Vector2f getSize() const;
	void render(sf::RenderTarget& target) const override;
	void serialize(std::ostream& output) const override;
	void setPosition(sf::Vector2f position);
	void deserialize(std::istream& input) override;
};
