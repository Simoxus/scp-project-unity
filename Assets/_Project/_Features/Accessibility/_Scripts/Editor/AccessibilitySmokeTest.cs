using UnityEditor;
using UnityEngine;

/// <summary>
/// Batchmode-verifiable checks for the accessibility layer:
/// Unity.exe -batchmode -quit -projectPath (proj) -executeMethod AccessibilitySmokeTest.Run
/// Exits with code 0 when all checks pass, 1 otherwise.
/// </summary>
public static class AccessibilitySmokeTest
{
    private static int _failures;

    public static void Run()
    {
        _failures = 0;

        TestIntervalMapping();
        TestBeepGeneration();
        TestTargetSelection();
        TestOcclusionPredicate();
        TestWallBumpRule();
        TestBlinkWarningRule();
        TestCompassHeadings();
        TestRelativeDirections();
        TestScanPitch();
        TestDamageHaptics();
        TestAltTextRegistry();
        TestVitalStatusTexts();
        TestUIElementDescription();

        Debug.Log(_failures == 0
            ? "A11Y SMOKE TESTS: ALL PASS"
            : $"A11Y SMOKE TESTS: {_failures} FAILURE(S)");

        if (Application.isBatchMode) EditorApplication.Exit(_failures == 0 ? 0 : 1);
    }

    private static void Check(bool condition, string label)
    {
        if (condition)
        {
            Debug.Log($"[PASS] {label}");
        }
        else
        {
            _failures++;
            Debug.LogError($"[FAIL] {label}");
        }
    }

    private static void TestIntervalMapping()
    {
        float min = 0.13f, max = 1.1f, radius = 12f;
        Check(Mathf.Approximately(SonarLogic.IntervalForDistance(0f, radius, min, max), min), "Interval at distance 0 == min");
        Check(Mathf.Approximately(SonarLogic.IntervalForDistance(radius, radius, min, max), max), "Interval at radius == max");
        Check(SonarLogic.IntervalForDistance(radius * 2f, radius, min, max) <= max + 0.0001f, "Interval clamps beyond radius");
        float near = SonarLogic.IntervalForDistance(2f, radius, min, max);
        float far = SonarLogic.IntervalForDistance(9f, radius, min, max);
        Check(near < far, "Closer target beeps faster");
    }

    private static void TestBeepGeneration()
    {
        // AudioClip creation itself is not testable in batchmode (audio engine disabled:
        // "AudioClip.SetData failed"), so we verify the pure sample generation instead.
        var data = SonarAudio.GenerateBeepSamples(520f, 0.09f);
        Check(data != null && data.Length > 100, "Beep samples generated");

        bool hasSignal = false;
        bool inRange = true;
        foreach (var sample in data)
        {
            if (Mathf.Abs(sample) > 0.1f) hasSignal = true;
            if (float.IsNaN(sample) || Mathf.Abs(sample) > 1f) inRange = false;
        }
        Check(hasSignal, "Beep contains audible signal");
        Check(inRange, "Beep samples within [-1, 1] and not NaN");

        // Distinct timbres: door and item beeps must differ in frequency content
        var doorData = SonarAudio.GenerateBeepSamples(520f, 0.05f);
        var itemData = SonarAudio.GenerateBeepSamples(1175f, 0.05f);
        int doorCrossings = CountZeroCrossings(doorData);
        int itemCrossings = CountZeroCrossings(itemData);
        Check(itemCrossings > doorCrossings * 15 / 10, "Item beep is audibly higher-pitched than door beep");

        // PCM16 conversion for FMOD OPENRAW: length, little-endian layout and clipping
        var pcm = SonarAudio.SamplesToPcm16(new float[] { 0f, 1f, -1f, 2f });
        Check(pcm.Length == 8, "PCM16 output is 2 bytes per sample");
        Check(pcm[0] == 0 && pcm[1] == 0, "Zero sample encodes as zero");
        short maxVal = (short)(pcm[2] | (pcm[3] << 8));
        short minVal = (short)(pcm[4] | (pcm[5] << 8));
        short clipped = (short)(pcm[6] | (pcm[7] << 8));
        Check(maxVal == short.MaxValue && minVal == -short.MaxValue, "Full-scale samples encode to PCM16 limits");
        Check(clipped == short.MaxValue, "Out-of-range samples clip instead of wrapping");
    }

    private static void TestRelativeDirections()
    {
        Vector3 fwd = Vector3.forward;
        Check(TargetNavigator.RelativeDirectionName(fwd, Vector3.forward) == "adelante", "Target ahead = adelante");
        Check(TargetNavigator.RelativeDirectionName(fwd, Vector3.right) == "a la derecha", "Target right = a la derecha");
        Check(TargetNavigator.RelativeDirectionName(fwd, Vector3.back) == "atrás", "Target behind = atrás");
        Check(TargetNavigator.RelativeDirectionName(fwd, Vector3.left) == "a la izquierda", "Target left = a la izquierda");
        Check(TargetNavigator.RelativeDirectionName(fwd, new Vector3(1f, 0f, 1f)) == "adelante a la derecha", "Diagonal = adelante a la derecha");
        Check(TargetNavigator.RelativeDirectionName(fwd, new Vector3(0f, 5f, 0.01f)) == "adelante", "Vertical offset ignored");
    }

