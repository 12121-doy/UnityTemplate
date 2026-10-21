using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 鬼怪实体，固定 Type 为 Ghost。
    /// </summary>
    public sealed class GhostEntity : BaseEntity // 鬼怪实体，固定 Type 为 Ghost。
    { // 开始类型。
        /// <summary>
        /// 获取实体类别。
        /// </summary>
        public override EntityType Type => EntityType.Ghost; // 获取实体类别。
    } // 结束类型。
} // 结束命名空间。
