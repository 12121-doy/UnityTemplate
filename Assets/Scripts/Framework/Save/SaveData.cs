using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 单机存档数据：场景、检查点、角色属性、物品、世界标记与玩法进度。
    /// </summary>
    [Serializable] // 允许 Unity/序列化系统保存该数据类型的字段。
    public sealed class SaveData // 单机存档数据：场景、检查点、角色属性、物品、世界标记与玩法进度。
    { // 开始类型。
        /// <summary>
        /// 保存存档格式版本。
        /// </summary>
        public int version = 1; // 把 1 保存为 存档格式版本。
        /// <summary>
        /// 保存场景的完整资源路径。
        /// </summary>
        public string scenePath = ""; // 把 "" 保存为 场景的完整资源路径。
        /// <summary>
        /// 保存当前副本编号。
        /// </summary>
        public int dungeon; // 声明 当前副本编号，后续初始化。
        /// <summary>
        /// 保存保存的检查点标识。
        /// </summary>
        public string checkpointId = ""; // 把 "" 保存为 保存的检查点标识。
        /// <summary>
        /// 保存世界位置。
        /// </summary>
        public Vector3 position; // 声明 世界位置，后续初始化。
        /// <summary>
        /// 保存世界旋转。
        /// </summary>
        public Quaternion rotation = Quaternion.identity; // 把 Quaternion.identity 保存为 世界旋转。
        /// <summary>
        /// 保存保存的生命值、理智值或消耗倍率、体力值或消耗倍率。
        /// </summary>
        public float health = 100, san = 100, stamina = 100; // 把 100 保存为 保存的生命值；把 100 保存为 理智值或消耗倍率；把 100 保存为 体力值或消耗倍率。
        /// <summary>
        /// 保存背包组件。
        /// </summary>
        public string[] inventory = Array.Empty<string>(); // 把 Array.Empty<string> 的返回值 保存为 背包组件。
        /// <summary>
        /// 保存世界持久标记快照。
        /// </summary>
        public string[] worldFlags = Array.Empty<string>(); // 把 Array.Empty<string> 的返回值 保存为 世界持久标记快照。
        /// <summary>
        /// 保存存档是否含玩家数据。
        /// </summary>
        public bool hasPlayer; // 声明 存档是否含玩家数据，后续初始化。
        /// <summary>
        /// 保存保存的玩法枚举。
        /// </summary>
        public GameMode gameMode = GameMode.Story; // 把 GameMode.Story 保存为 保存的玩法枚举。
        /// <summary>
        /// 保存保存的模式计时。
        /// </summary>
        public float modeElapsed; // 声明 保存的模式计时，后续初始化。
        /// <summary>
        /// 保存保存的生存波次。
        /// </summary>
        public int survivalWave; // 声明 保存的生存波次，后续初始化。
    } // 结束类型。
} // 结束命名空间。
