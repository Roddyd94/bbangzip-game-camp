#pragma once
#include "base/Types.hpp"
#include "Subsystem.hpp"
#include <SFML/Graphics/RenderWindow.hpp>
#include <string>
#include <memory>

class WindowManager : public Subsystem
{
private:
    std::unique_ptr<sf::RenderWindow> m_window{std::make_unique<sf::RenderWindow>()};

public:
    bool shouldClose() const;
    sf::Vector2u getSize() const;
    sf::Vector2i getPosition() const;
    sf::RenderWindow& getWindow()
    {
        return *m_window;
    }
    void initialize() override;
    void deinitialize() noexcept override;
    void createWindow(uint32 width, uint32 height, const std::string& title);
    void destroyWindow();
    void setSize(sf::Vector2u size);
    void setPosition(sf::Vector2i position);
};
