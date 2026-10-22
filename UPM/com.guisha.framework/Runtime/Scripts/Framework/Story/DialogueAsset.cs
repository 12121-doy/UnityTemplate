using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 一条对白选项及其跳转节点。
    /// </summary>
    [Serializable] // 允许 Unity/序列化系统保存该数据类型的字段。
    public sealed class DialogueChoice // 一条对白选项及其跳转节点。
    { // 开始类型。
        /// <summary>
        /// 保存正文或待解析字符串。
        /// </summary>
        public string text; // 声明 正文或待解析字符串，后续初始化。
        /// <summary>
        /// 保存选项跳转的对白节点索引。
        /// </summary>
        public int next = -1; // 把 -1 保存为 选项跳转的对白节点索引。
    } // 结束类型。

    /// <summary>
    /// 对白角色、文本、下一节点与选项。
    /// </summary>
    [Serializable] // 允许 Unity/序列化系统保存该数据类型的字段。
    public sealed class DialogueNode // 对白角色、文本、下一节点与选项。
    { // 开始类型。
        /// <summary>
        /// 保存说话角色名。
        /// </summary>
        public string speaker; // 声明 说话角色名，后续初始化。
        /// <summary>
        /// 保存正文或待解析字符串。
        /// </summary>
        [TextArea] // 让该文本字段在 Inspector 中显示为多行编辑区域。
        public string text; // 声明 正文或待解析字符串，后续初始化。
        /// <summary>
        /// 保存下一对白节点索引。
        /// </summary>
        public int next = -1; // 把 -1 保存为 下一对白节点索引。
        /// <summary>
        /// 保存该节点的分支选项。
        /// </summary>
        public DialogueChoice[] choices = Array.Empty<DialogueChoice>(); // 把 Array.Empty<DialogueChoice> 的返回值 保存为 该节点的分支选项。
    } // 结束类型。

    /// <summary>
    /// 对白节点数组资产。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Dialogue")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class DialogueAsset : ScriptableObject // 对白节点数组资产。
    { // 开始类型。
        /// <summary>
        /// 保存对白节点数组。
        /// </summary>
        public DialogueNode[] nodes = Array.Empty<DialogueNode>(); // 把 Array.Empty<DialogueNode> 的返回值 保存为 对白节点数组。
    } // 结束类型。
} // 结束命名空间。
