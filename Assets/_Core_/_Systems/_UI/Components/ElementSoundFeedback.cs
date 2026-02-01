using UnityEngine;

public class ElementSoundFeedback : MonoBehaviour
{
    [Space]
    [SerializeField] private BaseSettingsApplier applier;

    public void PlaySound()
    {
        if (applier != null && applier.inBatchMode) return;
        FMODHelper.PlayOneShot(Core.AudioDataAccess.UI.PressSound);
    }
}
