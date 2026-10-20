using System;
using System.Collections.Generic;
using Guisha.Core;
using UnityEngine;
namespace Guisha.Framework
{
    public readonly struct WorldStateChanged { }
    public static class WorldState
    {
        private static readonly HashSet<string> Flags = new HashSet<string>();
        public static bool Has(string id) => !string.IsNullOrEmpty(id) && Flags.Contains(id);
        public static void Set(string id)
        { if (!string.IsNullOrEmpty(id) && Flags.Add(id)) EventBus.Publish(new WorldStateChanged()); }
        public static bool Remove(string id)
        { bool removed = !string.IsNullOrEmpty(id) && Flags.Remove(id); if (removed) EventBus.Publish(new WorldStateChanged()); return removed; }
        public static string[] Snapshot() { var result = new string[Flags.Count]; Flags.CopyTo(result); return result; }
        public static void Restore(string[] values)
        { Flags.Clear(); foreach (var value in values ?? Array.Empty<string>()) if (!string.IsNullOrEmpty(value)) Flags.Add(value); EventBus.Publish(new WorldStateChanged()); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { Flags.Clear(); }
    }
}
