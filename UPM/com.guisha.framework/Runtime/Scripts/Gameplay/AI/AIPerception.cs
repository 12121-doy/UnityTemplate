using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 鬼怪视觉锥、遮挡与听觉感知，记录最近目标位置和时间。
    /// </summary>
    public sealed class AIPerception : MonoBehaviour // 鬼怪视觉锥、遮挡与听觉感知，记录最近目标位置和时间。
    { // 开始类型。
        /// <summary>
        /// 保存最大视觉距离、最大听觉范围。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float sightRange = 12, hearingRange = 15; // 把 12 保存为 最大视觉距离；把 15 保存为 最大听觉范围。
        /// <summary>
        /// 保存完整视觉锥角。
        /// </summary>
        [Range(0, 360)] // 约束 Inspector 中可编辑的数值范围。
        public float sightAngle = 100; // 把 100 保存为 完整视觉锥角。
        /// <summary>
        /// 保存候选目标层掩码、视线遮挡层掩码。
        /// </summary>
        public LayerMask targetMask = ~0, obstructionMask = ~0; // 把 ~0 保存为 候选目标层掩码；把 ~0 保存为 视线遮挡层掩码。
        /// <summary>
        /// 保存视觉检测起点。
        /// </summary>
        public Transform eye; // 声明 视觉检测起点，后续初始化。
        /// <summary>
        /// 获取当前可见目标。
        /// </summary>
        public Transform VisibleTarget { get; private set; } // 获取当前可见目标。
        /// <summary>
        /// 获取最近感知到的目标位置。
        /// </summary>
        public Vector3 LastKnownPosition { get; private set; } // 获取最近感知到的目标位置。
        /// <summary>
        /// 获取最近感知时间。
        /// </summary>
        public float LastSenseTime { get; private set; } = -100; // 获取最近感知时间。

        /// <summary>
        /// 判断距上次记忆是否未超过给定秒数。
        /// </summary>
        /// <param name="seconds">延时秒数。</param>
        /// <returns>true 表示最近感知仍处于记忆有效秒数内。</returns>
        public bool HasRecentSense(float seconds) => Time.time - LastSenseTime <= seconds; // 判断距上次记忆是否未超过给定秒数。
        /// <summary>
        /// 保存下次感知扫描时间。
        /// </summary>
        private float nextScan; // 声明 下次感知扫描时间，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 订阅玩家脚步噪声，并在听觉范围内更新目标记忆。
        /// </summary>
        private void OnEnable() // 订阅玩家脚步噪声，并在听觉范围内更新目标记忆。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<NoiseEmitted>(e => // 广播 PerceptionChanged 事件，通知 AI 最新感知目标。
            { // 开始回调。
                if (!GameSession.SimulationEnabled || e.Source == null || e.Source.GetComponent<PlayerEntity>() == null) // 当（当前不推进游戏模拟；或e.Source为空；或e.Source.GetComponent小于PlayerEntity>() == null）时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                if (MathUtility.InRange(transform.position, e.Position, Mathf.Min(hearingRange, e.Radius))) // 当条件 MathUtility.InRange(transform.position, e.Position, Mathf.Min(hearingRange, e.Radius)) 成立时，执行下方分支。
                { // 开始条件分支。
                    Remember(e.Position); // 更新最近位置和感知时间。
                    EventBus.Publish(new PerceptionChanged(gameObject, e.Source.transform, e.Position, false)); // 广播 PerceptionChanged 事件，通知 AI 最新感知目标。
                } // 结束条件分支。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 释放噪声订阅并清空当前可见目标。
        /// </summary>
        private void OnDisable() // 释放噪声订阅并清空当前可见目标。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            VisibleTarget = null; // 清空 当前可见目标 引用，表示当前不再持有该对象。
        } // 结束方法。

        /// <summary>
        /// 更新最近位置和感知时间。
        /// </summary>
        /// <param name="position">世界位置。</param>
        private void Remember(Vector3 position) // 更新最近位置和感知时间。
        { // 开始方法。
            LastKnownPosition = position; // 更新 最近感知到的目标位置，使用 世界位置。
            LastSenseTime = Time.time; // 更新 最近感知时间，使用 当前缩放时间。
        } // 结束方法。

        /// <summary>
        /// 每 0.15 秒扫描玩家、校验视锥与遮挡，更新可见目标和记忆并广播变化。
        /// </summary>
        private void Update() // 每 0.15 秒扫描玩家、校验视锥与遮挡，更新可见目标和记忆并广播变化。
        { // 开始方法。
            if (!GameSession.SimulationEnabled || Time.time < nextScan) // 当（当前不推进游戏模拟；或当前缩放时间小于下次感知扫描时间）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            nextScan = Time.time + 0.15f; // 更新 下次感知扫描时间，使用 Time.time + 0.15f。
            var previous = VisibleTarget; // 把 当前可见目标 保存为 修改前的旧值。
            VisibleTarget = null; // 清空 当前可见目标 引用，表示当前不再持有该对象。
            var origin = eye != null ? eye.position : transform.position + Vector3.up * 1.5f; // 把 按条件 eye != null 选择的值 保存为 检测原点。
            float nearest = float.MaxValue; // 把 float.MaxValue 保存为 nearest。
            foreach (var hit in Physics.OverlapSphere(origin, sightRange, targetMask, QueryTriggerInteraction.Ignore)) // 遍历 Physics.OverlapSphere(origin, sightRange, targetMask, QueryTriggerInteraction.Ignore)，每次把一个元素保存为 hit。
            { // 开始遍历循环。
                var player = hit.GetComponentInParent<PlayerEntity>(); // 把 hit.GetComponentInParent<PlayerEntity> 的返回值 保存为 玩家组件或玩家对象。
                if (player == null || !player.IsAlive || !player.gameObject.activeInHierarchy) // 当（玩家组件或玩家对象为空；或条件 player.IsAlive 不成立；或条件 player.gameObject.activeInHierarchy 不成立）时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                var point = player.transform.position + Vector3.up; // 把 player.transform.position + Vector3.up 保存为 待检测位置。
                if (!MathUtility.InCone(origin, transform.forward, point, sightRange, sightAngle)) // 当条件 MathUtility.InCone(origin, transform.forward, point, sightRange, sightAngle) 不成立时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                var direction = point - origin; // 把 point - origin 保存为 direction。
                bool blocked = false; // 把 false（关闭/不满足） 保存为 是否存在阻挡。
                foreach (var obstacle in Physics.RaycastAll(origin, direction.normalized, direction.magnitude, obstructionMask, QueryTriggerInteraction.Ignore)) // 遍历 Physics.RaycastAll(origin, direction.normalized, direction.magnitude, obstructionMask, QueryTriggerInteraction.Ignore)，每次把一个元素保存为 obstacle。
                    if (!obstacle.transform.IsChildOf(transform) && !obstacle.transform.IsChildOf(player.transform)) // 当（条件 obstacle.transform.IsChildOf(transform) 不成立；且条件 obstacle.transform.IsChildOf(player.transform) 不成立）时，执行下方分支。
                    { // 开始条件分支。
                        blocked = true; // 更新 是否存在阻挡，使用 true（启用/满足）。
                        break; // 结束当前循环或 switch 分支。
                    } // 结束条件分支。

                if (blocked || direction.sqrMagnitude >= nearest) // 当（是否存在阻挡为 true；或direction.sqrMagnitude不小于nearest）时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                nearest = direction.sqrMagnitude; // 更新 nearest，使用 direction.sqrMagnitude。
                VisibleTarget = player.transform; // 更新 当前可见目标，使用 player.transform。
            } // 结束遍历循环。

            if (VisibleTarget != null) // 当当前可见目标不为空时，执行下方分支。
                Remember(VisibleTarget.position); // 更新最近位置和感知时间。
            if (previous != VisibleTarget) // 当修改前的旧值不等于当前可见目标时，执行下方分支。
                EventBus.Publish(new PerceptionChanged(gameObject, VisibleTarget, LastKnownPosition, true)); // 广播 PerceptionChanged 事件，通知 AI 最新感知目标。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
