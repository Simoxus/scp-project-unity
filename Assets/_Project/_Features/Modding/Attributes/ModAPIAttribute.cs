using System;

[AttributeUsage(AttributeTargets.Class)]
public class ModAPIAttribute : Attribute
{
    public string Name { get; }
    public bool PerMod { get; }

    public ModAPIAttribute(string name, bool perMod = false)
    {
        Name = name;
        PerMod = perMod;
    }
}