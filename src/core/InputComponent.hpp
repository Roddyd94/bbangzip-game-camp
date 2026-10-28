#pragma once
#include "base/Types.hpp"
#include "base/SfTypes.hpp"
#include <unordered_map>

struct GlobalKeyBindings
{
    SfKey reset{SfKey::Unknown};
    SfKey close{SfKey::Unknown};
};

struct PlayerKeyBindings
{
    SfKey up{SfKey::Unknown};
    SfKey down{SfKey::Unknown};
    SfKey fire{SfKey::Unknown};
};

struct InputSettings
{
    PlayerKeyBindings player1;
    PlayerKeyBindings player2;
    GlobalKeyBindings global;
};

class InputComponent
{
public:
    using KeyReader = bool (*)(SfKey);

private:
    enum class ValueType : uint8
    {
        Bool,
        Float,
        Vector2
    };

    struct Binding
    {
        float2 state{};
        SfKey keyDecX{SfKey::Unknown};
        SfKey keyIncX{SfKey::Unknown};
        SfKey keyDecY{SfKey::Unknown};
        SfKey keyIncY{SfKey::Unknown};
        ValueType type{ValueType::Bool};
    };

    std::unordered_map<uint8, Binding> m_bindings;

public:
    float getKeyState(uint8 keyType) const;
    bool getBoolState(uint8 keyType) const;
    float2 getVector2State(uint8 keyType) const;
    bool registerBool(uint8 keyType, SfKey key);
    bool registerFloat(uint8 keyType, SfKey keyDec, SfKey keyInc);
    bool registerVector2(uint8 keyType, SfKey keyDecX, SfKey keyIncX, SfKey keyDecY, SfKey keyIncY);
    void update(KeyReader readKey = sf::Keyboard::isKeyPressed);
    void clear();
};
