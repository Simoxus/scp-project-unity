using UnityEngine;

public class ElementSoundFeedback : MonoBehaviour
{
    public void PlaySound()
    {
        FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.PressSound);
    }
}
