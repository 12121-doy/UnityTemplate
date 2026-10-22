using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 枚举定义，可选值与实际源码一致。
    /// </summary>
    public enum EntityAnimationState // 枚举定义，可选值与实际源码一致。
    { // 开始枚举。
        /// <summary>
        /// 空闲待机动作。
        /// </summary>
        Idle, // 空闲待机动作。
        /// <summary>
        /// 步行动作。
        /// </summary>
        Walk, // 步行动作。
        /// <summary>
        /// 奔跑动作。
        /// </summary>
        Run, // 奔跑动作。
        /// <summary>
        /// 蹲伏动作。
        /// </summary>
        Crouch, // 蹲伏动作。
        /// <summary>
        /// 攻击动作。
        /// </summary>
        Attack, // 攻击动作。
        /// <summary>
        /// 受击动作。
        /// </summary>
        Hit, // 受击动作。
        /// <summary>
        /// 死亡动作。
        /// </summary>
        Dead, // 死亡动作。
        /// <summary>
        /// 交互动作。
        /// </summary>
        Interact // 交互动作。
    } // 结束枚举。

    /// <summary>
    /// 配置动画参数名、状态名与淡入淡出时间的资产。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Animation Settings")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class AnimationSettings : ScriptableObject // 配置动画参数名、状态名与淡入淡出时间的资产。
    { // 开始类型。
        /// <summary>
        /// 保存移动速度浮点参数名、蹲伏布尔参数名、接地布尔参数名、死亡布尔参数名。
        /// </summary>
        public string speedParameter = "Speed", crouchParameter = "Crouched", groundedParameter = "Grounded", deadParameter = "Dead"; // 把 "Speed" 保存为 移动速度浮点参数名；把 "Crouched" 保存为 蹲伏布尔参数名；把 "Grounded" 保存为 接地布尔参数名；把 "Dead" 保存为 死亡布尔参数名。
        /// <summary>
        /// 保存攻击触发参数名、受击触发参数名、交互触发参数名。
        /// </summary>
        public string attackTrigger = "Attack", hitTrigger = "Hit", interactTrigger = "Interact"; // 把 "Attack" 保存为 攻击触发参数名；把 "Hit" 保存为 受击触发参数名；把 "Interact" 保存为 交互触发参数名。
        /// <summary>
        /// 保存动画淡入淡出秒数。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float crossFade = 0.15f; // 把 0.15f 保存为 动画淡入淡出秒数。
        /// <summary>
        /// 保存奔跑速度。
        /// </summary>
        [Min(0.01f)] // 约束 Inspector 中可编辑的数值范围。
        public float runSpeed = 4; // 把 4 保存为 奔跑速度。
        /// <summary>
        /// 保存空闲动画状态名、步行动画状态名、奔跑动画状态名、蹲伏动画状态名、攻击动画状态名、受击动画状态名、死亡动画状态名、交互动画状态名。
        /// </summary>
        public string idleState = "Idle", walkState = "Walk", runState = "Run", crouchState = "Crouch", attackState = "Attack", hitState = "Hit", deadState = "Dead", interactState = "Interact"; // 把 "Idle" 保存为 空闲动画状态名；把 "Walk" 保存为 步行动画状态名；把 "Run" 保存为 奔跑动画状态名；把 "Crouch" 保存为 蹲伏动画状态名；把 "Attack" 保存为 攻击动画状态名；把 "Hit" 保存为 受击动画状态名；把 "Dead" 保存为 死亡动画状态名；把 "Interact" 保存为 交互动画状态名。
        /// <summary>
        /// 把动作枚举映射到配置的 Animator/Legacy 状态名。
        /// </summary>
        /// <param name="state">实体动作枚举。</param>
        /// <returns>string 类型的处理结果。</returns>
        public string StateName(EntityAnimationState state) // 把动作枚举映射到配置的 Animator/Legacy 状态名。
        { // 开始方法。
            switch (state) // 根据 state 选择匹配分支。
            { // 开始多分支选择。
                case EntityAnimationState.Walk: // 匹配此 case/default 时返回或执行对应分支。
                    return walkState; // 返回 步行动画状态名。
                case EntityAnimationState.Run: // 匹配此 case/default 时返回或执行对应分支。
                    return runState; // 返回 奔跑动画状态名。
                case EntityAnimationState.Crouch: // 匹配此 case/default 时返回或执行对应分支。
                    return crouchState; // 返回 蹲伏动画状态名。
                case EntityAnimationState.Attack: // 匹配此 case/default 时返回或执行对应分支。
                    return attackState; // 返回 攻击动画状态名。
                case EntityAnimationState.Hit: // 匹配此 case/default 时返回或执行对应分支。
                    return hitState; // 返回 受击动画状态名。
                case EntityAnimationState.Dead: // 匹配此 case/default 时返回或执行对应分支。
                    return deadState; // 返回 死亡动画状态名。
                case EntityAnimationState.Interact: // 匹配此 case/default 时返回或执行对应分支。
                    return interactState; // 返回 交互动画状态名。
                default: // 匹配此 case/default 时返回或执行对应分支。
                    return idleState; // 返回 空闲动画状态名。
            } // 结束多分支选择。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct EntityAnimationRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存事件对应的实体。
        /// </summary>
        public readonly BaseEntity Entity; // 声明 事件对应的实体，后续初始化。
        /// <summary>
        /// 保存当前状态。
        /// </summary>
        public readonly EntityAnimationState State; // 声明 当前状态，后续初始化。
        /// <summary>
        /// 创建 EntityAnimationRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="entity">当前实体组件。</param>
        /// <param name="state">目标状态或保存的状态数据。</param>
        public EntityAnimationRequested(BaseEntity entity, EntityAnimationState state) // 构造 EntityAnimationRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Entity = entity; // 更新 事件对应的实体，使用 当前实体组件。
            State = state; // 更新 当前状态，使用 目标状态或保存的状态数据。
        } // 结束构造函数。
    } // 结束数据结构。
} // 结束命名空间。
