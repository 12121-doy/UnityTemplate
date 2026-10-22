using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 门开合与机关解锁。
    /// </summary>
    public sealed class Door : MonoBehaviour, IInteractable // 门开合与机关解锁。
    { // 开始类型。
        /// <summary>
        /// 保存门铰链 Transform。
        /// </summary>
        public Transform hinge; // 声明 门铰链 Transform，后续初始化。
        /// <summary>
        /// 保存用于开锁的机关标识。
        /// </summary>
        public string unlockMechanismId; // 声明 用于开锁的机关标识，后续初始化。
        /// <summary>
        /// 保存持久世界标记 ID。
        /// </summary>
        public string persistentId; // 声明 持久世界标记 ID，后续初始化。
        /// <summary>
        /// 保存开门旋转角。
        /// </summary>
        public float openAngle = 90; // 把 90 保存为 开门旋转角。
        /// <summary>
        /// 保存输入是否锁定。
        /// </summary>
        public bool locked; // 声明 输入是否锁定，后续初始化。
        /// <summary>
        /// 保存门当前是否打开。
        /// </summary>
        private bool opened; // 声明 门当前是否打开，后续初始化。
        /// <summary>
        /// 保存关闭时的局部旋转。
        /// </summary>
        private Quaternion closedRotation; // 声明 关闭时的局部旋转，后续初始化。
        /// <summary>
        /// 保存初始是否上锁。
        /// </summary>
        private bool initiallyLocked; // 声明 初始是否上锁，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => locked ? "门被锁住了" : opened ? "关闭门" : "打开门"; // 获取交互提示文本。

        /// <summary>
        /// 绑定门铰链，保存关闭旋转和初始上锁状态。
        /// </summary>
        private void Awake() // 绑定门铰链，保存关闭旋转和初始上锁状态。
        { // 开始方法。
            if (hinge == null) // 当门铰链 Transform为空时，执行下方分支。
                hinge = transform; // 更新 门铰链 Transform，使用 transform。
            closedRotation = hinge.localRotation; // 更新 关闭时的局部旋转，使用 hinge.localRotation。
            initiallyLocked = locked; // 更新 初始是否上锁，使用 输入是否锁定。
        } // 结束方法。

        /// <summary>
        /// 订阅机关激活与世界/存档变化，恢复门锁并刷新门状态。
        /// </summary>
        private void OnEnable() // 订阅机关激活与世界/存档变化，恢复门锁并刷新门状态。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<MechanismActivated>(e => // 订阅 MechanismActivated 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (string.IsNullOrEmpty(unlockMechanismId) || e.Id != unlockMechanismId) // 当（条件 string.IsNullOrEmpty(unlockMechanismId) 成立；或e.Id不等于用于开锁的机关标识）时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                locked = false; // 更新 输入是否锁定，使用 false（关闭/不满足）。
                if (!string.IsNullOrEmpty(persistentId)) // 当条件 string.IsNullOrEmpty(persistentId) 不成立时，执行下方分支。
                    WorldState.Set(persistentId + ".unlocked"); // 首次加入标记时广播 WorldStateChanged。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                opened = false; // 更新 门当前是否打开，使用 false（关闭/不满足）。
                hinge.localRotation = closedRotation; // 更新 hinge.localRotation，使用 关闭时的局部旋转。
                Refresh(); // 按永久解锁标记和初始条件刷新 locked。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<WorldStateChanged>(_ => Refresh())); // 订阅 WorldStateChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            Refresh(); // 按永久解锁标记和初始条件刷新 locked。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 按永久解锁标记和初始条件刷新 locked。
        /// </summary>
        public void Refresh() // 按永久解锁标记和初始条件刷新 locked。
        { // 开始方法。
            if (!string.IsNullOrEmpty(persistentId)) // 当条件 string.IsNullOrEmpty(persistentId) 不成立时，执行下方分支。
                locked = !WorldState.Has(persistentId + ".unlocked") && (initiallyLocked || !string.IsNullOrEmpty(unlockMechanismId)); // 更新 输入是否锁定，使用 !WorldState.Has(persistentId + ".unlocked") && (initiallyLocked || !string.IsNullOrEmpty(unlockMechanismId))。
        } // 结束方法。

        /// <summary>
        /// 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => true; // 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// <summary>
        /// 锁定时显示提示，未锁时切换门扇角度并发出噪声。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 锁定时显示提示，未锁时切换门扇角度并发出噪声。
        { // 开始方法。
            if (locked) // 当输入是否锁定为 true时，执行下方分支。
            { // 开始条件分支。
                EventBus.Publish(new Notification("需要先解开机关")); // 广播 Notification 事件，让 HUD 显示短通知。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            opened = !opened; // 更新 门当前是否打开，使用 !opened。
            hinge.localRotation = closedRotation * Quaternion.Euler(0, opened ? openAngle : 0, 0); // 更新 hinge.localRotation，使用 closedRotation * Quaternion.Euler(0, opened ? openAngle : 0, 0)。
            EventBus.Publish(new NoiseEmitted(transform.position, 8, actor)); // 广播 NoiseEmitted 事件，让听觉感知组件记录声音位置。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
