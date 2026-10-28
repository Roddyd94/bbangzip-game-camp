#pragma once
#include "Subsystem.hpp"

class World;

class CollisionManager : public Subsystem
{
public:
    void collision(World& world) const;
    void initialize() override;
    void deinitialize() noexcept override;
};
