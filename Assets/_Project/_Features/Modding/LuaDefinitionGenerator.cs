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
    private static string VSCODE_DIRECTORY = ".vscode";
    private static string DEFINITIONS_PATH = ".vscode/lua-definitions";
    private static string STATIC_DEFINITIONS_PATH = ".vscode/lua-definitions/Built-in";
    private static string MODS_PATH = "Mods";

    private static readonly HashSet<string> _luaKeywords = new HashSet<string>
    {
        "and", "break", "do", "else", "elseif", "end", "false", "for",
        "function", "goto", "if", "in", "local", "nil", "not", "or",
        "repeat", "return", "then", "true", "until", "while"
    };

    private class UnityTypeDefinition
    {
        public string Name;
        public Dictionary<string, string> Fields = new Dictionary<string, string>();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    public static void GenerateDefinitions()
    {
        string projectRoot = Path.Combine(Application.dataPath, "..");
        string fullDefinitionsPath = Path.Combine(projectRoot, DEFINITIONS_PATH);
        string fullStaticDefinitionsPath = Path.Combine(projectRoot, STATIC_DEFINITIONS_PATH);
        string fullVSCodePath = Path.Combine(projectRoot, VSCODE_DIRECTORY);

        Directory.CreateDirectory(fullDefinitionsPath);
        Directory.CreateDirectory(fullStaticDefinitionsPath);
        SetHiddenAttribute(fullVSCodePath);

        if (Directory.Exists(fullDefinitionsPath))
        {
            foreach (string file in Directory.GetFiles(fullDefinitionsPath, "*.lua"))
            {
                File.Delete(file);
            }
        }

        if (Directory.Exists(fullStaticDefinitionsPath))
        {
            foreach (string file in Directory.GetFiles(fullStaticDefinitionsPath, "*.lua"))
            {
                File.Delete(file);
            }
        }

        var moonSharpTypes = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t =>
                        t.GetCustomAttribute<MoonSharpUserDataAttribute>() != null &&
                        t.GetCustomAttribute<StaticModAPIAttribute>() == null);
                moonSharpTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Exception(ex);
                continue;
            }
        }

        foreach (var type in moonSharpTypes)
        {
            GenerateDefinitionForType(type, fullDefinitionsPath);
        }

        GenerateGlobalDefinitions(fullDefinitionsPath, fullStaticDefinitionsPath);
        GenerateModdingWorkspace();
        GenerateExtensionsJson();

        AssetDatabase.Refresh();
    }

    private static void GenerateDefinitionForType(Type type, string outputPath)
    {
        var sb = new StringBuilder();
        sb.Append(GetGeneratedHeader());

        string luaClassName = RemoveAPISuffix(type.Name);
        sb.AppendLine($"---@class {luaClassName}");
        sb.AppendLine($"{luaClassName} = {{}}");
        sb.AppendLine();

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m =>
            {
                var attribute = m.GetCustomAttribute<MoonSharpVisibleAttribute>();
                return attribute != null && attribute.Visible;
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
        var docAttribute = method.GetCustomAttribute<LuaDocAttribute>();
        var paramAttributes = method.GetCustomAttributes<LuaParamAttribute>()
            .ToDictionary(p => p.Name, p => p.Description);

        if (docAttribute != null)
        {
            sb.AppendLine($"--- {docAttribute.Description}");
        }

        ParameterInfo[] parameters = method.GetParameters();
        foreach (var parameter in parameters)
        {
            string luaType = GetLuaType(parameter.ParameterType);
            string safeName = _luaKeywords.Contains(parameter.Name) ? parameter.Name + "_" : parameter.Name;

            if (paramAttributes.TryGetValue(parameter.Name, out string paramDesc))
            {
                sb.AppendLine($"---@param {safeName} {luaType} {paramDesc}");
            }
            else
            {
                sb.AppendLine($"---@param {safeName} {luaType}");
            }
        }

        LuaReturnAttribute returnAttribute = method.GetCustomAttribute<LuaReturnAttribute>();
        if (returnAttribute != null)
        {
            if (returnAttribute.Type != "void")
            {
                string desc = string.IsNullOrEmpty(returnAttribute.Description) ? "" : $" {returnAttribute.Description}";
                sb.AppendLine($"---@return {returnAttribute.Type}{desc}");
            }
        }
        else if (method.ReturnType != typeof(void))
        {
            string luaReturnType = GetLuaType(method.ReturnType);
            if (luaReturnType != "void")
            {
                sb.AppendLine($"---@return {luaReturnType}");
            }
        }

        string paramList = string.Join(", ", parameters.Select(p => _luaKeywords.Contains(p.Name) ? p.Name + "_" : p.Name));
        sb.AppendLine($"function {className}.{method.Name}({paramList}) end");
        sb.AppendLine();
    }

    private static void GenerateGlobalDefinitions(string outputPath, string staticOutputPath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---@class Global");
        sb.AppendLine("Global = {}");
        sb.AppendLine();

        var globalFunctions = ModSandboxSettings.GetGlobalFunctions();
        foreach (var globalFunction in globalFunctions)
        {
            if (!string.IsNullOrEmpty(globalFunction.Description))
            {
                sb.AppendLine($"--- {globalFunction.Description}");
            }

            foreach (var param in globalFunction.Parameters)
            {
                string desc = string.IsNullOrEmpty(param.description) ? "" : $" {param.description}";
                sb.AppendLine($"---@param {param.name} {param.type}{desc}");
            }

            if (globalFunction.ReturnType != "void" && !string.IsNullOrEmpty(globalFunction.ReturnType))
            {
                sb.AppendLine($"---@return {globalFunction.ReturnType}");
            }

            string paramList = string.Join(", ", globalFunction.Parameters.Select(p => p.name));
            sb.AppendLine($"function {globalFunction.Name}({paramList}) end");
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

        foreach (var staticAPI in ModSandboxSettings.GetStaticAPIs())
        {
            GenerateStaticAPIDefinition(staticAPI, staticOutputPath);
        }
    }

    private static void GenerateStaticAPIDefinition(ModSandboxSettings.StaticAPI api, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"---@class {api.GlobalName}");
        sb.AppendLine($"{api.GlobalName} = {{}}");
        sb.AppendLine();

        var methods = api.Type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => !m.IsSpecialName)
            .GroupBy(m => m.Name)
            .Select(g => g.First());

        foreach (var method in methods)
        {
            ParameterInfo[] parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                string safeName = _luaKeywords.Contains(param.Name) ? param.Name + "_" : param.Name;
                sb.AppendLine($"---@param {safeName} {GetLuaType(param.ParameterType)}");
            }

            if (method.ReturnType != typeof(void))
            {
                sb.AppendLine($"---@return {GetLuaType(method.ReturnType)}");
            }

            string paramList = string.Join(", ", parameters.Select(p => _luaKeywords.Contains(p.Name) ? p.Name + "_" : p.Name));
            sb.AppendLine($"function {api.GlobalName}.{method.Name}({paramList}) end");
            sb.AppendLine();
        }

        var props = api.Type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var prop in props)
        {
            sb.AppendLine($"---@type {GetLuaType(prop.PropertyType)}");
            sb.AppendLine($"{api.GlobalName}.{prop.Name} = nil");
            sb.AppendLine();
        }

        string filePath = Path.Combine(outputPath, $"{api.GlobalName}.lua");
        File.WriteAllText(filePath, sb.ToString());
    }

    private static string GetGeneratedHeader()
    {
        return "--[[\n" +
               $"    THIS FILE WAS AUTOMATICALLY GENERATED!\n" +
               $"    Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
               "    Changes made here will be overwritten on the next script reload. :)\n" +
               "--]]\n\n";
    }

    private static void GenerateModdingWorkspace()
    {
        string path = Path.Combine(Application.dataPath, "..", MODS_PATH, "Modding.code-workspace");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,
@"{
  ""folders"": [
    { ""name"": ""Mods"", ""path"": ""."" },
    { ""name"": ""Definitions"", ""path"": ""../.vscode/lua-definitions"" }
  ],
  ""settings"": {
    ""Lua.runtime.version"": ""Lua 5.2"",
    ""Lua.workspace.library"": [""../.vscode/lua-definitions""],
    ""Lua.workspace.checkThirdParty"": false,
    ""Lua.diagnostics.globals"": [""Global""],
    ""Lua.diagnostics.disable"": [""lowercase-global"", ""missing-return"", ""undefined-doc-name""],
    ""files.associations"": { ""*.lua"": ""lua"" },
    ""files.exclude"": {
      ""**/.git"": true,
      ""**/.DS_Store"": true,
      ""*.code-workspace"": true
    }
  }
}");
    }

    private static void GenerateExtensionsJson()
    {
        string path = Path.Combine(Application.dataPath, "..", VSCODE_DIRECTORY, "extensions.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,
@"{
  ""recommendations"": [
    ""sumneko.lua""
  ]
}");
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
        if (actualType == typeof(DynValue)) return "void";
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
}
#endif