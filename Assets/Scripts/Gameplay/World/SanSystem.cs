using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(PlayerStatus))]
    public sealed class SanSystem : MonoBehaviour
    {
        [Min(0)] public float environmentDrain = 0.5f, ghostDrain = 6, ghostRadius = 10;
        public LayerMask ghostMask = ~0;
        private PlayerStatus status;
        private float nextTick;
        private void Awake() { status = GetComponent<PlayerStatus>(); }
        private void Update()
        {
            if (!GameSession.SimulationEnabled || Time.time < nextTick) return;
            const float tick = 0.25f; nextTick = Time.time + tick;
            float danger = 0;
            foreach (var hit in Physics.OverlapSphere(transform.position, ghostRadius, ghostMask, QueryTriggerInteraction.Ignore))
            {
                var ghost = hit.GetComponentInParent<GhostEntity>(); if (ghost == null || !ghost.IsAlive) continue;
                danger = Mathf.Max(danger, 1 - Vector3.Distance(transform.position, ghost.transform.position) / Mathf.Max(0.1f, ghostRadius));
            }
            status.ChangeSan(-(environmentDrain + ghostDrain * danger) * tick * GameModeRuntime.Rules.SanDrain);
        }
    }
}
