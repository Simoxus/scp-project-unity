using System;
[AttributeUsage(AttributeTargets.Method)]
public class LuaReturnAttribute : Attribute
{
    public string Type { get; }
    public string Description { get; }
    public LuaReturnAttribute(string type, string description = "")
    {
        Type = type;
        Description = description;
    }
}