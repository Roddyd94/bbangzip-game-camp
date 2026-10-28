#pragma once
#include "base/Types.hpp"
#include <cstdint>

class Object
{
    friend class World;

public:
    using Id = uint64;

    Object();
    virtual ~Object() = default;
    Object(const Object&) = delete;
    Object& operator=(const Object&) = delete;
    Object(Object&&) = delete;
    Object& operator=(Object&&) = delete;

private:
    Id m_id;
    uint8 m_active : 1;

public:
    bool isActive() const
    {
        return m_active != 0;
    }

    Id getId() const
    {
        return m_id;
    }
};
