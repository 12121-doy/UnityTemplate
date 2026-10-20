using Guisha.Core;
using UnityEngine;
namespace Guisha.Framework
{
    public abstract class BaseEntity : MonoBehaviour, IDamageable
    {
        [SerializeField] protected EntityDefinition definition;
        [SerializeField, Min(1)] private float fallbackMaxHealth = 100;
        [SerializeField] protected Animator animator;
        private DamagePipeline damagePipeline;
        public abstract EntityType Type { get; }
        public EntityDefinition Definition => definition;
        public float MaxHealth => definition != null ? Mathf.Max(1, definition.maxHealth) : Mathf.Max(1, fallbackMaxHealth);
        public float Health { get; private set; }
        public bool IsAlive => Health > 0;
        public EntityLifeState LifeState => IsAlive ? EntityLifeState.Alive : EntityLifeState.Dead;
        protected virtual void Awake() { Health = MaxHealth; damagePipeline = GetComponent<DamagePipeline>(); if (animator == null) animator = GetComponentInChildren<Animator>(); }
        public void Configure(EntityDefinition config) { definition = config; SetHealth(MaxHealth); }
        public virtual void TakeDamage(DamageInfo damage)
        {
            if (!IsAlive || damage.Amount <= 0 || float.IsNaN(damage.Amount)) return;
            float amount = damage.Amount * (Type == EntityType.Player ? GameModeRuntime.Rules.PlayerDamage : 1);
            if (damagePipeline != null) amount = damagePipeline.Resolve(amount);
            if (amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return;
            damage = new DamageInfo(amount, damage.Source, damage.Point);
            Health = Mathf.Max(0, Health - amount);
            EventBus.Publish(new EntityDamaged(this, damage));
            EventBus.Publish(new HealthChanged(this, Health, MaxHealth));
            if (!IsAlive) EventBus.Publish(new EntityDied(this));
        }
        public void SetHealth(float value)
        { Health = Mathf.Clamp(float.IsNaN(value) ? MaxHealth : value, 0, MaxHealth); EventBus.Publish(new HealthChanged(this, Health, MaxHealth)); }
    }
}
