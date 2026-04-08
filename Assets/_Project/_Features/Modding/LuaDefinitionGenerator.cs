#if UNITY_EDITOR
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LuaDefinitionGenerator
{
    private static string vscodeDirectory = ".vscode";
    private static string definitionsPath = ".vscode/lua-definitions";
    private static string modsPath = "Mods";

    [Serializable]
    private class VSCodeSettings
    {
        public string luaRuntimeVersion = "Lua 5.2";
        public string[] luaWorkspaceLibrary = new string[] { "../.vscode/lua-definitions" };
        public bool luaWorkspaceCheckThirdParty = false;
        public string[] luaDiagnosticsGlobals = new string[] { "Global" };
        public string[] luaDiagnosticsDisable = new string[] { "lowercase-global", "missing-return", "undefined-doc-name" };
        public Dictionary<string, string> filesAssociations = new Dictionary<string, string> { { "*.lua", "lua" } };
        public Dictionary<string, bool> filesExclude = new Dictionary<string, bool>
        {
            { "**/.git", true },
            { "**/.DS_Store", true },
            { "*.code-workspace", true }
        };
    }

    private class UnityTypeDefinition
    {
        public string Name;
        public Dictionary<string, string> Fields = new Dictionary<string, string>();
    }

    private static List<UnityTypeDefinition> GetUnityTypeDefinitions()
    {
        return new List<UnityTypeDefinition>
        {
            new UnityTypeDefinition
            {
                Name = "Vector3",
                Fields = new Dictionary<string, string>
                {
                    { "x", "number" },
                    { "y", "number" },
                    { "z", "number" }
                }
            },
            new UnityTypeDefinition
            {
                Name = "Vector2",
                Fields = new Dictionary<string, string>
                {
                    { "x", "number" },
                    { "y", "number" }
                }
            },
            new UnityTypeDefinition
            {
                Name = "Quaternion",
                Fields = new Dictionary<string, string>
                {
                    { "x", "number" },
                    { "y", "number" },
                    { "z", "number" },
                    { "w", "number" }
                }
            },
            new UnityTypeDefinition
            {
                Name = "Color",
                Fields = new Dictionary<string, string>
                {
                    { "r", "number" },
                    { "g", "number" },
                    { "b", "number" },
                    { "a", "number" }
                }
            },
            new UnityTypeDefinition
            {
                Name = "GameObject",
                Fields = new Dictionary<string, string>
                {
                    { "name", "string" },
                    { "transform", "Transform" }
                }
            },
            new UnityTypeDefinition
            {
                Name = "Transform",
                Fields = new Dictionary<string, string>
                {
                    { "position", "Vector3" },
                    { "rotation", "Quaternion" },
                    { "eulerAngles", "Vector3" },
                    { "localPosition", "Vector3" },
                    { "localRotation", "Quaternion" },
                    { "localScale", "Vector3" }
                }
            }
        };
    }

    [UnityEditor.Callbacks.DidReloadScripts] // The performance impact seems to be negligibl
    [MenuItem("Mods/Generate Lua Definitions")]
    public static void GenerateDefinitions()
    {
        string projectRoot = Path.Combine(Application.dataPath, "..");
        string fullDefinitionsPath = Path.Combine(projectRoot, definitionsPath);
        string fullVSCodePath = Path.Combine(projectRoot, vscodeDirectory);

        Directory.CreateDirectory(fullDefinitionsPath);
        SetHiddenAttribute(fullVSCodePath);

        if (Directory.Exists(fullDefinitionsPath))
        {
            foreach (string file in Directory.GetFiles(fullDefinitionsPath, "*.lua"))
            {
                File.Delete(file);
            }
        }

        var moonSharpTypes = GetMoonSharpTypes();

        foreach (var type in moonSharpTypes)
        {
            GenerateDefinitionForType(type, fullDefinitionsPath);
        }

        GenerateGlobalDefinitions(fullDefinitionsPath);
        GenerateModdingWorkspace();
        GenerateExtensionsJson();

        AssetDatabase.Refresh();
    }

    private static List<Type> GetMoonSharpTypes()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var moonSharpTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<MoonSharpUserDataAttribute>() != null);
                moonSharpTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Exception(ex);
                continue;
            }
        }

        return moonSharpTypes;
    }

    private static void SetHiddenAttribute(string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            dirInfo.Attributes |= FileAttributes.Hidden;
        }
    }

    private static string RemoveAPISuffix(string typeName)
    {
        if (typeName.EndsWith("API"))
        {
            return typeName.Substring(0, typeName.Length - 3);
        }
        return typeName;
    }

    private static void GenerateDefinitionForType(Type type, string outputPath)
    {
        var sb = new StringBuilder();
        string luaClassName = RemoveAPISuffix(type.Name);

        sb.AppendLine($"---@class {luaClassName}");
        sb.AppendLine($"{luaClassName} = {{}}");
        sb.AppendLine();

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m =>
            {
                var attr = m.GetCustomAttribute<MoonSharpVisibleAttribute>();
                return attr != null && attr.Visible;
            });

        foreach (var method in methods)
        {
            GenerateMethodDefinition(sb, luaClassName, method);
        }

        string filePath = Path.Combine(outputPath, $"{luaClassName}.lua");
        File.WriteAllText(filePath, sb.ToString());
    }

    private static void GenerateMethodDefinition(StringBuilder sb, string className, MethodInfo method)
    {
        var parameters = method.GetParameters();

        foreach (var param in parameters)
        {
            string luaType = GetLuaType(param.ParameterType);
            sb.AppendLine($"---@param {param.Name} {luaType}");
        }

        if (method.ReturnType != typeof(void))
        {
            string luaReturnType = GetLuaType(method.ReturnType);
            sb.AppendLine($"---@return {luaReturnType}");
        }

        string paramList = string.Join(", ", parameters.Select(p => p.Name));
        sb.AppendLine($"function {className}.{method.Name}({paramList}) end");
        sb.AppendLine();
    }

    private static string GetLuaType(Type type)
    {
        Type actualType = Nullable.GetUnderlyingType(type) ?? type;

        if (actualType == typeof(bool)) return "boolean";
        if (actualType == typeof(string)) return "string";
        if (actualType == typeof(int) || actualType == typeof(float) ||
            actualType == typeof(double) || actualType == typeof(long) ||
            (actualType.IsPrimitive && actualType != typeof(bool)))
        {
            return "number";
        }
        if (actualType == typeof(void)) return "void";
        if (actualType == typeof(Vector3)) return "Vector3";
        if (actualType == typeof(Vector2)) return "Vector2";
        if (actualType == typeof(Quaternion)) return "Quaternion";
        if (actualType == typeof(Color)) return "Color";
        if (actualType == typeof(GameObject)) return "GameObject";
        if (actualType == typeof(Transform)) return "Transform";
        if (actualType == typeof(Component)) return "Component";
        if (actualType.Name == "Table") return "table";
        if (actualType.Name == "Closure") return "function";
        if (actualType.Name == "UniTask") return "void";

        return actualType.Name;
    }

    private static void GenerateGlobalDefinitions(string outputPath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---@class Global");
        sb.AppendLine("Global = {}");
        sb.AppendLine();

        var globalFunctions = ModSandboxSettings.GetGlobalFunctions();
        foreach (var func in globalFunctions)
        {
            if (!string.IsNullOrEmpty(func.Description))
            {
                sb.AppendLine($"--- {func.Description}");
            }

            foreach (var param in func.Parameters)
            {
                sb.AppendLine($"---@param {param.name} {param.type}");
            }

            if (func.ReturnType != "void" && !string.IsNullOrEmpty(func.ReturnType))
            {
                sb.AppendLine($"---@return {func.ReturnType}");
            }

            string paramList = string.Join(", ", func.Parameters.Select(p => p.name));
            sb.AppendLine($"function {func.Name}({paramList}) end");
            sb.AppendLine();
        }

        var unityTypes = GetUnityTypeDefinitions();
        foreach (var unityType in unityTypes)
        {
            sb.AppendLine($"---@class {unityType.Name}");
            foreach (var field in unityType.Fields)
            {
                sb.AppendLine($"---@field {field.Key} {field.Value}");
            }
            sb.AppendLine($"{unityType.Name} = {{}}");
            sb.AppendLine();
        }

        string filePath = Path.Combine(outputPath, "Global.lua");
        File.WriteAllText(filePath, sb.ToString());
    }

    private static void GenerateExtensionsJson()
    {
        string projectRoot = Path.Combine(Application.dataPath, "..");
        string fullVSCodePath = Path.Combine(projectRoot, vscodeDirectory);
        string extensionsPath = Path.Combine(fullVSCodePath, "extensions.json");

        Directory.CreateDirectory(fullVSCodePath);

        if (File.Exists(extensionsPath))
        {
            File.Delete(extensionsPath);
        }

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"recommendations\": [");
        sb.AppendLine("    \"sumneko.lua\"");
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(extensionsPath, sb.ToString());
    }

    private static void GenerateModdingWorkspace()
    {
        string projectRoot = Path.Combine(Application.dataPath, "..");
        string fullModsPath = Path.Combine(projectRoot, modsPath);
        string workspacePath = Path.Combine(fullModsPath, "Modding.code-workspace");

        Directory.CreateDirectory(fullModsPath);

        if (File.Exists(workspacePath))
        {
            File.Delete(workspacePath);
        }

        var settings = new VSCodeSettings();

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"folders\": [");
        sb.AppendLine("    {");
        sb.AppendLine("      \"name\": \"Mods\",");
        sb.AppendLine("      \"path\": \".\"");
        sb.AppendLine("    },");
        sb.AppendLine("    {");
        sb.AppendLine("      \"name\": \"Definitions\",");
        sb.AppendLine("      \"path\": \"../.vscode/lua-definitions\"");
        sb.AppendLine("    }");
        sb.AppendLine("  ],");
        sb.AppendLine("  \"settings\": {");
        sb.AppendLine($"    \"Lua.runtime.version\": \"{settings.luaRuntimeVersion}\",");
        sb.AppendLine($"    \"Lua.workspace.library\": [{string.Join(", ", settings.luaWorkspaceLibrary.Select(s => $"\"{s}\""))}],");
        sb.AppendLine($"    \"Lua.workspace.checkThirdParty\": {settings.luaWorkspaceCheckThirdParty.ToString().ToLower()},");
        sb.AppendLine($"    \"Lua.diagnostics.globals\": [{string.Join(", ", settings.luaDiagnosticsGlobals.Select(g => $"\"{g}\""))}],");
        sb.AppendLine($"    \"Lua.diagnostics.disable\": [{string.Join(", ", settings.luaDiagnosticsDisable.Select(d => $"\"{d}\""))}],");
        sb.AppendLine($"    \"files.associations\": {{{string.Join(", ", settings.filesAssociations.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""))}}},");
        sb.AppendLine("    \"files.exclude\": {");

        var excludeEntries = settings.filesExclude.Select(kv => $"      \"{kv.Key}\": {kv.Value.ToString().ToLower()}");
        sb.AppendLine(string.Join(",\n", excludeEntries));

        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        File.WriteAllText(workspacePath, sb.ToString());
    }
}
#endif