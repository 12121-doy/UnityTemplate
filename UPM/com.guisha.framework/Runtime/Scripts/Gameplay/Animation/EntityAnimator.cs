using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.AI; // 引入 UnityEngine.AI 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 为 BaseEntity 绑定动画后端，更新移动参数并处理攻击、受击、死亡和交互事件。
    /// </summary>
    [RequireComponent(typeof(BaseEntity))] // 声明本组件必须同时具备的依赖组件。
    public sealed class EntityAnimator : MonoBehaviour // 为 BaseEntity 绑定动画后端，更新移动参数并处理攻击、受击、死亡和交互事件。
    { // 开始类型。
        /// <summary>
        /// 保存框架或动画配置。
        /// </summary>
        public AnimationSettings settings; // 声明 框架或动画配置，后续初始化。
        /// <summary>
        /// 保存目标 Animator。
        /// </summary>
        public Animator targetAnimator; // 声明 目标 Animator，后续初始化。
        /// <summary>
        /// 保存旧版 Animation 组件。
        /// </summary>
        public Animation legacyAnimation; // 声明 旧版 Animation 组件，后续初始化。
        /// <summary>
        /// 保存是否输出动作日志。
        /// </summary>
        public bool logActions; // 声明 是否输出动作日志，后续初始化。
        /// <summary>
        /// 保存当前实体组件。
        /// </summary>
        private BaseEntity entity; // 声明 当前实体组件，后续初始化。
        /// <summary>
        /// 保存统一动画驱动。
        /// </summary>
        private IAnimationDriver driver; // 声明 统一动画驱动，后续初始化。
        /// <summary>
        /// 保存上一帧位置。
        /// </summary>
        private Vector3 lastPosition; // 声明 上一帧位置，后续初始化。
        /// <summary>
        /// 保存角色对象。
        /// </summary>
        private CharacterController character; // 声明 角色对象，后续初始化。
        /// <summary>
        /// 保存NavMeshAgent 寻路组件。
        /// </summary>
        private NavMeshAgent agent; // 声明 NavMeshAgent 寻路组件，后续初始化。
        /// <summary>
        /// 保存玩家组件或玩家对象。
        /// </summary>
        private PlayerController player; // 声明 玩家组件或玩家对象，后续初始化。
        /// <summary>
        /// 保存Animator 原始更新模式。
        /// </summary>
        private AnimatorUpdateMode regularUpdateMode; // 声明 Animator 原始更新模式，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 获取可用的统一动画驱动。
        /// </summary>
        public IAnimationDriver Driver => driver ?? NullAnimationDriver.Instance; // 获取可用的统一动画驱动。

        /// <summary>
        /// 缓存实体、移动和寻路组件，选择并绑定动画驱动。
        /// </summary>
        private void Awake() // 缓存实体、移动和寻路组件，选择并绑定动画驱动。
        { // 开始方法。
            entity = GetComponent<BaseEntity>(); // 更新 当前实体组件，使用 GetComponent<BaseEntity> 的返回值。
            character = GetComponent<CharacterController>(); // 更新 角色对象，使用 GetComponent<CharacterController> 的返回值。
            agent = GetComponent<NavMeshAgent>(); // 更新 NavMeshAgent 寻路组件，使用 GetComponent<NavMeshAgent> 的返回值。
            player = GetComponent<PlayerController>(); // 更新 玩家组件或玩家对象，使用 GetComponent<PlayerController> 的返回值。
            Rebind(); // 寻找动画组件，保存 Animator 更新模式，通过工厂选择驱动，并可包装动作日志装饰器。
        } // 结束方法。

        /// <summary>
        /// 寻找动画组件，保存 Animator 更新模式，通过工厂选择驱动，并可包装动作日志装饰器。
        /// </summary>
        public void Rebind() // 寻找动画组件，保存 Animator 更新模式，通过工厂选择驱动，并可包装动作日志装饰器。
        { // 开始方法。
            if (targetAnimator == null) // 当目标 Animator为空时，执行下方分支。
                targetAnimator = GetComponentInChildren<Animator>(); // 更新 目标 Animator，使用 GetComponentInChildren<Animator> 的返回值。
            if (targetAnimator != null) // 当目标 Animator不为空时，执行下方分支。
                regularUpdateMode = targetAnimator.updateMode; // 更新 Animator 原始更新模式，使用 targetAnimator.updateMode。
            if (legacyAnimation == null) // 当旧版 Animation 组件为空时，执行下方分支。
                legacyAnimation = GetComponentInChildren<Animation>(); // 更新 旧版 Animation 组件，使用 GetComponentInChildren<Animation> 的返回值。
            driver = targetAnimator != null ? new AnimatorDriverFactory(targetAnimator).Create() : new LegacyAnimationDriverFactory(legacyAnimation).Create(); // 更新 统一动画驱动，使用 按条件 targetAnimator != null 选择的值。
            if (logActions) // 当是否输出动作日志为 true时，执行下方分支。
                driver = new AnimationLoggingDecorator(driver, Guisha.Core.Logger.Info); // 更新 统一动画驱动，使用 新建的 AnimationLoggingDecorator 对象。
        } // 结束方法。

        /// <summary>
        /// 保存初始位置，订阅受击、死亡、交互与读档等动画相关事件。
        /// </summary>
        private void OnEnable() // 保存初始位置，订阅受击、死亡、交互与读档等动画相关事件。
        { // 开始方法。
            lastPosition = transform.position; // 更新 上一帧位置，使用 本对象世界位置。
            subscriptions.Add(EventBus.Subscribe<EntityDamaged>(e => // 订阅 EntityDamaged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Entity == entity && entity.IsAlive) // 当（e.Entity等于当前实体组件；且当前实体存活状态为 true）时，执行下方分支。
                    Play(EntityAnimationState.Hit); // 按动作映射触发器/状态；死亡设 Dead 并改用非缩放时间，使 GameOver 后死亡动画继续。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => // 订阅 EntityDied 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Entity == entity) // 当e.Entity等于当前实体组件时，执行下方分支。
                    Play(EntityAnimationState.Dead); // 按动作映射触发器/状态；死亡设 Dead 并改用非缩放时间，使 GameOver 后死亡动画继续。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<EntityAnimationRequested>(e => // 订阅 EntityAnimationRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Entity == entity) // 当e.Entity等于当前实体组件时，执行下方分支。
                    Play(e.State); // 按动作映射触发器/状态；死亡设 Dead 并改用非缩放时间，使 GameOver 后死亡动画继续。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                lastPosition = transform.position; // 更新 上一帧位置，使用 本对象世界位置。
                Driver.Bool(settings != null ? settings.deadParameter : "Dead", !entity.IsAlive); // 设置布尔动画参数。
                if (targetAnimator != null && entity.IsAlive) // 当（目标 Animator不为空；且当前实体存活状态为 true）时，执行下方分支。
                    targetAnimator.updateMode = regularUpdateMode; // 更新 targetAnimator.updateMode，使用 Animator 原始更新模式。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 测量角色/Agent 速度，写入 Speed、Crouched、Grounded 和 Dead 参数。
        /// </summary>
        private void LateUpdate() // 测量角色/Agent 速度，写入 Speed、Crouched、Grounded 和 Dead 参数。
        { // 开始方法。
            float dt = Time.deltaTime; // 把 当前帧缩放后的秒数 保存为 当前帧秒数。
            float speed = dt > 0 ? (transform.position - lastPosition).magnitude / dt : 0; // 把 按条件 dt > 0 选择的值 保存为 移动速度。
            lastPosition = transform.position; // 更新 上一帧位置，使用 本对象世界位置。
            if (!GameSession.SimulationEnabled) // 当当前不推进游戏模拟时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (character != null) // 当角色对象不为空时，执行下方分支。
                speed = new Vector3(character.velocity.x, 0, character.velocity.z).magnitude; // 更新 移动速度，使用 new Vector3(character.velocity.x, 0, character.velocity.z).magnitude。
            else if (agent != null && agent.enabled && agent.isOnNavMesh) // 前面的条件不成立时，执行此备用分支。
                speed = agent.velocity.magnitude; // 更新 移动速度，使用 agent.velocity.magnitude。
            Driver.Float(settings != null ? settings.speedParameter : "Speed", speed); // 设置浮点动画参数。
            Driver.Bool(settings != null ? settings.crouchParameter : "Crouched", player != null && player.IsCrouched); // 设置布尔动画参数。
            Driver.Bool(settings != null ? settings.groundedParameter : "Grounded", character == null || character.isGrounded); // 设置布尔动画参数。
            Driver.Bool(settings != null ? settings.deadParameter : "Dead", !entity.IsAlive); // 设置布尔动画参数。
        } // 结束方法。

        /// <summary>
        /// 按动作映射触发器/状态；死亡设 Dead 并改用非缩放时间，使 GameOver 后死亡动画继续。
        /// </summary>
        /// <param name="state">目标状态或保存的状态数据。</param>
        public void Play(EntityAnimationState state) // 按动作映射触发器/状态；死亡设 Dead 并改用非缩放时间，使 GameOver 后死亡动画继续。
        { // 开始方法。
            string trigger = state == EntityAnimationState.Attack ? (settings != null ? settings.attackTrigger : "Attack") : state == EntityAnimationState.Hit ? (settings != null ? settings.hitTrigger : "Hit") : state == EntityAnimationState.Interact ? (settings != null ? settings.interactTrigger : "Interact") : null; // 把 按条件 state == EntityAnimationState.Attack 选择的值 保存为 trigger。
            if (trigger != null) // 当trigger不为空时，执行下方分支。
                Driver.Trigger(trigger); // 触发动画参数。
            if (state == EntityAnimationState.Dead) // 当目标状态或保存的状态数据等于EntityAnimationState.Dead时，执行下方分支。
            { // 开始条件分支。
                if (targetAnimator != null) // 当目标 Animator不为空时，执行下方分支。
                    targetAnimator.updateMode = AnimatorUpdateMode.UnscaledTime; // 更新 targetAnimator.updateMode，使用 AnimatorUpdateMode.UnscaledTime。
                Driver.Bool(settings != null ? settings.deadParameter : "Dead", true); // 设置布尔动画参数。
            } // 结束条件分支。

            // 移动状态过渡由 Mecanim 控制器管理；显式动作播放同时支持 Legacy 片段。
            if (targetAnimator == null || trigger == null && state != EntityAnimationState.Dead) // 当（目标 Animator为空；或（trigger为空；且目标状态或保存的状态数据不等于EntityAnimationState.Dead））时，执行下方分支。
                Driver.Play(settings != null ? settings.StateName(state) : state.ToString(), settings != null ? settings.crossFade : 0.15f); // 播放指定动作状态。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
