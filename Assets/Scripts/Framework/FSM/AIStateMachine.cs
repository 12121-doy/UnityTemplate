using System;
namespace Guisha.Framework
{
    // The pure FSM lives here; Gameplay/GhostBrain binds it to Unity components.
    public sealed class AIStateMachine
    {
        public AIBaseState Current { get; private set; }
        private bool changing;
        public void Change(AIBaseState next)
        {
            if (ReferenceEquals(Current, next)) return;
            if (changing) throw new InvalidOperationException("Do not change state inside Enter/Exit.");
            changing = true;
            try { Current?.Exit(); Current = next; Current?.Enter(); }
            finally { changing = false; }
        }
        public void Tick(float deltaTime) { Current?.Tick(deltaTime); }
        public void Stop() { Change(null); }
    }
}
