using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guisha.Core
{
    // Synchronous, main-thread bus. Dispose subscriptions in OnDisable.
    public static class EventBus
    {
        private sealed class Listener
        {
            public Delegate Callback;
            public bool Active = true;
        }
        private sealed class Token : IDisposable
        {
            private Action release;
            public Token(Action release) { this.release = release; }
            public void Dispose() { var action = release; release = null; action?.Invoke(); }
        }
        private static readonly Dictionary<Type, List<Listener>> Channels = new Dictionary<Type, List<Listener>>();
        public static Action<Exception> ExceptionHandler = Debug.LogException;

        public static IDisposable Subscribe<T>(Action<T> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (!Channels.TryGetValue(typeof(T), out var list))
                Channels.Add(typeof(T), list = new List<Listener>());
            var listener = new Listener { Callback = callback };
            list.Add(listener);
            return new Token(() => { listener.Active = false; list.Remove(listener); });
        }
        public static void Publish<T>(T message)
        {
            if (!Channels.TryGetValue(typeof(T), out var list)) return;
            // Snapshot allows nested publish and subscribe/unsubscribe during delivery.
            var snapshot = list.ToArray();
            foreach (var listener in snapshot)
            {
                if (!listener.Active) continue;
                try { ((Action<T>)listener.Callback)(message); }
                catch (Exception error) { ExceptionHandler?.Invoke(error); }
            }
        }
        public static void Clear()
        {
            foreach (var channel in Channels.Values)
                foreach (var listener in channel) listener.Active = false;
            Channels.Clear();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Clear(); ExceptionHandler = Debug.LogException; }
    }
    public sealed class SubscriptionBag : IDisposable
    {
        private readonly List<IDisposable> items = new List<IDisposable>();
        public void Add(IDisposable item) { if (item != null) items.Add(item); }
        public void Dispose() { foreach (var item in items) item.Dispose(); items.Clear(); }
    }
}
