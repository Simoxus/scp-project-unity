using System;

[AttributeUsage(AttributeTargets.Class)]
public class StaticModAPIAttribute : Attribute
{
    public string GlobalName { get; }
    public StaticModAPIAttribute(string globalName) => GlobalName = globalName;
}