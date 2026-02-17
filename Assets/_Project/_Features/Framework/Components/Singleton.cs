using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual bool PersistAcrossScenes => false;

    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (PersistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        OnSingletonAwake();
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        OnSingletonDestroy();
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;

        OnSingletonApplicationQuit();
    }

    protected virtual void OnSingletonAwake() { }
    protected virtual void OnSingletonDestroy() { }
    protected virtual void OnSingletonApplicationQuit() { }
}