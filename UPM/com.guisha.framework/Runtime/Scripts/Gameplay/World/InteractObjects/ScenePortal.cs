using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 通过事件请求进入附加场景。
    /// </summary>
    public sealed class ScenePortal : MonoBehaviour, IInteractable // 通过事件请求进入附加场景。
    { // 开始类型。
        /// <summary>
        /// 保存场景的完整资源路径。
        /// </summary>
        public string scenePath; // 声明 场景的完整资源路径，后续初始化。
        /// <summary>
        /// 保存目标副本编号。
        /// </summary>
        public int dungeonId; // 声明 目标副本编号，后续初始化。
        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => "进入血门"; // 获取交互提示文本。

        /// <summary>
        /// 仅 Playing 状态允许进入。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => GameSession.State == GameState.Playing; // 仅 Playing 状态允许进入。
        /// <summary>
        /// 发布包含目标场景路径和副本 ID 的 SceneTransitionRequested。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 发布包含目标场景路径和副本 ID 的 SceneTransitionRequested。
        { // 开始方法。
            if (CanInteract(actor)) // 当条件 CanInteract(actor) 成立时，执行下方分支。
                EventBus.Publish(new SceneTransitionRequested(scenePath, dungeonId)); // 广播 SceneTransitionRequested 事件，交给场景管理器执行附加加载/切换。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
