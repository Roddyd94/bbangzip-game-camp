#include "GameCore.hpp"
#include <SFML/Graphics.hpp>
#include <SFML/Window.hpp>
#include <cmath>
#include <iostream>
#include <string>
static void Draw(sf::RenderTarget& target, const GameSnapshot& state) {
    target.clear(sf::Color(18, 23, 36));
    for (std::size_t i=0; i<state.bodies.size(); ++i) {
        sf::RectangleShape shape(state.bodies[i].size);
        shape.setPosition(state.bodies[i].position);
        shape.setFillColor(i==0 ? sf::Color(90,220,170) : sf::Color(110,170,255));
        target.draw(shape);
    }
    sf::CircleShape ball(8.f);
    ball.setOrigin({8.f,8.f});
    ball.setPosition(state.ball);
    target.draw(ball);
    sf::RectangleShape line({2.f,600.f});
    line.setPosition({399.f,0.f}); line.setFillColor(sf::Color(45,55,70));
    target.draw(line);
}
int main(int argc, char** argv) {
    GameCore core;
    if (argc>1 && std::string(argv[1])=="--smoke") {
        sf::RenderTexture surface({800u,600u});
        Draw(surface, core.Snapshot()); surface.display();
        if (!surface.getTexture().copyToImage().saveToFile("startup.png")) return 1;
        core.Update(1.f/60.f, {});
        std::cout << "Starter renderer and core seam OK; gameplay TODOs remain.\n";
        return 0;
    }
    sf::RenderWindow window(sf::VideoMode({800u,600u}), "Pong - starter: gameplay TODO");
    window.setFramerateLimit(60);
    sf::Clock clock;
    while (window.isOpen()) {
        bool reset=false;
        while (const auto event=window.pollEvent()) {
            if (event->is<sf::Event::Closed>()) window.close();
            if (const auto* key=event->getIf<sf::Event::KeyPressed>())
                if (key->code==sf::Keyboard::Key::R) reset=true;
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Escape)) window.close();
        if (!window.isOpen()) break;
        InputState input;
        input.left=float(sf::Keyboard::isKeyPressed(sf::Keyboard::Key::S))-float(sf::Keyboard::isKeyPressed(sf::Keyboard::Key::W));
        input.right=float(sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Down))-float(sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Up));
        input.fire=sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Space);
        input.reset=reset;
        core.Update(clock.restart().asSeconds(), input);
        Draw(window, core.Snapshot());
        window.display();
    }
}
