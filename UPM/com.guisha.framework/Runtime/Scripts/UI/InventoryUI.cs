using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine.UI; // 引入 UnityEngine.UI 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 背包面板与使用物品请求入口。
    /// </summary>
    public sealed class InventoryUI : UIPanelBase // 背包面板与使用物品请求入口。
    { // 开始类型。
        /// <summary>
        /// 保存背包列表文本。
        /// </summary>
        public Text itemsText; // 声明 背包列表文本，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 执行面板注册，订阅背包快照并刷新物品列表。
        /// </summary>
        protected override void OnEnable() // 执行面板注册，订阅背包快照并刷新物品列表。
        { // 开始方法。
            base.OnEnable(); // 先执行基类启用注册流程。
            subscriptions.Add(EventBus.Subscribe<InventoryChanged>(e => // 订阅 InventoryChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (itemsText == null) // 当背包列表文本为空时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                itemsText.text = "背包\n"; // 更新 itemsText.text，使用 "背包\n"。
                for (int i = 0; i < e.ItemIds.Length; i++) // 按初始化与递增步骤循环；继续条件为 i < e.ItemIds.Length。
                    itemsText.text += $"{i + 1}. {e.ItemIds[i]}\n"; // 累加 itemsText.text，使用 $"{i + 1}. {e.ItemIds[i]}\n"。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 广播槽位使用请求。
        /// </summary>
        /// <param name="slot">传入的 slot，类型为 int。</param>
        public void Use(int slot) // 广播槽位使用请求。
        { // 开始方法。
            EventBus.Publish(new UseItemRequested(slot)); // 广播 UseItemRequested 事件，请求使用背包槽位。
        } // 结束方法。

        /// <summary>
        /// 释放背包订阅，再执行面板注销。
        /// </summary>
        protected override void OnDisable() // 释放背包订阅，再执行面板注销。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            base.OnDisable(); // 执行基类禁用与退订流程。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
