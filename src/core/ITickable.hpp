#pragma once

class ITickable
{
public:
    virtual ~ITickable() = default;

public:
    virtual void tick(float dt) = 0;
};
