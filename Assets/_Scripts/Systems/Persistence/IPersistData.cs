namespace Facility.Persistence
{
    public interface IPersistData
    {
        string PersistDataType { get; }
        string FileName { get; }
        bool SavePerSlot { get; }
        string ToJson();
    }

    public interface IPersistable<T> where T : IPersistData
    {
        T SaveToData();
        bool LoadFromData(T data);
    }
}