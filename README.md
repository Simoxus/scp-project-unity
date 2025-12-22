# SCP: Project Unity

SCP: Project Unity is a remake of the indie horror game, SCP - Containment Breach, released in 2012.

This is a Unity engine remake of SCP - Containment Breach, and is licensed under [Creative Commons Attribution-Sharealike 4.0 International](https://creativecommons.org/licenses/by-sa/4.0/).

---

### [Changelog](CHANGELOG.md)
### [Attributions](ATTRIBUTIONS.md)
### [Credits](CREDITS.md)

---

## Social Media
<div style="display: flex; align-items: center; gap: 4px;">
  <a href="https://bsky.app/profile/scppuofficial.com"><img src="https://github.com/user-attachments/assets/54c21e0c-6934-4466-9874-3bd735f119a6" alt="Bluesky" width="200"></a>
  <a href="https://discord.gg/dVdY4PuGfp"><img src="https://github.com/user-attachments/assets/70c480f9-99a3-4868-bee7-39476ed4a038" alt="Discord" width="200"></a>
  <a href="https://www.reddit.com/r/scpprojectunity/"><img src="https://github.com/user-attachments/assets/51cb9f6b-e8c1-40d0-aee4-f433c7deaf21" alt="Reddit" width="200"></a>
  <a href="https://www.youtube.com/@simoxusofficial"><img src="https://github.com/user-attachments/assets/7f64541a-176e-4625-8e4a-173ac49a772b" alt="YouTube" width="200"></a>
</div>

---

## Development Roadmap

* **Version 0.0.1 - 0.1.0**: Core game framework, like basic movement, item interaction, and enemy AI
* **Version 0.1.0 - 0.9.0**: More advanced features and polish, like new systems (e.g., sound occlusion, proximity chat), a complete UI redesign, and game mechanics that expand upon SCP - Containment Breach
* **Version 1.4.0**: Full release, including a comprehensive modding API and all other planned features that may surface throughout development :D

---

## Planned Features

<sub>none of this was AI i just like writing :(</sub>

* **An easy-to-use modding API**
* **Better performance and optimization**
* **More rooms, events, and interesting environments**
* **More items and SCPs**
* **Improved AI behavior** (smarter SCPs, more dynamic pathfinding)
* **Enhanced sound design using FMOD's API and FMOD Studio** (already sorta implemented)
* **Expanded difficulty options to pick from** (like Apollyon)
* **Overhauled lighting and rendering** (using URP's post-processing)
* **Save/Load system** (with multiple slots and quicksaving capability)
* **Rewritten inventory system** (stacking support, drag & drop, item hotkeys)
* **Accessibility options** (colorblind support, scalable UI, input mapping)
* **Achievements and progression tracking using Steamworks** (when it's published haha)
* **Localization support** (for menus, UI, item names, subtitles, etc.)
* **Improved animations and player controller mechanics**
* **Steam Workshop integration** (with an awesome modding API)
* **Expanded lore content using templates** (documents, terminals, SCP documents)
* **Custom game launcher** (like the one you see used in SCP - Containment Breach)

---

## Frequently Asked Questions

* ### **Why make this?**

I don't really have a good answer to that question, but over the years, I've seen so many amazing projects and mods inspired by SCP - Containment Breach struggle and lose their momentum just because Blitz3D is so ancient and difficult to work with. Creators had nowhere solid to build off of. This project here aims to change that; I want to provide a modern, open-source foundation that absolutely anyone can fork, modify, and build upon. Even if Project Unity doesn't reach completion, the open-source nature of the project means that anyone in the community can always pick it up, and take it in their own direction.

* ### **How do I play the latest releases?**

The #builds channel in the [Discord](https://discord.gg/dVdY4PuGfp). If a build is tagged as extremely stable in the Discord #builds channel, you will also be able to find it on [Itch](https://simoxus.itch.io/scp-project-unity).

* ### **How often are new builds released?**
Build releases vary based on progress. If you want to keep up with this project, you should join the Discord for the latest updates!

* ### **Will Project Unity be free to play?**
Yes, the game is completely free and the majority of content included in the game is licensed under CC BY-SA 4.0, although there are some exceptions, which will be outlined in the README as they are added.

* ### **Is this a continuation of SCP: Unity?**
No, SCP: Project Unity is a separate project with its own vision.

* ### **Is this affiliated with SCP - Containment Breach?**
No, but Project Unity is a remake of the game. It's just not officially affiliated with the original game's developer(s).

* ### **Will all content from the original game be included?**
Yes, the project's main goal is to practically port the game, as well as expanding upon the original content, with improvements and additions planned over time.

* ### **Can I contribute to the project?**
Yes! Contributions are actually very much appreciated. Join the Discord to get involved. :)

* ### **I want to make a bug report or suggestion. How do I make one?**

You can make a bug report OR suggestion in GitHub Issues on the repository, but preferably in the [Discord](https://discord.gg/dVdY4PuGfp).

* ### **Can I create mods or custom content?**
Yes, the game is designed to support modding! More information about modding support will be provided as development progresses.

* ### **Will there be multiplayer support?**
No, multiplayer is not currently planned.

* ### **What platforms does Project Unity support?**

SCP: Project Unity supports **Windows**, **Linux**, and **macOS**.

---

## For Developers

#### **1. Getting the Project**

1.  Click the green **"Code"** button on the GitHub page and select **"Download ZIP"**.
2.  Extract the downloaded `.ZIP` file to your desired location.

#### **2. Opening the Project in Unity**

1.  Launch **Unity Hub**.
2.  Click the **"Add"** button and select **"Add project from disk"**.
3.  Find and select the folder you extracted in the previous step.

#### **3. Installing the Unity Editor and Modules**

You will need the correct version of the Unity Editor to open the project. (if you don't have it installed, Unity Hub will prompt you to download the recommended version)

* **Platform Modules:** Ensure you also install the necessary **Build Support** modules for the platforms you plan to compile for (e.g., Windows, Linux, macOS).

#### **4. Building the Project**

Before building, confirm the correct scenes are included in your build settings.

1.  Open the project in the Unity Editor.
2.  Navigate to **File** > **Build Settings**.
3.  In the **"Scenes In Build"** section, verify that the following scenes are checked:
    * `_Scenes/MainMenu`
    * `_Scenes/MainMenu_Credits`
    * `_Scenes/Facility`
    * `_Scenes/Facility_Exterior`
4. Hit "**Build**" or "**Build and Run**".

---
