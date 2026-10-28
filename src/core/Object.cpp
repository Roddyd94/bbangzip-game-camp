#include "Object.hpp"
#include <atomic>

namespace
{
    std::atomic<Object::Id> nextId{1};
}

Object::Object() : m_id(nextId.fetch_add(1, std::memory_order_relaxed)), m_active(1)
{
}
