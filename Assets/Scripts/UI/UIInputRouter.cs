using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 处理 F5/F9、Esc、Tab、M 等 UI 快捷键。
    /// </summary>
    public sealed class UIInputRouter : MonoBehaviour // 处理 F5/F9、Esc、Tab、M 等 UI 快捷键。
    { // 开始类型。
        /// <summary>
        /// 保存玩法选择面板或配置数组。
        /// </summary>
        public GameModePanel modes; // 声明 玩法选择面板或配置数组，后续初始化。
        /// <summary>
        /// 保存背包面板打开标记。
        /// </summary>
        private bool inventoryOpen; // 声明 背包面板打开标记，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 订阅读档完成事件，重置本地背包开关状态。
        /// </summary>
        private void OnEnable() // 订阅读档完成事件，重置本地背包开关状态。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => inventoryOpen = false)); // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
        } // 结束方法。

        /// <summary>
        /// 释放订阅并清空本地背包打开标记。
        /// </summary>
        private void OnDisable() // 释放订阅并清空本地背包打开标记。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            inventoryOpen = false; // 更新 背包面板打开标记，使用 false（关闭/不满足）。
        } // 结束方法。

        /// <summary>
        /// 处理 M 模式、F5/F9 存读档、Esc 暂停/关闭面板和 Tab 背包。
        /// </summary>
        private void Update() // 处理 M 模式、F5/F9 存读档、Esc 暂停/关闭面板和 Tab 背包。
        { // 开始方法。
            if (Input.GetKeyDown(KeyCode.M) && (GameSession.State == GameState.Playing || GameSession.State == GameState.Paused)) // 当（条件 Input.GetKeyDown(KeyCode.M) 成立；且（GameSession.State等于GameState.Playing；或GameSession.State等于GameState.Paused））时，执行下方分支。
                EventBus.Publish(new PanelRequested("Modes", modes == null || !modes.IsOpen)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            if (Input.GetKeyDown(KeyCode.F5)) // 当条件 Input.GetKeyDown(KeyCode.F5) 成立时，执行下方分支。
                EventBus.Publish(new SaveRequested()); // 广播 SaveRequested 事件，请求 GameManager 保存。
            if (Input.GetKeyDown(KeyCode.F9)) // 当条件 Input.GetKeyDown(KeyCode.F9) 成立时，执行下方分支。
                EventBus.Publish(new LoadRequested()); // 广播 LoadRequested 事件，请求 GameManager 读档。
            if (Input.GetKeyDown(KeyCode.Escape) && (GameSession.State == GameState.Playing || GameSession.State == GameState.Paused)) // 当（条件 Input.GetKeyDown(KeyCode.Escape) 成立；且（GameSession.State等于GameState.Playing；或GameSession.State等于GameState.Paused））时，执行下方分支。
            { // 开始条件分支。
                if (modes != null && modes.IsOpen) // 当（玩法选择面板或配置数组不为空；且modes.IsOpen为 true）时，执行下方分支。
                { // 开始条件分支。
                    EventBus.Publish(new PanelRequested("Modes", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                if (inventoryOpen) // 当背包面板打开标记为 true时，执行下方分支。
                { // 开始条件分支。
                    inventoryOpen = false; // 更新 背包面板打开标记，使用 false（关闭/不满足）。
                    EventBus.Publish(new PanelRequested("Inventory", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                bool pause = GameSession.State != GameState.Paused; // 把 GameSession.State != GameState.Paused 保存为 是否暂停。
                EventBus.Publish(new PauseRequested(pause)); // 广播 PauseRequested 事件，请求切换暂停流程。
                EventBus.Publish(new PanelRequested("Pause", pause)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            } // 结束条件分支。

            if (Input.GetKeyDown(KeyCode.Tab) && GameSession.State == GameState.Playing) // 当（条件 Input.GetKeyDown(KeyCode.Tab) 成立；且GameSession.State等于GameState.Playing）时，执行下方分支。
            { // 开始条件分支。
                inventoryOpen = !inventoryOpen; // 更新 背包面板打开标记，使用 !inventoryOpen。
                EventBus.Publish(new PanelRequested("Inventory", inventoryOpen)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            } // 结束条件分支。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
