#include "World.hpp"
#include "ITickable.hpp"
#include "GameEngine.hpp"
#include "FileSerializer.hpp"
#include <iostream>

bool World::isActive(Object::Id id) const
{
    for (const auto& object : m_objects)
    {
        if (object->getId() == id)
            return object->isActive();
    }

    for (const auto& object : m_pending)
    {
        if (object->getId() == id)
            return object->isActive();
    }

    return false;
}

Object* World::find(Object::Id id) const
{
    for (const auto& object : m_objects)
    {
        if (object->getId() == id && object->isActive())
            return object.get();
    }

    return nullptr;
}

bool World::initialize(const std::filesystem::path& file,
    const std::function<std::unique_ptr<Object>(const std::string&)>& createObject)
{
    auto* serializer = g_engine.getSubsystem<FileSerializer>();

    if (!serializer)
        return false;

    std::vector<std::unique_ptr<Object>> loaded;
    if (!serializer->deserialize(file, createObject, loaded))
    {
        std::cerr << serializer->getLastError() << '\n';
        return false;
    }

    m_objects = std::move(loaded);
    m_pending.clear();

    return true;
}

void World::destroy(Object::Id id)
{
    for (const auto& object : m_objects)
    {
        if (object->getId() == id)
        {
            object->m_active = 0;
            return;
        }
    }

    for (const auto& object : m_pending)
    {
        if (object->getId() == id)
        {
            object->m_active = 0;
            return;
        }
    }
}

void World::tick(float dt)
{
    for (const auto& object : m_objects)
    {
        if (!object->isActive())
            continue;

        if (auto* tickable=dynamic_cast<ITickable*>(object.get()))
            tickable->tick(dt);
    }
}

void World::flush()
{
    for (auto it = m_objects.begin(); it != m_objects.end();)
    {
        if (!(*it)->isActive())
            it = m_objects.erase(it);
        else
            ++it;
    }

    for (auto it = m_pending.begin(); it != m_pending.end();)
    {
        if (!(*it)->isActive())
            it = m_pending.erase(it);
        else
            ++it;
    }

    for (auto& object : m_pending)
        m_objects.push_back(std::move(object));

    m_pending.clear();
}
