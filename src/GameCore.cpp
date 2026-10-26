#include "GameCore.hpp"
GameCore::GameCore() {
    state_.bodies = {{{30.f, 250.f}, {14.f, 100.f}}, {{756.f, 250.f}, {14.f, 100.f}}};
}
void GameCore::Update(float dt, const InputState& input) {
    // TODO (participant): update gameplay using dt and input.
    // The supplied renderer must not implement movement or collision rules.
    (void)dt;
    if (input.reset) Reset();
}
void GameCore::Reset() {
    // TODO (participant): reset gameplay state and timers.
}
