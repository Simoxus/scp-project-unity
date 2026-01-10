using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

[MoonSharpUserData]
public class PlayerAPI
{
    private Player _player => Core.Player;

    /// <summary>
    /// Gets the player's current walk speed
    /// </summary>
    /// <returns>The walk speed in units per second</returns>
    [MoonSharpVisible(true)]
    public float GetWalkSpeed()
    {
        if (_player.PlayerController != null)
        {
            return _player.PlayerController.WalkSpeed;
        }
        return 0f;
    }

    /// <summary>
    /// Sets the player's walk speed
    /// </summary>
    /// <param name="speed">The desired walk speed in units per second</param>
    [MoonSharpVisible(true)]
    public void SetWalkSpeed(float speed)
    {
        if (_player.PlayerController != null)
        {
            _player.PlayerController.WalkSpeed = speed;
            Log.VerboseInfo($"Walk speed set to {speed}");
        }
    }

    [MoonSharpVisible(true)]
    public float GetSprintSpeed()
    {
        if (_player.PlayerController != null)
        {
            return _player.PlayerController.SprintSpeed;
        }
        return 0f;
    }

    [MoonSharpVisible(true)]
    public void SetSprintSpeed(float speed)
    {
        if (_player.PlayerController != null)
        {
            _player.PlayerController.SprintSpeed = speed;
            Log.VerboseInfo($"Sprint speed set to {speed}");
        }
    }

    [MoonSharpVisible(true)]
    public float GetCrouchSpeed()
    {
        if (_player.PlayerController != null)
        {
            return _player.PlayerController.CrouchSpeed;
        }
        return 0f;
    }

    [MoonSharpVisible(true)]
    public void SetCrouchSpeed(float speed)
    {
        if (_player.PlayerController != null)
        {
            _player.PlayerController.CrouchSpeed = speed;
            Log.VerboseInfo($"Crouch speed set to {speed}");
        }
    }

    [MoonSharpVisible(true)]
    public float GetHealth()
    {
        return _player.PlayerHealth.GetHealth();
    }

    [MoonSharpVisible(true)]
    public float GetMaxHealth()
    {
        return _player.PlayerHealth.GetMaxHealth();
    }

    [MoonSharpVisible(true)]
    public float GetHealthPercent()
    {
        return _player.PlayerHealth.GetHealthPercent();
    }

    [MoonSharpVisible(true)]
    public void DamagePlayer(float amount)
    {
        _player.PlayerHealth.Take(amount);
    }

    [MoonSharpVisible(true)]
    public void HealPlayer(float amount)
    {
        _player.PlayerHealth.Heal(amount);
    }

    [MoonSharpVisible(true)]
    public void SetHealth(float amount)
    {
        _player.PlayerHealth.Set(amount);
    }

    [MoonSharpVisible(true)]
    public string GetHealthLevel()
    {
        // Return as string for easier Lua comparison
        float percent = GetHealthPercent();
        if (percent <= 0f) return "Dead";
        if (percent <= 0.25f) return "NearDeath";
        if (percent <= 0.5f) return "Critical";
        if (percent <= 0.75f) return "Injured";
        return "Healthy";
    }

    [MoonSharpVisible(true)]
    public string GetPlayerState()
    {
        return _player.CurrentState.ToString();
    }

    [MoonSharpVisible(true)]
    public bool IsPlayerState(string stateName)
    {
        return _player.CurrentState.ToString() == stateName;
    }

    [MoonSharpVisible(true)]
    public bool IsWalking()
    {
        return _player.CurrentState == PlayerState.Walking;
    }

    [MoonSharpVisible(true)]
    public bool IsSprinting()
    {
        return _player.CurrentState == PlayerState.Sprinting;
    }

    [MoonSharpVisible(true)]
    public bool IsCrouching()
    {
        return _player.CurrentState == PlayerState.Crouching;
    }

    [MoonSharpVisible(true)]
    public bool IsNoclip()
    {
        return _player.CurrentState == PlayerState.Noclip;
    }

    [MoonSharpVisible(true)]
    public bool IsGrounded()
    {
        return _player.IsGrounded();
    }

