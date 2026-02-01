using UnityEngine;

public class ObjectLifetime : MonoBehaviour
{
    public float lifetime = 15f;

    void Start()
    {
        if (gameObject != null)
        {
            Destroy(gameObject, lifetime);
        }
    }
}
