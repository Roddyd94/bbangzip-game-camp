#pragma once
#include "SfTypes.hpp"
#include <algorithm>
#include <cmath>

namespace utility
{
    inline constexpr float pi = 3.14159265358979323846f;

    // minimum <= maximum인 범위를 전달합니다.
    inline float clamp(float value, float minimum, float maximum)
    {
        return std::clamp(value, minimum, maximum);
    }

    inline float saturate(float value)
    {
        return clamp(value, 0.f, 1.f);
    }

    // t를 제한하지 않으므로 0~1 밖에서는 외삽합니다.
    inline float lerp(float from, float to, float t)
    {
        return from + (to - from) * t;
    }

    inline float2 lerp(float2 from, float2 to, float t)
    {
        return from + (to - from) * t;
    }

    // 구간의 두 끝이 같으면 0을 반환합니다. 결과 범위는 제한하지 않습니다.
    inline float inverseLerp(float from, float to, float value)
    {
        if (from == to)
            return 0.f;

        return (value - from) / (to - from);
    }

    inline float toRadians(float degrees)
    {
        return degrees * (pi / 180.f);
    }

    inline float toDegrees(float radians)
    {
        return radians * (180.f / pi);
    }

    inline float dot(float2 first, float2 second)
    {
        return first.x * second.x + first.y * second.y;
    }

    inline float lengthSquared(float2 value)
    {
        return dot(value, value);
    }

    inline float length(float2 value)
    {
        return std::hypot(value.x, value.y);
    }

    // 길이가 epsilon 이하이거나 유한하지 않으면 영벡터를 반환합니다.
    // epsilon은 0 이상인 유한 값으로 전달합니다.
    inline float2 normalize(float2 value, float epsilon = 0.000001f)
    {
        const float magnitude = length(value);

        if (!std::isfinite(magnitude)
            || magnitude <= epsilon)
            return {};

        return value / magnitude;
    }
}
