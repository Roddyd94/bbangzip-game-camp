#include "base/Types.hpp"
#include "WindowManager.hpp"

bool WindowManager::shouldClose() const
{
    return !m_window->isOpen();
}

sf::Vector2u WindowManager::getSize() const
{
    return m_window->getSize();
}

sf::Vector2i WindowManager::getPosition() const
{
    return m_window->getPosition();
}

void WindowManager::initialize()
{
}

void WindowManager::deinitialize() noexcept
{
    destroyWindow();
}

void WindowManager::createWindow(uint32 width, uint32 height, const std::string& title)
{
    m_window->create(sf::VideoMode({width, height}), title, sf::Style::Titlebar | sf::Style::Close);
}

void WindowManager::destroyWindow()
{
    if (m_window->isOpen())
        m_window->close();
}

void WindowManager::setSize(sf::Vector2u size)
{
    m_window->setSize(size);
}

void WindowManager::setPosition(sf::Vector2i position)
{
    m_window->setPosition(position);
}
