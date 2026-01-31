using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class IntroSectionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject lights;
    [SerializeField] private IntroSectionTriggerChild[] childTriggers;
    [SerializeField] private float checkInterval = 15f;

    private Dictionary<int, int> _triggerColliderCount = new Dictionary<int, int>();
    private bool _isCheckingActive = false;

    private Collider _mainCollider;
    private Collider[] _childColliders;
    private Collider[] _overlapBuffer = new Collider[10];
    private List<int> _triggersToRemove = new List<int>();
    private int _mainID;
    private bool _hasActiveTriggers = false;

    private void Awake()
    {
        _mainCollider = GetComponent<Collider>();
        _mainID = gameObject.GetInstanceID();

        if (childTriggers != null && childTriggers.Length > 0)
        {
            _childColliders = new Collider[childTriggers.Length];
            for (int i = 0; i < childTriggers.Length; i++)
            {
                if (childTriggers[i] != null)
                {
                    childTriggers[i].SetParent(this);
                    _childColliders[i] = childTriggers[i].GetComponent<Collider>();
                }
            }
        }

        // Start with lights off
        if (lights != null)
        {
            lights.SetActive(false);
        }
    }

    private void Start()
    {
        StartPeriodicCheck().Forget();
    }

    private void OnDestroy()
    {
        _isCheckingActive = false;
    }

    private async UniTaskVoid StartPeriodicCheck()
    {
        _isCheckingActive = true;

        while (_isCheckingActive && this != null)
        {
            await UniTask.WaitForSeconds(checkInterval);
            if (!_isCheckingActive || this == null) break;

            if (_hasActiveTriggers)
            {
                CheckPlayerPresence();
            }
        }
    }

    private void CheckPlayerPresence()
    {
        if (Core.Player == null || Core.Player.CharacterController == null)
            return;

        _triggersToRemove.Clear();

        if (_mainCollider != null && _triggerColliderCount.ContainsKey(_mainID))
        {
            if (!IsPlayerInCollider(_mainCollider))
            {
                _triggersToRemove.Add(_mainID);
            }
        }

        if (_childColliders != null)
        {
            for (int i = 0; i < _childColliders.Length; i++)
            {
                if (_childColliders[i] == null || childTriggers[i] == null) continue;

                int childID = childTriggers[i].gameObject.GetInstanceID();
                if (_triggerColliderCount.ContainsKey(childID))
                {
                    if (!IsPlayerInCollider(_childColliders[i]))
                    {
                        _triggersToRemove.Add(childID);
                    }
                }
            }
        }

        for (int i = 0; i < _triggersToRemove.Count; i++)
        {
            _triggerColliderCount.Remove(_triggersToRemove[i]);
        }

        _hasActiveTriggers = _triggerColliderCount.Count > 0;
        if (!_hasActiveTriggers && lights != null && lights.activeSelf)
        {
            lights.SetActive(false);
        }
    }

    private bool IsPlayerInCollider(Collider collider)
    {
        if (collider == null || Core.Player == null)
            return false;

        Vector3 playerPosition = Core.Player.transform.position;
        int hitCount = Physics.OverlapSphereNonAlloc(playerPosition, 0.1f, _overlapBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            if (_overlapBuffer[i] == collider)
            {
                return true;
            }
        }

        return false;
    }

    public void OnPlayerEnterCollider(int triggerID)
    {
        if (!_triggerColliderCount.ContainsKey(triggerID))
        {
            _triggerColliderCount[triggerID] = 0;
        }

        _triggerColliderCount[triggerID]++;
        _hasActiveTriggers = true;

        if (GetTotalColliderCount() == 1 && lights != null)
        {
            lights.SetActive(true);
        }
    }

    public void OnPlayerExitCollider(int triggerID)
    {
        if (_triggerColliderCount.ContainsKey(triggerID))
        {
            _triggerColliderCount[triggerID]--;

            if (_triggerColliderCount[triggerID] <= 0)
            {
                _triggerColliderCount.Remove(triggerID);
            }
        }

        _hasActiveTriggers = _triggerColliderCount.Count > 0;

        if (!_hasActiveTriggers && lights != null)
        {
            lights.SetActive(false);
        }
    }

    public void SetLights(bool enabled)
    {
        if (lights != null)
        {
            lights.SetActive(enabled);
        }
    }

    public void ForceResetTriggers()
    {
        _triggerColliderCount.Clear();
        _hasActiveTriggers = false;

        if (lights != null)
        {
            lights.SetActive(false);
        }
    }

    private int GetTotalColliderCount()
    {
        int total = 0;
        foreach (var count in _triggerColliderCount.Values)
        {
            total += count;
        }
        return total;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterCollider(_mainID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExitCollider(_mainID);
        }
    }
}