using UnityEngine;
using System;

namespace Guisha.Core
{
    // Explicit bootstrap: Instance never creates hidden managers or resurrects them on quit.
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static T Instance { get; private set; }
        public static bool Exists => Instance != null;
        public static bool TryGetInstance(out T instance) { instance = Instance; return instance != null; }
        public static T RequireInstance()
        {
            if (Instance == null) throw new InvalidOperationException(typeof(T).Name + " has not been bootstrapped.");
            return Instance;
        }
        protected virtual bool PersistAcrossScenes => true;
        protected bool IsPrimaryInstance => Instance == this;
        protected virtual void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = (T)this;
            if (PersistAcrossScenes)
            {
                if (transform.parent != null) transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }
        protected virtual void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
