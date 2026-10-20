using System;
using Guisha.Core.Patterns;
using UnityEngine;
namespace Guisha.Framework
{
    public sealed class SetWorldFlagCommand : ICommand
    {
        private readonly string id;
        private bool previous;
        public SetWorldFlagCommand(string id) { if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Flag ID is empty."); this.id = id; }
        public bool CanExecute => true;
        public void Execute() { previous = WorldState.Has(id); WorldState.Set(id); }
        public void Undo() { if (previous) WorldState.Set(id); else WorldState.Remove(id); }
    }
    public sealed class MoveTransformCommand : ICommand
    {
        private readonly Transform target;
        private readonly Vector3 destination;
        private Vector3 previous;
        public MoveTransformCommand(Transform target, Vector3 destination) { this.target = target; this.destination = destination; }
        public bool CanExecute => target != null;
        public void Execute() { if (!CanExecute) throw new InvalidOperationException("Target was destroyed."); previous = target.position; target.position = destination; }
        public void Undo() { if (target == null) throw new InvalidOperationException("Target was destroyed."); target.position = previous; }
    }
    public sealed class ScriptablePrototype<T> : IPrototype<T> where T : ScriptableObject
    {
        private readonly T template;
        public ScriptablePrototype(T template) { if (template == null) throw new ArgumentNullException(nameof(template)); this.template = template; }
        // Caller owns the runtime copy and must Destroy it when done.
        public T Clone() => UnityEngine.Object.Instantiate(template);
    }
}