    private static int CountZeroCrossings(float[] data)
    {
        int crossings = 0;
        for (int i = 1; i < data.Length; i++)
        {
            if ((data[i - 1] < 0f) != (data[i] < 0f)) crossings++;
        }
        return crossings;
    }

    private static void TestTargetSelection()
    {
        var doorGo = new GameObject("SmokeDoorButton");
        doorGo.transform.position = new Vector3(0f, 0f, 5f);
        var doorCollider = doorGo.AddComponent<BoxCollider>();
        doorGo.AddComponent<ButtonDoorActivator>();

        var itemGo = new GameObject("SmokeItem");
        itemGo.transform.position = new Vector3(0f, 0f, 2f);
        var itemCollider = itemGo.AddComponent<BoxCollider>();
        itemGo.AddComponent<BasicInteract>();

        var emptyGo = new GameObject("SmokeNotInteractable");
        emptyGo.transform.position = Vector3.zero;
        var emptyCollider = emptyGo.AddComponent<BoxCollider>();

        try
        {
            var hits = new Collider[] { doorCollider, emptyCollider, itemCollider };

            bool found = SonarLogic.TryChooseTarget(Vector3.zero, hits, hits.Length, out var kind, out var chosen, out var pos, out var dist);
            Check(found, "Target found among mixed colliders");
            Check(kind == SonarTargetKind.Item, "Nearest target (item at 2m) wins over door at 5m");
            Check(chosen == itemCollider, "Chosen collider is the item's");
            Check(Mathf.Approximately(dist, 2f), "Distance to chosen target is 2m");

            var doorOnly = new Collider[] { doorCollider, emptyCollider };
            found = SonarLogic.TryChooseTarget(Vector3.zero, doorOnly, doorOnly.Length, out kind, out chosen, out pos, out dist);
            Check(found && kind == SonarTargetKind.Door, "BaseDoorActivator subclass classified as Door");

            var noneOnly = new Collider[] { emptyCollider };
            found = SonarLogic.TryChooseTarget(Vector3.zero, noneOnly, noneOnly.Length, out kind, out chosen, out pos, out dist);
            Check(!found, "No interactables -> no target");
        }
        finally
        {
            Object.DestroyImmediate(doorGo);
            Object.DestroyImmediate(itemGo);
            Object.DestroyImmediate(emptyGo);
        }
    }

    private static void TestOcclusionPredicate()
    {
        var doorGo = new GameObject("SmokeOccludedDoor");
        doorGo.transform.position = new Vector3(0f, 0f, 5f);
        var doorCollider = doorGo.AddComponent<BoxCollider>();
        doorGo.AddComponent<ButtonDoorActivator>();

        var itemGo = new GameObject("SmokeVisibleItem");
        itemGo.transform.position = new Vector3(0f, 0f, 2f);
        var itemCollider = itemGo.AddComponent<BoxCollider>();
        itemGo.AddComponent<BasicInteract>();

        try
        {
            var hits = new Collider[] { doorCollider, itemCollider };

            // Predicate rejects the nearer item (as if a wall blocked it): the sonar must fall back to the door
            bool found = SonarLogic.TryChooseTarget(Vector3.zero, hits, hits.Length, out var kind, out var chosen, out _, out var dist,
                collider => collider != itemCollider);
            Check(found && kind == SonarTargetKind.Door && Mathf.Approximately(dist, 5f), "Occluded nearest target is skipped in favor of a visible one");

            // Predicate rejects everything: total occlusion means silence
            found = SonarLogic.TryChooseTarget(Vector3.zero, hits, hits.Length, out kind, out chosen, out _, out _, _ => false);
            Check(!found, "All targets occluded -> no beep");
        }
        finally
        {
            Object.DestroyImmediate(doorGo);
            Object.DestroyImmediate(itemGo);
        }
    }

    private static void TestWallBumpRule()
    {
        Vector3 wallNormal = new Vector3(0f, 0f, -1f); // wall facing the player
        Check(SonarLogic.ShouldBumpFeedback(Vector3.forward, wallNormal), "Walking head-on into a wall bumps");
        Check(!SonarLogic.ShouldBumpFeedback(Vector3.right, wallNormal), "Sliding parallel to a wall stays silent");
        Check(!SonarLogic.ShouldBumpFeedback(Vector3.forward, Vector3.up), "Floor contact never bumps");
        Check(!SonarLogic.ShouldBumpFeedback(Vector3.zero, wallNormal), "No movement input, no bump");
        Vector3 diagonal = new Vector3(0.5f, 0f, 0.5f).normalized; // 45 degrees into the wall (dot ~0.707)
        Check(SonarLogic.ShouldBumpFeedback(diagonal, wallNormal), "Diagonal push into the wall still bumps");
    }

