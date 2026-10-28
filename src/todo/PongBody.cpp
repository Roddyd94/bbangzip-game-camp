#include "base/Types.hpp"
#include "PongBody.hpp"
#include <SFML/Graphics.hpp>
#include <cmath>
#include <iomanip>
#include <limits>
#include <sstream>

sf::Vector2f PongBody::getPosition() const
{
    return m_position;
}

sf::Vector2f PongBody::getSize() const
{
    return m_size;
}

void PongBody::render(sf::RenderTarget& target) const
{
    // 기본 렌더링은 중앙선 같은 정적 장식에만 사용합니다.
    sf::RectangleShape shape(m_size);
    shape.setPosition(m_position);
    shape.setFillColor(m_color);
    target.draw(shape);
}

void PongBody::serialize(std::ostream& output) const
{
    std::ostringstream record;
    record.imbue(std::locale::classic());
    record << std::setprecision(std::numeric_limits<float>::max_digits10)
           << m_position.x << ' ' << m_position.y << ' '
           << m_size.x << ' ' << m_size.y << ' '
           << uint32(m_color.r) << ' ' << uint32(m_color.g) << ' '
           << uint32(m_color.b) << ' ' << uint32(m_color.a);
    output << record.str();
}

void PongBody::setPosition(sf::Vector2f position)
{
    m_position = position;
}

void PongBody::deserialize(std::istream& input)
{
    sf::Vector2f position;
    sf::Vector2f size;
    int32 red;
    int32 green;
    int32 blue;
    int32 alpha;
    if (!(input >> position.x >> position.y >> size.x >> size.y
          >> red >> green >> blue >> alpha))
    {
        input.setstate(std::ios::failbit);
        return;
    }
    if (!std::isfinite(position.x)
        || !std::isfinite(position.y)
        || !std::isfinite(size.x)
        || !std::isfinite(size.y)
        || size.x <= 0
        || size.y <= 0
        || red < 0
        || red > 255
        || green < 0
        || green > 255
        || blue < 0
        || blue > 255
        || alpha < 0
        || alpha > 255)
    {
        input.setstate(std::ios::failbit);
        return;
    }
    m_position = position;
    m_size = size;
    m_color = sf::Color(static_cast<uint8>(red), static_cast<uint8>(green),
                        static_cast<uint8>(blue), static_cast<uint8>(alpha));
}
