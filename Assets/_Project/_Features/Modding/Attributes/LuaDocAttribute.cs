using System;

[AttributeUsage(AttributeTargets.Method)]
public class LuaDocAttribute : Attribute
{
    public string Description { get; }
    public LuaDocAttribute(string description) => Description = description;
}