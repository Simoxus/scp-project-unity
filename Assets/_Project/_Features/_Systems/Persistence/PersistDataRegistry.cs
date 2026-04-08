using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Facility.Persistence
{
    public class PersistDataRegistry
    {
        private Dictionary<string, PersistDataHandler> _handlers = new Dictionary<string, PersistDataHandler>();

        public void RegisterAllTypes()
        {
            var persistTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IPersistData).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Where(t => t.Namespace == Core.PERSIST_DATA_NAMESPACE);

            foreach (var type in persistTypes)
            {
                try
                {
                    var tempInstance = Activator.CreateInstance(type) as IPersistData;
                    if (tempInstance == null) continue;

                    var fromJsonMethod = type.GetMethod("FromJson", BindingFlags.Public | BindingFlags.Static);
                    if (fromJsonMethod == null)
                    {
                        Log.Warning($"Type {type.Name} does not have a static FromJson method");
                        continue;
                    }

                    _handlers[tempInstance.PersistDataType] = new PersistDataHandler
                    {
                        DataType = tempInstance.PersistDataType,
                        FileName = tempInstance.FileName,
                        TypeInfo = type,
                        Deserialize = json => fromJsonMethod.Invoke(null, new object[] { json }) as IPersistData
                    };

                    Log.VerboseInfo($"Registered persist data type '{tempInstance.PersistDataType}'");
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        public string GetFileName(string dataType)
        {
            return _handlers.TryGetValue(dataType, out var handler) ? handler.FileName : null;
        }

        public IPersistData Deserialize(string dataType, string json)
        {
            if (_handlers.TryGetValue(dataType, out var handler))
            {
                return handler.Deserialize(json);
            }
            return null;
        }

        public IEnumerable<string> GetRegisteredTypes()
        {
            return _handlers.Keys;
        }

        private class PersistDataHandler
        {
            public string DataType;
            public string FileName;
            public Type TypeInfo;
            public Func<string, IPersistData> Deserialize;
        }
    }
}