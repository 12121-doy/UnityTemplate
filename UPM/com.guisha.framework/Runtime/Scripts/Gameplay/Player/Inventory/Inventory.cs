using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using System.Linq; // 引入 System.Linq 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 玩家背包、物品使用、事件快照和存档恢复。
    /// </summary>
    public sealed class Inventory : MonoBehaviour // 玩家背包、物品使用、事件快照和存档恢复。
    { // 开始类型。
        /// <summary>
        /// 保存可用物品配置目录。
        /// </summary>
        public Item[] catalog = Array.Empty<Item>(); // 把 Array.Empty<Item> 的返回值 保存为 可用物品配置目录。
        /// <summary>
        /// 保存容器容量上限。
        /// </summary>
        [SerializeField, Min(1)] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private int capacity = 12; // 把 12 保存为 容器容量上限。
        /// <summary>
        /// 保存背包物品列表。
        /// </summary>
        private readonly List<Item> items = new List<Item>(); // 创建 List<Item>，保存为 背包物品列表。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 获取只读背包物品列表。
        /// </summary>
        public IReadOnlyList<Item> Items => items; // 获取只读背包物品列表。

        /// <summary>
        /// 延迟发布背包快照，订阅拾取、使用及存读档事件。
        /// </summary>
        private void OnEnable() // 延迟发布背包快照，订阅拾取、使用及存读档事件。
        { // 开始方法。
            AsyncHelper.Delay(this, 0, Publish); // 在组件上启动延时协程。
            subscriptions.Add(EventBus.Subscribe<ItemPickupRequested>(e => // 订阅 ItemPickupRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Actor == gameObject) // 当e.Actor等于gameObject时，执行下方分支。
                    e.Completed?.Invoke(TryAdd(e.ItemId)); // 仅在 e.Completed 非空时，执行 e.Completed 回调；仅在回调非空时调用。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<UseItemRequested>(e => Use(e.Slot))); // 订阅 UseItemRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SaveCapture>(e => e.Data.inventory = Snapshot())); // 订阅 SaveCapture 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(e => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                items.Clear(); // 清空 背包物品列表 中保存的记录。
                foreach (var id in e.Data.inventory ?? Array.Empty<string>()) // 遍历 e.Data.inventory ?? Array.Empty<string>()，每次把一个元素保存为 id。
                { // 开始遍历循环。
                    var item = Array.Find(catalog, x => x != null && x.id == id); // 把 Array.Find 的返回值 保存为 当前对象。
                    if (item != null && items.Count < capacity) // 当（当前对象不为空；且items.Count小于容器容量上限）时，执行下方分支。
                        items.Add(item); // 向 背包物品列表 加入 item。
                } // 结束遍历循环。

                Publish(); // 发布 InventoryChanged。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// Unity 首次启用后的初始化回调，发布当前状态或完成初次配置。
        /// </summary>
        private void Start() // Unity 首次启用后的初始化回调，发布当前状态或完成初次配置。
        { // 开始方法。
            Publish(); // 发布 InventoryChanged。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 允许玩家输入时，用数字 1 使用第一个物品。
        /// </summary>
        private void Update() // 允许玩家输入时，用数字 1 使用第一个物品。
        { // 开始方法。
            if (GameSession.InputEnabled && Input.GetKeyDown(KeyCode.Alpha1)) // 当（当前允许玩家输入；且条件 Input.GetKeyDown(KeyCode.Alpha1) 成立）时，执行下方分支。
                Use(0); // 检查模拟状态与槽位，成功使用后按 consumable 移除，并广播快照。
        } // 结束方法。

        /// <summary>
        /// 按物品 ID 查 Catalog，容量允许时加入并广播快照。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <returns>true 表示找到物品且背包容量允许，已经添加成功。</returns>
        public bool TryAdd(string id) // 按物品 ID 查 Catalog，容量允许时加入并广播快照。
        { // 开始方法。
            var item = Array.Find(catalog, x => x != null && x.id == id); // 把 Array.Find 的返回值 保存为 当前对象。
            if (item == null || items.Count >= capacity) // 当（当前对象为空；或items.Count不小于容器容量上限）时，执行下方分支。
            { // 开始条件分支。
                EventBus.Publish(new Notification("背包已满或物品未配置")); // 广播 Notification 事件，让 HUD 显示短通知。
                return false; // 返回 false，将当前分支的布尔结果交给调用方。
            } // 结束条件分支。

            items.Add(item); // 向 背包物品列表 加入 item。
            Publish(); // 发布 InventoryChanged。
            return true; // 返回 true，将当前分支的布尔结果交给调用方。
        } // 结束方法。

        /// <summary>
        /// 检查模拟状态与槽位，成功使用后按 consumable 移除，并广播快照。
        /// </summary>
        /// <param name="slot">传入的 slot，类型为 int。</param>
        public void Use(int slot) // 检查模拟状态与槽位，成功使用后按 consumable 移除，并广播快照。
        { // 开始方法。
            if (!GameSession.SimulationEnabled || slot < 0 || slot >= items.Count) // 当（当前不推进游戏模拟；或slot小于0；或slot不小于items.Count）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var item = items[slot]; // 把 items[slot] 保存为 当前对象。
            if (!item.Use(gameObject)) // 当条件 item.Use(gameObject) 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (item.consumable) // 当item.consumable为 true时，执行下方分支。
                items.RemoveAt(slot); // 从 背包物品列表 移除 slot。
            Publish(); // 发布 InventoryChanged。
        } // 结束方法。

        /// <summary>
        /// 复制当前背包物品 ID。
        /// </summary>
        /// <returns>独立复制的当前状态快照。</returns>
        private string[] Snapshot() => items.Select(x => x.id).ToArray(); // 复制当前背包物品 ID。
        /// <summary>
        /// 发布 InventoryChanged。
        /// </summary>
        private void Publish() // 发布 InventoryChanged。
        { // 开始方法。
            EventBus.Publish(new InventoryChanged(Snapshot())); // 广播 InventoryChanged 事件，刷新背包与快捷栏。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
