using UnityEngine;

namespace Events.Base
{
    public class EventTriggerVolume : MonoBehaviour
    {
        [SerializeField] private RoomEvent eventToTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                eventToTrigger?.Trigger();
        }
    }
}
