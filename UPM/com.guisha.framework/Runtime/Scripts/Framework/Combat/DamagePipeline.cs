using Guisha.Core.Patterns; // 引入 Guisha.Core.Patterns 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    // 可选的实体本地组件；伤害链不依赖全局可变注册表或模块注册顺序。
    /// <summary>
    /// 可选的实体本地伤害链，依次处理有效性/无敌、固定减伤和倍率。
    /// </summary>
    public sealed class DamagePipeline : MonoBehaviour // 可选的实体本地伤害链，依次处理有效性/无敌、固定减伤和倍率。
    { // 开始类型。
        /// <summary>
        /// 保存是否无敌。
        /// </summary>
        public bool invulnerable; // 声明 是否无敌，后续初始化。
        /// <summary>
        /// 保存固定伤害减免。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float flatReduction; // 声明 固定伤害减免，后续初始化。
        /// <summary>
        /// 保存伤害倍率。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float multiplier = 1; // 把 1 保存为 伤害倍率。
        /// <summary>
        /// 伤害链中的临时伤害数值上下文。
        /// </summary>
        private sealed class Context // 伤害链中的临时伤害数值上下文。
        { // 开始类型。
            /// <summary>
            /// 保存伤害数值。
            /// </summary>
            public float Amount; // 声明 伤害数值，后续初始化。
        } // 结束类型。

        /// <summary>
        /// 保存顺序处理链。
        /// </summary>
        private RuleChain<Context> chain; // 声明 顺序处理链，后续初始化。
        /// <summary>
        /// 初始化有效性/无敌、固定减伤和倍率三条本地伤害规则。
        /// </summary>
        private void Awake() // 初始化有效性/无敌、固定减伤和倍率三条本地伤害规则。
        { // 开始方法。
            chain = new RuleChain<Context>(new DelegateRule<Context>(c => !invulnerable && !float.IsNaN(c.Amount) && !float.IsInfinity(c.Amount) && c.Amount > 0), new DelegateRule<Context>(c => // 更新 顺序处理链，使用 新建的 RuleChain<Context> 对象。
            { // 开始回调。
                c.Amount = Mathf.Max(0, c.Amount - flatReduction); // 更新 c.Amount，使用 Mathf.Max 的返回值。
                return c.Amount > 0; // 返回 c.Amount > 0。
            }), new DelegateRule<Context>(c => // 结束回调。
            { // 开始回调。
                c.Amount *= multiplier; // 按倍率更新 c.Amount，使用 伤害倍率。
                return !float.IsNaN(c.Amount) && !float.IsInfinity(c.Amount) && c.Amount > 0; // 返回 !float.IsNaN(c.Amount) && !float.IsInfinity(c.Amount) && c.Amount > 0。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 创建本次伤害上下文，经过免伤/有效性、固定减伤、倍率三条规则；不通过时返回 0。
        /// </summary>
        /// <param name="amount">本次伤害量。</param>
        /// <returns>float 类型的处理结果。</returns>
        public float Resolve(float amount) // 创建本次伤害上下文，经过免伤/有效性、固定减伤、倍率三条规则；不通过时返回 0。
        { // 开始方法。
            if (chain == null) // 当顺序处理链为空时，执行下方分支。
                Awake(); // Unity 初始化回调，准备本组件依赖和内部数据；常驻管理器先调用单例基类并检查主实例。
            var context = new Context // 创建 Context，保存为 当前操作上下文。
            { // 开始成员/集合初始化。
                Amount = amount // 更新 伤害数值，使用 本次伤害量。
            }; // 结束成员/集合初始化。
            return chain.Apply(context) ? context.Amount : 0; // 返回 按条件 chain.Apply(context) 选择的值。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
