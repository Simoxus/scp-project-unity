using Cysharp.Threading.Tasks;
using Facility.Generation;
using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FacilityManager : Singleton<FacilityManager>
{
    private List<RoomInstance> _registeredRooms = new List<RoomInstance>();
    private List<RoomLight> _registeredLights = new List<RoomLight>();
    private List<ItemData> _registeredItems = new List<ItemData>();
    private CancellationTokenSource _lifecycleCts;

    [Space]
    [SerializeField] private bool inIntro = false;

    [Header("Fog Settings")]
    [SerializeField] private bool fogEnabled = true;
    [SerializeField] private Color introFogColor = new Color32(255, 255, 255, 255);
    [SerializeField] private float introFogDensity = 0.002f;
    [SerializeField] private Color defaultFogColor = new Color32(0, 0, 0, 255);
    [SerializeField] private float defaultFogDensity = 0.06f;
    [SerializeField] private float defaultFogTransitionDuration = 1.5f;

    [Header("Ambient Settings")]
    [SerializeField] private bool fullbrightEnabled = false;
    [SerializeField] private float defaultAmbientTransitionDuration = 1f;
    [SerializeField] private Color defaultAmbientColor = new Color32(11, 11, 11, 255);
    [SerializeField] private Color fullbrightAmbientColor = new Color32(164, 164, 164, 255);

    // Queue system
    private Queue<System.Func<UniTask>> _fogQueue = new Queue<System.Func<UniTask>>();
    private Queue<System.Func<UniTask>> _ambientQueue = new Queue<System.Func<UniTask>>();
    private bool _processingFogQueue = false;
    private bool _processingAmbientQueue = false;

    // Active tweens
    private Tween _fogColorTween;
    private Tween _fogDensityTween;
    private Tween _ambientColorTween;

    // State tracking
    private Color _currentFogColor;
    private float _currentFogDensity;
    private Color _currentAmbientColor;
    private bool _currentFogEnabled;
    private bool _currentFullbrightEnabled;

    protected override void OnSingletonAwake()
    {
        _lifecycleCts = new CancellationTokenSource();
        InitializeAtmosphere();
        InitializeItems();
    }

    protected override void OnSingletonDestroy()
    {
        StopAllTransitions();
        _lifecycleCts?.Cancel();
        _lifecycleCts?.Dispose();
    }

    private void InitializeAtmosphere()
    {
        _currentFogEnabled = fogEnabled;
        _currentFogColor = inIntro ? introFogColor : defaultFogColor;
        _currentFogDensity = inIntro ? introFogDensity : defaultFogDensity;
        _currentFullbrightEnabled = fullbrightEnabled;
        _currentAmbientColor = fullbrightEnabled ? fullbrightAmbientColor : defaultAmbientColor;

        RenderSettings.fog = _currentFogEnabled;
        RenderSettings.fogColor = _currentFogColor;
        RenderSettings.fogDensity = _currentFogDensity;
        RenderSettings.ambientLight = _currentAmbientColor;
    }

    private async void InitializeItems()
    {
        _registeredItems.Clear();

        var handle = Addressables.LoadAssetsAsync<ItemData>(
            "Item",
            loadedItem =>
            {
                if (loadedItem != null && !string.IsNullOrEmpty(loadedItem.itemID))
                {
                    _registeredItems.Add(loadedItem);
                }
            }
        );

        await handle.Task;

        Log.Info($"FacilityManager loaded {_registeredItems.Count} items from Addressables");
    }

    public void RegisterRoom(RoomInstance room)
    {
        if (room != null && !_registeredRooms.Contains(room))
        {
            _registeredRooms.Add(room);
        }
    }

    public void UnregisterRoom(RoomInstance room)
    {
        if (room != null)
        {
            _registeredRooms.Remove(room);
        }
    }

    public void ClearRooms()
    {
        _registeredRooms.Clear();
    }

    public IReadOnlyList<RoomInstance> GetAllRooms()
    {
        return _registeredRooms.AsReadOnly();
    }

    public RoomInstance FindRoom(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm) || _registeredRooms.Count == 0)
        {
            return null;
        }

        RoomInstance foundRoom = null;

        foreach (var room in _registeredRooms)
        {
            if (room == null || room.RoomData == null)
                continue;

            var roomData = room.RoomData;

            // Match for room ID
            if (!string.IsNullOrEmpty(roomData.RoomID) &&
                roomData.RoomID.Equals(searchTerm, System.StringComparison.OrdinalIgnoreCase))
            {
                return room; // Exact ID match - return immediately
            }

            // Match for room name
            if (!string.IsNullOrEmpty(roomData.RoomName) &&
                roomData.RoomName.Equals(searchTerm, System.StringComparison.OrdinalIgnoreCase))
            {
                foundRoom = room; // Store exact name match
            }

            // Check for partial matches
            if (foundRoom == null)
            {
                if ((!string.IsNullOrEmpty(roomData.RoomID) &&
                     roomData.RoomID.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(roomData.RoomName) &&
                     roomData.RoomName.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    foundRoom = room;
                }
            }
        }

        return foundRoom;
    }

    public List<RoomInstance> FindRoomsByType(RoomLayout roomLayout)
    {
        List<RoomInstance> matchingRooms = new List<RoomInstance>();

        foreach (var room in _registeredRooms)
        {
            if (room != null && room.RoomData != null && room.RoomData.Layout == roomLayout)
            {
                matchingRooms.Add(room);
            }
        }

        return matchingRooms;
    }

    public RoomInstance FindRoomAtGridPosition(Vector2Int gridPosition)
    {
        foreach (var room in _registeredRooms)
        {
            if (room != null && room.GridCoordinate == gridPosition)
            {
                return room;
            }
        }

        return null;
    }

    public void RegisterLight(RoomLight light)
    {
        if (light != null && !_registeredLights.Contains(light))
        {
            _registeredLights.Add(light);
        }
    }

    public void UnregisterLight(RoomLight light)
    {
        if (light != null)
        {
            _registeredLights.Remove(light);
        }
    }

    public IReadOnlyList<ItemData> GetAllItems()
    {
        return _registeredItems.AsReadOnly();
    }

    public int GetRegisteredItemCount() => _registeredItems.Count;

    public ItemData FindItem(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm) || _registeredItems.Count == 0)
            return null;

        // Exact ID match
        var exactMatch = _registeredItems.FirstOrDefault(item =>
            item.itemID.Equals(searchTerm, System.StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null) return exactMatch;

        // Exact name match
        var nameMatch = _registeredItems.FirstOrDefault(item =>
        {
            string itemName = item.GetItemName();
            return !string.IsNullOrEmpty(itemName) &&
                   itemName.Equals(searchTerm, System.StringComparison.OrdinalIgnoreCase);
        });
        if (nameMatch != null) return nameMatch;

        // Partial ID match
        var partialMatch = _registeredItems.FirstOrDefault(item =>
            item.itemID.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0);
        if (partialMatch != null) return partialMatch;

        // Partial name match
        return _registeredItems.FirstOrDefault(item =>
        {
            string itemName = item.GetItemName();
            return !string.IsNullOrEmpty(itemName) &&
                   itemName.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0;
        });
    }

    public List<ItemData> GetItemsByType(ItemData.ItemType itemType)
    {
        return _registeredItems.Where(item => item.itemType == itemType).ToList();
    }

    public void SetFogColor(Color targetColor, float duration = -1)
    {
        if (duration < 0) duration = defaultFogTransitionDuration;
        EnqueueFogTransition(() => TransitionFogColorAsync(targetColor, duration, _lifecycleCts.Token));
    }

    public void SetFogDensity(float targetDensity, float duration = -1)
    {
        if (duration < 0) duration = defaultFogTransitionDuration;
        EnqueueFogTransition(() => TransitionFogDensityAsync(targetDensity, duration, _lifecycleCts.Token));
    }

    public void SetFogEnabled(bool enabled, float fadeDuration = -1)
    {
        if (fadeDuration < 0) fadeDuration = defaultFogTransitionDuration;

        if (enabled)
        {
            EnqueueFogTransition(() => EnableFogAsync(fadeDuration, _lifecycleCts.Token));
        }
        else
        {
            EnqueueFogTransition(() => DisableFogAsync(fadeDuration, _lifecycleCts.Token));
        }
    }

    public void ResetFog(float duration = -1)
    {
        if (duration < 0) duration = defaultFogTransitionDuration;
        SetFogColor(defaultFogColor, duration);
        SetFogDensity(defaultFogDensity, duration);
        SetFogEnabled(fogEnabled, duration);
    }

    public void ClearFogQueue()
    {
        _fogQueue.Clear();
        _processingFogQueue = false;
        StopFogTransitions();
    }

    public void ClearAmbientQueue()
    {
        _ambientQueue.Clear();
        _processingAmbientQueue = false;
        StopAmbientTransitions();
    }

    public void ClearAllQueues()
    {
        ClearFogQueue();
        ClearAmbientQueue();
    }

    private void EnqueueFogTransition(System.Func<UniTask> transition)
    {
        _fogQueue.Enqueue(transition);
        if (!_processingFogQueue)
        {
            ProcessFogQueueAsync(_lifecycleCts.Token).Forget();
        }
    }

    private async UniTaskVoid ProcessFogQueueAsync(CancellationToken cancellationToken)
    {
        _processingFogQueue = true;

        while (_fogQueue.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            var transition = _fogQueue.Dequeue();
            await transition();
        }

        _processingFogQueue = false;
    }

    private async UniTask TransitionFogColorAsync(Color targetColor, float duration, CancellationToken cancellationToken)
    {
        _fogColorTween.Stop();

        Color startColor = _currentFogColor;
        _fogColorTween = Tween.Custom(
            startColor,
            targetColor,
            duration,
            onValueChange: newColor =>
            {
                RenderSettings.fogColor = newColor;
                _currentFogColor = newColor;
            },
            ease: Ease.InOutCubic
        );

        await _fogColorTween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
    }

    private async UniTask TransitionFogDensityAsync(float targetDensity, float duration, CancellationToken cancellationToken)
    {
        _fogDensityTween.Stop();

        float startDensity = _currentFogDensity;
        _fogDensityTween = Tween.Custom(
            startDensity,
            targetDensity,
            duration,
            onValueChange: newDensity =>
            {
                RenderSettings.fogDensity = newDensity;
                _currentFogDensity = newDensity;
            },
            ease: Ease.InOutCubic
        );

        await _fogDensityTween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
    }

    private async UniTask EnableFogAsync(float duration, CancellationToken cancellationToken)
    {
        if (_currentFogEnabled) return;

        RenderSettings.fog = true;
        _currentFogEnabled = true;

        // Fade in from zero density
        await TransitionFogDensityAsync(defaultFogDensity, duration, cancellationToken);
    }

    private async UniTask DisableFogAsync(float duration, CancellationToken cancellationToken)
    {
        if (!_currentFogEnabled) return;

        // Fade out to zero density
        await TransitionFogDensityAsync(0f, duration, cancellationToken);

        RenderSettings.fog = false;
        _currentFogEnabled = false;
    }

    public void SetAmbientColor(Color targetColor, float duration = -1)
    {
        if (duration < 0) duration = defaultAmbientTransitionDuration;
        EnqueueAmbientTransition(() => TransitionAmbientColorAsync(targetColor, duration, _lifecycleCts.Token));
    }

    public void SetFullbright(bool enabled, float duration = -1)
    {
        if (duration < 0) duration = defaultAmbientTransitionDuration;
        _currentFullbrightEnabled = enabled;
        Color targetColor = enabled ? fullbrightAmbientColor : defaultAmbientColor;
        SetAmbientColor(targetColor, duration);
    }

    public void ResetAmbient(float duration = -1)
    {
        SetFullbright(fullbrightEnabled, duration);
    }

    private void EnqueueAmbientTransition(System.Func<UniTask> transition)
    {
        _ambientQueue.Enqueue(transition);
        if (!_processingAmbientQueue)
        {
            ProcessAmbientQueueAsync(_lifecycleCts.Token).Forget();
        }
    }

    private async UniTaskVoid ProcessAmbientQueueAsync(CancellationToken cancellationToken)
    {
        _processingAmbientQueue = true;

        while (_ambientQueue.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            var transition = _ambientQueue.Dequeue();
            await transition();
        }

        _processingAmbientQueue = false;
    }

    private async UniTask TransitionAmbientColorAsync(Color targetColor, float duration, CancellationToken cancellationToken)
    {
        _ambientColorTween.Stop();

        Color startColor = _currentAmbientColor;
        _ambientColorTween = Tween.Custom(
            startColor,
            targetColor,
            duration,
            onValueChange: newColor =>
            {
                RenderSettings.ambientLight = newColor;
                _currentAmbientColor = newColor;
            },
            ease: Ease.OutCubic
        );

        await _ambientColorTween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
    }

    public void FlickerAllLights(float duration = 0.5f, float intensityMin = 0f, float intensityMax = 1f, int flickerCount = 5)
    {
        foreach (var light in _registeredLights)
        {
            if (light != null && light.CanFlicker)
            {
                light.Flicker(duration, intensityMin, intensityMax, flickerCount).Forget();
            }
        }
    }

    public void FlickerLightsInRange(Vector3 position, float range, float duration = 0.5f, float intensityMin = 0f, float intensityMax = 1f, int flickerCount = 5)
    {
        foreach (var light in _registeredLights)
        {
            if (light != null && light.CanFlicker)
            {
                float distance = Vector3.Distance(light.transform.position, position);
                if (distance <= range)
                {
                    light.Flicker(duration, intensityMin, intensityMax, flickerCount).Forget();
                }
            }
        }
    }

    private void StopAllTransitions()
    {
        StopFogTransitions();
        StopAmbientTransitions();
    }

    private void StopFogTransitions()
    {
        _fogColorTween.Stop();
        _fogDensityTween.Stop();
    }

    private void StopAmbientTransitions()
    {
        _ambientColorTween.Stop();
    }

    // Getters for current state
    public float GetCurrentFogDensity() => _currentFogDensity;
    public Color GetCurrentFogColor() => _currentFogColor;
    public bool GetFogEnabled() => _currentFogEnabled;
    public float GetDefaultFogDensity() => defaultFogDensity;
    public bool GetFullbrightEnabled() => _currentFullbrightEnabled;
}