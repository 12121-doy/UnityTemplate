using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class PlayerCamera : MonoBehaviour
    {
        public Transform target;
        public LayerMask collisionMask = ~0;
        [SerializeField] private float distance = 3, height = 1.6f, sensitivity = 2, radius = 0.2f;
        private float yaw, pitch = 15, shakeTime, shakeStrength;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private void OnEnable()
        {
            subscriptions.Add(EventBus.Subscribe<CameraShakeRequested>(e => { shakeTime = e.Duration; shakeStrength = e.Strength; }));
        }
        private void OnDisable() { subscriptions.Dispose(); }
        private void LateUpdate()
        {
            if (target == null) return;
            if (GameSession.InputEnabled)
            { yaw += Input.GetAxis("Mouse X") * sensitivity; pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -25, 70); }
            var rotation = Quaternion.Euler(pitch, yaw, 0);
            var pivot = target.position + Vector3.up * height;
            var direction = rotation * Vector3.back;
            float allowed = distance;
            // Ignore the target hierarchy even when all layers are selected.
            foreach (var hit in Physics.SphereCastAll(pivot, radius, direction, distance, collisionMask, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(target)) allowed = Mathf.Min(allowed, Mathf.Max(0.05f, hit.distance - 0.05f));
            var position = pivot + direction * allowed;
            if (shakeTime > 0 && GameSession.SimulationEnabled) { shakeTime -= Time.deltaTime; position += Random.insideUnitSphere * shakeStrength; }
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
