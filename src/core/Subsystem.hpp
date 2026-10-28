#pragma once

class Subsystem
{
public:
    virtual ~Subsystem() = default;

public:
    virtual void initialize() = 0;
    virtual void deinitialize() noexcept = 0;
};
