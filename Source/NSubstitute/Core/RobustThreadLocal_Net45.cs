#if NET45

using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting.Messaging;

namespace NSubstitute.Core
{
    /// <summary>
    /// Delegates to CallContext.LogicalGetData and .LogicalSetData, as this is the Task-aware way to
    /// get the equivalent of thread local storage.
    /// </summary>
    class RobustThreadLocal<T>
    {
        readonly ConcurrentDictionary<string, T> contextMappings = new ConcurrentDictionary<string, T>();
        readonly Func<T> initialValue;
        readonly string key;

        public RobustThreadLocal()
            : this(() => default(T)) { }

        public RobustThreadLocal(Func<T> initialValue)
        {
            this.initialValue = initialValue;
            key = Guid.NewGuid().ToString();

            CallContext.LogicalSetData(key, default(T));
        }

        string GetMappingKey()
        {
            var mappingKey = (string)CallContext.LogicalGetData(key);
            if (mappingKey == null)
            {
                mappingKey = Guid.NewGuid().ToString();
                CallContext.LogicalSetData(key, mappingKey);
            }

            return mappingKey;
        }

        public T Value
        {
            get
            {
                var mappingKey = GetMappingKey();
                return contextMappings.GetOrAdd(mappingKey, _ => initialValue());
            }
            set
            {
                var mappingKey = GetMappingKey();
                contextMappings[mappingKey] = value;
            }
        }
    }
}

#endif