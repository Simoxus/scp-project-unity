using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Browsable audio-cue glossary, modeled on The Last of Us Part II's: while the game
/// is paused, press G to hear each accessibility sound followed by its spoken meaning —
/// learn the vocabulary before you need it in a panic. Cycles on repeated presses.
/// Pause-only by QA design decision: the glossary is a reference you consult calmly,
/// not a gameplay key (additive stand-in for a proper pause-menu entry).
/// Lives on the accessibility manager object (works in any scene).
/// </summary>
public class SoundGlossary : MonoBehaviour
{
    private struct Entry
    {
        public string fileName;
        public float fallbackFrequency;
        public float fallbackDuration;
        public string meaning;
    }

    private static readonly Entry[] Entries =
    {
        new Entry { fileName = "door_beep.ogg", fallbackFrequency = 520f, fallbackDuration = 0.09f, meaning = "Una puerta." },
        new Entry { fileName = "item_beep.ogg", fallbackFrequency = 1175f, fallbackDuration = 0.05f, meaning = "Un objeto agarrable." },
        new Entry { fileName = "wall_bump_000.ogg", fallbackFrequency = 90f, fallbackDuration = 0.06f, meaning = "Choque contra una pared." },
        new Entry { fileName = "blink_warning.ogg", fallbackFrequency = 880f, fallbackDuration = 0.045f, meaning = "Parpadeo inminente." },
    };

    private A11yFmodAudio.A11ySound[] _sounds;
    private int _index = -1;
    private InputAction _glossaryAction;

    private void Awake()
    {
        _sounds = new A11yFmodAudio.A11ySound[Entries.Length];
        for (int i = 0; i < Entries.Length; i++)
        {
            _sounds[i] = A11yFmodAudio.LoadOrGenerate(Entries[i].fileName, Entries[i].fallbackFrequency, Entries[i].fallbackDuration);
        }

        _glossaryAction = new InputAction("A11yGlossary", binding: "<Keyboard>/g");
        _glossaryAction.performed += _ => Next();
    }

    private void OnEnable() => _glossaryAction.Enable();
    private void OnDisable() => _glossaryAction.Disable();

    private void OnDestroy()
    {
        _glossaryAction.Dispose();
        for (int i = 0; i < _sounds.Length; i++)
        {
            A11yFmodAudio.Release(ref _sounds[i]);
        }
    }

    private void Next()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;

        var gameManager = Core.GameManager;
        bool paused = gameManager != null ? gameManager.gamePaused : Time.timeScale == 0f;
        if (!paused)
        {
            ScreenReaderOutput.Speak("El glosario se abre con el juego en pausa.", true);
            return;
        }

        _index = (_index + 1) % Entries.Length;
        A11yFmodAudio.Play2D(_sounds[_index], manager.sonarVolume);
        ScreenReaderOutput.Speak($"{Entries[_index].meaning} {_index + 1} de {Entries.Length}.", true);
    }
}
