using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using System.Collections.Generic;

public class LuaMod
{
    public ModInfo Info { get; private set; }
    public Script Script { get; private set; }
    public bool IsEnabled { get; private set; }

    private DynValue _initFunction;
    private DynValue _updateFunction;
    private DynValue _fixedUpdateFunction;
    private DynValue _lateUpdateFunction;
    private DynValue _unloadFunction;

    private Dictionary<string, DynValue> _eventHandlers = new Dictionary<string, DynValue>();

    public LuaMod(ModInfo info)
    {
        Info = info;
        Script = new Script(CoreModules.Preset_SoftSandbox);
        IsEnabled = true;

        Script.Options.DebugPrint = s => Log.Info(s);
    }

    public void RegisterAPI(string name, object api)
    {
        Script.Globals[name] = api;
    }

    public async UniTask Load(string scriptContent)
    {
        try
        {
            Script.DoString(scriptContent);
            CacheFunctions();

            await UniTask.Yield();
        }
        catch (SyntaxErrorException ex)
        {
            Log.Exception(ex, message: $"SYNTAX: {ex.DecoratedMessage}");
            throw;
        }
        catch (ScriptRuntimeException ex)
        {
            Log.Exception(ex, message: $"RUNTIME: {ex.DecoratedMessage}");
            throw;
        }
    }

    private void CacheFunctions()
    {
        _initFunction = Script.Globals.Get("OnInit");
        _updateFunction = Script.Globals.Get("OnUpdate");
        _fixedUpdateFunction = Script.Globals.Get("OnFixedUpdate");
        _lateUpdateFunction = Script.Globals.Get("OnLateUpdate");
        _unloadFunction = Script.Globals.Get("OnUnload");
    }

    public async UniTask Initialize()
    {
        if (_initFunction != null && _initFunction.Type == DataType.Function)
        {
            try
            {
                Script.Call(_initFunction);
                await UniTask.Yield();
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage);
            }
        }
    }

    public async UniTask Unload()
    {
        if (_unloadFunction != null && _unloadFunction.Type == DataType.Function)
        {
            try
            {
                Script.Call(_unloadFunction);
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage);
            }
        }

        IsEnabled = false;
        _eventHandlers.Clear();
        await UniTask.Yield();
    }

    public void Update(float deltaTime)
    {
        if (_updateFunction != null && _updateFunction.Type == DataType.Function)
        {
            Script.Call(_updateFunction, deltaTime);
        }
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        if (_fixedUpdateFunction != null && _fixedUpdateFunction.Type == DataType.Function)
        {
            Script.Call(_fixedUpdateFunction, fixedDeltaTime);
        }
    }

    public void LateUpdate(float deltaTime)
    {
        if (_lateUpdateFunction != null && _lateUpdateFunction.Type == DataType.Function)
        {
            Script.Call(_lateUpdateFunction, deltaTime);
        }
    }

    public void RegisterEventHandler(string eventName, DynValue handler)
    {
        if (handler.Type != DataType.Function)
        {
            Log.Warning($"Tried to register non-function as event handler for '{eventName}'");
            return;
        }

        _eventHandlers[eventName] = handler;
    }

    public void CallEventHandler(string eventName, params object[] args)
    {
        if (_eventHandlers.TryGetValue(eventName, out DynValue handler))
        {
            try
            {
                Script.Call(handler, args);
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage);
            }
        }
    }

    public DynValue CallFunction(string functionName, params object[] args)
    {
        DynValue func = Script.Globals.Get(functionName);

        if (func.Type != DataType.Function)
        {
            Log.Warning($"Function '{functionName}' not found or is not a function");
            return DynValue.Nil;
        }

        try
        {
            return Script.Call(func, args);
        }
        catch (ScriptRuntimeException ex)
        {
            Log.Exception(ex, message: ex.DecoratedMessage);
            return DynValue.Nil;
        }
    }

    public T GetGlobal<T>(string name)
    {
        DynValue value = Script.Globals.Get(name);

        if (value.Type == DataType.Nil)
            return default(T);

        return (T)value.ToObject();
    }

    public void SetGlobal(string name, object value)
    {
        Script.Globals[name] = value;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}