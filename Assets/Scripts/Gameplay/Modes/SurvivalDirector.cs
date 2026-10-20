using System.Collections.Generic;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.AI;
namespace Guisha.Gameplay
{
    public sealed class SurvivalDirector : MonoBehaviour
    {
        public EntityDefinition ghost;
        public int dungeon;
        [Min(1)] public int maxActive = 8;
        [Min(1)] public float waveInterval = 30;
        private readonly List<BaseEntity> owned = new List<BaseEntity>();
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private float nextWave;
        private int wave, pending, generation;
        private void OnEnable()
        {
            nextWave = Time.time; wave = 0;
            subscriptions.Add(EventBus.Subscribe<GameModeChanged>(_ => Clear()));
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => { if (owned.Remove(e.Entity)) Destroy(e.Entity.gameObject); }));
        }
        private void Update()
        {
            owned.RemoveAll(e => e == null);
            if (dungeon <= 0 || ghost == null || !GameSession.SimulationEnabled || GameModeRuntime.Rules.Mode != GameMode.Endless || Time.time < nextWave) return;
            nextWave = Time.time + waveInterval;
            int count = Mathf.Min(1 + wave / 2, maxActive - owned.Count - pending);
            if (count <= 0) return;
            wave++; EventBus.Publish(new SurvivalWaveStarted(wave));
            for (int i = 0; i < count; i++)
            {
                var point = transform.position + Quaternion.Euler(0, i * 137 + wave * 47, 0) * Vector3.forward * 10;
                if (!NavMesh.SamplePosition(point, out var location, 5, NavMesh.AllAreas)) continue;
                // EntityFactory reports success by event. A failed address is logged by ResourceManager.
                pending++; int ticket = generation;
                EventBus.Publish(new SpawnEntityRequested(ghost, location.position, Quaternion.identity, transform, entity =>
                {
                    if (ticket != generation) { if (entity != null) Destroy(entity.gameObject); return; }
                    pending = Mathf.Max(0, pending - 1); if (entity != null) owned.Add(entity);
                }));
            }
        }
        private void Clear()
        { generation++; foreach (var entity in owned) if (entity != null) Destroy(entity.gameObject); owned.Clear(); pending = 0; wave = 0; nextWave = Time.time; }
        private void OnDisable() { subscriptions.Dispose(); Clear(); }
    }
}
