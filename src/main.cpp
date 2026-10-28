#include "base/Types.hpp"
#include "core/GameEngine.hpp"

int32 main()
{
    g_engine.initialize();
    return g_engine.run();
}
