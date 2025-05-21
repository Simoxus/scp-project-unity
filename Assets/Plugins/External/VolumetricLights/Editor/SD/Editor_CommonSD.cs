#if UNITY_EDITOR
namespace VLB
{
    public class Editor_CommonSD : Editor_Common
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            RetrieveSerializedProperties("_");
        }
    }
}
#endif // UNITY_EDITOR

