using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 交互后保存检查点。
    /// </summary>
    public sealed class Checkpoint : MonoBehaviour, IInteractable // 交互后保存检查点。
    { // 开始类型。
        /// <summary>
        /// 保存保存的检查点标识。
        /// </summary>
        public string checkpointId; // 声明 保存的检查点标识，后续初始化。
        /// <summary>
        /// 保存检查点重生 Transform。
        /// </summary>
        public Transform respawnPoint; // 声明 检查点重生 Transform，后续初始化。
        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => "保存检查点"; // 获取交互提示文本。

        /// <summary>
        /// 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => true; // 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// <summary>
        /// 选择重生点或玩家位置，发布 CheckpointReached。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 选择重生点或玩家位置，发布 CheckpointReached。
        { // 开始方法。
            var point = respawnPoint != null ? respawnPoint : actor.transform; // 把 按条件 respawnPoint != null 选择的值 保存为 待检测位置。
            EventBus.Publish(new CheckpointReached(checkpointId, point.position, point.rotation)); // 广播 CheckpointReached 事件，更新检查点并触发保存。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
