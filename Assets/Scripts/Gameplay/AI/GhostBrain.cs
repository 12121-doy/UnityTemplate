using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.AI;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(GhostEntity), typeof(AIPerception), typeof(NavMeshAgent))]
    public sealed class GhostBrain : MonoBehaviour
    {
        public Transform[] patrolPoints = System.Array.Empty<Transform>();
        [Min(0)] public float attackRange = 1.6f, attackCooldown = 1.2f, lostDelay = 3, investigationTime = 3;
        private NavMeshAgent agent;
        private AIPerception perception;
        private GhostEntity entity;
        private AIStateMachine machine;
        private State patrol, investigate, chase, attack, returning;
        private Vector3 home;
        private float stateTime, nextAttack;
        private int waypoint;
        public string CurrentState => machine?.Current?.GetType().Name;
        private bool CanMove => agent.enabled && agent.isOnNavMesh;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>(); perception = GetComponent<AIPerception>(); entity = GetComponent<GhostEntity>();
            home = transform.position; machine = new AIStateMachine();
            patrol = new Patrol(this); investigate = new Investigate(this); chase = new Chase(this); attack = new Attack(this); returning = new Return(this);
        }
        private void OnEnable() { machine?.Change(patrol); }
        private void OnDisable() { machine?.Stop(); }
        private void Update()
        {
            if (!CanMove) return;
            bool simulate = GameSession.SimulationEnabled && entity.IsAlive && GameModeRuntime.Rules.HostileGhosts;
            agent.isStopped = !simulate;
            if (!simulate) return;
            agent.speed = (entity.Definition != null ? entity.Definition.moveSpeed : 3) * GameModeRuntime.EnemySpeed;
            stateTime += Time.deltaTime; machine.Tick(Time.deltaTime);
        }
        private void Go(Vector3 point)
        {
            if (!CanMove) return;
            if (NavMesh.SamplePosition(point, out var sample, 3, agent.areaMask)) agent.SetDestination(sample.position);
        }
        private bool Arrived => CanMove && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f;
        private bool See => perception.VisibleTarget != null;
        private bool InAttackRange => See && MathUtility.InRange(transform.position, perception.VisibleTarget.position, attackRange);
        private abstract class State : AIBaseState
        {
            protected readonly GhostBrain b;
            protected State(GhostBrain brain) { b = brain; }
            public override void Enter() { b.stateTime = 0; }
        }
        private sealed class Patrol : State
        {
            public Patrol(GhostBrain b) : base(b) { }
            public override void Enter() { base.Enter(); MoveNext(); }
            private void MoveNext()
            {
                if (b.patrolPoints.Length == 0) { b.Go(b.home); return; }
                var point = b.patrolPoints[b.waypoint++ % b.patrolPoints.Length];
                if (point != null) b.Go(point.position);
            }
            public override void Tick(float dt)
            {
                if (b.See) { b.machine.Change(b.chase); return; }
                if (b.perception.HasRecentSense(1)) { b.machine.Change(b.investigate); return; }
                if (b.Arrived && b.stateTime > 1) { b.stateTime = 0; MoveNext(); }
            }
        }
        private sealed class Investigate : State
        {
            public Investigate(GhostBrain b) : base(b) { }
            public override void Enter() { base.Enter(); b.Go(b.perception.LastKnownPosition); }
            public override void Tick(float dt)
            {
                if (b.See) { b.machine.Change(b.chase); return; }
                if (b.perception.HasRecentSense(0.3f)) { b.Go(b.perception.LastKnownPosition); b.stateTime = 0; }
                if (b.stateTime > b.investigationTime) b.machine.Change(b.returning);
            }
        }
        private sealed class Chase : State
        {
            public Chase(GhostBrain b) : base(b) { }
            public override void Tick(float dt)
            {
                if (b.InAttackRange) { b.machine.Change(b.attack); return; }
                if (!b.perception.HasRecentSense(b.lostDelay)) { b.machine.Change(b.returning); return; }
                b.Go(b.perception.LastKnownPosition);
            }
        }
        private sealed class Attack : State
        {
            public Attack(GhostBrain b) : base(b) { }
            public override void Enter() { base.Enter(); if (b.CanMove) b.agent.ResetPath(); }
            public override void Tick(float dt)
            {
                if (!b.InAttackRange) { b.machine.Change(b.chase); return; }
                var target = b.perception.VisibleTarget;
                var direction = MathUtility.Planar(target.position - b.transform.position);
                if (direction.sqrMagnitude > 0.001f) b.transform.rotation = Quaternion.LookRotation(direction);
                if (Time.time < b.nextAttack) return;
                b.nextAttack = Time.time + Mathf.Max(0.1f, b.attackCooldown);
                EventBus.Publish(new EntityAnimationRequested(b.entity, EntityAnimationState.Attack));
                var victim = target.GetComponent<IDamageable>();
                victim?.TakeDamage(new DamageInfo((b.entity.Definition != null ? (b.entity.Definition.attackDamage) : 10) * GameModeRuntime.EnemyDamage, b.gameObject, target.position));
            }
        }
        private sealed class Return : State
        {
            public Return(GhostBrain b) : base(b) { }
            public override void Enter() { base.Enter(); b.Go(b.home); }
            public override void Tick(float dt)
            { if (b.See) b.machine.Change(b.chase); else if (b.Arrived || b.stateTime > 10) b.machine.Change(b.patrol); }
        }
    }
}
