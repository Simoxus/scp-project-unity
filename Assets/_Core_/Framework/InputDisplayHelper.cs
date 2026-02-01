using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public static class InputDisplayHelper
{
    public static string GetDisplay(InputAction action, string schemeName)
    {
        if (action == null) return "Unbound";

        var asset = action.actionMap?.asset;
        if (asset == null) return "Unbound";

        var controlScheme = asset.controlSchemes.FirstOrDefault(cs => cs.name == schemeName);
        if (controlScheme.Equals(default(InputControlScheme)))
            return "Unbound";

        string bindingGroup = controlScheme.bindingGroup;

        // Also check for common variations
        var groupsToCheck = new List<string> { bindingGroup };
        if (bindingGroup == "KeyboardMouse" || bindingGroup == "Keyboard&Mouse")
        {
            groupsToCheck.Add("Keyboard");
            groupsToCheck.Add("Mouse");
        }

        var displays = new List<string>();
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isPartOfComposite)
                continue;

            if (binding.isComposite)
            {
                if (CompositeMatchesScheme(action, i, groupsToCheck))
                {
                    string display = GetCompositeDisplayName(action, i);
                    if (!string.IsNullOrEmpty(display) && !displays.Contains(display))
                        displays.Add(display);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(binding.groups) && groupsToCheck.Any(g => binding.groups.Contains(g)))
                {
                    string display = action.GetBindingDisplayString(i);
                    if (!string.IsNullOrEmpty(display))
                    {
                        display = CleanupDisplay(display);
                        if (!displays.Contains(display))
                            displays.Add(display);
                    }
                }
            }
        }

        if (displays.Count == 0)
            return "Unbound";

        return string.Join("/", displays);
    }

    private static bool CompositeMatchesScheme(InputAction action, int compositeIndex, List<string> groupsToCheck)
    {
        for (int i = compositeIndex + 1; i < action.bindings.Count; i++)
        {
            var part = action.bindings[i];

            // Stop when we reach the end of this composite's parts
            if (!part.isPartOfComposite)
                break;

            // Check if this part matches any of our groups
            if (!string.IsNullOrEmpty(part.groups) && groupsToCheck.Any(g => part.groups.Contains(g)))
                return true;
        }

        return false;
    }

    private static string GetCompositeDisplayName(InputAction action, int compositeIndex)
    {
        var partKeys = new List<string>();

        // Collect all part bindings
        for (int i = compositeIndex + 1; i < action.bindings.Count; i++)
        {
            var part = action.bindings[i];

            if (!part.isPartOfComposite)
                break;

            string key = InputControlPath.ToHumanReadableString(
                part.path,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            ).Trim();

            if (!string.IsNullOrEmpty(key))
                partKeys.Add(key);
        }

        if (partKeys.Count == 0)
            return null;

        // Pattern matching for known composites
        var keySet = new HashSet<string>(partKeys, StringComparer.OrdinalIgnoreCase);

        if (keySet.Count == 8 &&
            keySet.Contains("W") && keySet.Contains("A") && keySet.Contains("S") && keySet.Contains("D") &&
            partKeys.Any(k => k.Contains("Up") && k.Contains("Arrow")) &&
            partKeys.Any(k => k.Contains("Down") && k.Contains("Arrow")) &&
            partKeys.Any(k => k.Contains("Left") && k.Contains("Arrow")) &&
            partKeys.Any(k => k.Contains("Right") && k.Contains("Arrow")))
            return "WASD/Arrow Keys";

        if (keySet.Count == 4 && keySet.Contains("W") && keySet.Contains("A") && keySet.Contains("S") && keySet.Contains("D"))
            return "WASD";

        if (keySet.Count == 4 &&
            partKeys.Any(k => k.Contains("Up") && k.Contains("Arrow")) &&
            partKeys.Any(k => k.Contains("Down") && k.Contains("Arrow")) &&
            partKeys.Any(k => k.Contains("Left") && k.Contains("Arrow")) &&
            partKeys.Any(k => k.Contains("Right") && k.Contains("Arrow")))
            return "Arrow Keys";

        if (partKeys.All(k => k.Contains("Left Stick")))
            return "Left Stick";

        if (partKeys.All(k => k.Contains("Right Stick")))
            return "Right Stick";

        if (partKeys.All(k => k.Contains("D-Pad") || k.Contains("Dpad")))
            return "D-Pad";

        if (partKeys.Any(k => k.Contains("Scroll")))
            return "Mouse Wheel";

        return null;
    }

    private static string CleanupDisplay(string display)
    {
        if (string.IsNullOrEmpty(display))
            return display;

        var cleanups = new Dictionary<string, string>
        {
            { "LS", "Left Stick" },
            { "RS", "Right Stick" },
            { "Delta", "Mouse" },
            { "Scroll/Y", "Mouse Wheel" },
        };

        foreach (var kvp in cleanups)
        {
            display = display.Replace(kvp.Key, kvp.Value);
        }

        display = display.Replace(" | ", "/");

        return display;
    }

    public static string GetCombinedDisplay(InputAction action)
    {
        string keyboardMouse = GetDisplay(action, "KeyboardMouse");
        string gamepad = GetDisplay(action, "Gamepad");

        if (keyboardMouse != "Unbound" && gamepad != "Unbound")
            return $"{keyboardMouse}/{gamepad}";
        if (keyboardMouse != "Unbound")
            return keyboardMouse;
        if (gamepad != "Unbound")
            return gamepad;

        return "Unbound";
    }

    public static string GetCombinedDisplayBold(InputAction action)
    {
        string display = GetCombinedDisplay(action);
        return $"<b>{display}</b>";
    }

    public static string GetModifiedDisplay(InputAction mainAction, InputAction modifierAction, string schemeName)
    {
        if (mainAction == null) return "Unbound";

        string main = GetDisplay(mainAction, schemeName);

        if (main != "Unbound" && modifierAction != null)
        {
            string modifier = GetDisplay(modifierAction, schemeName);
            if (modifier != "Unbound")
                return $"{modifier} + {main}";
        }

        return main;
    }
}