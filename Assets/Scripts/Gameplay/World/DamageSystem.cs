using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    // Scene-owned feedback, separate from entity health and hit detection.
    public sealed class DamageSystem : MonoBehaviour
    {
        public GameObject hitEffectPrefab;
        public AudioClip hitClip;
        private ObjectPool<GameObject> pool;
        private readonly System.Collections.Generic.HashSet<GameObject> active = new System.Collections.Generic.HashSet<GameObject>();
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private void Awake()
        {
            if (hitEffectPrefab != null)
                pool = new ObjectPool<GameObject>(() => Instantiate(hitEffectPrefab, transform), x => x.SetActive(true), x => x.SetActive(false), x => Destroy(x), 16);
        }
        private void OnEnable()
        {
            subscriptions.Add(EventBus.Subscribe<EntityDamaged>(e =>
            {
                EventBus.Publish(new PlaySoundRequested(hitClip, e.Damage.Point));
                if (e.Entity.Type == EntityType.Player) EventBus.Publish(new CameraShakeRequested(0.08f, 0.25f));
                if (pool == null) return;
                var effect = pool.Get(); active.Add(effect); effect.transform.position = e.Damage.Point;
                var particles = effect.GetComponentInChildren<ParticleSystem>(); if (particles != null) particles.Play(true);
                AsyncHelper.Delay(this, 2, () => { if (active.Remove(effect)) pool.Return(effect); });
            }));
        }
        private void OnDisable()
        {
            subscriptions.Dispose(); StopAllCoroutines();
            if (pool == null) return;
            foreach (var effect in active) pool.Return(effect); active.Clear();
        }
        private void OnDestroy()
        { if (pool == null) return; foreach (var effect in active) pool.Return(effect); active.Clear(); pool.Dispose(); }
    }
}
