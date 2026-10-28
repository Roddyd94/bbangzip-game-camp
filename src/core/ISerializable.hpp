#pragma once
#include <iosfwd>

class ISerializable
{
public:
    virtual ~ISerializable() = default;

public:
    friend std::ostream& operator<<(std::ostream& output, const ISerializable& object)
    {
        object.serialize(output);
        return output;
    }

    friend std::istream& operator>>(std::istream& input, ISerializable& object)
    {
        object.deserialize(input);
        return input;
    }

public:
    virtual void serialize(std::ostream& output) const = 0;
    virtual void deserialize(std::istream& input) = 0;
};
