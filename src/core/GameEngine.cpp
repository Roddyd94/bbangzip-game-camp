#include "base/Types.hpp"
#include "GameEngine.hpp"
#include "WindowManager.hpp"
#include "FileSerializer.hpp"
#include "RenderManager.hpp"
#include "CollisionManager.hpp"
#include "TimeManager.hpp"
#include <SFML/Graphics.hpp>
#include <SFML/Window.hpp>
#include <iostream>

GameEngine g_engine;

GameEngine::GameEngine()
    : m_world(std::make_unique<World>()), m_input(std::make_unique<InputComponent>()),
      m_initializing(0), m_deinitializing(0), m_ready(0)
{
	addSubsystem<FileSerializer>();
	addSubsystem<WindowManager>();
	addSubsystem<RenderManager>();
	addSubsystem<CollisionManager>();
	addSubsystem<TimeManager>();
}

GameEngine::~GameEngine()
{
	deinitialize();
}

bool GameEngine::initialize()
{
	if (m_initializing || m_deinitializing)
		return false;

	m_initializing = true;

	try
	{
		while (m_initializedCount < m_subsystems.size())
		{
			auto* subsystem = m_subsystems[m_initializedCount].get();
			++m_initializedCount;
			subsystem->initialize();
		}

		if (!m_ready)
			m_ready = initializeWorld();
	}
	catch (const std::exception& error)
	{
		m_initializing = false;
		deinitialize();
		std::cerr << "Engine initialization failed: " << error.what() << '\n';
	}

	m_initializing = false;

	if (!m_ready)
		deinitialize();

	return m_ready;
}

void GameEngine::deinitialize() noexcept
{
	if (m_initializing || m_deinitializing)
		return;

	m_deinitializing = true;
	m_ready = false;

	while (m_initializedCount > 0)
	{
		--m_initializedCount;
		m_subsystems[m_initializedCount]->deinitialize();
	}

	m_deinitializing = false;
}

bool GameEngine::step(float dt, sf::RenderTarget& target)
{
	auto* collision = getSubsystem<CollisionManager>();
	auto* renderer = getSubsystem<RenderManager>();

	if (!collision || !renderer)
		return false;

	m_world->flush();
	m_world->tick(dt);

	collision->collision(*m_world);
	renderer->render(*m_world, target);

	m_world->flush();

	return true;
}

int32 GameEngine::run()
{
	if (!m_ready)
	{
		deinitialize();
		return 1;
	}

	auto* windowManager = getSubsystem<WindowManager>();
	auto* timeManager = getSubsystem<TimeManager>();

	if (!windowManager || !timeManager)
	{
		deinitialize();
		return 1;
	}
	try
	{
		windowManager->createWindow(m_settings.width, m_settings.height, m_settings.title);
		auto& window = windowManager->getWindow();
		window.setFramerateLimit(m_settings.frameLimit);

		timeManager->reset();
		while (!windowManager->shouldClose())
		{
			while (const auto event = window.pollEvent())
			{
				if (event->is<sf::Event::Closed>())
					windowManager->destroyWindow();
			}

			m_input->update();

			if (m_input->getBoolState(static_cast<uint8>(GlobalInputKey::Close)))
				windowManager->destroyWindow();

			if (windowManager->shouldClose())
				break;

			timeManager->update();

			if (!step(timeManager->getDeltaTime(), window))
			{
				deinitialize();
				return 1;
			}

			window.display();
		}
	}
	catch (const std::exception& error)
	{
		windowManager->destroyWindow();
		std::cerr << "Engine execution failed: " << error.what() << '\n';

		deinitialize();
		return 1;
	}

	deinitialize();

	return 0;
}
