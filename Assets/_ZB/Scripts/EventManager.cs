using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace ZB
{
    [AutoStaticsCleanup]
    public static partial class EventManager
    {
        private static readonly Dictionary<string, Action> events = new();
        private static readonly Dictionary<(string Name, Type ArgumentType), Delegate> typedEvents = new();

        public static void AddListener(string name, Action callback)
        {
            events.TryGetValue(name, out var listeners);
            events[name] = listeners + callback;
        }

        public static void RemoveListener(string name, Action callback)
        {
            if (events.TryGetValue(name, out var listeners))
                events[name] = listeners - callback;
        }

        public static void Trigger(string name)
        {
            if (events.TryGetValue(name, out var listeners))
                listeners?.Invoke();
        }

        public static void Trigger<T>(string name, T argument)
        {
            Trigger(name);
            
            if (typedEvents.TryGetValue((name, typeof(T)), out var listeners))
                ((Action<T>)listeners)?.Invoke(argument);
        }
        
        public static void AddListener<T>(string name, Action<T> callback)
        {
            var key = (name, typeof(T));

            typedEvents.TryGetValue(key, out var listeners);
            typedEvents[key] = (Action<T>)listeners + callback;
        }

        public static void RemoveListener<T>(string name, Action<T> callback)
        {
            var key = (name, typeof(T));

            if (typedEvents.TryGetValue(key, out var listeners))
                typedEvents[key] = (Action<T>)listeners - callback;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            events.Clear();
            typedEvents.Clear();
        }
    }
}