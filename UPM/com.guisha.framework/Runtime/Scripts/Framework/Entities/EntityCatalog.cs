using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 以 EntityDefinition 数组保存实体配置表。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Entity Catalog")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class EntityCatalog : ScriptableObject // 以 EntityDefinition 数组保存实体配置表。
    { // 开始类型。
        /// <summary>
        /// 保存实体配置列表。
        /// </summary>
        public EntityDefinition[] definitions = Array.Empty<EntityDefinition>(); // 把 Array.Empty<EntityDefinition> 的返回值 保存为 实体配置列表。
    } // 结束类型。
} // 结束命名空间。
