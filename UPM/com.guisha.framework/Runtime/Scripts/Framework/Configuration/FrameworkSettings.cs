using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 集中配置资源目录、常驻场景、日志、音频和默认玩法。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Framework Settings")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class FrameworkSettings : ScriptableObject // 集中配置资源目录、常驻场景、日志、音频和默认玩法。
    { // 开始类型。
        /// <summary>
        /// 保存音频遮挡层掩码。
        /// </summary>
        public LayerMask audioOcclusionMask = ~0; // 把 ~0 保存为 音频遮挡层掩码。
        /// <summary>
        /// 保存框架配置的并发声部数。
        /// </summary>
        [Min(1)] // 约束 Inspector 中可编辑的数值范围。
        public int audioMaxVoices = 32; // 把 32 保存为 框架配置的并发声部数。
        /// <summary>
        /// 保存框架配置的 BGM 淡变秒数。
        /// </summary>
        [Min(0.1f)] // 约束 Inspector 中可编辑的数值范围。
        public float bgmFadeSeconds = 1; // 把 1 保存为 框架配置的 BGM 淡变秒数。
        /// <summary>
        /// 保存日志总开关。
        /// </summary>
        public bool enableLogs = true; // 把 true（启用/满足） 保存为 日志总开关。
        /// <summary>
        /// 保存是否写本地日志文件。
        /// </summary>
        public bool writeLogFile; // 声明 是否写本地日志文件，后续初始化。
        /// <summary>
        /// 保存保留加载的主场景路径。
        /// </summary>
        public string persistentScenePath; // 声明 保留加载的主场景路径，后续初始化。
        /// <summary>
        /// 保存实体目录资产。
        /// </summary>
        public EntityCatalog entityCatalog; // 声明 实体目录资产，后续初始化。
        /// <summary>
        /// 保存默认玩法。
        /// </summary>
        public GameMode defaultGameMode = GameMode.Story; // 把 GameMode.Story 保存为 默认玩法。
        /// <summary>
        /// 保存玩法配置目录。
        /// </summary>
        public GameModeCatalog gameModes; // 声明 玩法配置目录，后续初始化。
    } // 结束类型。
} // 结束命名空间。
