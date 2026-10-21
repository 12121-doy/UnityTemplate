using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 选择四种玩法并关闭模式面板。
    /// </summary>
    public sealed class GameModePanel : UIPanelBase // 选择四种玩法并关闭模式面板。
    { // 开始类型。
        /// <summary>
        /// 请求玩法切换并关闭模式面板。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        public void Select(int mode) // 请求玩法切换并关闭模式面板。
        { // 开始方法。
            EventBus.Publish(new GameModeRequested((GameMode)mode)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            EventBus.Publish(new PanelRequested(Id, false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
        } // 结束方法。

        /// <summary>
        /// 关闭模式面板。
        /// </summary>
        public void Back() // 关闭模式面板。
        { // 开始方法。
            EventBus.Publish(new PanelRequested(Id, false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
