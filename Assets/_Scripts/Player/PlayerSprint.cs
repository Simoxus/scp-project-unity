using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;


public class PlayerSprint : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>(); 
    }

    private void HandleSprint()
    {
        
    }
}