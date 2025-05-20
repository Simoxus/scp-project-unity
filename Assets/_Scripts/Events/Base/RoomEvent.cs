using UnityEngine;

namespace Events.Base
{
    public abstract class RoomEvent : MonoBehaviour
    {
        public abstract void Trigger();
        public virtual void ResetEvent() { }
    }
}