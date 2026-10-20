using UnityEngine;
namespace Guisha.Framework
{
    public enum EntityAnimationState { Idle, Walk, Run, Crouch, Attack, Hit, Dead, Interact }
    [CreateAssetMenu(menuName = "Guisha/Animation Settings")]
    public sealed class AnimationSettings : ScriptableObject
    {
        public string speedParameter = "Speed", crouchParameter = "Crouched", groundedParameter = "Grounded", deadParameter = "Dead";
        public string attackTrigger = "Attack", hitTrigger = "Hit", interactTrigger = "Interact";
        [Min(0)] public float crossFade = 0.15f;
        [Min(0.01f)] public float runSpeed = 4;
        public string idleState = "Idle", walkState = "Walk", runState = "Run", crouchState = "Crouch", attackState = "Attack", hitState = "Hit", deadState = "Dead", interactState = "Interact";
        public string StateName(EntityAnimationState state)
        {
            switch (state)
            {
                case EntityAnimationState.Walk: return walkState; case EntityAnimationState.Run: return runState;
                case EntityAnimationState.Crouch: return crouchState; case EntityAnimationState.Attack: return attackState;
                case EntityAnimationState.Hit: return hitState; case EntityAnimationState.Dead: return deadState;
                case EntityAnimationState.Interact: return interactState; default: return idleState;
            }
        }
    }
    public readonly struct EntityAnimationRequested
    {
        public readonly BaseEntity Entity;
        public readonly EntityAnimationState State;
        public EntityAnimationRequested(BaseEntity entity, EntityAnimationState state) { Entity = entity; State = state; }
    }
}
