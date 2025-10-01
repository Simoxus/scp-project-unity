using UnityEngine;
using FMODUnity;

public class ElementSoundFeedback : MonoBehaviour
{
    [Tooltip("The AudioSource component used to play the sound.")]
    public EventReference uiPressEvent;

    public void PlaySound()
    {
        FMODHelper.PlayOneShot(uiPressEvent);
    }
}
