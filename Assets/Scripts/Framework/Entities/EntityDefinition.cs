using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 保存实体 ID、类别、Addressables 地址与战斗/移动参数。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Entity Definition")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class EntityDefinition : ScriptableObject // 保存实体 ID、类别、Addressables 地址与战斗/移动参数。
    { // 开始类型。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public EntityId id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 保存类型标识。
        /// </summary>
        public EntityType type; // 声明 类型标识，后续初始化。
        /// <summary>
        /// 保存Addressables 预制体地址。
        /// </summary>
        public string prefabAddress; // 声明 Addressables 预制体地址，后续初始化。
        /// <summary>
        /// 保存生命上限。
        /// </summary>
        [Min(1)] // 约束 Inspector 中可编辑的数值范围。
        public float maxHealth = 100; // 把 100 保存为 生命上限。
        /// <summary>
        /// 保存实体基础移动速度。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float moveSpeed = 3; // 把 3 保存为 实体基础移动速度。
        /// <summary>
        /// 保存攻击伤害。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float attackDamage = 10; // 把 10 保存为 攻击伤害。
    } // 结束类型。
} // 结束命名空间。
