# SCP: Project Unity

SCP: Project Unity is a remake of the iconic indie horror game, SCP - Containment Breach, released in 2012.

This is a Unity engine remake of SCP - Containment Breach, and is licensed under [Creative Commons Attribution-Sharealike 4.0 International](https://creativecommons.org/licenses/by-sa/4.0/).

---

### [Changelog](CHANGELOG.md)
### [Attributions](ATTRIBS.md)
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

## Showcase
https://github.com/user-attachments/assets/449e926a-f46f-4d1e-89a9-9e3fbebdf99d

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

SCP - Containment Breach was originally developed on an engine known as Blitz3D, and it's very cranky.

* ### **How do I play the latest releases?**

The #builds channel in the [Discord](https://discord.gg/dVdY4PuGfp). If a build is tagged as extremely stable in the Discord #builds channel, you will also be able to find it on [Itch](https://simoxus.itch.io/scp-project-unity).

* ### **What platforms does Project Unity support?**

SCP: Project Unity supports **Windows**, **Linux**, and **macOS**.

* ### **I want to make a bug report or suggestion. How do I make one?**

You can make a bug report OR suggestion in GitHub Issues on the repository, but preferably in the [Discord](https://discord.gg/dVdY4PuGfp).

* ### **Will Project Unity be free to play?**

Yes, SCP: Project Unity is completely free and the majority of content included in the game is licensed under CC BY-SA 4.0, although there are some exceptions, which will be outlined in the README as they are added.

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


