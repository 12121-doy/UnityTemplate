using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.AI;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(BaseEntity))]
    public sealed class EntityAnimator : MonoBehaviour
    {
        public AnimationSettings settings;
        public Animator targetAnimator;
        public Animation legacyAnimation;
        public bool logActions;
        private BaseEntity entity;
        private IAnimationDriver driver;
        private Vector3 lastPosition;
        private CharacterController character;
        private NavMeshAgent agent;
        private PlayerController player;
        private AnimatorUpdateMode regularUpdateMode;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        public IAnimationDriver Driver => driver ?? NullAnimationDriver.Instance;
        private void Awake()
        {
            entity = GetComponent<BaseEntity>(); character = GetComponent<CharacterController>(); agent = GetComponent<NavMeshAgent>(); player = GetComponent<PlayerController>();
            Rebind();
        }
        public void Rebind()
        {
            if (targetAnimator == null) targetAnimator = GetComponentInChildren<Animator>();
            if (targetAnimator != null) regularUpdateMode = targetAnimator.updateMode;
            if (legacyAnimation == null) legacyAnimation = GetComponentInChildren<Animation>();
            driver = targetAnimator != null ? new AnimatorDriverFactory(targetAnimator).Create() : new LegacyAnimationDriverFactory(legacyAnimation).Create();
            if (logActions) driver = new AnimationLoggingDecorator(driver, Guisha.Core.Logger.Info);
        }
        private void OnEnable()
        {
            lastPosition = transform.position;
            subscriptions.Add(EventBus.Subscribe<EntityDamaged>(e => { if (e.Entity == entity && entity.IsAlive) Play(EntityAnimationState.Hit); }));
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => { if (e.Entity == entity) Play(EntityAnimationState.Dead); }));
            subscriptions.Add(EventBus.Subscribe<EntityAnimationRequested>(e => { if (e.Entity == entity) Play(e.State); }));
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ =>
            {
                lastPosition = transform.position;
                Driver.Bool(settings != null ? settings.deadParameter : "Dead", !entity.IsAlive);
                if (targetAnimator != null && entity.IsAlive) targetAnimator.updateMode = regularUpdateMode;
            }));
        }
        private void OnDisable() { subscriptions.Dispose(); }
        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            float speed = dt > 0 ? (transform.position - lastPosition).magnitude / dt : 0; lastPosition = transform.position;
            if (!GameSession.SimulationEnabled) return;
            if (character != null) speed = new Vector3(character.velocity.x, 0, character.velocity.z).magnitude;
            else if (agent != null && agent.enabled && agent.isOnNavMesh) speed = agent.velocity.magnitude;
            Driver.Float(settings != null ? settings.speedParameter : "Speed", speed);
            Driver.Bool(settings != null ? settings.crouchParameter : "Crouched", player != null && player.IsCrouched);
            Driver.Bool(settings != null ? settings.groundedParameter : "Grounded", character == null || character.isGrounded);
            Driver.Bool(settings != null ? settings.deadParameter : "Dead", !entity.IsAlive);
        }
        public void Play(EntityAnimationState state)
        {
            string trigger = state == EntityAnimationState.Attack ? (settings != null ? settings.attackTrigger : "Attack")
                : state == EntityAnimationState.Hit ? (settings != null ? settings.hitTrigger : "Hit")
                : state == EntityAnimationState.Interact ? (settings != null ? settings.interactTrigger : "Interact") : null;
            if (trigger != null) Driver.Trigger(trigger);
            if (state == EntityAnimationState.Dead)
            {
                if (targetAnimator != null) targetAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                Driver.Bool(settings != null ? settings.deadParameter : "Dead", true);
            }
            // Mecanim controllers own their locomotion transitions; explicit state playback also supports Legacy clips.
            if (targetAnimator == null || trigger == null && state != EntityAnimationState.Dead)
                Driver.Play(settings != null ? settings.StateName(state) : state.ToString(), settings != null ? settings.crossFade : 0.15f);
        }
    }
}
