using TMPro;
using UnityEngine;

public class ConsoleRandomMotto : MonoBehaviour
{
    [Header("Chosen Mottos")]
    public bool isConsole;
    public bool isLogger;

    [Header("Console-Specific")]
    [SerializeField] private string consoleMottoSuffix = "Type 'help' if you've forgotten how to be God.";

    [Header("Text Settings")]
    [HideInInspector, SerializeField] private TMP_Text displayText;
    [SerializeField] private string mottoSeperator = 
        "--------------------------------------------------------------------------------------------------------------------------------------------";

    private string _combinedText;

    private string[] consoleMottos = new string[]
    {
        "\"Unauthorized access is a breach of protocol... but we’ll allow it.. just this once.\"",
        "\"Deviations from protocol will be recorded for later review.\"",
        "\"That key press was unnecessary.. but amusing. I do hope you enjoy cheating >:(.\"",
        "\"All paths lead here.. and yes, we were expecting you.\"",
        "\"How are you already in over your head? Either way, congratulations.\"",
    };

    private string[] loggerMottos = new string[]
    {
        "Because pressing the debug button makes you omnipotent.",
        "Today is pizza day!",
        "What have you done?",
        "The logs are the only truly scary part of the game.",
        "Your sanity is a precious resource to you. Please don't look at the logs."
    };

    private void Start()
    {
        if (displayText == null) { displayText = GetComponent<TMP_Text>(); }

        if (isConsole)
        {
            int randomIndex = Random.Range(0, consoleMottos.Length);
            string randomMotto = consoleMottos[randomIndex];
            _combinedText = $"<b>{randomMotto}\n{consoleMottoSuffix}</b>\n{mottoSeperator}";
        }
        else if (isLogger)
        {
            int randomIndex = Random.Range(0, loggerMottos.Length);
            string randomMotto = loggerMottos[randomIndex];
            _combinedText = $"<b>“{randomMotto}”</b>\n{mottoSeperator}";
        }

        // Set text of the TextMeshPro sibling component to the combined message
        displayText.text = _combinedText;
    }
}
