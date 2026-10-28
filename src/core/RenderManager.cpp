#include "RenderManager.hpp"
#include "IRenderable.hpp"
#include "World.hpp"
#include <SFML/Graphics/RenderTarget.hpp>

void RenderManager::render(const World& world,sf::RenderTarget& target) const
{
    target.clear(m_clear);
    for (const auto& object : world.objects())
    {
        if (!object->isActive())
            continue;

        if (const auto* drawable=dynamic_cast<const IRenderable*>(object.get()))
            drawable->render(target);
    }
}

void RenderManager::initialize()
{
}

void RenderManager::deinitialize() noexcept
{
}
