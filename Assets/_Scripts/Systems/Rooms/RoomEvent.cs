using Cysharp.Threading.Tasks;
using EditorAttributes;
using System.Threading;
using UnityEngine;

public abstract class RoomEvent : MonoBehaviour
{
    [Header("Event Settings")]
    public bool autoStart = false;
    public float startDelay = 0f;

    [Header("Event State"), ReadOnly]
    public bool isStarted = false;
    public bool isFinished = false;

    protected Room parentRoom;
    protected CancellationTokenSource _eventCts;

    private void Awake()
    {
        _eventCts = new CancellationTokenSource();
        parentRoom = GetComponentInParent<Room>();

        if (parentRoom == null)
        {
            Log.VerboseWarning($"[RoomEvent: {GetType().Name}] has no parent Room!");
        }
    }

    private void Start()
    {
        EventLoad();

        if (autoStart)
        {
            if (startDelay > 0f)
                DelayedStart().Forget();
            else
                EventStart();
        }
    }

    private async UniTaskVoid DelayedStart()
    {
        await UniTask.WaitForSeconds(startDelay, cancellationToken: _eventCts.Token);
        EventStart();
    }

    private void Update()
    {
        if (isStarted && !isFinished)
        {
            EventUpdate();
        }
    }

    private void OnDestroy()
    {
        EventUnload();

        if (_eventCts != null && !_eventCts.IsCancellationRequested)
            _eventCts.Cancel();

        _eventCts?.Dispose();
        _eventCts = null;
    }

    // Called when event is loaded (Start)
    public virtual void EventLoad()
    {
    }

    // Called when event is unloaded (OnDestroy)
    public virtual void EventUnload()
    {

    }

    // Start the event
    public virtual void EventStart()
    {
        isStarted = true;
    }

    // Called every frame while event is active
    public virtual void EventUpdate()
    {
    }

    // Mark event as finished
    public virtual void EventFinish()
    {
        isFinished = true;
        isStarted = false;
    }
}