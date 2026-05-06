# Bolt Busters

Copyright © 2026 Else Hell Entertainment

This game was produced as a project work for Tampere University of Applied
Sciences (TAMK) Games Academy course '4A00HB74 Game Project 2' for spring 2026.
The goal of the project was to create a simple 3D game using Godot game engine
during the spring semester as a team of 6 people.

Game was made in Godot 4.5.1


## Description

Bolt Busters is a 3D twin stick shooter where the player takes the role of a
robot trapped in a gladiator arena, fighting against increasingly difficult
waves of robotic enemies during the course of 20 rounds until they either
survive or end up as scrap metal.


## Notable Features

- 3 upgradeable weapons
- 3 types of enemies
- 20 rounds of intense combat
- 6 original music tracks

Player weapons:
- Chaingun: Simple, fast firing weapon which can overheat if fired too long
- Railgun: Penetrating high damage weapon which requires a charge-up time to fire
- Rocket launcher: Fires a salvo of 4 rockets


## Instructions for running the game

- Option 1: Download and build the project in Godot yourself!
- Option 2: Release build can be found here: https://elsehellentertainment.itch.io/boltbusters


## Controls

- Movement: WASD / Gamepad Left Joystick
- Aiming: Mouse / Gamepad Right Joystick
- Chaingun: LMB / Gamepad Right Bumper
- Railgun (requires button to be held down until charge completes): RMB / Gamepad Left Bumper
- Rockets: Space / Gamepad Right Trigger


## Tools used in the project

- Godot 4.5.1
- C# with .NET SDK 8.0
- Blender 5.0
- JetBrains Rider IDE
- Aseprite
- Audacity
- Reaper
- Ardour
- SurgeXT Synth/FX
- VSCodium


## AI Disclaimer

AI tools such as IDE integrated Github Copilot or web-based platforms such as
ChatGPT or Claude were used to generate comments, debugging and general
assistance in programming solutions. Spesifically AI agents were not used to
generate any meaningful amounts of code and AI was not used at all in any part
of the artistic asset creation pipeline.


## Known Issues

- The round start countdown does not fire when starting a new game or when
  loading a save from the main menu.
- HUD elements are visible during the countdown, even though they should be
  hidden except for currencies, when starting a new game or when loading a save
  from the main menu or after death.
- The pause menu cannot be exited using the ESC key or by pressing the
  Start/Menu button on a controller.
- The fullscreen toggle is not highlighted when navigating the settings menu
  with a controller.
- If the pause menu is opened using a controller during a round, switching to
  mouse input does not display the mouse cursor.
- The mouse is not locked to the game window during rounds or gameplay.
- When the Settings menu is opened from the main menu, the background theme
  music fades out and does not resume until the main menu is exited and entered
  again.
- When volume sliders are set to 0%, the label incorrectly shows 100% when the
  menu is opened again.
