using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System.Collections.Generic;

[ModAPI("Event", perMod: true)]
[MoonSharpUserData]
public class EventAPI
{
    private struct Subscription
    {
        public int Id;
        public string ModId;
        public Closure Callback;
    }

    private static readonly Dictionary<string, List<Subscription>> _listeners = new();
    private static readonly HashSet<int> _onceSubs = new();
    private static int _nextSubId = 1;

    private readonly string _modId;
    public EventAPI(string modId) => _modId = modId;

    [MoonSharpVisible(true)]
    [LuaDoc("Subscribes to an event. The callback is called every time the event is fired. Returns a subscription ID.")]
    [LuaParam("eventName", "Name of the event to listen to")]
    [LuaParam("callback", "Function called when the event fires. Receives a data table")]
    public int On(string eventName, Closure callback)
    {
        if (!_listeners.ContainsKey(eventName))
        {
            _listeners[eventName] = new List<Subscription>();
        }

        int id = _nextSubId++;
        _listeners[eventName].Add(new Subscription { Id = id, ModId = _modId, Callback = callback });
        return id;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Subscribes to an event for a single firing only. The callback is automatically removed after it runs once. Returns a subscription ID.")]
    [LuaParam("eventName", "Name of the event to listen to")]
    [LuaParam("callback", "Function called when the event fires. Receives a data table")]
    public int Once(string eventName, Closure callback)
    {
        int id = On(eventName, callback);
        _onceSubs.Add(id);
        return id;
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Unsubscribes a single listener by its subscription ID.")]
    [LuaParam("subscriptionId", "ID returned from On or Once")]
    public void Off(int subscriptionId)
    {
        foreach (var list in _listeners.Values)
        {
            list.RemoveAll(s => s.Id == subscriptionId);
        }

        _onceSubs.Remove(subscriptionId);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Unsubscribes all listeners registered by this mod.")]
    public void OffAll()
    {
        foreach (var list in _listeners.Values)
        {
            list.RemoveAll(s => s.ModId == _modId);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Fires an event, calling all subscribed listeners. Any mod can listen to events fired by other mods.")]
    [LuaParam("eventName", "Name of the event to fire")]
    [LuaParam("data", "Optional table of data passed to each listener")]
    public void Fire(string eventName, Table data = null)
    {
        if (!_listeners.TryGetValue(eventName, out var subs) || subs.Count == 0) { return; }

        var toRemove = new List<int>();

        foreach (var sub in new List<Subscription>(subs))
        {
            try
            {
                sub.Callback.Call(data ?? new Table(sub.Callback.OwnerScript));
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage, header: sub.ModId);
            }

            if (_onceSubs.Contains(sub.Id))
            {
                toRemove.Add(sub.Id);
            }
        }

        foreach (int id in toRemove)
        {
            Off(id);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns the number of active listeners on an event.")]
    [LuaParam("eventName", "Name of the event to check")]
    public int ListenerCount(string eventName)
    {
        return _listeners.TryGetValue(eventName, out var subs) ? subs.Count : 0;
    }
}