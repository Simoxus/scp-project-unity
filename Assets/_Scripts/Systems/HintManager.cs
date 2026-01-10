[System.Serializable]
public struct LocalizationString
{
    public string Table;
    public string Key;

    public LocalizationString(string table, string key)
    {
        Table = table;
        Key = key;
    }

    public string GetLocalized(params object[] args)
    {
        return LocalizationHelper.GetString(Table, Key, args);
    }

    public bool IsEmpty => string.IsNullOrEmpty(Table) || string.IsNullOrEmpty(Key);
}

public class HintManager : Singleton<HintManager>
{

}