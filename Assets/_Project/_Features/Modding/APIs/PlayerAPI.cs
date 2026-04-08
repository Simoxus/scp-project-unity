using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

[ModAPI("Player")]
[MoonSharpUserData]
public class PlayerAPI
{
    private static readonly Script _sharedScript = new Script();
    private Player _player => Core.Player;

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current walk speed.")]
    public float GetWalkSpeed()
    {
        if (_player.Controller != null)
        {
            return _player.Controller.WalkSpeed;
        }
        return 0f;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets the player's walk speed.")]
    [LuaParam("speed", "Walk speed in units per second")]
    public void SetWalkSpeed(float speed)
    {
        if (_player.Controller != null)
        {
            _player.Controller.WalkSpeed = speed;
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current sprint speed.")]
    public float GetSprintSpeed()
    {
        if (_player.Controller != null)
        {
            return _player.Controller.SprintSpeed;
        }
        return 0f;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets the player's sprint speed.")]
    [LuaParam("speed", "Sprint speed in units per second")]
    public void SetSprintSpeed(float speed)
    {
        if (_player.Controller != null)
        {
            _player.Controller.SprintSpeed = speed;
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current crouch speed.")]
    public float GetCrouchSpeed()
    {
        if (_player.Controller != null)
        {
            return _player.Controller.CrouchSpeed;
        }
        return 0f;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets the player's crouch speed.")]
    [LuaParam("speed", "Crouch speed in units per second")]
    public void SetCrouchSpeed(float speed)
    {
        if (_player.Controller != null)
        {
            _player.Controller.CrouchSpeed = speed;
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current health value.")]
    public float GetHealth()
    {
        return _player.Health.GetHealth();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's maximum health value.")]
    public float GetMaxHealth()
    {
        return _player.Health.GetMaxHealth();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's health as a 0-1 percentage.")]
    public float GetHealthPercent()
    {
        return _player.Health.GetHealthPercent();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Deals damage to the player.")]
    [LuaParam("amount", "Amount of damage to deal")]
    public void DamagePlayer(float amount)
    {
        _player.Health.Take(amount);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Heals the player by the given amount, clamped to max health.")]
    [LuaParam("amount", "Amount of health to restore")]
    public void HealPlayer(float amount)
    {
        _player.Health.Heal(amount);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets the player's health to an exact value.")]
    [LuaParam("amount", "Health value to set")]
    public void SetHealth(float amount)
    {
        _player.Health.Set(amount);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's health as a named level.")]
    public string GetHealthLevel()
    {
        float percent = GetHealthPercent();
        if (percent <= 0f) { return "Dead"; }
        if (percent <= 0.25f) { return "NearDeath"; }
        if (percent <= 0.5f) { return "Critical"; }
        if (percent <= 0.75f) { return "Injured"; }
        return "Healthy";
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current state as a string.")]
    public string GetPlayerState()
    {
        return _player.CurrentState.ToString();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player's current state matches the given state name.")]
    [LuaParam("stateName", "State name to check against")]
    public bool IsPlayerState(string stateName)
    {
        return _player.CurrentState.ToString() == stateName;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is currently walking.")]
    public bool IsWalking()
    {
        return _player.CurrentState == PlayerState.Walking;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is currently sprinting.")]
    public bool IsSprinting()
    {
        return _player.CurrentState == PlayerState.Sprinting;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is currently crouching.")]
    public bool IsCrouching()
    {
        return _player.CurrentState == PlayerState.Crouching;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is in noclip mode.")]
    public bool IsNoclip()
    {
        return _player.CurrentState == PlayerState.Noclip;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is currently on the ground.")]
    public bool IsGrounded()
    {
        return _player.IsGrounded();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is currently moving.")]
    public bool IsMoving()
    {
        return _player.IsMoving();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player is currently allowed to move.")]
    public bool CanMove()
    {
        return _player.CanMove();
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current world position.")]
    public Vector3 GetPosition()
    {
        return _player.transform.position;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets the player's world position directly; use Teleport for safe movement.")]
    [LuaParam("x", "X coordinate")]
    [LuaParam("y", "Y coordinate (up)")]
    [LuaParam("z", "Z coordinate")]
    public void SetPosition(float x, float y, float z)
    {
        _player.transform.position = new Vector3(x, y, z);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Safely teleports the player to a world position.")]
    [LuaParam("x", "X coordinate")]
    [LuaParam("y", "Y coordinate (up)")]
    [LuaParam("z", "Z coordinate")]
    public void Teleport(float x, float y, float z)
    {
        _player.CharacterController.enabled = false;
        _player.transform.position = new Vector3(x, y, z);
        _player.CharacterController.enabled = true;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current rotation.")]
    public Vector3 GetRotation()
    {
        return _player.transform.eulerAngles;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Sets the player's rotation.")]
    [LuaParam("x", "Pitch")]
    [LuaParam("y", "Yaw")]
    [LuaParam("z", "Roll")]
    public void SetRotation(float x, float y, float z)
    {
        _player.transform.eulerAngles = new Vector3(x, y, z);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's current forward direction.")]
    public Vector3 GetForward()
    {
        return _player.transform.forward;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the world position of the player's camera.")]
    public Vector3 GetCameraPosition()
    {
        return _player.CameraBrain.transform.position;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the forward direction vector of the player's camera.")]
    public Vector3 GetCameraForward()
    {
        return _player.CameraBrain.transform.forward;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Shakes the player's camera using a Cinemachine impulse.")]
    [LuaParam("intensity", "Strength of the shake")]
    [LuaParam("duration", "Duration passed to the impulse source")]
    public void ShakeCamera(float intensity, float duration)
    {
        if (_player.CameraImpulseSource != null)
        {
            _player.CameraImpulseSource.GenerateImpulse(intensity);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Enables or disables player movement.")]
    [LuaParam("enabled", "True to allow movement, false to block it")]
    public void SetMovementEnabled(bool enabled)
    {
        if (_player.Controller != null)
        {
            _player.Controller.enabled = enabled;
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the player's movement controller is currently enabled.")]
    public bool IsMovementEnabled()
    {
        return _player.Controller != null && _player.Controller.enabled;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Disables player movement for a set duration, then re-enables it.")]
    [LuaParam("duration", "Time in seconds to freeze the player")]
    public async UniTask FreezePlayer(float duration)
    {
        SetMovementEnabled(false);
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        SetMovementEnabled(true);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true while the given key is held down. Uses Unity KeyCode names, like 'Space', and 'LeftShift'.")]
    [LuaParam("keyCode", "Unity KeyCode as a string")]
    public bool IsKeyDown(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, out KeyCode key))
        {
            return Input.GetKey(key);
        }

        return false;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true on the frame the given key is first pressed down.")]
    [LuaParam("keyCode", "Unity KeyCode as a string")]
    public bool IsKeyPressed(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, out KeyCode key))
        {
            return Input.GetKeyDown(key);
        }

        return false;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true on the frame the given key is released.")]
    [LuaParam("keyCode", "Unity KeyCode as a string")]
    public bool IsKeyReleased(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, out KeyCode key))
        {
            return Input.GetKeyUp(key);
        }

        return false;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's root GameObject.")]
    public GameObject GetPlayerObject()
    {
        return _player.gameObject;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the player's Transform.")]
    public Transform GetTransform()
    {
        return _player.transform;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Registers a callback that fires whenever the player's health value changes.")]
    [LuaParam("callback", "Function receiving (current: number, max: number)")]
    public void OnHealthChanged(Closure callback)
    {
        if (_player.Health != null)
        {
            _player.Health.OnHealthChanged += (current, max) =>
            {
                try
                {
                    callback.Call(current, max);
                }
                catch (ScriptRuntimeException ex)
                {
                    Log.Exception(ex, message: ex.DecoratedMessage);
                }
            };
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Registers a callback that fires whenever the player's health level category changes.")]
    [LuaParam("callback", "Function receiving")]
    public void OnHealthLevelChanged(Closure callback)
    {
        if (_player.Health != null)
        {
            _player.Health.OnHealthLevelChanged += (level) =>
            {
                try
                {
                    callback.Call(level.ToString());
                }
                catch (ScriptRuntimeException ex)
                {
                    Log.Exception(ex, message: ex.DecoratedMessage);
                }
            };
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Shoots a ray from the center of the camera. Returns a table with: hit (bool), point (Vector3), normal (Vector3), distance (number), objectName (string), and the object (GameObject).")]
    [LuaParam("maxDistance", "Maximum ray distance in units")]
    public Table RaycastFromCamera(float maxDistance)
    {
        Table result = new Table(_sharedScript);
        Ray ray = _player.CameraBrain.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            result["hit"] = true;
            result["point"] = hit.point;
            result["normal"] = hit.normal;
            result["distance"] = hit.distance;

            if (hit.collider.gameObject != null)
            {
                result["objectName"] = hit.collider.gameObject.name;
                result["object"] = hit.collider.gameObject;
            }
        }
        else
        {
            result["hit"] = false;
        }

        return result;
    }
}