using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 按环境与鬼怪距离扣减 San，并应用玩法倍率。
    /// </summary>
    [RequireComponent(typeof(PlayerStatus))] // 声明本组件必须同时具备的依赖组件。
    public sealed class SanSystem : MonoBehaviour // 按环境与鬼怪距离扣减 San，并应用玩法倍率。
    { // 开始类型。
        /// <summary>
        /// 保存环境每秒理智消耗、鬼怪接近时的每秒理智消耗、鬼怪产生理智压力的半径。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float environmentDrain = 0.5f, ghostDrain = 6, ghostRadius = 10; // 把 0.5f 保存为 环境每秒理智消耗；把 6 保存为 鬼怪接近时的每秒理智消耗；把 10 保存为 鬼怪产生理智压力的半径。
        /// <summary>
        /// 保存鬼怪检测层掩码。
        /// </summary>
        public LayerMask ghostMask = ~0; // 把 ~0 保存为 鬼怪检测层掩码。
        /// <summary>
        /// 保存玩家状态组件。
        /// </summary>
        private PlayerStatus status; // 声明 玩家状态组件，后续初始化。
        /// <summary>
        /// 保存下次低频更新时刻。
        /// </summary>
        private float nextTick; // 声明 下次低频更新时刻，后续初始化。
        /// <summary>
        /// 缓存本玩家的状态组件，供低频理智扣减使用。
        /// </summary>
        private void Awake() // 缓存本玩家的状态组件，供低频理智扣减使用。
        { // 开始方法。
            status = GetComponent<PlayerStatus>(); // 更新 玩家状态组件，使用 GetComponent<PlayerStatus> 的返回值。
        } // 结束方法。

        /// <summary>
        /// 每 0.25 秒计算附近鬼怪压力与环境消耗，按玩法倍率扣减 San。
        /// </summary>
        private void Update() // 每 0.25 秒计算附近鬼怪压力与环境消耗，按玩法倍率扣减 San。
        { // 开始方法。
            if (!GameSession.SimulationEnabled || Time.time < nextTick) // 当（当前不推进游戏模拟；或当前缩放时间小于下次低频更新时刻）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            const float tick = 0.25f; // 把 0.25f 保存为 tick。
            nextTick = Time.time + tick; // 更新 下次低频更新时刻，使用 Time.time + tick。
            float danger = 0; // 把 0 保存为 danger。
            foreach (var hit in Physics.OverlapSphere(transform.position, ghostRadius, ghostMask, QueryTriggerInteraction.Ignore)) // 遍历 Physics.OverlapSphere(transform.position, ghostRadius, ghostMask, QueryTriggerInteraction.Ignore)，每次把一个元素保存为 hit。
            { // 开始遍历循环。
                var ghost = hit.GetComponentInParent<GhostEntity>(); // 把 hit.GetComponentInParent<GhostEntity> 的返回值 保存为 鬼怪实体或鬼怪配置。
                if (ghost == null || !ghost.IsAlive) // 当（鬼怪实体或鬼怪配置为空；或条件 ghost.IsAlive 不成立）时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                danger = Mathf.Max(danger, 1 - Vector3.Distance(transform.position, ghost.transform.position) / Mathf.Max(0.1f, ghostRadius)); // 更新 danger，使用 Mathf.Max 的返回值。
            } // 结束遍历循环。

            status.ChangeSan(-(environmentDrain + ghostDrain * danger) * tick * GameModeRuntime.Rules.SanDrain); // 按 delta 改变 San，钳制到有效范围，数值变化时广播。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
