using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 通过背包事件确认拾取，并用世界标记隐藏/恢复对象。
    /// </summary>
    public sealed class PickupItem : MonoBehaviour, IInteractable // 通过背包事件确认拾取，并用世界标记隐藏/恢复对象。
    { // 开始类型。
        /// <summary>
        /// 保存物品配置标识。
        /// </summary>
        public string itemId; // 声明 物品配置标识，后续初始化。
        /// <summary>
        /// 保存持久世界标记 ID。
        /// </summary>
        public string persistentId; // 声明 持久世界标记 ID，后续初始化。
        /// <summary>
        /// 保存物品已拾取标记。
        /// </summary>
        private bool collected; // 声明 物品已拾取标记，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => "拾取鬼器"; // 获取交互提示文本。

        /// <summary>
        /// 订阅世界标记变化并按已收集状态刷新外观与碰撞。
        /// </summary>
        private void OnEnable() // 订阅世界标记变化并按已收集状态刷新外观与碰撞。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<WorldStateChanged>(_ => Refresh())); // 订阅 WorldStateChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            Refresh(); // 根据世界标记更新 collected，并显隐渲染与碰撞。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 根据世界标记更新 collected，并显隐渲染与碰撞。
        /// </summary>
        public void Refresh() // 根据世界标记更新 collected，并显隐渲染与碰撞。
        { // 开始方法。
            if (!string.IsNullOrEmpty(persistentId)) // 当条件 string.IsNullOrEmpty(persistentId) 不成立时，执行下方分支。
                collected = WorldState.Has(persistentId); // 更新 物品已拾取标记，使用 WorldState.Has 的返回值。
            foreach (var renderer in GetComponentsInChildren<Renderer>()) // 遍历 GetComponentsInChildren<Renderer>()，每次把一个元素保存为 renderer。
                renderer.enabled = !collected; // 更新 renderer.enabled，使用 !collected。
            foreach (var collider in GetComponentsInChildren<Collider>()) // 遍历 GetComponentsInChildren<Collider>()，每次把一个元素保存为 collider。
                collider.enabled = !collected; // 更新 collider.enabled，使用 !collected。
        } // 结束方法。

        /// <summary>
        /// 仅未被收集时允许交互。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => !collected; // 仅未被收集时允许交互。
        /// <summary>
        /// 请求玩家背包接收物品，仅在回调确认成功后设置拾取标记并隐藏。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 请求玩家背包接收物品，仅在回调确认成功后设置拾取标记并隐藏。
        { // 开始方法。
            if (collected) // 当物品已拾取标记为 true时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            EventBus.Publish(new ItemPickupRequested(actor, itemId, success => // 广播 ItemPickupRequested 事件，请求目标背包接收物品并回调结果。
            { // 开始回调。
                if (!success) // 当条件 success 不成立时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                collected = true; // 更新 物品已拾取标记，使用 true（启用/满足）。
                WorldState.Set(persistentId); // 首次加入标记时广播 WorldStateChanged。
                Refresh(); // 根据世界标记更新 collected，并显隐渲染与碰撞。
            })); // 结束回调。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
