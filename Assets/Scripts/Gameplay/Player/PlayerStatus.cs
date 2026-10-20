using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class PlayerStatus : MonoBehaviour
    {
        [SerializeField, Min(1)] private float maxSan = 100, maxStamina = 100;
        [SerializeField, Min(0)] private float staminaRecovery = 15;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private PlayerEntity entity;
        private float lastExertion = -10;
        private bool low;
        public float San { get; private set; }
        public float Stamina { get; private set; }
        public bool Injured => entity != null && entity.Health < entity.MaxHealth * 0.4f;
        private void Awake() { entity = GetComponent<PlayerEntity>(); San = maxSan; Stamina = maxStamina; }
        private void OnEnable()
        {
            AsyncHelper.Delay(this, 0, Publish);
            subscriptions.Add(EventBus.Subscribe<SaveCapture>(e =>
            { e.Data.hasPlayer = true; e.Data.position = transform.position; e.Data.rotation = transform.rotation; e.Data.health = entity.Health; e.Data.san = San; e.Data.stamina = Stamina; }));
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(e =>
            {
                var controller = GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;
                transform.SetPositionAndRotation(e.Data.position, e.Data.rotation);
                if (controller != null) controller.enabled = true;
                entity.SetHealth(e.Data.health); San = Mathf.Clamp(e.Data.san, 0, maxSan); Stamina = Mathf.Clamp(e.Data.stamina, 0, maxStamina); Publish();
            }));
        }
        private void Start() { Publish(); }
        private void OnDisable() { subscriptions.Dispose(); }
        private void Update()
        {
            if (!GameSession.SimulationEnabled || Time.time - lastExertion < 0.5f || Stamina >= maxStamina) return;
            Stamina = Mathf.Min(maxStamina, Stamina + staminaRecovery * Time.deltaTime); Publish();
        }
        public bool ConsumeStamina(float amount)
        {
            amount *= GameModeRuntime.Rules.StaminaCost;
            if (Stamina < amount || amount < 0) return false;
            Stamina -= amount; lastExertion = Time.time; Publish(); return true;
        }
        public void ChangeSan(float delta) { float value = Mathf.Clamp(San + delta, 0, maxSan); if (Mathf.Approximately(value, San)) return; San = value; Publish(); }
        private void Publish()
        {
            EventBus.Publish(new PlayerStatsChanged(gameObject, San, maxSan, Stamina, maxStamina));
            bool next = San <= maxSan * 0.25f;
            if (next != low) { low = next; EventBus.Publish(new LowSanChanged(low)); }
        }
    }
}