    [MoonSharpVisible(true)]
    public bool IsMoving()
    {
        return _player.IsMoving();
    }

    [MoonSharpVisible(true)]
    public bool CanMove()
    {
        return _player.CanMove();
    }

    [MoonSharpVisible(true)]
    public Vector3 GetPosition()
    {
        return _player.transform.position;
    }

    [MoonSharpVisible(true)]
    public void SetPosition(float x, float y, float z)
    {
        _player.transform.position = new Vector3(x, y, z);
    }

    [MoonSharpVisible(true)]
    public void Teleport(float x, float y, float z)
    {
        // Disable character controller temporarily for teleport
        _player.CharacterController.enabled = false;
        _player.transform.position = new Vector3(x, y, z);
        _player.CharacterController.enabled = true;
    }

    [MoonSharpVisible(true)]
    public Vector3 GetRotation()
    {
        return _player.transform.eulerAngles;
    }

    [MoonSharpVisible(true)]
    public void SetRotation(float x, float y, float z)
    {
        _player.transform.eulerAngles = new Vector3(x, y, z);
    }

    [MoonSharpVisible(true)]
    public Vector3 GetForward()
    {
        return _player.transform.forward;
    }

    [MoonSharpVisible(true)]
    public Vector3 GetCameraPosition()
    {
        return _player.CameraBrain.transform.position;
    }

    [MoonSharpVisible(true)]
    public Vector3 GetCameraForward()
    {
        return _player.CameraBrain.transform.forward;
    }

    [MoonSharpVisible(true)]
    public void ShakeCamera(float intensity, float duration)
    {
        if (_player.CameraImpulseSource != null)
        {
            _player.CameraImpulseSource.GenerateImpulse(intensity);
        }
    }

    [MoonSharpVisible(true)]
    public void SetMovementEnabled(bool enabled)
    {
        if (_player.PlayerController != null)
        {
            _player.PlayerController.enabled = enabled;
        }
    }

    [MoonSharpVisible(true)]
    public bool IsMovementEnabled()
    {
        return _player.PlayerController != null && _player.PlayerController.enabled;
    }

    [MoonSharpVisible(true)]
    public async UniTask FreezePlayer(float duration)
    {
        SetMovementEnabled(false);
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        SetMovementEnabled(true);
    }

    [MoonSharpVisible(true)]
    public bool IsKeyDown(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, out KeyCode key))
        {
            return Input.GetKey(key);
        }
        return false;
    }

    [MoonSharpVisible(true)]
    public bool IsKeyPressed(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, out KeyCode key))
        {
            return Input.GetKeyDown(key);
        }
        return false;
    }

    [MoonSharpVisible(true)]
    public bool IsKeyReleased(string keyCode)
    {
        if (System.Enum.TryParse<KeyCode>(keyCode, out KeyCode key))
        {
            return Input.GetKeyUp(key);
        }
        return false;
    }

    [MoonSharpVisible(true)]
    public GameObject GetPlayerObject()
    {
        return _player.gameObject;
    }

    [MoonSharpVisible(true)]
    public Transform GetTransform()
    {
        return _player.transform;
    }

    [MoonSharpVisible(true)]
    public void OnHealthChanged(Closure callback)
    {
        if (_player.PlayerHealth != null)
        {
            _player.PlayerHealth.OnHealthChanged += (current, max) =>
            {
                try
                {
                    callback.Call(current, max);
                }
                catch (ScriptRuntimeException)
                {
                }
            };
        }
    }

    [MoonSharpVisible(true)]
    public void OnHealthLevelChanged(Closure callback)
    {
        if (_player.PlayerHealth != null)
        {
            _player.PlayerHealth.OnHealthLevelChanged += (level) =>
            {
                try
                {
                    callback.Call(level.ToString());
                }
                catch (ScriptRuntimeException)
                {
                }
            };
        }
    }

    [MoonSharpVisible(true)]
    public Table RaycastFromCamera(float maxDistance)
    {
        Script script = new Script();
        Table result = new Table(script);

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