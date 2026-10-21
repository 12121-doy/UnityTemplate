using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 阅读线索并设置世界标记。
    /// </summary>
    public sealed class ClueNote : MonoBehaviour, IInteractable // 阅读线索并设置世界标记。
    { // 开始类型。
        /// <summary>
        /// 保存线索持久标识。
        /// </summary>
        public string clueId; // 声明 线索持久标识，后续初始化。
        /// <summary>
        /// 保存正文或待解析字符串。
        /// </summary>
        [TextArea] // 让该文本字段在 Inspector 中显示为多行编辑区域。
        public string text; // 声明 正文或待解析字符串，后续初始化。
        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => "阅读纸条"; // 获取交互提示文本。

        /// <summary>
        /// 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => true; // 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// <summary>
        /// 设置线索标记，广播 ClueRead 并显示文本通知。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 设置线索标记，广播 ClueRead 并显示文本通知。
        { // 开始方法。
            WorldState.Set(clueId); // 首次加入标记时广播 WorldStateChanged。
            EventBus.Publish(new ClueRead(clueId, text)); // 广播 ClueRead 事件，让线索订阅者处理已读内容。
            EventBus.Publish(new Notification(text)); // 广播 Notification 事件，让 HUD 显示短通知。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
