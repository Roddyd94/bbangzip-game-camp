#include "base/Types.hpp"
#include "core/GameEngine.hpp"
#include "PongBody.hpp"
#include "ScoreUI.hpp"
#include "PongGameRule.hpp"
#include "Paddle.hpp"
#include "Ball.hpp"
#include <cmath>
#include <Windows.h>
#include <charconv>
#include <filesystem>
#include <fstream>
#include <map>
#include <iostream>

namespace
{
	std::string trim(const std::string& text)
	{
		const auto begin = text.find_first_not_of(" \t\r\n");
		if (begin == std::string::npos)
			return {};
		return text.substr(begin, text.find_last_not_of(" \t\r\n") - begin + 1);
	}

	bool reportError(const std::string& message)
	{
		std::cerr << "Settings: " << message << '\n';
		return false;
	}

	bool readSettings(const std::filesystem::path& file, std::map<std::string, std::string>& settings)
	{
		std::ifstream input(file);
		if (!input)
			return reportError("Cannot open " + file.string());
		std::string line;
		while (std::getline(input, line))
		{
			line = trim(line);
			if (line.empty() || line.front() == '#')
				continue;
			const auto split = line.find('=');
			if (split == std::string::npos)
				return reportError("Expected key = value: " + line);
			const auto key = trim(line.substr(0, split));
			const auto value = trim(line.substr(split + 1));
			if (key.empty() || value.empty() || !settings.emplace(key, value).second)
				return reportError("Empty or duplicate setting: " + key);
		}
		return input.eof() || reportError("Failed reading " + file.string());
	}

	bool takeSetting(std::map<std::string, std::string>& settings, const std::string& key, std::string& value)
	{
		const auto found = settings.find(key);
		if (found == settings.end())
			return reportError("Missing setting: " + key);
		value = found->second;
		settings.erase(found);
		return true;
	}

	bool readUnsigned(std::map<std::string, std::string>& settings, const std::string& key,
		uint32 minimum, uint32 maximum, uint32& result)
	{
		std::string value;
		if (!takeSetting(settings, key, value))
			return false;
		uint32 number{};
		const auto parsed = std::from_chars(value.data(), value.data() + value.size(), number);
		if (parsed.ec != std::errc{} || parsed.ptr != value.data() + value.size()
			|| number < minimum || number > maximum)
			return reportError("Invalid numeric setting: " + key);
		result = number;
		return true;
	}

	bool readKey(std::map<std::string, std::string>& settings, const std::string& key, SfKey& result)
	{
		std::string value;
		if (!takeSetting(settings, key, value))
			return false;
		if (value.size() == 1 && value[0] >= 'A' && value[0] <= 'Z')
		{
			result = static_cast<SfKey>(static_cast<int32>(SfKey::A) + value[0] - 'A');
			return true;
		}
		const std::map<std::string, SfKey> keys{
			{ "Up", SfKey::Up },{ "Down", SfKey::Down },{ "Left", SfKey::Left },{ "Right", SfKey::Right },
			{ "Space", SfKey::Space },{ "Escape", SfKey::Escape },{ "Enter", SfKey::Enter } };
		const auto found = keys.find(value);
		if (found == keys.end())
			return reportError("Unsupported key: " + value);
		result = found->second;
		return true;
	}

    bool readFloat(std::map<std::string, std::string>& settings, const std::string& key,
        float& result, bool allowZero)
    {
        std::string value;
        if (!takeSetting(settings, key, value))
            return false;
        float number{};
        const auto parsed = std::from_chars(value.data(), value.data() + value.size(), number);
        if (parsed.ec != std::errc{} || parsed.ptr != value.data() + value.size()
            || !std::isfinite(number) || number < 0.f || (!allowZero && number == 0.f))
            return reportError("Invalid number: " + key);
        result = number;
        return true;
    }

    struct PongFactory
    {
        World* world{nullptr};
        InputSettings bindings;
        sf::FloatRect bounds;
        BallSettings ball;
        PaddleSettings paddle;

