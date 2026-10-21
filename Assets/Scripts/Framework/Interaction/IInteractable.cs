using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 统一交互提示、交互条件与交互操作。
    /// </summary>
    public interface IInteractable // 统一交互提示、交互条件与交互操作。
    { // 开始接口。
        /// <summary>
        /// 交互目标在 HUD 上显示的提示文本。
        /// </summary>
        string Prompt { get; } // 交互目标在 HUD 上显示的提示文本。

        /// <summary>
        /// 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        bool CanInteract(GameObject actor); // 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// <summary>
        /// 执行交互契约。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        void Interact(GameObject actor); // 执行交互契约。
    } // 结束接口。
} // 结束命名空间。