    private static void TestBlinkWarningRule()
    {
        Check(SonarLogic.ShouldWarnBlink(true, false, 0.10f, 0.15f), "Warns when meter drains past threshold");
        Check(!SonarLogic.ShouldWarnBlink(true, false, 0.50f, 0.15f), "Silent while meter is high");
        Check(!SonarLogic.ShouldWarnBlink(true, true, 0.10f, 0.15f), "Silent while already blinking");
        Check(!SonarLogic.ShouldWarnBlink(false, false, 0.10f, 0.15f), "Fires only once per blink cycle (disarmed)");
    }

    private static void TestCompassHeadings()
    {
        Check(KeyboardLook.HeadingName(0f) == "norte", "Yaw 0 = norte");
        Check(KeyboardLook.HeadingName(90f) == "este", "Yaw 90 = este");
        Check(KeyboardLook.HeadingName(180f) == "sur", "Yaw 180 = sur");
        Check(KeyboardLook.HeadingName(270f) == "oeste", "Yaw 270 = oeste");
        Check(KeyboardLook.HeadingName(45f) == "noreste", "Yaw 45 = noreste");
        Check(KeyboardLook.HeadingName(-45f) == "noroeste", "Negative yaw wraps (-45 = noroeste)");
        Check(KeyboardLook.HeadingName(359f) == "norte", "Yaw 359 rounds to norte");
        Check(KeyboardLook.HeadingName(675f) == "noroeste", "Yaw beyond 360 wraps (675 = noroeste)");
    }

    private static void TestScanPitch()
    {
        Check(Mathf.Approximately(SonarLogic.PitchForHeightDelta(0f), 1f), "Level target = neutral pitch");
        Check(SonarLogic.PitchForHeightDelta(3f) > 1f, "Target above = higher pitch");
        Check(SonarLogic.PitchForHeightDelta(-3f) < 1f, "Target below = lower pitch");
        Check(SonarLogic.PitchForHeightDelta(100f) <= 1.6f, "Pitch clamps upward");
        Check(SonarLogic.PitchForHeightDelta(-100f) >= 0.6f, "Pitch clamps downward");
    }

    private static void TestDamageHaptics()
    {
        Vector3 small = A11yHaptics.DamagePulseParams(0.05f);
        Vector3 big = A11yHaptics.DamagePulseParams(0.6f);
        Vector3 over = A11yHaptics.DamagePulseParams(5f);
        Check(big.x > small.x && big.y > small.y && big.z > small.z, "Bigger hits rumble harder and longer");
        Check(over.x <= 1f && over.y <= 1f, "Rumble intensities clamp at 1");
        Check(small.x >= 0.35f, "Even small hits are clearly felt (tense-moment rule)");
    }

    private static void TestAltTextRegistry()
    {
        Check(A11yAltText.TryGet("MainMenu_BG", out var bgText) && bgText.Contains("Containment Breach"),
            "Alt text for menu background loads from JSON");
        Check(A11yAltText.TryGet("scene:MainMenu", out var sceneText) && sceneText.Contains("Discord"),
            "Scene description for MainMenu loads from JSON");
        Check(A11yAltText.TryGet("discord", out _), "Alt text lookup is case-insensitive");
        Check(!A11yAltText.TryGet("NoExisteEstaClave", out _), "Unknown keys report a miss");
        Check(A11yAltText.HumanizeName("Puerta_Oficina(Clone)") == "Puerta Oficina", "Humanized fallback strips clutter");
    }

    private static void TestVitalStatusTexts()
    {
        var levels = (PlayerHealth.HealthLevel[])System.Enum.GetValues(typeof(PlayerHealth.HealthLevel));
        var seen = new System.Collections.Generic.HashSet<string>();
        bool allValid = true;
        foreach (var level in levels)
        {
            string text = VitalStatusAnnouncer.HealthLevelText(level);
            if (string.IsNullOrWhiteSpace(text) || !seen.Add(text)) allValid = false;
        }
        Check(allValid, "Every health level has a distinct spoken message");
        Check(VitalStatusAnnouncer.HealthLevelText(PlayerHealth.HealthLevel.Dead) == "Has muerto.", "Dead state message is explicit");
    }

    private static void TestUIElementDescription()
    {
        // A button labeled by its child legacy Text, with a role suffix
        var buttonGo = new GameObject("SmokeButton");
        buttonGo.AddComponent<UnityEngine.UI.Button>();
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(buttonGo.transform);
        var label = labelGo.AddComponent<UnityEngine.UI.Text>();
        label.text = "Jugar";

        // An unlabeled selectable falls back to its humanized name
        var sliderGo = new GameObject("Volumen_Musica");
        sliderGo.AddComponent<UnityEngine.UI.Slider>();

        try
        {
            Check(UIReader.DescribeElement(buttonGo) == "Jugar, botón", "Button description = visible label + role");
            Check(UIReader.DescribeElement(sliderGo) == "Volumen Musica, deslizador", "Slider falls back to humanized name + role");
        }
        finally
        {
            Object.DestroyImmediate(buttonGo);
            Object.DestroyImmediate(sliderGo);
        }
    }
}
