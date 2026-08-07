# Accessibility Layer for SCP: Project Unity

A gameplay accessibility layer that makes SCP: Project Unity playable by blind and
low-vision players, built and QA-tested by a blind IAAP-certified accessibility
specialist (WCAG 2.2) who is a native screen reader user, with an AI coding agent
doing the implementation under her direction.

*Author / QA: María Pía  Peña Foissac — find me on any social network as **@blindaudiogamer**.*

The project roadmap already lists **"Accessibility options"** as a planned feature —
this layer is a working, tested implementation of that roadmap item, offered upstream.

Every feature maps to a specific guideline from
[gameaccessibilityguidelines.com](https://gameaccessibilityguidelines.com/) (noted per
feature below as GAG, with its category and tier).

## Features

### Audio navigation
- **Proximity sonar**: a spatialized 3D sound plays from the nearest reachable
  interactable, parking-sensor style (faster cadence = closer), with a double
  beat inside interaction range. Doors knock (wood), pickable items chime (bell) —
  semantic sounds, chosen by blind QA over abstract beeps.
  *GAG Vision, advanced: "Provide a pingable sonar-style audio map"; intermediate:
  "Use distinct sound/music design for all objects and events".*
- **Wall occlusion**: the sonar never sounds through walls (eye-height raycast over
  level geometry; occluded targets are skipped for the nearest visible one).
- **Wall contact feedback**: a soft thud (plus a subtle high-frequency rumble tick on
  gamepads) when deliberately walking into a wall. Sliding along walls stays silent.
  *GAG Vision, basic: "Ensure no essential information is conveyed by visuals alone".*

### Object navigation (KOTOR-style, requested by blind QA)
- **Q / E** (gamepad: d-pad left/right) cycle through reachable interactables, nearest
  first, announcing type, distance and relative direction via the screen reader:
  *"Door, 4 meters, ahead to the right. 1 of 3."*
- **Enter** (gamepad: d-pad down) interacts immediately if the target is in reach,
  or auto-walks to it and then interacts. Any manual movement cancels the walk;
  a watchdog reports *"Path blocked"* instead of walking into walls forever.
  *GAG Motor: "Include assist modes such as auto-aim and assisted steering".*

### Mouse-free camera
- **Comma / period**: smooth turn (speed persisted, adjustable).
- **Shift+comma / Shift+period**: 45° snap turns, announced ("northeast") —
  predictable orientation, an audiogame convention. **M**: 180° turn. **N** (gamepad:
  R3): speak compass heading. Pitch is deliberately unmapped: with no mouse input the
  vertical view never drifts.
  *GAG Motor: "Ensure that all areas of the user interface can be accessed using the
  same input method". Note: Shift was chosen over Ctrl as snap modifier because Ctrl
  is Crouch in this game — blind QA discovered Ctrl-combos were silently crouching
  the player.*

### Screen reader output (NVDA)
- Direct integration with the official **NVDA Controller Client** (LGPL 2.1,
  distributed unmodified with its license; P/Invoke following NV Access's own C#
  example). Degrades silently when NVDA is absent.
- Spoken: layer startup confirmation, scene descriptions on load (from an editable
  JSON alt-text registry, including transcriptions of text baked into menu images),
  health level changes, sanity threshold, crouch state changes, target announcements,
  fall/rescue events, sonar toggle state.
  *GAG Vision, advanced: "Provide pre-recorded voiceovers for all text including
  menus" (via the player's own TTS); basic: "no essential information by visuals
  alone".*
- **UI reader**: announces the focused uGUI element (alt-text → visible label →
  humanized object name, plus role: "button", "slider"...) and keeps an element
  focused in menu contexts so keyboard navigation always has a starting point.

### Situational awareness on demand (modeled on TLOU Part II, tuned by blind QA)
- **Area scan** (B / L3): pings every reachable interactable, nearest first — each
  target's own semantic sound plays at its position, with **pitch encoding relative
  height** (above = higher), ending with a spoken count. Preferred by our blind QA
  over a continuous sonar after playing TLOU's Enhanced Listen Mode; the continuous
  parking-sensor mode remains available as an opt-in toggle (Shift+F6).
- **Status report** (H, for "health" — it reads first): speaks health, stance,
  compass heading and nearest target:
  *"Healthy. Standing. Facing north. Door, 4 meters, ahead."*
- **Audio cue glossary** (G, while paused): cycles through every accessibility sound
  followed by its spoken meaning — learn the vocabulary before you need it in a panic
  (a TLOU feature our QA rated essential). Pause-only by QA design: a reference you
  consult calmly, and the natural candidate for a pause-menu entry once the real
  menu exists.
- **Room announcements**: on entering a new area, the layer says where you are
  ("You entered: Light Containment Zone"), using the game's own room system in
  generated facilities (RoomInstance bounds + RoomData name + zone) and a hierarchy
  fallback in hand-built scenes. Descriptions are editable JSON alt-text. *This is
  our blind QA's own addition — her one criticism of TLOU: "it never tells you which
  room you walked into."*
- **Ledge warning**: probes the floor ahead of your movement and warns "Ledge ahead"
  before the drop (a full movement-blocking Ledge Guard needs an upstream hook —
  see recommendations).

### Safety net (born from a real QA incident)
The blind tester walked through a door at the edge of the test map, fell into the
void, and got **total silence** — no footsteps, no sonar, nothing to indicate what
happened. Now:
- *"You are falling."* after 1.5 s airborne; *"You fell out of the map."* plus
  **automatic rescue** to the last safe ground after a 20 m drop below it.
