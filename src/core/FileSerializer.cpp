#include "base/Types.hpp"
#include "FileSerializer.hpp"
#include "ISerializable.hpp"
#include <fstream>
#include <sstream>

FileSerializer::FileSerializer()
    : m_initialized(0)
{
}

bool FileSerializer::serialize(const std::filesystem::path& file, const ISerializable& object) const
{
    if (!m_initialized)
        return fail("FileSerializer is not initialized");

    // 객체 직렬화가 실패해도 기존 파일이 잘리지 않도록 먼저 메모리에서 직렬화합니다.
    std::ostringstream record;
    record.imbue(std::locale::classic());
    record << object;

    if (!record)
        return fail("Failed serializing object");

    std::ofstream output(file, std::ios::trunc);

    if (!output)
        return fail("Cannot open object file for writing: " + file.string());

    output << record.str();
    output.close();

    if (!output)
        return fail("Failed writing object file: " + file.string());

    m_lastError.clear();

    return true;
}

bool FileSerializer::deserialize(const std::filesystem::path& file, ISerializable& object) const
{
    if (!m_initialized)
        return fail("FileSerializer is not initialized");

    std::ifstream input(file);

    if (!input)
        return fail("Cannot open object file: " + file.string());

    input.imbue(std::locale::classic());
    input >> object;

    if (input.fail()
        || input.bad())
        return fail("Failed reading object file: " + file.string());

    m_lastError.clear();

    return true;
}

bool FileSerializer::deserialize(const std::filesystem::path& file,
    const std::function<std::unique_ptr<Object>(const std::string&)>& createObject,
    std::vector<std::unique_ptr<Object>>& objects) const
{
    if (!m_initialized)
        return fail("FileSerializer is not initialized");

    std::ifstream input(file);

    if (!input)
        return fail("Cannot open world file: " + file.string());

    input.imbue(std::locale::classic());

    std::string header;

    if (!std::getline(input, header)
        || header != "StudyWorld 1")
        return fail("Invalid world header: expected StudyWorld 1");

    if (!createObject)
        return fail("Missing object factory");

    std::vector<std::unique_ptr<Object>> loaded;
    std::string line;
    uint32 lineNumber = 1;

    while (std::getline(input, line))
    {
        ++lineNumber;
        std::istringstream record(line);
        record.imbue(std::locale::classic());

        record >> std::ws;

        if (record.eof())
            continue;

        std::string type;
        record >> type;

        auto object = createObject(type);

        if (!object)
            return fail("Unknown object type at line " + std::to_string(lineNumber));

        auto* serializable = dynamic_cast<ISerializable*>(object.get());

        if (nullptr == serializable)
            return fail("Object is not serializable: " + type);

        record >> *serializable;

        if (record.fail()
            || record.bad())
            return fail("Invalid object data at world line " + std::to_string(lineNumber));

        record >> std::ws;

        if (!record.eof())
            return fail("Unexpected trailing data at world line " + std::to_string(lineNumber));

        loaded.push_back(std::move(object));
    }

    if (!input.eof())
        return fail("Failed reading world file");

    objects = std::move(loaded);
    m_lastError.clear();

    return true;
}

void FileSerializer::initialize()
{
    m_lastError.clear();
    m_initialized = true;
}

void FileSerializer::deinitialize() noexcept
{
    m_initialized = false;
}

bool FileSerializer::fail(const std::string& message) const
{
    m_lastError = message;
    return false;
}
