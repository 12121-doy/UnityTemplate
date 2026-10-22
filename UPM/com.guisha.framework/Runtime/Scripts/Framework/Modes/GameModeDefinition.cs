using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 枚举定义，可选值与实际源码一致。
    /// </summary>
    public enum GameMode // 枚举定义，可选值与实际源码一致。
    { // 开始枚举。
        /// <summary>
        /// 剧情模式，允许保存并降低生存压力。
        /// </summary>
        Story, // 剧情模式，允许保存并降低生存压力。
        /// <summary>
        /// 探索模式，免伤且关闭鬼怪敌对行为。
        /// </summary>
        Exploration, // 探索模式，免伤且关闭鬼怪敌对行为。
        /// <summary>
        /// 限时挑战：在时限内激活目标机关，禁用保存。
        /// </summary>
        Challenge, // 限时挑战：在时限内激活目标机关，禁用保存。
        /// <summary>
        /// 无尽生存：逐波生成增援鬼怪，禁用保存。
        /// </summary>
        Endless // 无尽生存：逐波生成增援鬼怪，禁用保存。
    } // 结束枚举。

    /// <summary>
    /// 保存一种玩法的倍率、敌对性、存档权限和挑战目标。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Game Mode Definition")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class GameModeDefinition : ScriptableObject // 保存一种玩法的倍率、敌对性、存档权限和挑战目标。
    { // 开始类型。
        /// <summary>
        /// 保存当前玩法。
        /// </summary>
        public GameMode mode; // 声明 当前玩法，后续初始化。
        /// <summary>
        /// 保存玩家承伤倍率、理智消耗倍率、体力消耗倍率、鬼怪速度倍率。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float playerDamage = 1, sanDrain = 1, staminaCost = 1, enemySpeed = 1; // 把 1 保存为 玩家承伤倍率；把 1 保存为 理智消耗倍率；把 1 保存为 体力消耗倍率；把 1 保存为 鬼怪速度倍率。
        /// <summary>
        /// 保存鬼怪是否敌对、是否允许保存。
        /// </summary>
        public bool hostileGhosts = true, allowSave = true; // 把 true（启用/满足） 保存为 鬼怪是否敌对；把 true（启用/满足） 保存为 是否允许保存。
        /// <summary>
        /// 保存挑战秒数上限。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float timeLimit; // 声明 挑战秒数上限，后续初始化。
        /// <summary>
        /// 保存挑战目标机关标识。
        /// </summary>
        [Tooltip("留空时，副本内成功启动任意机关即完成限时挑战。")] // 为 Inspector 配置字段提供悬停说明。
        public string objectiveMechanismId; // 声明 挑战目标机关标识，后续初始化。
    } // 结束类型。

    /// <summary>
    /// 不可变的玩法规则快照。
    /// </summary>
    public readonly struct GameModeRules // 不可变的玩法规则快照。
    { // 开始数据结构。
        /// <summary>
        /// 保存玩法模式。
        /// </summary>
        public readonly GameMode Mode; // 声明 玩法模式，后续初始化。
        /// <summary>
        /// 保存玩家承伤倍率、理智消耗倍率、体力消耗倍率、鬼怪速度倍率、挑战时限。
        /// </summary>
        public readonly float PlayerDamage, SanDrain, StaminaCost, EnemySpeed, TimeLimit; // 声明 玩家承伤倍率，后续初始化；声明 理智消耗倍率，后续初始化；声明 体力消耗倍率，后续初始化；声明 鬼怪速度倍率，后续初始化；声明 挑战时限，后续初始化。
        /// <summary>
        /// 保存鬼怪是否敌对、是否允许保存。
        /// </summary>
        public readonly bool HostileGhosts, AllowSave; // 声明 鬼怪是否敌对，后续初始化；声明 是否允许保存，后续初始化。
        /// <summary>
        /// 创建 GameModeRules 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        /// <param name="damage">玩家承伤倍率。</param>
        /// <param name="san">理智消耗倍率。</param>
        /// <param name="stamina">体力消耗倍率。</param>
        /// <param name="speed">鬼怪移动速度倍率。</param>
        /// <param name="hostile">鬼怪是否敌对。</param>
        /// <param name="save">当前模式是否允许磁盘保存。</param>
        /// <param name="limit">挑战计时上限，单位为秒。</param>
        public GameModeRules(GameMode mode, float damage, float san, float stamina, float speed, bool hostile, bool save, float limit = 0) // 构造 GameModeRules，初始化其依赖与状态。
        { // 开始构造函数。
            Mode = mode; // 更新 玩法模式，使用 当前玩法。
            PlayerDamage = Mathf.Max(0, damage); // 更新 玩家承伤倍率，使用 Mathf.Max 的返回值。
            SanDrain = Mathf.Max(0, san); // 更新 理智消耗倍率，使用 Mathf.Max 的返回值。
            StaminaCost = Mathf.Max(0, stamina); // 更新 体力消耗倍率，使用 Mathf.Max 的返回值。
            EnemySpeed = Mathf.Max(0, speed); // 更新 鬼怪速度倍率，使用 Mathf.Max 的返回值。
            HostileGhosts = hostile; // 更新 鬼怪是否敌对，使用 鬼怪是否敌对。
            AllowSave = save; // 更新 是否允许保存，使用 是否允许保存或存档数据。
            TimeLimit = Mathf.Max(0, limit); // 更新 挑战时限，使用 Mathf.Max 的返回值。
        } // 结束构造函数。

        /// <summary>
        /// 返回剧情、探索、限时挑战或无尽生存的默认规则。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        /// <returns>Guisha.Framework.GameModeRules 类型的处理结果。</returns>
        public static GameModeRules Default(GameMode mode) // 返回剧情、探索、限时挑战或无尽生存的默认规则。
        { // 开始方法。
            switch (mode) // 根据 mode 选择匹配分支。
            { // 开始多分支选择。
                case GameMode.Exploration: // 匹配此 case/default 时返回或执行对应分支。
                    return new GameModeRules(mode, 0, 0, 0, 1, false, true); // 返回 新建的 GameModeRules 对象。
                case GameMode.Challenge: // 匹配此 case/default 时返回或执行对应分支。
                    return new GameModeRules(mode, 1.25f, 1.5f, 1.2f, 1.15f, true, false, 180); // 返回 新建的 GameModeRules 对象。
                case GameMode.Endless: // 匹配此 case/default 时返回或执行对应分支。
                    return new GameModeRules(mode, 1, 1, 1, 1, true, false); // 返回 新建的 GameModeRules 对象。
                default: // 匹配此 case/default 时返回或执行对应分支。
                    return new GameModeRules(GameMode.Story, 0.75f, 0.7f, 0.8f, 0.9f, true, true); // 返回 新建的 GameModeRules 对象。
            } // 结束多分支选择。
        } // 结束方法。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct GameModeRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存玩法模式。
        /// </summary>
        public readonly GameMode Mode; // 声明 玩法模式，后续初始化。
        /// <summary>
        /// 创建 GameModeRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        public GameModeRequested(GameMode mode) // 构造 GameModeRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Mode = mode; // 更新 玩法模式，使用 当前玩法。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct GameModeChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存玩法规则。
        /// </summary>
        public readonly GameModeRules Rules; // 声明 玩法规则，后续初始化。
        /// <summary>
        /// 创建 GameModeChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="rules">规则序列或规则快照。</param>
        public GameModeChanged(GameModeRules rules) // 构造 GameModeChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Rules = rules; // 更新 玩法规则，使用 规则序列或规则快照。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct GameModeProgress // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存已经过的秒数、剩余秒数。
        /// </summary>
        public readonly float Elapsed, Remaining; // 声明 已经过的秒数，后续初始化；声明 剩余秒数，后续初始化。
        /// <summary>
        /// 保存生存波次。
        /// </summary>
        public readonly int Wave; // 声明 生存波次，后续初始化。
        /// <summary>
        /// 创建 GameModeProgress 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="elapsed">已经过的秒数。</param>
        /// <param name="remaining">剩余秒数。</param>
        /// <param name="wave">当前生存波次。</param>
        public GameModeProgress(float elapsed, float remaining, int wave) // 构造 GameModeProgress，初始化其依赖与状态。
        { // 开始构造函数。
            Elapsed = elapsed; // 更新 已经过的秒数，使用 已经过的秒数。
            Remaining = remaining; // 更新 剩余秒数，使用 剩余秒数。
            Wave = wave; // 更新 生存波次，使用 当前生存波次。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SurvivalWaveStarted // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存生存波次。
        /// </summary>
        public readonly int Wave; // 声明 生存波次，后续初始化。
        /// <summary>
        /// 创建 SurvivalWaveStarted 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="wave">当前生存波次。</param>
        public SurvivalWaveStarted(int wave) // 构造 SurvivalWaveStarted，初始化其依赖与状态。
        { // 开始构造函数。
            Wave = wave; // 更新 生存波次，使用 当前生存波次。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct ModeRoundEnded // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存结束原因。
        /// </summary>
        public readonly string Reason; // 声明 结束原因，后续初始化。
        /// <summary>
        /// 创建 ModeRoundEnded 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="reason">模式结束原因。</param>
        public ModeRoundEnded(string reason) // 构造 ModeRoundEnded，初始化其依赖与状态。
        { // 开始构造函数。
            Reason = reason; // 更新 结束原因，使用 模式结束原因。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct ChallengeCompleted // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存秒数。
        /// </summary>
        public readonly float Seconds; // 声明 秒数，后续初始化。
        /// <summary>
        /// 保存机关标识。
        /// </summary>
        public readonly string MechanismId; // 声明 机关标识，后续初始化。
        /// <summary>
        /// 创建 ChallengeCompleted 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="seconds">延时秒数。</param>
        /// <param name="mechanismId">机关持久标识。</param>
        public ChallengeCompleted(float seconds, string mechanismId) // 构造 ChallengeCompleted，初始化其依赖与状态。
        { // 开始构造函数。
            Seconds = seconds; // 更新 秒数，使用 延时秒数。
            MechanismId = mechanismId; // 更新 机关标识，使用 机关持久标识。
        } // 结束构造函数。
    } // 结束数据结构。
} // 结束命名空间。
