#pragma once
#include "Subsystem.hpp"
#include "base/Types.hpp"
#include "Object.hpp"
#include <filesystem>
#include <functional>
#include <memory>
#include <string>
#include <vector>

class ISerializable;

class FileSerializer : public Subsystem
{
public:
    FileSerializer();

private:
    mutable std::string m_lastError;
    uint8 m_initialized : 1;

public:
    const std::string& getLastError() const
    {
        return m_lastError;
    }
    bool serialize(const std::filesystem::path& file, const ISerializable& object) const;
    bool deserialize(const std::filesystem::path& file, ISerializable& object) const;
    bool deserialize(const std::filesystem::path& file,
        const std::function<std::unique_ptr<Object>(const std::string&)>& createObject,
        std::vector<std::unique_ptr<Object>>& objects) const;

    void initialize() override;
    void deinitialize() noexcept override;

private:
    bool fail(const std::string& message) const;
};
