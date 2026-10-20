using System.Collections.Generic;
using Guisha.Core;
using UnityEngine;
namespace Guisha.Framework
{
    // Read model used by components; writes are centralized by GameManager.
    public static class GameSession
    {
        private static readonly HashSet<object> Locks = new HashSet<object>();
        public static GameState State { get; private set; } = GameState.Playing;
        public static bool InputEnabled => State == GameState.Playing && Locks.Count == 0;
        public static bool SimulationEnabled => State == GameState.Playing;
        public static void SetState(GameState state) { State = state; EventBus.Publish(new GameStateChanged(state)); }
        public static void SetLock(object owner, bool locked)
        { if (owner == null) return; if (locked) Locks.Add(owner); else Locks.Remove(owner); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Locks.Clear(); State = GameState.Playing; }
    }
}
