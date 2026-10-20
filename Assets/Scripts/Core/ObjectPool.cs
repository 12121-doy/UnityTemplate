using System;
using System.Collections.Generic;

namespace Guisha.Core
{
    // Main-thread pool; reference identity prevents custom Equals from confusing ownership.
    public sealed class ObjectPool<T> : IDisposable where T : class
    {
        private sealed class Identity : IEqualityComparer<T>
        {
            public bool Equals(T a, T b) => ReferenceEquals(a, b);
            public int GetHashCode(T value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }
        private readonly Stack<T> idle = new Stack<T>();
        private readonly HashSet<T> rented = new HashSet<T>(new Identity());
        private readonly Func<T> create;
        private readonly Action<T> onGet, onReturn, onDestroy;
        private readonly int capacity;
        private bool disposed;
        public int IdleCount => idle.Count;
        public int ActiveCount => rented.Count;
        public ObjectPool(Func<T> create, Action<T> onGet = null, Action<T> onReturn = null,
            Action<T> onDestroy = null, int capacity = 64)
        {
            this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.onGet = onGet; this.onReturn = onReturn; this.onDestroy = onDestroy;
            this.capacity = Math.Max(0, capacity);
        }
        public T Get()
        {
            if (disposed) throw new ObjectDisposedException(nameof(ObjectPool<T>));
            var item = idle.Count > 0 ? idle.Pop() : create();
            if (ReferenceEquals(item, null) || !rented.Add(item)) throw new InvalidOperationException("Pool factory returned an invalid object.");
            onGet?.Invoke(item); return item;
        }
        public void Return(T item)
        {
            if (ReferenceEquals(item, null) || !rented.Remove(item))
                throw new InvalidOperationException("Foreign object or duplicate pool return.");
            onReturn?.Invoke(item);
            if (!disposed && idle.Count < capacity) idle.Push(item); else onDestroy?.Invoke(item);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            while (idle.Count > 0)
            { var item = idle.Pop(); onDestroy?.Invoke(item); }
            // Active objects belong to callers until Return; do not destroy them prematurely.
        }
    }
}