        std::unique_ptr<Object> operator()(const std::string& type) const
        {
            std::unique_ptr<GameObject> object;
            if (type == "ScoreUI")
                object = std::make_unique<ScoreUI>(*world);
            else if (type == "GameObject")
                object = std::make_unique<PongBody>(*world);
            else if (type == "Paddle")
                object = std::make_unique<Paddle>(*world, paddle, bindings);
            else if (type == "Ball")
                object = std::make_unique<Ball>(*world, ball);
            return object;
        }
    };

}

bool GameEngine::initializeWorld()
{
	wchar_t executablePath[32768];
	const uint32 length = GetModuleFileNameW(nullptr, executablePath, 32768);
	if (length == 0 || length == 32768)
		return reportError("Cannot locate executable");
	const auto content = std::filesystem::path(executablePath).parent_path() / "content";
	std::map<std::string, std::string> values;
	if (!readSettings(content / "game.ini", values))
		return false;
	std::string worldFile;
	std::string inputFile;
    std::string paddleFile;
    std::string ballFile;
	EngineSettings settings;
	if (!takeSetting(values, "world", worldFile)
		|| !takeSetting(values, "input", inputFile)
        || !takeSetting(values, "paddle", paddleFile)
        || !takeSetting(values, "ball", ballFile)
		|| !takeSetting(values, "window.title", settings.title)
		|| !readUnsigned(values, "window.width", 1, 16384, settings.width)
		|| !readUnsigned(values, "window.height", 1, 16384, settings.height)
		|| !readUnsigned(values, "window.frameLimit", 0, 1000, settings.frameLimit))
		return false;
	if (!values.empty())
		return reportError("Unknown game setting: " + values.begin()->first);
	std::map<std::string, std::string> inputValues;
	if (!readSettings(content / inputFile, inputValues))
		return false;
	InputSettings inputSettings;
	if (!readKey(inputValues, "global.reset", inputSettings.global.reset)
		|| !readKey(inputValues, "global.close", inputSettings.global.close)
		|| !readKey(inputValues, "player1.up", inputSettings.player1.up)
		|| !readKey(inputValues, "player1.down", inputSettings.player1.down)
		|| !readKey(inputValues, "player1.fire", inputSettings.player1.fire)
		|| !readKey(inputValues, "player2.up", inputSettings.player2.up)
		|| !readKey(inputValues, "player2.down", inputSettings.player2.down)
		|| !readKey(inputValues, "player2.fire", inputSettings.player2.fire))
		return false;
	if (!inputValues.empty())
		return reportError("Unknown input setting: " + inputValues.begin()->first);
    std::map<std::string, std::string> paddleValues;
    std::map<std::string, std::string> ballValues;
    PongFactory factory;
    factory.world = m_world.get();
    factory.bounds = {{0.f, 0.f}, {float(settings.width), float(settings.height)}};
    if (!readSettings(content / paddleFile, paddleValues)
        || !readFloat(paddleValues, "speed", factory.paddle.speed, false)
        || !readSettings(content / ballFile, ballValues)
        || !readFloat(ballValues, "speed", factory.ball.speed, false)
        || !readFloat(ballValues, "collisionSpeedIncrease", factory.ball.collisionSpeedIncrease, true)
        || !readFloat(ballValues, "start.x", factory.ball.startPosition.x, true)
        || !readFloat(ballValues, "start.y", factory.ball.startPosition.y, true))
        return false;
    if (!paddleValues.empty()
        || !ballValues.empty())
        return reportError("Unknown paddle/ball setting");
    if (!factory.bounds.contains(factory.ball.startPosition))
        return reportError("Ball start position is outside the level");
    factory.bindings = inputSettings;
    InputComponent globalInput;
    if (!globalInput.registerBool(static_cast<uint8>(GlobalInputKey::Reset), inputSettings.global.reset)
        || !globalInput.registerBool(static_cast<uint8>(GlobalInputKey::Close), inputSettings.global.close))
        return false;
    m_world->setGameRule(std::make_unique<PongGameRule>(factory.bounds));

    if (!m_world->initialize(content / worldFile, factory))
		return false;
	m_settings = std::move(settings);
	*m_input = std::move(globalInput);
	return true;
}
