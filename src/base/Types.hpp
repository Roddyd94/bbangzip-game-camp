#pragma once
#include <climits>
#include <cstdint>

using int8 = std::int8_t;
using int16 = std::int16_t;
using int32 = std::int32_t;
using int64 = std::int64_t;

using uint8 = unsigned char;
using uint16 = std::uint16_t;
using uint32 = std::uint32_t;
using uint64 = std::uint64_t;

static_assert(CHAR_BIT == 8);
static_assert(sizeof(int8) == 1 && sizeof(uint8) == 1);
static_assert(sizeof(int16) == 2 && sizeof(uint16) == 2);
static_assert(sizeof(int32) == 4 && sizeof(uint32) == 4);
static_assert(sizeof(int64) == 8 && sizeof(uint64) == 8);
