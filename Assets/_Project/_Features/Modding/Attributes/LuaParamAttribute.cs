using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class LuaParamAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public LuaParamAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}