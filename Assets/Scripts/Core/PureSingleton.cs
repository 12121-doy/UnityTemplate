using System;

namespace Guisha.Core
{
    public abstract class PureSingleton<T> where T : class, new()
    {
        private static readonly Lazy<T> Value = new Lazy<T>(() => new T());
        public static T Instance => Value.Value;
    }
}
