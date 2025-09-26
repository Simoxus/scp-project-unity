# Updates

All notable changes to this project will be documented here.
Any updates that are currently in progress may feature some of their contents.
Unless the version is considered to be major (x.x), it will not have a nickname for the version :(

---

## **v0.0.2** | xxx Git commits | TBD (To be Decided)

*A bit less spaghetti (and clanker code)! Settings are a work-in-progress but it's going great so far.*

### PLEASE KNOW
At the start of this project, I didn't know nearly as much about C# (the coding language) as I do now. So, for many things, I used AI "tools" to "help" me write my code. I'm moving away from it completely, as I want all my code to be made by me. I want to apologize for even using it in the first place, because it has made me need to waste a lot more time refactoring and improving upon the clanker code than if I were to just write it authentically. Also, no! The README is not at all written by AI (anymore lol), and I've put a ton of effort into making it look presentable >:(

### General
* More interactable objects
* Reworked/rewrote every player system
* Reworked/rewrote every manager system

### World Environment
* Added new materials
* Rewrote shaders
* Added a keypad door with changeable code
* Rewrote all door code to make it more fail-safe
* Reorganize door folders and responsibilties
* New sparks particle that is emitted from doors, broken panels, and more

### Audio
* Reorganized the FMOD project once more
* Added a FMOD helper class with a ton of functions to more easily call different types of playing
* Add spatialization to a ton of door/button events I forgot to add them to

### Debug Console
* Rewrote command identification system for the debug console using Reflection
    * Put all commands in a namespace that gets registered ^
* Rewrote a lot of code related to the debug console that used some sort of AI
    * Rewrote practically every command (unfortunately lol) ^
* Removed clanker 'sanity' command until I rewrite PlayerSanity
* Fixed the 'fmod' command and added an optional argument called 'list' that you can also manually refresh
* Renamed 'info' command to 'sysinfo'
* Added a new line to the debugger
* Added 'fpscap' command
* Added 'clearprefs' command
* Added an optional argument to 'help' that lets you specify a specific command and it will give you usage instructions and a description
* Added 'locale' command that returns your system language
* Added 'uptime' command that tells you how long the game has been running
* Added 'physics' command that gives you a bunch of different stats on physics simulation
* Added 'freecam' command that connects with the work-in-progress PlayerFreecam/filmmaker

### Miscellanous
* Updated a ton of packages

## v0.0.1 | 153 Git commits | 2025-7-9 (July 9th, 2025)

*So much spaghetti!*
*(Open the Debug Console using F2 on PC or D-Pad Up + Menu Button on controller)*

### General
* Added player controller scripts
* Added a player interaction system (over 600 lines 😭)
* Added a player sanity manager
* Added a player effects manager
* Added a game manager with queue

### World Environment
* Made a cute little working camera that follows you
* Working doors/gates
* Ability to break any door/gate

### Audio
* Migrated over to FMOD for audio
* Made the FMOD Studio project an optional, seperate Git submodule that's committed to seperately

### Debug Console
* Added a debug console
* Added midget command :D
* Added effect command
* Added FMOD event handler command
* Added health command
* Added help command
* Added info command
* Added log command
* Added MAV command
* Added quit command
* Added sanity command
* Added scene loader command
* Added time command

### Miscellanous
* Added a bar meter element helper that can be used for any type of UI, like loading screens, indicators, and more!
* So much more that I don't even remember haha

---

Although there isn't much gameplay to speak of, the core gameplay mechanics are all there (except for levers, but that can be added pretty easily). I really do hope this turns out to be a great project that meets everyone's expectations :)