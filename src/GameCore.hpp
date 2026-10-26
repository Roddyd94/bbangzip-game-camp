#pragma once
#include <SFML/System/Vector2.hpp>
#include <vector>
struct InputState { float left{}, right{}; bool fire{}, reset{}; };
struct Body { sf::Vector2f position; sf::Vector2f size; };
struct GameSnapshot {
    std::vector<Body> bodies;
    sf::Vector2f ball{400.f, 300.f};
    unsigned leftScore{}, rightScore{};
};
class GameCore {
public:
    GameCore();
    void Update(float dt, const InputState& input);
    void Reset();
    const GameSnapshot& Snapshot() const { return state_; }
private:
    GameSnapshot state_;
};
