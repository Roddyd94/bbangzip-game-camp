#include "base/Types.hpp"
#include "CollisionManager.hpp"
#include "ICollidable.hpp"
#include "World.hpp"

void CollisionManager::collision(World& world) const
{
    const auto& objects = world.objects();

    for (uint64 i = 0; i < objects.size(); ++i)
    {
        auto* a = dynamic_cast<ICollidable*>(objects[i].get());

        if (nullptr == a)
            continue;

        for (uint64 j = i + 1; j < objects.size(); ++j)
        {
            if (!objects[i]->isActive())
                break;

            if (!objects[j]->isActive())
                continue;

            auto* b = dynamic_cast<ICollidable*>(objects[j].get());

            if (nullptr == b
                || !a->getCollisionBounds().findIntersection(b->getCollisionBounds()))
                continue;

            a->onCollision(*objects[j]);

            if (objects[i]->isActive()
                && objects[j]->isActive())
                b->onCollision(*objects[i]);
        }
    }

}

void CollisionManager::initialize()
{
}

void CollisionManager::deinitialize() noexcept
{
    // Collision detection currently owns no persistent resources.
}
