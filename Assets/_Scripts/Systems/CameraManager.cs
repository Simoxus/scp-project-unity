using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public CinemachineBrain cameraBrain;
    public CinemachineCamera cameraMain;
    public CinemachineImpulseSource impulseSource;

    private void Reset()
    {
        cameraBrain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineBrain>();
        cameraMain = GameObject.Find("CameraMain").GetComponent<CinemachineCamera>();
        impulseSource = GameObject.FindGameObjectWithTag("ImpulseSource").GetComponent<CinemachineImpulseSource>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        if (impulseSource == null)
        {
            Debug.LogError("CameraManager is missing reference to a Cinemachine Impulse Source. Please assign it.", this);
        }
    }

    public void GenerateShake(float intensity)
    {
        if (impulseSource == null) { return; }
        if (GameManager.Instance.cameraShaking == false) { return; }

        Vector3 impulseVelocity = new Vector3(intensity, intensity, intensity);
        impulseSource.GenerateImpulseWithVelocity(impulseVelocity);
    }

    public void GenerateShakeWithVector3(Vector3 impulseVelocity)
    {
        if (impulseSource == null) { return; }
        if (GameManager.Instance.cameraShaking == false) { return; }

        impulseSource.GenerateImpulseWithVelocity(impulseVelocity);
    }
}
