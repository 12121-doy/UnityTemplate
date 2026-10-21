using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 物品资产基类，派生类实现 Use。
    /// </summary>
    public abstract class Item : ScriptableObject // 物品资产基类，派生类实现 Use。
    { // 开始类型。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public string id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 保存物品显示名称。
        /// </summary>
        public string displayName; // 声明 物品显示名称，后续初始化。
        /// <summary>
        /// 保存物品图标。
        /// </summary>
        public Sprite icon; // 声明 物品图标，后续初始化。
        /// <summary>
        /// 保存成功使用后是否消耗。
        /// </summary>
        public bool consumable = true; // 把 true（启用/满足） 保存为 成功使用后是否消耗。
        /// <summary>
        /// 执行物品使用逻辑，返回是否使用成功。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示使用成功；背包仅在成功且为消耗品时移除物品。</returns>
        public abstract bool Use(GameObject actor); // 执行物品使用逻辑，返回是否使用成功。
    } // 结束类型。
} // 结束命名空间。
