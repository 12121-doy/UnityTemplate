using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 暂停继续、保存、读取和主音量入口。
    /// </summary>
    public sealed class MenuPanel : UIPanelBase // 暂停继续、保存、读取和主音量入口。
    { // 开始类型。
        /// <summary>
        /// 关闭本面板并请求继续游戏。
        /// </summary>
        public void Resume() // 关闭本面板并请求继续游戏。
        { // 开始方法。
            EventBus.Publish(new PanelRequested(Id, false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            EventBus.Publish(new PauseRequested(false)); // 广播 PauseRequested 事件，请求切换暂停流程。
        } // 结束方法。

        /// <summary>
        /// 发送保存请求。
        /// </summary>
        public void Save() // 发送保存请求。
        { // 开始方法。
            EventBus.Publish(new SaveRequested()); // 广播 SaveRequested 事件，请求 GameManager 保存。
        } // 结束方法。

        /// <summary>
        /// 发送读取请求。
        /// </summary>
        public void Load() // 发送读取请求。
        { // 开始方法。
            EventBus.Publish(new LoadRequested()); // 广播 LoadRequested 事件，请求 GameManager 读档。
        } // 结束方法。

        /// <summary>
        /// 把 AudioListener.volume 设为钳制后的 0-1 数值。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        public void SetMasterVolume(float value) // 把 AudioListener.volume 设为钳制后的 0-1 数值。
        { // 开始方法。
            UnityEngine.AudioListener.volume = UnityEngine.Mathf.Clamp01(value); // 更新 UnityEngine.AudioListener.volume，使用 UnityEngine.Mathf.Clamp01 的返回值。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
