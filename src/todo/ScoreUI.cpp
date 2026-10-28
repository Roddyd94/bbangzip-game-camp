#include "ScoreUI.hpp"
#include "base/SfTypes.hpp"
#include "PongGameRule.hpp"
#include "core/World.hpp"
#include <SFML/Graphics/RectangleShape.hpp>
#include <SFML/Graphics/RenderTarget.hpp>
#include <array>
#include <string>

namespace
{
    // 위쪽부터 시계 방향의 외곽 여섯 선분과 중앙 선분으로 숫자를 표시합니다.
    constexpr std::array<uint8, 10> digitSegments{
        0x3f, 0x06, 0x5b, 0x4f, 0x66, 0x6d, 0x7d, 0x07, 0x7f, 0x6f
    };

    void drawNumber(sf::RenderTarget& target, uint32 score, float2 center,
        float2 digitSize, sf::Color color)
    {
        const auto text = std::to_string(score);
        const float thickness = digitSize.x / 5.f;
        const float spacing = digitSize.x + thickness * 2.f;
        const float width = float(text.size()) * spacing - thickness * 2.f;
        const float halfHeight = digitSize.y / 2.f;
        const std::array<sf::FloatRect, 7> segments{
            sf::FloatRect{{thickness, 0.f}, {digitSize.x - thickness * 2.f, thickness}},
            sf::FloatRect{{digitSize.x - thickness, thickness}, {thickness, halfHeight - thickness}},
            sf::FloatRect{{digitSize.x - thickness, halfHeight}, {thickness, halfHeight - thickness}},
            sf::FloatRect{{thickness, digitSize.y - thickness}, {digitSize.x - thickness * 2.f, thickness}},
            sf::FloatRect{{0.f, halfHeight}, {thickness, halfHeight - thickness}},
            sf::FloatRect{{0.f, thickness}, {thickness, halfHeight - thickness}},
            sf::FloatRect{{thickness, halfHeight - thickness / 2.f}, {digitSize.x - thickness * 2.f, thickness}}
        };
        float2 position{center.x - width / 2.f, center.y};
        sf::RectangleShape shape;
        shape.setFillColor(color);

        for (const char digit : text)
        {
            const uint8 mask = digitSegments[static_cast<uint8>(digit - '0')];

            for (uint8 index = 0; index < segments.size(); ++index)
            {
                if ((mask & (1u << index)) == 0)
                    continue;

                shape.setPosition(position + segments[index].position);
                shape.setSize(segments[index].size);
                target.draw(shape);
            }

            position.x += spacing;
        }
    }
}

ScoreUI::ScoreUI(World& world) : PongBody(world)
{
}

void ScoreUI::render(sf::RenderTarget& target) const
{
    const auto* gameRule = dynamic_cast<const PongGameRule*>(m_world.getGameRule());

    if (nullptr == gameRule)
        return;

    const auto& boundary = gameRule->getBoundary();
    const float top = boundary.position.y + m_position.y;
    const float left = boundary.position.x + m_position.x;

    drawNumber(target, gameRule->getLeftScore(),
        {left + boundary.size.x * 0.25f, top}, m_size, m_color);
    drawNumber(target, gameRule->getRightScore(),
        {left + boundary.size.x * 0.75f, top}, m_size, m_color);
}
