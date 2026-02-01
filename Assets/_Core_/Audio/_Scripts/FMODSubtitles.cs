using AOT;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class FMODSubtitles
{
    private const string DEFAULT_TABLE_NAME = "Subtitles";
    public static float ExtraSubtitleTime = 0.7f;

    private static readonly HashSet<EventInstance> _registeredEvents = new HashSet<EventInstance>();
    private static readonly Dictionary<string, int> _activeStartMarkers = new Dictionary<string, int>();

    public static void RegisterEvent(EventInstance eventInstance)
    {
        if (!eventInstance.isValid() || _registeredEvents.Contains(eventInstance)) return;

        eventInstance.setCallback(EventCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.DESTROYED);
        _registeredEvents.Add(eventInstance);
    }

    public static void UnregisterEvent(EventInstance eventInstance)
    {
        if (eventInstance.isValid() && _registeredEvents.Contains(eventInstance))
        {
            eventInstance.setCallback(null);
            _registeredEvents.Remove(eventInstance);
        }
    }

    public static void ClearAll()
    {
        foreach (var evt in _registeredEvents)
        {
            if (evt.isValid())
            {
                evt.setCallback(null);
            }
        }
        _registeredEvents.Clear();
        _activeStartMarkers.Clear();
    }

    [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static FMOD.RESULT EventCallback(EVENT_CALLBACK_TYPE type, IntPtr eventPtr, IntPtr parameterPtr)
    {
        if (Core.UI?.Subtitles == null)
            return FMOD.RESULT.OK;

        EventInstance eventInstance = new EventInstance(eventPtr);

        if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var parameter = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(
                parameterPtr,
                typeof(TIMELINE_MARKER_PROPERTIES)
            );

            string markerName = parameter.name;

            if (markerName.StartsWith("subtitle:", StringComparison.OrdinalIgnoreCase))
            {
                ParseAndShowSubtitle(markerName, eventInstance);
            }
        }
        else if (type == EVENT_CALLBACK_TYPE.DESTROYED)
        {
            _registeredEvents.Remove(eventInstance);
        }

        return FMOD.RESULT.OK;
    }

    private static void ParseAndShowSubtitle(string markerName, EventInstance eventInstance)
    {
        // Remove "subtitle:" prefix
        string content = markerName.Substring(9);
        if (string.IsNullOrEmpty(content)) return;

        string[] parts = content.Split(':');
        if (parts.Length == 0) return;

        // Check for START/END markers
        string firstPart = parts[0].Trim();
        if (firstPart.Equals("START", StringComparison.OrdinalIgnoreCase))
        {
            HandleStartMarker(parts, eventInstance);
            return;
        }
        else if (firstPart.Equals("END", StringComparison.OrdinalIgnoreCase))
        {
            HandleEndMarker(parts);
            return;
        }

        // Fall back to original duration-based logic
        string speaker = null;
        string message = null;
        string tableName = null;
        string localizationKey = null;
        float duration = -1f; // -1 means use sound duration

        int currentIndex = 0;

        if (parts.Length > currentIndex && float.TryParse(parts[currentIndex].Trim(), out float parsedDuration))
        {
            duration = parsedDuration;
            currentIndex++;
        }

        if (parts.Length <= currentIndex) return;

        string nextPart = parts[currentIndex].Trim();
        if (nextPart.Contains("."))
        {
            string[] locParts = nextPart.Split(new[] { '.' }, 2);
            if (locParts.Length == 2)
            {
                tableName = locParts[0].Trim();
                // Auto-prepend "Subtitles_" if not already present
                if (!tableName.StartsWith("Subtitles", StringComparison.OrdinalIgnoreCase))
                {
                    tableName = "Subtitles_" + tableName;
                }
                localizationKey = locParts[1].Trim();
            }
            currentIndex++;
        }
        else if (parts.Length > currentIndex + 1)
        {
            speaker = nextPart;
            currentIndex++;

            string messagePart = parts[currentIndex].Trim();
            if (messagePart.Contains("."))
            {
                string[] locParts = messagePart.Split(new[] { '.' }, 2);
                if (locParts.Length == 2)
                {
                    tableName = locParts[0].Trim();
                    // Auto-prepend "Subtitles_" if not already present
                    if (!tableName.StartsWith("Subtitles", StringComparison.OrdinalIgnoreCase))
                    {
                        tableName = "Subtitles_" + tableName;
                    }
                    localizationKey = locParts[1].Trim();
                }
                currentIndex++;
            }
            else
            {
                message = string.Join(":", parts, currentIndex, parts.Length - currentIndex).Trim();
            }
        }
        else
        {
            // Check if it contains an underscore - treat as table_key format
            if (nextPart.Contains("_"))
            {
                int firstUnderscore = nextPart.IndexOf('_');
                string possibleTable = nextPart.Substring(0, firstUnderscore);
                string possibleKey = nextPart.Substring(firstUnderscore + 1);

                // If it starts with "Subtitles", use it as the table name
                if (possibleTable.StartsWith("Subtitles", StringComparison.OrdinalIgnoreCase))
                {
                    tableName = possibleTable;
                    localizationKey = possibleKey;
                }
                // Otherwise treat the whole thing as a key with default table
                else
                {
                    tableName = DEFAULT_TABLE_NAME;
                    localizationKey = nextPart;
                }
            }
            // No underscore and looks like uppercase key - use default table
            else if (!nextPart.Contains(" ") && nextPart.ToUpper() == nextPart)
            {
                tableName = DEFAULT_TABLE_NAME;
                localizationKey = nextPart;
            }
            else
            {
                message = nextPart;
            }
        }

        if (duration < 0f)
        {
            duration = GetSoundDuration(eventInstance);
            if (duration <= 0f) duration = 3f;
        }

        duration += ExtraSubtitleTime;

        // Show subtitle
        if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(localizationKey))
        {
            Core.UI.Subtitles.ShowLocalizedSubtitle(tableName, localizationKey, duration, speaker);
        }
        else if (!string.IsNullOrEmpty(message))
        {
            Core.UI.Subtitles.ShowSubtitle(message, duration, speaker);
        }
    }

    private static void HandleStartMarker(string[] parts, EventInstance eventInstance)
    {
        if (parts.Length < 2) return;

        string identifier = null;
        string speaker = null;
        string tableName = null;
        string localizationKey = null;
        string message = null;

        int currentIndex = 1;
        string nextPart = parts[currentIndex].Trim();

        // Check if this is a localized subtitle (contains a dot)
        if (nextPart.Contains("."))
        {
            string[] locParts = nextPart.Split(new[] { '.' }, 2);
            if (locParts.Length == 2)
            {
                tableName = locParts[0].Trim();
                if (!tableName.StartsWith("Subtitles", StringComparison.OrdinalIgnoreCase))
                {
                    tableName = "Subtitles_" + tableName;
                }
                localizationKey = locParts[1].Trim();
                identifier = nextPart; // Use full "Table.Key" as identifier
            }
        }
        // Check for speaker:message format
        else if (parts.Length > currentIndex + 1)
        {
            speaker = nextPart;
            currentIndex++;

            string messagePart = parts[currentIndex].Trim();
            if (messagePart.Contains("."))
            {
                string[] locParts = messagePart.Split(new[] { '.' }, 2);
                if (locParts.Length == 2)
                {
                    tableName = locParts[0].Trim();
                    if (!tableName.StartsWith("Subtitles", StringComparison.OrdinalIgnoreCase))
                    {
                        tableName = "Subtitles_" + tableName;
                    }
                    localizationKey = locParts[1].Trim();
                    identifier = messagePart;
                }
            }
            else
            {
                message = string.Join(":", parts, currentIndex, parts.Length - currentIndex).Trim();
                identifier = speaker + ":" + message;
            }
        }
        else
        {
            message = nextPart;
            identifier = message;
        }

        if (string.IsNullOrEmpty(identifier)) return;

        // Show subtitle with a very long duration (will be stopped by END marker or event end)
        float duration = 999f;
        int handle = -1;

        if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(localizationKey))
        {
            handle = Core.UI.Subtitles.ShowLocalizedSubtitleWithHandle(tableName, localizationKey, duration, speaker);
        }
        else if (!string.IsNullOrEmpty(message))
        {
            handle = Core.UI.Subtitles.ShowSubtitleWithHandle(message, duration, speaker);
        }

        if (handle >= 0)
        {
            _activeStartMarkers[identifier] = handle;
        }
    }

    private static void HandleEndMarker(string[] parts)
    {
        if (parts.Length < 2) return;

        // Reconstruct the identifier from remaining parts
        string identifier = string.Join(":", parts, 1, parts.Length - 1).Trim();

        if (_activeStartMarkers.TryGetValue(identifier, out int handle))
        {
            _activeStartMarkers.Remove(identifier);
            Core.UI.Subtitles.RemoveSubtitle(handle);
        }
    }

    private static float GetSoundDuration(EventInstance eventInstance)
    {
        if (!eventInstance.isValid()) return 0f;

        eventInstance.getDescription(out FMOD.Studio.EventDescription description);
        if (description.isValid())
        {
            description.getLength(out int length);
            return length / 1000f;
        }

        return 0f;
    }
}