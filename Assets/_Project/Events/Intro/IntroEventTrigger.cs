using System;
using TriInspector;
using UnityEngine;

public class IntroEventTrigger : MonoBehaviour
{
    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;

    [Space]
    [SerializeField] private string targetEnterTag = "Player";
    [SerializeField] private string targetExitTag = "Player";

    [Header("Behavior")]
    public bool TriggerOnce = true;
    public bool ModifyDoor = false;
    [SerializeField, ShowIf(nameof(ModifyDoor))] public bool openDoor = true;
    [SerializeField, ShowIf(nameof(ModifyDoor))] private BaseDoorController door;

    private bool _playerInside = false;
    private Collider _collider;

    public bool IsPlayerInside => _playerInside;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetEnterTag) && !_playerInside)
        {
            if (TriggerOnce)
            {
                _collider.enabled = false;
            }

            _playerInside = true;
            OnPlayerEntered?.Invoke();

            if (ModifyDoor)
            {
                if (openDoor)
                {
                    door.OpenDoor();
                }
                else
                {
                    door.CloseDoor();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetExitTag) && _playerInside)
        {
            _playerInside = false;
            OnPlayerExited?.Invoke();
        }
    }

    private void OnDisable()
    {
        _playerInside = false;
    }
}