using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 管理面板注册、栈顺序、模态交互和输入锁。
    /// </summary>
    public sealed class UIManager : MonoSingleton<UIManager> // 管理面板注册、栈顺序、模态交互和输入锁。
    { // 开始类型。
        /// <summary>
        /// 保存面板 ID 到面板的注册表。
        /// </summary>
        private readonly Dictionary<string, IPanel> panels = new Dictionary<string, IPanel>(); // 创建 Dictionary<string, IPanel>，保存为 面板 ID 到面板的注册表。
        /// <summary>
        /// 保存已经打开的面板栈。
        /// </summary>
        private readonly List<IPanel> stack = new List<IPanel>(); // 创建 List<IPanel>，保存为 已经打开的面板栈。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 先建立主单例，再订阅面板注册、打开/关闭与读档清栈事件。
        /// </summary>
        protected override void Awake() // 先建立主单例，再订阅面板注册、打开/关闭与读档清栈事件。
        { // 开始方法。
            base.Awake(); // 执行基类 Awake，保留基类初始化流程。
            if (Instance != this) // 当当前类型的主单例实例不等于this时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            subscriptions.Add(EventBus.Subscribe<PanelRegistered>(e => // 订阅 PanelRegistered 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (panels.ContainsKey(e.Panel.Id)) // 当条件 panels.ContainsKey(e.Panel.Id) 成立时，执行下方分支。
                { // 开始条件分支。
                    Guisha.Core.Logger.Warn("Duplicate panel: " + e.Panel.Id); // 按 WARN 级别输出警告，并按开关写文件。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                panels.Add(e.Panel.Id, e.Panel); // 向 面板 ID 到面板的注册表 加入 e.Panel.Id、e.Panel。
                e.Panel.Close(); // 隐藏面板。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<PanelUnregistered>(e => // 广播 InputLockChanged 事件，按拥有者增删输入锁。
            { // 开始回调。
                if (panels.TryGetValue(e.Panel.Id, out var current) && ReferenceEquals(current, e.Panel)) // 当（条件 panels.TryGetValue(e.Panel.Id, out var current) 成立；且条件 ReferenceEquals(current, e.Panel) 成立）时，执行下方分支。
                    panels.Remove(e.Panel.Id); // 从 面板 ID 到面板的注册表 移除 e.Panel.Id。
                stack.Remove(e.Panel); // 从 已经打开的面板栈 移除 e.Panel。
                EventBus.Publish(new InputLockChanged(e.Panel, false)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
                Refresh(); // 从上向下设置可交互状态，模态面板阻止其下方交互。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                foreach (var panel in stack.ToArray()) // 遍历 stack.ToArray()，每次把一个元素保存为 panel。
                    Close(panel.Id); // 从栈移除并关闭面板，释放对应锁并刷新栈。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<PanelRequested>(e => // 订阅 PanelRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Open) // 当e.Open为 true时，执行下方分支。
                    Open(e.Id); // 找到注册面板、置顶并打开，模态面板加输入锁，然后刷新栈交互。
                else // 前面的条件不成立时，执行此备用分支。
                    Close(e.Id); // 从栈移除并关闭面板，释放对应锁并刷新栈。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 找到注册面板、置顶并打开，模态面板加输入锁，然后刷新栈交互。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        public void Open(string id) // 找到注册面板、置顶并打开，模态面板加输入锁，然后刷新栈交互。
        { // 开始方法。
            if (!panels.TryGetValue(id, out var panel)) // 当条件 panels.TryGetValue(id, out var panel) 不成立时，执行下方分支。
            { // 开始条件分支。
                Guisha.Core.Logger.Warn("Panel not registered: " + id); // 按 WARN 级别输出警告，并按开关写文件。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            stack.Remove(panel); // 从 已经打开的面板栈 移除 panel。
            stack.Add(panel); // 向 已经打开的面板栈 加入 panel。
            panel.Open(); // 显示面板。
            if (panel.Modal) // 当panel.Modal为 true时，执行下方分支。
                EventBus.Publish(new InputLockChanged(panel, true)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
            Refresh(); // 从上向下设置可交互状态，模态面板阻止其下方交互。
        } // 结束方法。

        /// <summary>
        /// 从栈移除并关闭面板，释放对应锁并刷新栈。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        public void Close(string id) // 从栈移除并关闭面板，释放对应锁并刷新栈。
        { // 开始方法。
            if (!panels.TryGetValue(id, out var panel)) // 当条件 panels.TryGetValue(id, out var panel) 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            stack.Remove(panel); // 从 已经打开的面板栈 移除 panel。
            panel.Close(); // 隐藏面板。
            EventBus.Publish(new InputLockChanged(panel, false)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
            Refresh(); // 从上向下设置可交互状态，模态面板阻止其下方交互。
        } // 结束方法。

        /// <summary>
        /// 关闭当前栈顶面板。
        /// </summary>
        public void CloseTop() // 关闭当前栈顶面板。
        { // 开始方法。
            if (stack.Count > 0) // 当stack.Count大于0时，执行下方分支。
                Close(stack[stack.Count - 1].Id); // 从栈移除并关闭面板，释放对应锁并刷新栈。
        } // 结束方法。

        /// <summary>
        /// 从上向下设置可交互状态，模态面板阻止其下方交互。
        /// </summary>
        private void Refresh() // 从上向下设置可交互状态，模态面板阻止其下方交互。
        { // 开始方法。
            bool blocked = false; // 把 false（关闭/不满足） 保存为 是否存在阻挡。
            for (int i = stack.Count - 1; i >= 0; i--) // 按初始化与递增步骤循环；继续条件为 i >= 0。
            { // 开始计数循环。
                stack[i].SetInteractable(!blocked); // 设置面板是否参与交互。
                if (stack[i].Modal) // 当条件 stack[i].Modal 成立时，执行下方分支。
                    blocked = true; // 更新 是否存在阻挡，使用 true（启用/满足）。
            } // 结束计数循环。
        } // 结束方法。

        /// <summary>
        /// 释放自身事件订阅，最后调用基类清理主单例引用。
        /// </summary>
        protected override void OnDestroy() // 释放自身事件订阅，最后调用基类清理主单例引用。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            base.OnDestroy(); // 执行基类销毁清理，保留单例引用等释放约定。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
