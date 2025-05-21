#if UNITY_EDITOR
namespace VLB
{
    public class Editor_CommonHD : Editor_Common
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            RetrieveSerializedProperties("m_");
        }
    }
}
#endif // UNITY_EDITOR

