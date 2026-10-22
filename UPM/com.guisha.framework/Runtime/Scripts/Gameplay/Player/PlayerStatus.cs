using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 玩家 San、体力与受伤状态，并参与存档。
    /// </summary>
    [RequireComponent(typeof(PlayerEntity))] // 声明本组件必须同时具备的依赖组件。
    public sealed class PlayerStatus : MonoBehaviour // 玩家 San、体力与受伤状态，并参与存档。
    { // 开始类型。
        /// <summary>
        /// 保存理智上限、体力上限。
        /// </summary>
        [SerializeField, Min(1)] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float maxSan = 100, maxStamina = 100; // 把 100 保存为 理智上限；把 100 保存为 体力上限。
        /// <summary>
        /// 保存每秒体力恢复量。
        /// </summary>
        [SerializeField, Min(0)] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float staminaRecovery = 15; // 把 15 保存为 每秒体力恢复量。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存玩家实体组件。
        /// </summary>
        private PlayerEntity entity; // 声明 玩家实体组件，后续初始化。
        /// <summary>
        /// 保存上次体力消耗时间。
        /// </summary>
        private float lastExertion = -10; // 把 -10 保存为 上次体力消耗时间。
        /// <summary>
        /// 保存上一帧低理智状态。
        /// </summary>
        private bool low; // 声明 上一帧低理智状态，后续初始化。
        /// <summary>
        /// 获取当前理智。
        /// </summary>
        public float San { get; private set; } // 获取当前理智。
        /// <summary>
        /// 获取当前体力。
        /// </summary>
        public float Stamina { get; private set; } // 获取当前体力。
        /// <summary>
        /// 玩家生命低于上限的 40% 时返回 true。
        /// </summary>
        public bool Injured => entity != null && entity.Health < entity.MaxHealth * 0.4f; // 玩家生命低于上限的 40% 时返回 true。

        /// <summary>
        /// 缓存玩家实体，并把理智和体力设为各自上限。
        /// </summary>
        private void Awake() // 缓存玩家实体，并把理智和体力设为各自上限。
        { // 开始方法。
            entity = GetComponent<PlayerEntity>(); // 更新 玩家实体组件，使用 GetComponent<PlayerEntity> 的返回值。
            San = maxSan; // 更新 当前理智，使用 理智上限。
            Stamina = maxStamina; // 更新 当前体力，使用 体力上限。
        } // 结束方法。

        /// <summary>
        /// 延迟发布玩家属性，订阅存档采集、读档恢复和受伤事件。
        /// </summary>
        private void OnEnable() // 延迟发布玩家属性，订阅存档采集、读档恢复和受伤事件。
        { // 开始方法。
            AsyncHelper.Delay(this, 0, Publish); // 在组件上启动延时协程。
            subscriptions.Add(EventBus.Subscribe<SaveCapture>(e => // 订阅 SaveCapture 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                e.Data.hasPlayer = true; // 更新 e.Data.hasPlayer，使用 true（启用/满足）。
                e.Data.position = transform.position; // 更新 e.Data.position，使用 本对象世界位置。
                e.Data.rotation = transform.rotation; // 更新 e.Data.rotation，使用 本对象世界旋转。
                e.Data.health = entity.Health; // 更新 e.Data.health，使用 当前实体生命。
                e.Data.san = San; // 更新 e.Data.san，使用 当前理智。
                e.Data.stamina = Stamina; // 更新 e.Data.stamina，使用 当前体力。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(e => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                var controller = GetComponent<CharacterController>(); // 把 GetComponent<CharacterController> 的返回值 保存为 控制器组件。
                if (controller != null) // 当控制器组件不为空时，执行下方分支。
                    controller.enabled = false; // 更新 controller.enabled，使用 false（关闭/不满足）。
                transform.SetPositionAndRotation(e.Data.position, e.Data.rotation); // 同时恢复世界位置与旋转。
                if (controller != null) // 当控制器组件不为空时，执行下方分支。
                    controller.enabled = true; // 更新 controller.enabled，使用 true（启用/满足）。
                entity.SetHealth(e.Data.health); // 钳制生命到 0-MaxHealth，并广播 HealthChanged；此入口本身不广播 EntityDied。
                San = Mathf.Clamp(e.Data.san, 0, maxSan); // 更新 当前理智，使用 Mathf.Clamp 的返回值。
                Stamina = Mathf.Clamp(e.Data.stamina, 0, maxStamina); // 更新 当前体力，使用 Mathf.Clamp 的返回值。
                Publish(); // 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// Unity 首次启用后的初始化回调，发布当前状态或完成初次配置。
        /// </summary>
        private void Start() // Unity 首次启用后的初始化回调，发布当前状态或完成初次配置。
        { // 开始方法。
            Publish(); // 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 允许模拟且停止消耗满 0.5 秒后恢复体力，并广播变化。
        /// </summary>
        private void Update() // 允许模拟且停止消耗满 0.5 秒后恢复体力，并广播变化。
        { // 开始方法。
            if (!GameSession.SimulationEnabled || Time.time - lastExertion < 0.5f || Stamina >= maxStamina) // 当（当前不推进游戏模拟；或Time.time - lastExertion小于0.5f；或当前体力不小于体力上限）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            Stamina = Mathf.Min(maxStamina, Stamina + staminaRecovery * Time.deltaTime); // 更新 当前体力，使用 Mathf.Min 的返回值。
            Publish(); // 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
        } // 结束方法。

        /// <summary>
        /// 应用玩法消耗倍率，体力足够时扣除、更新时间并广播属性，返回是否成功。
        /// </summary>
        /// <param name="amount">本次伤害量。</param>
        /// <returns>true 表示规则允许且体力充足，消耗已经提交。</returns>
        public bool ConsumeStamina(float amount) // 应用玩法消耗倍率，体力足够时扣除、更新时间并广播属性，返回是否成功。
        { // 开始方法。
            amount *= GameModeRuntime.Rules.StaminaCost; // 按倍率更新 本次伤害量，使用 GameModeRuntime.Rules.StaminaCost。
            if (Stamina < amount || amount < 0) // 当（当前体力小于本次伤害量；或本次伤害量小于0）时，执行下方分支。
                return false; // 返回 false，将当前分支的布尔结果交给调用方。
            Stamina -= amount; // 扣减 当前体力，使用 本次伤害量。
            lastExertion = Time.time; // 更新 上次体力消耗时间，使用 当前缩放时间。
            Publish(); // 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
            return true; // 返回 true，将当前分支的布尔结果交给调用方。
        } // 结束方法。

        /// <summary>
        /// 按 delta 改变 San，钳制到有效范围，数值变化时广播。
        /// </summary>
        /// <param name="delta">变化量。</param>
        public void ChangeSan(float delta) // 按 delta 改变 San，钳制到有效范围，数值变化时广播。
        { // 开始方法。
            float value = Mathf.Clamp(San + delta, 0, maxSan); // 把 Mathf.Clamp 的返回值 保存为 当前处理值。
            if (Mathf.Approximately(value, San)) // 当条件 Mathf.Approximately(value, San) 成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            San = value; // 更新 当前理智，使用 当前处理值。
            Publish(); // 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
        } // 结束方法。

        /// <summary>
        /// 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
        /// </summary>
        private void Publish() // 广播 San/体力，低 San 状态切换时额外广播 LowSanChanged。
        { // 开始方法。
            EventBus.Publish(new PlayerStatsChanged(gameObject, San, maxSan, Stamina, maxStamina)); // 广播 PlayerStatsChanged 事件，刷新理智和体力 UI。
            bool next = San <= maxSan * 0.25f; // 把 San <= maxSan * 0.25f 保存为 下一项。
            if (next != low) // 当下一项不等于上一帧低理智状态时，执行下方分支。
            { // 开始条件分支。
                low = next; // 更新 上一帧低理智状态，使用 下一项。
                EventBus.Publish(new LowSanChanged(low)); // 广播 LowSanChanged 事件，切换低理智表现。
            } // 结束条件分支。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
