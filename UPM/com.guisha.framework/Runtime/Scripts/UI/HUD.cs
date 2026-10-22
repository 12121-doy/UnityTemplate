using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.UI; // 引入 UnityEngine.UI 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 监听玩家状态、提示、物品与模式进度并更新界面。
    /// </summary>
    public sealed class HUD : MonoBehaviour // 监听玩家状态、提示、物品与模式进度并更新界面。
    { // 开始类型。
        /// <summary>
        /// 保存理智 UI 滑条、体力 UI 滑条。
        /// </summary>
        public Slider sanBar, staminaBar; // 声明 理智 UI 滑条，后续初始化；声明 体力 UI 滑条，后续初始化。
        /// <summary>
        /// 保存玩法信息文本。
        /// </summary>
        public Text modeText; // 声明 玩法信息文本，后续初始化。
        /// <summary>
        /// 保存HUD 属性文本、HUD 交互提示文本、HUD 临时通知文本、HUD 物品快捷栏文本。
        /// </summary>
        public Text statsText, promptText, notificationText, quickSlotsText; // 声明 HUD 属性文本，后续初始化；声明 HUD 交互提示文本，后续初始化；声明 HUD 临时通知文本，后续初始化；声明 HUD 物品快捷栏文本，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存通知到期隐藏时间。
        /// </summary>
        private float hideNotificationAt; // 声明 通知到期隐藏时间，后续初始化。
        /// <summary>
        /// 刷新模式显示，并订阅玩家属性、模式、交互提示、背包和短通知。
        /// </summary>
        private void OnEnable() // 刷新模式显示，并订阅玩家属性、模式、交互提示、背包和短通知。
        { // 开始方法。
            RefreshMode(); // 更新模式名、挑战剩余秒数或生存波次和 M 键提示。
            subscriptions.Add(EventBus.Subscribe<GameModeChanged>(_ => RefreshMode())); // 订阅 GameModeChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<GameModeProgress>(_ => RefreshMode())); // 订阅 GameModeProgress 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<PlayerStatsChanged>(e => // 订阅 PlayerStatsChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (sanBar != null) // 当理智 UI 滑条不为空时，执行下方分支。
                    sanBar.value = e.San / Mathf.Max(1, e.MaxSan); // 更新 sanBar.value，使用 e.San / Mathf.Max(1, e.MaxSan)。
                if (staminaBar != null) // 当体力 UI 滑条不为空时，执行下方分支。
                    staminaBar.value = e.Stamina / Mathf.Max(1, e.MaxStamina); // 更新 staminaBar.value，使用 e.Stamina / Mathf.Max(1, e.MaxStamina)。
                if (statsText != null) // 当HUD 属性文本不为空时，执行下方分支。
                    statsText.text = $"SAN {e.San:F0} / {e.MaxSan:F0}     体力 {e.Stamina:F0} / {e.MaxStamina:F0}"; // 更新 statsText.text，使用 $"SAN {e.San:F0} / {e.MaxSan:F0}     体力 {e.Stamina:F0} / {e.MaxStamina:F0}"。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<InteractionPromptChanged>(e => // 订阅 InteractionPromptChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (promptText != null) // 当HUD 交互提示文本不为空时，执行下方分支。
                    promptText.text = e.Text; // 更新 promptText.text，使用 e.Text。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<Notification>(e => // 订阅 Notification 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (notificationText != null) // 当HUD 临时通知文本不为空时，执行下方分支。
                    notificationText.text = e.Text; // 更新 notificationText.text，使用 e.Text。
                hideNotificationAt = Time.unscaledTime + 5; // 更新 通知到期隐藏时间，使用 Time.unscaledTime + 5。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<InventoryChanged>(e => // 订阅 InventoryChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (quickSlotsText != null) // 当HUD 物品快捷栏文本不为空时，执行下方分支。
                    quickSlotsText.text = "快捷栏 [1]  " + string.Join(" | ", e.ItemIds); // 更新 quickSlotsText.text，使用 "快捷栏 [1]  " + string.Join(" | ", e.ItemIds)。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => // 广播 Notification 事件，让 HUD 显示短通知。
            { // 开始回调。
                if (e.Entity.Type == EntityType.Player) // 当e.Entity.Type等于EntityType.Player时，执行下方分支。
                    EventBus.Publish(new Notification("你已死亡，按 F9 读取检查点")); // 广播 Notification 事件，让 HUD 显示短通知。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<LowSanChanged>(e => // 订阅 LowSanChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (statsText != null) // 当HUD 属性文本不为空时，执行下方分支。
                    statsText.color = e.Active ? Color.red : Color.white; // 更新 statsText.color，使用 按条件 e.Active 选择的值。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 更新模式名、挑战剩余秒数或生存波次和 M 键提示。
        /// </summary>
        private void RefreshMode() // 更新模式名、挑战剩余秒数或生存波次和 M 键提示。
        { // 开始方法。
            if (modeText == null) // 当玩法信息文本为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var rules = GameModeRuntime.Rules; // 把 当前玩法规则快照 保存为 规则序列或规则快照。
            modeText.text = GameModeRuntime.Label(rules.Mode) + (rules.TimeLimit > 0 ? " · 剩余 " + Mathf.CeilToInt(GameModeRuntime.Remaining) + " 秒" : rules.Mode == GameMode.Endless ? " · 波次 " + GameModeRuntime.Wave : "") + "  [M]"; // 更新 modeText.text，使用 GameModeRuntime.Label(rules.Mode) + (rules.TimeLimit > 0 ? " · 剩余 " + Mathf.CeilToInt(GameModeRuntime.Remaining) + " 秒" : rules.Mode == GameMode.Endless ? " · 波次 " + GameModeRuntime.Wave : "") + "  [M]"。
        } // 结束方法。

        /// <summary>
        /// 到期后清空短通知文字。
        /// </summary>
        private void Update() // 到期后清空短通知文字。
        { // 开始方法。
            if (notificationText != null && Time.unscaledTime > hideNotificationAt) // 当（HUD 临时通知文本不为空；且当前非缩放时间大于通知到期隐藏时间）时，执行下方分支。
                notificationText.text = ""; // 更新 notificationText.text，使用 ""。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
