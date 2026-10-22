using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 角色实体基类，实现伤害与生命状态，并发布实体事件。
    /// </summary>
    public abstract class BaseEntity : MonoBehaviour, IDamageable // 角色实体基类，实现伤害与生命状态，并发布实体事件。
    { // 开始类型。
        /// <summary>
        /// 保存实体配置。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        protected EntityDefinition definition; // 声明 实体配置，后续初始化。
        /// <summary>
        /// 保存缺少配置时的生命上限。
        /// </summary>
        [SerializeField, Min(1)] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float fallbackMaxHealth = 100; // 把 100 保存为 缺少配置时的生命上限。
        /// <summary>
        /// 保存Animator 动画组件。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        protected Animator animator; // 声明 Animator 动画组件，后续初始化。
        /// <summary>
        /// 保存实体本地伤害处理链。
        /// </summary>
        private DamagePipeline damagePipeline; // 声明 实体本地伤害处理链，后续初始化。
        /// <summary>
        /// 获取实体类别。
        /// </summary>
        public abstract EntityType Type { get; } // 获取实体类别。
        /// <summary>
        /// 获取实体配置。
        /// </summary>
        public EntityDefinition Definition => definition; // 获取实体配置。
        /// <summary>
        /// 读取配置中的生命上限；配置缺失时采用 fallbackMaxHealth，最小为 1。
        /// </summary>
        public float MaxHealth => definition != null ? Mathf.Max(1, definition.maxHealth) : Mathf.Max(1, fallbackMaxHealth); // 读取配置中的生命上限；配置缺失时采用 fallbackMaxHealth，最小为 1。
        /// <summary>
        /// 获取当前生命；通过 TakeDamage/SetHealth 修改以同步事件。
        /// </summary>
        public float Health { get; private set; } // 获取当前生命；通过 TakeDamage/SetHealth 修改以同步事件。
        /// <summary>
        /// 当前生命大于 0 时返回 true。
        /// </summary>
        public bool IsAlive => Health > 0; // 当前生命大于 0 时返回 true。
        /// <summary>
        /// 获取实体生命状态。
        /// </summary>
        public EntityLifeState LifeState => IsAlive ? EntityLifeState.Alive : EntityLifeState.Dead; // 获取实体生命状态。

        /// <summary>
        /// 初始化生命为上限，缓存可选伤害链，并补找 Animator 引用。
        /// </summary>
        protected virtual void Awake() // 初始化生命为上限，缓存可选伤害链，并补找 Animator 引用。
        { // 开始方法。
            Health = MaxHealth; // 初次初始化生命为当前配置上限。
            damagePipeline = GetComponent<DamagePipeline>(); // 更新 实体本地伤害处理链，使用 GetComponent<DamagePipeline> 的返回值。
            if (animator == null) // 当Animator 动画组件为空时，执行下方分支。
                animator = GetComponentInChildren<Animator>(); // 更新 Animator 动画组件，使用 GetComponentInChildren<Animator> 的返回值。
        } // 结束方法。

        /// <summary>
        /// 绑定新的实体配置，并将生命重设为新上限。
        /// </summary>
        /// <param name="config">注入的配置。</param>
        public void Configure(EntityDefinition config) // 绑定新的实体配置，并将生命重设为新上限。
        { // 开始方法。
            definition = config; // 更新 实体配置，使用 注入的配置。
            SetHealth(MaxHealth); // 钳制生命到 0-MaxHealth，并广播 HealthChanged；此入口本身不广播 EntityDied。
        } // 结束方法。

        /// <summary>
        /// 校验伤害和存活状态，应用玩法倍率与本地伤害链，更新生命并广播受击/生命/死亡事件。
        /// </summary>
        /// <param name="damage">本次伤害载荷。</param>
        public virtual void TakeDamage(DamageInfo damage) // 校验伤害和存活状态，应用玩法倍率与本地伤害链，更新生命并广播受击/生命/死亡事件。
        { // 开始方法。
            if (!IsAlive || damage.Amount <= 0 || float.IsNaN(damage.Amount)) // 当（实体已经死亡；或本次伤害值不大于0；或伤害值为 NaN）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            float amount = damage.Amount * (Type == EntityType.Player ? GameModeRuntime.Rules.PlayerDamage : 1); // 玩家应用当前玩法的承伤倍率；NPC/鬼怪先保留输入伤害。
            if (damagePipeline != null) // 当实体本地伤害处理链不为空时，执行下方分支。
                amount = damagePipeline.Resolve(amount); // 让可选的本地伤害链继续应用无敌、固定减伤与倍率。
            if (amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) // 当（处理后的伤害不大于零；或条件 float.IsNaN(amount) 成立；或条件 float.IsInfinity(amount) 成立）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            damage = new DamageInfo(amount, damage.Source, damage.Point); // 用最终伤害重新构造载荷，同时保留来源和命中点供表现订阅者使用。
            Health = Mathf.Max(0, Health - amount); // 扣除最终伤害，并把最低生命限制为 0。
            EventBus.Publish(new EntityDamaged(this, damage)); // 广播 EntityDamaged 事件，驱动受击音效、震动和动画反馈。
            EventBus.Publish(new HealthChanged(this, Health, MaxHealth)); // 广播 HealthChanged 事件，同步实体生命表现。
            if (!IsAlive) // 当实体已经死亡时，执行下方分支。
                EventBus.Publish(new EntityDied(this)); // 广播 EntityDied 事件，触发实体死亡与游戏结束处理。
        } // 结束方法。

        /// <summary>
        /// 钳制生命到 0-MaxHealth，并广播 HealthChanged；此入口本身不广播 EntityDied。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        public void SetHealth(float value) // 钳制生命到 0-MaxHealth，并广播 HealthChanged；此入口本身不广播 EntityDied。
        { // 开始方法。
            Health = Mathf.Clamp(float.IsNaN(value) ? MaxHealth : value, 0, MaxHealth); // 恢复生命时把 NaN 回退到上限，再限制在 0 到 MaxHealth 之间。
            EventBus.Publish(new HealthChanged(this, Health, MaxHealth)); // 广播 HealthChanged 事件，同步实体生命表现。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
