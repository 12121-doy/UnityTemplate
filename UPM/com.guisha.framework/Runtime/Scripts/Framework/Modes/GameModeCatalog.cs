using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 按玩法枚举查找规则配置，缺少配置时回退默认规则。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Game Mode Catalog")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class GameModeCatalog : ScriptableObject // 按玩法枚举查找规则配置，缺少配置时回退默认规则。
    { // 开始类型。
        /// <summary>
        /// 保存玩法配置列表。
        /// </summary>
        public GameModeDefinition[] definitions = Array.Empty<GameModeDefinition>(); // 把 Array.Empty<GameModeDefinition> 的返回值 保存为 玩法配置列表。
        /// <summary>
        /// 返回匹配配置的不可变规则，找不到时使用默认规则。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        /// <returns>Guisha.Framework.GameModeRules 类型的处理结果。</returns>
        public GameModeRules Resolve(GameMode mode) // 返回匹配配置的不可变规则，找不到时使用默认规则。
        { // 开始方法。
            foreach (var value in definitions) // 遍历 玩法配置列表，每次把一个元素保存为 value。
                if (value != null && value.mode == mode) // 当（当前处理值不为空；且value.mode等于当前玩法）时，执行下方分支。
                    return new GameModeRules(mode, value.playerDamage, value.sanDrain, value.staminaCost, value.enemySpeed, value.hostileGhosts, value.allowSave, value.timeLimit); // 返回 新建的 GameModeRules 对象。
            return GameModeRules.Default(mode); // 返回剧情、探索、限时挑战或无尽生存的默认规则，并返回调用结果。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
