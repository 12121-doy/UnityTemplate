using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.AI; // 引入 UnityEngine.AI 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 把感知和 NavMeshAgent 绑定到五态 FSM。
    /// </summary>
    [RequireComponent(typeof(GhostEntity), typeof(AIPerception), typeof(NavMeshAgent))] // 声明本组件必须同时具备的依赖组件。
    public sealed class GhostBrain : MonoBehaviour // 把感知和 NavMeshAgent 绑定到五态 FSM。
    { // 开始类型。
        /// <summary>
        /// 保存巡逻点数组。
        /// </summary>
        public Transform[] patrolPoints = System.Array.Empty<Transform>(); // 把 System.Array.Empty<Transform> 的返回值 保存为 巡逻点数组。
        /// <summary>
        /// 保存攻击范围、两次攻击间隔、丢失目标记忆秒数、调查停留秒数。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float attackRange = 1.6f, attackCooldown = 1.2f, lostDelay = 3, investigationTime = 3; // 把 1.6f 保存为 攻击范围；把 1.2f 保存为 两次攻击间隔；把 3 保存为 丢失目标记忆秒数；把 3 保存为 调查停留秒数。
        /// <summary>
        /// 保存NavMeshAgent 寻路组件。
        /// </summary>
        private NavMeshAgent agent; // 声明 NavMeshAgent 寻路组件，后续初始化。
        /// <summary>
        /// 保存AI 感知组件。
        /// </summary>
        private AIPerception perception; // 声明 AI 感知组件，后续初始化。
        /// <summary>
        /// 保存当前实体组件。
        /// </summary>
        private GhostEntity entity; // 声明 当前实体组件，后续初始化。
        /// <summary>
        /// 保存有限状态机。
        /// </summary>
        private AIStateMachine machine; // 声明 有限状态机，后续初始化。
        /// <summary>
        /// 保存巡逻状态对象、调查状态对象、追击状态对象、攻击状态对象、返回出生点状态对象。
        /// </summary>
        private State patrol, investigate, chase, attack, returning; // 声明 巡逻状态对象，后续初始化；声明 调查状态对象，后续初始化；声明 追击状态对象，后续初始化；声明 攻击状态对象，后续初始化；声明 返回出生点状态对象，后续初始化。
        /// <summary>
        /// 保存出生位置。
        /// </summary>
        private Vector3 home; // 声明 出生位置，后续初始化。
        /// <summary>
        /// 保存进入当前状态后经过的秒数、下一次允许攻击的时间。
        /// </summary>
        private float stateTime, nextAttack; // 声明 进入当前状态后经过的秒数，后续初始化；声明 下一次允许攻击的时间，后续初始化。
        /// <summary>
        /// 保存当前巡逻点索引。
        /// </summary>
        private int waypoint; // 声明 当前巡逻点索引，后续初始化。
        /// <summary>
        /// 获取当前 AI 状态类型名。
        /// </summary>
        public string CurrentState => machine?.Current?.GetType().Name; // 获取当前 AI 状态类型名。
        /// <summary>
        /// 获取寻路代理是否启用且位于 NavMesh。
        /// </summary>
        private bool CanMove => agent.enabled && agent.isOnNavMesh; // 获取寻路代理是否启用且位于 NavMesh。

        /// <summary>
        /// 缓存寻路、感知与实体组件，保存出生点并创建五态 FSM。
        /// </summary>
        private void Awake() // 缓存寻路、感知与实体组件，保存出生点并创建五态 FSM。
        { // 开始方法。
            agent = GetComponent<NavMeshAgent>(); // 更新 NavMeshAgent 寻路组件，使用 GetComponent<NavMeshAgent> 的返回值。
            perception = GetComponent<AIPerception>(); // 更新 AI 感知组件，使用 GetComponent<AIPerception> 的返回值。
            entity = GetComponent<GhostEntity>(); // 更新 当前实体组件，使用 GetComponent<GhostEntity> 的返回值。
            home = transform.position; // 更新 出生位置，使用 本对象世界位置。
            machine = new AIStateMachine(); // 更新 有限状态机，使用 新建的 AIStateMachine 对象。
            patrol = new Patrol(this); // 更新 巡逻状态对象，使用 新建的 Patrol 对象。
            investigate = new Investigate(this); // 更新 调查状态对象，使用 新建的 Investigate 对象。
            chase = new Chase(this); // 更新 追击状态对象，使用 新建的 Chase 对象。
            attack = new Attack(this); // 更新 攻击状态对象，使用 新建的 Attack 对象。
            returning = new Return(this); // 更新 返回出生点状态对象，使用 新建的 Return 对象。
        } // 结束方法。

        /// <summary>
        /// 将 FSM 切换到巡逻状态。
        /// </summary>
        private void OnEnable() // 将 FSM 切换到巡逻状态。
        { // 开始方法。
            machine?.Change(patrol); // 仅在 有限状态机 非空时，同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
        } // 结束方法。

        /// <summary>
        /// 停止 FSM，执行当前状态的退出逻辑。
        /// </summary>
        private void OnDisable() // 停止 FSM，执行当前状态的退出逻辑。
        { // 开始方法。
            machine?.Stop(); // 仅在 有限状态机 非空时，切换为 null，退出当前状态。
        } // 结束方法。

        /// <summary>
        /// 在可寻路、存活、模拟启用且模式敌对时设置速度并 Tick 当前 AI 状态。
        /// </summary>
        private void Update() // 在可寻路、存活、模拟启用且模式敌对时设置速度并 Tick 当前 AI 状态。
        { // 开始方法。
            if (!CanMove) // 当条件 CanMove 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            bool simulate = GameSession.SimulationEnabled && entity.IsAlive && GameModeRuntime.Rules.HostileGhosts; // 只有游戏正在模拟、鬼怪存活且当前玩法允许敌对 AI 时才推进鬼怪。
            agent.isStopped = !simulate; // 把模拟允许状态同步到寻路代理，探索/暂停/死亡时停止移动。
            if (!simulate) // 当条件 simulate 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            agent.speed = (entity.Definition != null ? entity.Definition.moveSpeed : 3) * GameModeRuntime.EnemySpeed; // 写入应用玩法倍率后的鬼怪移动速度；值来自 (entity.Definition != null ? entity.Definition.moveSpeed : 3) * GameModeRuntime.EnemySpeed。
            stateTime += Time.deltaTime; // 按缩放后的帧时长累积当前 AI 状态计时。
            machine.Tick(Time.deltaTime); // 调用当前状态的 Tick。
        } // 结束方法。

        /// <summary>
        /// 目标附近采样 NavMesh，成功后设置 agent 目的地。
        /// </summary>
        /// <param name="point">待检测位置。</param>
        private void Go(Vector3 point) // 目标附近采样 NavMesh，成功后设置 agent 目的地。
        { // 开始方法。
            if (!CanMove) // 当条件 CanMove 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (NavMesh.SamplePosition(point, out var sample, 3, agent.areaMask)) // 当条件 NavMesh.SamplePosition(point, out var sample, 3, agent.areaMask) 成立时，执行下方分支。
                agent.SetDestination(sample.position); // 提交 NavMeshAgent 的寻路目的地。
        } // 结束方法。

        /// <summary>
        /// 寻路代理可用、路径已完成计算且剩余距离到达停止阈值时返回 true。
        /// </summary>
        private bool Arrived => CanMove && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f; // 寻路代理可用、路径已完成计算且剩余距离到达停止阈值时返回 true。
        /// <summary>
        /// 获取当前是否有可见目标。
        /// </summary>
        private bool See => perception.VisibleTarget != null; // 获取当前是否有可见目标。
        /// <summary>
        /// 获取可见目标是否处于攻击距离内。
        /// </summary>
        private bool InAttackRange => See && MathUtility.InRange(transform.position, perception.VisibleTarget.position, attackRange); // 获取可见目标是否处于攻击距离内。

        /// <summary>
        /// 鬼怪状态的内部公共基类。
        /// </summary>
        private abstract class State : AIBaseState // 鬼怪状态的内部公共基类。
        { // 开始类型。
            /// <summary>
            /// 保存所属鬼怪大脑。
            /// </summary>
            protected readonly GhostBrain b; // 声明 所属鬼怪大脑，后续初始化。
            /// <summary>
            /// 构造 State，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="brain">当前状态绑定的鬼怪大脑，提供感知、寻路与状态机上下文。</param>
            protected State(GhostBrain brain) // 构造 State，初始化其依赖与状态。
            { // 开始构造函数。
                b = brain; // 更新 所属鬼怪大脑，使用 鬼怪大脑组件。
            } // 结束构造函数。

            /// <summary>
            /// 鬼怪内部状态进入时重置 stateTime，供当前状态计时。
            /// </summary>
            public override void Enter() // 鬼怪内部状态进入时重置 stateTime，供当前状态计时。
            { // 开始方法。
                b.stateTime = 0; // 更新 b.stateTime，使用 0。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 巡逻/停留状态。
        /// </summary>
        private sealed class Patrol : State // 巡逻/停留状态。
        { // 开始类型。
            /// <summary>
            /// 构造 Patrol，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="b">当前状态绑定的鬼怪大脑，提供感知、寻路与状态机上下文。</param>
            public Patrol(GhostBrain b) : base(b) // 构造 Patrol，初始化其依赖与状态。
            { // 开始构造函数。
            } // 结束构造函数。

            /// <summary>
            /// 重置状态计时并选择下一个巡逻点。
            /// </summary>
            public override void Enter() // 重置状态计时并选择下一个巡逻点。
            { // 开始方法。
                base.Enter(); // 进入状态时初始化上下文；默认基类钩子可为空。
                MoveNext(); // 按巡逻点循环选择下一点，无巡逻点时前往出生位置。
            } // 结束方法。

            /// <summary>
            /// 按巡逻点循环选择下一点，无巡逻点时前往出生位置。
            /// </summary>
            private void MoveNext() // 按巡逻点循环选择下一点，无巡逻点时前往出生位置。
            { // 开始方法。
                if (b.patrolPoints.Length == 0) // 当b.patrolPoints.Length等于0时，执行下方分支。
                { // 开始条件分支。
                    b.Go(b.home); // 目标附近采样 NavMesh，成功后设置 agent 目的地。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                var point = b.patrolPoints[b.waypoint++ % b.patrolPoints.Length]; // 把 b.patrolPoints[b.waypoint++ % b.patrolPoints.Length] 保存为 待检测位置。
                if (point != null) // 当待检测位置不为空时，执行下方分支。
                    b.Go(point.position); // 目标附近采样 NavMesh，成功后设置 agent 目的地。
            } // 结束方法。

            /// <summary>
            /// 看到目标转追击，听到近期声音转调查，到点停留后继续巡逻。
            /// </summary>
            /// <param name="dt">当前帧秒数。</param>
            public override void Tick(float dt) // 看到目标转追击，听到近期声音转调查，到点停留后继续巡逻。
            { // 开始方法。
                if (b.See) // 当b.See为 true时，执行下方分支。
                { // 开始条件分支。
                    b.machine.Change(b.chase); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                if (b.perception.HasRecentSense(1)) // 当条件 b.perception.HasRecentSense(1) 成立时，执行下方分支。
                { // 开始条件分支。
                    b.machine.Change(b.investigate); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                if (b.Arrived && b.stateTime > 1) // 当（b.Arrived为 true；且b.stateTime大于1）时，执行下方分支。
                { // 开始条件分支。
                    b.stateTime = 0; // 更新 b.stateTime，使用 0。
                    MoveNext(); // 按巡逻点循环选择下一点，无巡逻点时前往出生位置。
                } // 结束条件分支。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 前往最后感知位置并调查的状态。
        /// </summary>
        private sealed class Investigate : State // 前往最后感知位置并调查的状态。
        { // 开始类型。
            /// <summary>
            /// 构造 Investigate，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="b">当前状态绑定的鬼怪大脑，提供感知、寻路与状态机上下文。</param>
            public Investigate(GhostBrain b) : base(b) // 构造 Investigate，初始化其依赖与状态。
            { // 开始构造函数。
            } // 结束构造函数。

            /// <summary>
            /// 重置计时并前往最近感知到的位置。
            /// </summary>
            public override void Enter() // 重置计时并前往最近感知到的位置。
            { // 开始方法。
                base.Enter(); // 进入状态时初始化上下文；默认基类钩子可为空。
                b.Go(b.perception.LastKnownPosition); // 目标附近采样 NavMesh，成功后设置 agent 目的地。
            } // 结束方法。

            /// <summary>
            /// 看到目标转追击，新声音刷新目的地/计时，调查超时返回出生点。
            /// </summary>
            /// <param name="dt">当前帧秒数。</param>
            public override void Tick(float dt) // 看到目标转追击，新声音刷新目的地/计时，调查超时返回出生点。
            { // 开始方法。
                if (b.See) // 当b.See为 true时，执行下方分支。
                { // 开始条件分支。
                    b.machine.Change(b.chase); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                if (b.perception.HasRecentSense(0.3f)) // 当条件 b.perception.HasRecentSense(0.3f) 成立时，执行下方分支。
                { // 开始条件分支。
                    b.Go(b.perception.LastKnownPosition); // 目标附近采样 NavMesh，成功后设置 agent 目的地。
                    b.stateTime = 0; // 更新 b.stateTime，使用 0。
                } // 结束条件分支。

                if (b.stateTime > b.investigationTime) // 当b.stateTime大于b.investigationTime时，执行下方分支。
                    b.machine.Change(b.returning); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 追踪最近已知目标位置的状态。
        /// </summary>
        private sealed class Chase : State // 追踪最近已知目标位置的状态。
        { // 开始类型。
            /// <summary>
            /// 构造 Chase，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="b">当前状态绑定的鬼怪大脑，提供感知、寻路与状态机上下文。</param>
            public Chase(GhostBrain b) : base(b) // 构造 Chase，初始化其依赖与状态。
            { // 开始构造函数。
            } // 结束构造函数。

            /// <summary>
            /// 进入攻击范围时转攻击，记忆超时则返回，否则持续追踪最近感知位置。
            /// </summary>
            /// <param name="dt">当前帧秒数。</param>
            public override void Tick(float dt) // 进入攻击范围时转攻击，记忆超时则返回，否则持续追踪最近感知位置。
            { // 开始方法。
                if (b.InAttackRange) // 当b.InAttackRange为 true时，执行下方分支。
                { // 开始条件分支。
                    b.machine.Change(b.attack); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                if (!b.perception.HasRecentSense(b.lostDelay)) // 当条件 b.perception.HasRecentSense(b.lostDelay) 不成立时，执行下方分支。
                { // 开始条件分支。
                    b.machine.Change(b.returning); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                b.Go(b.perception.LastKnownPosition); // 目标附近采样 NavMesh，成功后设置 agent 目的地。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 停止寻路、面向目标并按冷却造成伤害的状态。
        /// </summary>
        private sealed class Attack : State // 停止寻路、面向目标并按冷却造成伤害的状态。
        { // 开始类型。
            /// <summary>
            /// 构造 Attack，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="b">当前状态绑定的鬼怪大脑，提供感知、寻路与状态机上下文。</param>
            public Attack(GhostBrain b) : base(b) // 构造 Attack，初始化其依赖与状态。
            { // 开始构造函数。
            } // 结束构造函数。

            /// <summary>
            /// 重置状态计时，并在寻路代理可用时停止当前路径。
            /// </summary>
            public override void Enter() // 重置状态计时，并在寻路代理可用时停止当前路径。
            { // 开始方法。
                base.Enter(); // 进入状态时初始化上下文；默认基类钩子可为空。
                if (b.CanMove) // 当b.CanMove为 true时，执行下方分支。
                    b.agent.ResetPath(); // 清空 NavMeshAgent 当前路径，使鬼怪在进入攻击状态时停止追踪移动。
            } // 结束方法。

            /// <summary>
            /// 目标离开攻击范围时转追击；否则面向目标，按冷却发布攻击动画并调用伤害接口。
            /// </summary>
            /// <param name="dt">当前帧秒数。</param>
            public override void Tick(float dt) // 目标离开攻击范围时转追击；否则面向目标，按冷却发布攻击动画并调用伤害接口。
            { // 开始方法。
                if (!b.InAttackRange) // 当条件 b.InAttackRange 不成立时，执行下方分支。
                { // 开始条件分支。
                    b.machine.Change(b.chase); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                var target = b.perception.VisibleTarget; // 把 b.perception.VisibleTarget 保存为 目标对象。
                var direction = MathUtility.Planar(target.position - b.transform.position); // 把 MathUtility.Planar 的返回值 保存为 direction。
                if (direction.sqrMagnitude > 0.001f) // 当direction.sqrMagnitude大于0.001f时，执行下方分支。
                    b.transform.rotation = Quaternion.LookRotation(direction); // 更新 b.transform.rotation，使用 Quaternion.LookRotation 的返回值。
                if (Time.time < b.nextAttack) // 当当前缩放时间小于b.nextAttack时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                b.nextAttack = Time.time + Mathf.Max(0.1f, b.attackCooldown); // 更新 b.nextAttack，使用 Time.time + Mathf.Max(0.1f, b.attackCooldown)。
                EventBus.Publish(new EntityAnimationRequested(b.entity, EntityAnimationState.Attack)); // 广播 EntityAnimationRequested 事件，让动画组件播放指定实体动作。
                var victim = target.GetComponent<IDamageable>(); // 把 target.GetComponent<IDamageable> 的返回值 保存为 victim。
                victim?.TakeDamage(new DamageInfo((b.entity.Definition != null ? (b.entity.Definition.attackDamage) : 10) * GameModeRuntime.EnemyDamage, b.gameObject, target.position)); // 仅在 victim 非空时，可受伤对象的统一伤害契约，由实现类型处理 DamageInfo。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 丢失目标后返回出生位置的状态。
        /// </summary>
        private sealed class Return : State // 丢失目标后返回出生位置的状态。
        { // 开始类型。
            /// <summary>
            /// 构造 Return，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="b">当前状态绑定的鬼怪大脑，提供感知、寻路与状态机上下文。</param>
            public Return(GhostBrain b) : base(b) // 构造 Return，初始化其依赖与状态。
            { // 开始构造函数。
            } // 结束构造函数。

            /// <summary>
            /// 重置计时并前往出生位置。
            /// </summary>
            public override void Enter() // 重置计时并前往出生位置。
            { // 开始方法。
                base.Enter(); // 进入状态时初始化上下文；默认基类钩子可为空。
                b.Go(b.home); // 目标附近采样 NavMesh，成功后设置 agent 目的地。
            } // 结束方法。

            /// <summary>
            /// 途中看到目标转追击；到达出生位置或返回超时后恢复巡逻。
            /// </summary>
            /// <param name="dt">当前帧秒数。</param>
            public override void Tick(float dt) // 途中看到目标转追击；到达出生位置或返回超时后恢复巡逻。
            { // 开始方法。
                if (b.See) // 当b.See为 true时，执行下方分支。
                    b.machine.Change(b.chase); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
                else if (b.Arrived || b.stateTime > 10) // 前面的条件不成立时，执行此备用分支。
                    b.machine.Change(b.patrol); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
            } // 结束方法。
        } // 结束类型。
    } // 结束类型。
} // 结束命名空间。
