using System; // 引入 System 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 读取当前规则、计时、波次及生存倍率。
    /// </summary>
    public static class GameModeRuntime // 读取当前规则、计时、波次及生存倍率。
    { // 开始类型。
        /// <summary>
        /// 获取玩法规则。
        /// </summary>
        public static GameModeRules Rules { get; private set; } = GameModeRules.Default(GameMode.Story); // 获取玩法规则。
        /// <summary>
        /// 获取已经过的秒数。
        /// </summary>
        public static float Elapsed { get; private set; } // 获取已经过的秒数。
        /// <summary>
        /// 获取生存波次。
        /// </summary>
        public static int Wave { get; private set; } // 获取生存波次。
        /// <summary>
        /// 获取剩余秒数。
        /// </summary>
        public static float Remaining => Mathf.Max(0, Rules.TimeLimit - Elapsed); // 获取剩余秒数。
        /// <summary>
        /// 读取规则速度倍率；无尽模式额外叠加有上限的波次增幅。
        /// </summary>
        public static float EnemySpeed => Rules.EnemySpeed * (Rules.Mode == GameMode.Endless ? 1 + Mathf.Min(0.75f, Wave * 0.05f) : 1); // 读取规则速度倍率；无尽模式额外叠加有上限的波次增幅。
        /// <summary>
        /// 读取鬼怪伤害倍率；无尽模式按波次提升并限制增幅。
        /// </summary>
        public static float EnemyDamage => Rules.Mode == GameMode.Endless ? 1 + Mathf.Min(2, Wave * 0.1f) : 1; // 读取鬼怪伤害倍率；无尽模式按波次提升并限制增幅。

        /// <summary>
        /// 把模式枚举转换为中文名称。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        /// <returns>string 类型的处理结果。</returns>
        public static string Label(GameMode mode) // 把模式枚举转换为中文名称。
        { // 开始方法。
            switch (mode) // 根据 mode 选择匹配分支。
            { // 开始多分支选择。
                case GameMode.Exploration: // 匹配此 case/default 时返回或执行对应分支。
                    return "自由探索"; // 返回 "自由探索"。
                case GameMode.Challenge: // 匹配此 case/default 时返回或执行对应分支。
                    return "限时挑战"; // 返回 "限时挑战"。
                case GameMode.Endless: // 匹配此 case/default 时返回或执行对应分支。
                    return "无尽生存"; // 返回 "无尽生存"。
                default: // 匹配此 case/default 时返回或执行对应分支。
                    return "剧情模式"; // 返回 "剧情模式"。
            } // 结束多分支选择。
        } // 结束方法。

        /// <summary>
        /// 替换规则，清空计时/波次并广播 GameModeChanged。
        /// </summary>
        /// <param name="rules">规则序列或规则快照。</param>
        public static void Apply(GameModeRules rules) // 替换规则，清空计时/波次并广播 GameModeChanged。
        { // 开始方法。
            Rules = rules; // 更新 玩法规则，使用 规则序列或规则快照。
            Elapsed = 0; // 更新 已经过的秒数，使用 0。
            Wave = 0; // 更新 生存波次，使用 0。
            EventBus.Publish(new GameModeChanged(rules)); // 广播 GameModeChanged 事件，让玩法参与者重置或应用规则。
        } // 结束方法。

        /// <summary>
        /// 写入非负计时和波次，非有限计时回退为 0。
        /// </summary>
        /// <param name="elapsed">已经过的秒数。</param>
        /// <param name="wave">当前生存波次。</param>
        public static void SetProgress(float elapsed, int wave) // 写入非负计时和波次，非有限计时回退为 0。
        { // 开始方法。
            Elapsed = float.IsNaN(elapsed) || float.IsInfinity(elapsed) ? 0 : Mathf.Max(0, elapsed); // 更新 已经过的秒数，使用 按条件 float.IsNaN(elapsed) || float.IsInfinity(elapsed) 选择的值。
            Wave = Math.Max(0, wave); // 更新 生存波次，使用 Math.Max 的返回值。
        } // 结束方法。

        /// <summary>
        /// SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 指定由 Unity 在配置的运行时初始化阶段自动调用。
        private static void Reset() // SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        { // 开始方法。
            Rules = GameModeRules.Default(GameMode.Story); // 更新 玩法规则，使用 GameModeRules.Default 的返回值。
            Elapsed = 0; // 更新 已经过的秒数，使用 0。
            Wave = 0; // 更新 生存波次，使用 0。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
