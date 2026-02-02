using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "_AudioDataAccess", menuName = "Custom/FMOD/Audio Data Access", order = 0)]
public class AudioDataAccess : ScriptableObject
{
    private static AudioDataAccess _instance;
    private static AsyncOperationHandle<AudioDataAccess> _loadHandle;

    public static AudioDataAccess GetInstanceSync()
    {
        if (_instance != null)
            return _instance;

        _loadHandle = Addressables.LoadAssetAsync<AudioDataAccess>("AudioDataAccess");
        _instance = _loadHandle.WaitForCompletion();

        if (_instance == null)
        {
            Debug.LogError("Failed to load AudioDataAccess from Addressables!");
        }

        return _instance;
    }

    public static AudioDataAccess Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GetInstanceSync();
            }
            return _instance;
        }
    }

    public static void Release()
    {
        if (_loadHandle.IsValid())
        {
            Addressables.Release(_loadHandle);
        }
        _instance = null;
    }

    [Header("Audio Data")]
    public AudioData_Alarms Alarms;
    public AudioData_Music Ambient;
    public AudioData_Characters Characters;
    public AudioData_Collision Collision;
    public AudioData_Doors Doors;
    public AudioData_Items Items;
    public AudioData_Music Music;
    public AudioData_Player Player;
    public AudioData_Props Props;
    public AudioData_SCPs SCPs;
    public AudioData_Special Special;
    public AudioData_UI UI;
}