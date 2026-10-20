namespace Guisha.Framework
{
    public abstract class AIBaseState
    {
        public virtual void Enter() { }
        public abstract void Tick(float deltaTime);
        public virtual void Exit() { }
    }
}
