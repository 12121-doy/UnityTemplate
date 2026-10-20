using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class AIPerception : MonoBehaviour
    {
        [Min(0)] public float sightRange = 12, hearingRange = 15;
        [Range(0, 360)] public float sightAngle = 100;
        public LayerMask targetMask = ~0, obstructionMask = ~0;
        public Transform eye;
        public Transform VisibleTarget { get; private set; }
        public Vector3 LastKnownPosition { get; private set; }
        public float LastSenseTime { get; private set; } = -100;
        public bool HasRecentSense(float seconds) => Time.time - LastSenseTime <= seconds;
        private float nextScan;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private void OnEnable()
        {
            subscriptions.Add(EventBus.Subscribe<NoiseEmitted>(e =>
            {
                if (!GameSession.SimulationEnabled || e.Source == null || e.Source.GetComponent<PlayerEntity>() == null) return;
                if (MathUtility.InRange(transform.position, e.Position, Mathf.Min(hearingRange, e.Radius)))
                { Remember(e.Position); EventBus.Publish(new PerceptionChanged(gameObject, e.Source.transform, e.Position, false)); }
            }));
        }
        private void OnDisable() { subscriptions.Dispose(); VisibleTarget = null; }
        private void Remember(Vector3 position) { LastKnownPosition = position; LastSenseTime = Time.time; }
        private void Update()
        {
            if (!GameSession.SimulationEnabled || Time.time < nextScan) return;
            nextScan = Time.time + 0.15f; var previous = VisibleTarget; VisibleTarget = null;
            var origin = eye != null ? eye.position : transform.position + Vector3.up * 1.5f;
            float nearest = float.MaxValue;
            foreach (var hit in Physics.OverlapSphere(origin, sightRange, targetMask, QueryTriggerInteraction.Ignore))
            {
                var player = hit.GetComponentInParent<PlayerEntity>();
                if (player == null || !player.IsAlive || !player.gameObject.activeInHierarchy) continue;
                var point = player.transform.position + Vector3.up;
                if (!MathUtility.InCone(origin, transform.forward, point, sightRange, sightAngle)) continue;
                var direction = point - origin;
                bool blocked = false;
                foreach (var obstacle in Physics.RaycastAll(origin, direction.normalized, direction.magnitude, obstructionMask, QueryTriggerInteraction.Ignore))
                    if (!obstacle.transform.IsChildOf(transform) && !obstacle.transform.IsChildOf(player.transform)) { blocked = true; break; }
                if (blocked || direction.sqrMagnitude >= nearest) continue;
                nearest = direction.sqrMagnitude; VisibleTarget = player.transform;
            }
            if (VisibleTarget != null) Remember(VisibleTarget.position);
            if (previous != VisibleTarget) EventBus.Publish(new PerceptionChanged(gameObject, VisibleTarget, LastKnownPosition, true));
        }
    }
}