- *"Movement blocked. R returns to safe ground."* when pushing against geometry
  without progress. **R** (gamepad: d-pad up) rescues on demand.

### Vitals and tension
- Health level changes are spoken (from the game's own `OnHealthLevelChanged` event);
  life-threatening states interrupt current speech.
- **Damage rumbles** on gamepads, scaled by hit size (design rule from blind QA:
  strong haptics are reserved for tense moments; ambient feedback stays subtle).
- **Blink warning** (dev branch): a short cue when the blink meter is about to force
  a blink — essential once SCP-173-style enemies land. Sound, not speech, on purpose:
  blinks recur every ~15 s and TTS would fatigue.

## Architecture: 100% additive

- All code lives in `Assets/_Project/_Features/Accessibility/` plus sound files in
  `StreamingAssets/A11y/`. **Zero edits to existing scripts, scenes, prefabs or the
  shared InputActionAsset.**
- A `[RuntimeInitializeOnLoadMethod]` bootstrap self-instantiates the layer in any
  scene; player-bound components attach at runtime via `Core.Player`.
- All accessibility keys are code-created `InputAction`s (audited against the
  project's bindings to avoid collisions); settings persist in PlayerPrefs under
  `A11y_*` keys.
- Audio goes through the **FMOD Core API** (the project rightly disables Unity's
  audio engine): file-based CC0 sounds with procedurally generated PCM fallbacks,
  no Studio banks required — so the layer keeps working in test scenes.
- Verified by **50+ batchmode smoke tests** (target selection, occlusion logic, wall
  bump rules, compass math, PCM encoding, damage-haptics curve, alt-text registry,
  UI descriptions) runnable via
  `Unity.exe -batchmode -quit -executeMethod AccessibilitySmokeTest.Run`.

## Licensing

| Component | Source | License | Compatibility |
| --- | --- | --- | --- |
| Layer source code | this contribution | CC BY-SA 4.0 (project license) | native |
| Sounds (`StreamingAssets/A11y`) | Kenney "Impact Sounds" & "Interface Sounds" (kenney.nl) | CC0 | verified on source pages |
| `nvdaControllerClient.dll` | NV Access official release (download.nvaccess.org) | LGPL 2.1, distributed unmodified with license file | dynamic linking, compatible |

## Recommendations for accessibility-friendly development

Practical, code-level advice for the remaining roadmap items, each grounded in
something we actually hit while building this layer:

1. **Raise C# events for every player-relevant state change.**
   `PlayerHealth.OnHealthLevelChanged` made spoken vitals a 20-line subscriber.
   Crouch has no event, so we had to poll — and a blind player spent a session
   mysteriously slow because crouching is communicated only visually. If a sighted
   player can perceive a state, there should be an event for it.
2. **Keep (and enrich) the `IInteractable` interface.** It is the single reason a
   sonar and an object navigator could be built without touching game code. Consider
   adding a display name / category to the interface (or a metadata component):
   assistive tech currently has to guess from GameObject names like "HandObject".
3. **Give every action a keyboard binding and keep bindings data-driven.**
   Interact currently has no keyboard key (mouse only), and one debug feature used a
   hardcoded `Input.GetKeyDown`. Everything in the InputActionAsset + user rebinding
   support covers GAG Motor "Allow controls to be remapped" almost for free.
4. **Render text as text.** The current menu is an image with the title baked in —
   we had to ship a hand-written alt-text registry. When the real UI arrives, uGUI/TMP
   labels (which the planned localization work needs anyway) are directly readable by
   assistive layers; localization keys double as speech strings.
5. **Design audio as a first-class information channel.** Each SCP will need a unique
   sound signature (GAG: "distinct sound design for all objects and events") — 173's
   presence, in particular, is life-or-death information that today exists only as
   pixels. FMOD events with distance/occlusion parameters (already the project's
   direction) are the right foundation; just keep them per-entity, not shared.
6. **Add kill-Z / world-bounds volumes to maps, including test maps.** Our tester
   fell through an unfinished map edge into an infinite, silent void. A bounds volume
   that respawns (or at least an event that fires) helps every player and every mod.
7. **Expose game events through the planned Lua modding API.** An accessibility layer
   is the prototypical mod: everything above consumes events, interfaces and
   registries. If the modding API exposes those, community a11y (and this layer)
   can live as a first-class mod.
8. **Prefer CLI-friendly build hooks.** FMOD's bank copy relies on an editor-update
   watcher that never fires in `-batchmode` builds; we had to call
   `EventManager.CopyToStreamingAssets` explicitly. Deterministic build steps help
   CI and modders alike.

## How to try it

1. Open the project with Unity 6000.0.67f1, run any Testing scene, have NVDA running.
2. Or build headless: `-executeMethod A11yBuild.BuildTestingCore` (declares scenes,
   refreshes FMOD banks, builds Addressables, Mono backend for QA machines without
   the C++ toolchain).
3. Keys: WASD move, Shift sprint, comma/period turn (Shift+ = snap 45°), M half turn,
   N compass, Q/E cycle targets, Enter go & interact, R rescue, F6 toggles the layer.
   DualSense/Xbox: d-pad left/right cycle, d-pad down go, d-pad up rescue, R3 compass.
