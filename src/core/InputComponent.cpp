#include "InputComponent.hpp"

namespace
{
bool validKey(SfKey key)
{
    const int32 value = static_cast<int32>(key);
    return value >= 0 && static_cast<uint32>(value) < sf::Keyboard::KeyCount;
}
}

float InputComponent::getKeyState(uint8 keyType) const
{
    const auto found = m_bindings.find(keyType);
    if (found == m_bindings.end()
        || found->second.type == ValueType::Vector2)
        return 0.f;

    return found->second.state.x;
}

bool InputComponent::getBoolState(uint8 keyType) const
{
    const auto found = m_bindings.find(keyType);
    auto result = found != m_bindings.end()
        && found->second.type == ValueType::Bool
        && found->second.state.x != 0.f;

    return result;
}

float2 InputComponent::getVector2State(uint8 keyType) const
{
    const auto found = m_bindings.find(keyType);
    if (found == m_bindings.end()
        || found->second.type != ValueType::Vector2)
        return {};

    return found->second.state;
}

bool InputComponent::registerBool(uint8 keyType, SfKey key)
{
    if (!validKey(key))
        return false;

    Binding binding;
    binding.keyIncX = key;

    return m_bindings.emplace(keyType, binding).second;
}

bool InputComponent::registerFloat(uint8 keyType, SfKey keyDec, SfKey keyInc)
{
    if (!validKey(keyDec)
        || !validKey(keyInc))
        return false;

    Binding binding;
    binding.type = ValueType::Float;
    binding.keyDecX = keyDec;
    binding.keyIncX = keyInc;

    return m_bindings.emplace(keyType, binding).second;
}

bool InputComponent::registerVector2(uint8 keyType, SfKey keyDecX, SfKey keyIncX, SfKey keyDecY, SfKey keyIncY)
{
    if (!validKey(keyDecX)
        || !validKey(keyIncX)
        || !validKey(keyDecY)
        || !validKey(keyIncY))
        return false;

    Binding binding;
    binding.type = ValueType::Vector2;
    binding.keyDecX = keyDecX;
    binding.keyIncX = keyIncX;
    binding.keyDecY = keyDecY;
    binding.keyIncY = keyIncY;

    return m_bindings.emplace(keyType, binding).second;
}

void InputComponent::update(KeyReader readKey)
{
    for (auto& entry : m_bindings)
    {
        auto& binding = entry.second;
        binding.state = {};

        if (!readKey)
            continue;

        binding.state.x = float(readKey(binding.keyIncX));

        if (binding.type != ValueType::Bool)
            binding.state.x -= float(readKey(binding.keyDecX));

        if (binding.type == ValueType::Vector2)
            binding.state.y = float(readKey(binding.keyIncY)) - float(readKey(binding.keyDecY));
    }
}

void InputComponent::clear()
{
    m_bindings.clear();
}
