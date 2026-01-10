using System;

[Serializable]
public class ModInfo
{
    public string id;
    public string name;
    public string version;
    public string author;
    public string description;
    public string entryPoint = "main.lua";
    public int loadOrder = 0;
    public string[] dependencies = new string[0];

    [NonSerialized]
    public string folderPath;
}