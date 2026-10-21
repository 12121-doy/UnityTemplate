using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    // 受击表现归场景拥有，与实体生命和伤害判定分开组合。
    /// <summary>
    /// 监听受击并播放音频、相机震动和池化特效。
    /// </summary>
    public sealed class DamageSystem : MonoBehaviour // 监听受击并播放音频、相机震动和池化特效。
    { // 开始类型。
        /// <summary>
        /// 保存受击特效预制体。
        /// </summary>
        public GameObject hitEffectPrefab; // 声明 受击特效预制体，后续初始化。
        /// <summary>
        /// 保存受击音频。
        /// </summary>
        public AudioClip hitClip; // 声明 受击音频，后续初始化。
        /// <summary>
        /// 保存可复用对象池。
        /// </summary>
        private ObjectPool<GameObject> pool; // 声明 可复用对象池，后续初始化。
        /// <summary>
        /// 保存活动对象或有效标记。
        /// </summary>
        private readonly System.Collections.Generic.HashSet<GameObject> active = new System.Collections.Generic.HashSet<GameObject>(); // 创建 System.Collections.Generic.HashSet<GameObject>，保存为 活动对象或有效标记。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 配置受击特效存在时创建对象池，绑定激活/隐藏/销毁回调。
        /// </summary>
        private void Awake() // 配置受击特效存在时创建对象池，绑定激活/隐藏/销毁回调。
        { // 开始方法。
            if (hitEffectPrefab != null) // 当受击特效预制体不为空时，执行下方分支。
                pool = new ObjectPool<GameObject>(() => Instantiate(hitEffectPrefab, transform), x => x.SetActive(true), x => x.SetActive(false), x => Destroy(x), 16); // 更新 可复用对象池，使用 新建的 ObjectPool<GameObject> 对象。
        } // 结束方法。

        /// <summary>
        /// 订阅实体受伤，触发音频、玩家震动和池化受击特效。
        /// </summary>
        private void OnEnable() // 订阅实体受伤，触发音频、玩家震动和池化受击特效。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<EntityDamaged>(e => // 广播 PlaySoundRequested 事件，交给音频管理器播放空间/UI 音效。
            { // 开始回调。
                EventBus.Publish(new PlaySoundRequested(hitClip, e.Damage.Point)); // 广播 PlaySoundRequested 事件，交给音频管理器播放空间/UI 音效。
                if (e.Entity.Type == EntityType.Player) // 当e.Entity.Type等于EntityType.Player时，执行下方分支。
                    EventBus.Publish(new CameraShakeRequested(0.08f, 0.25f)); // 广播 CameraShakeRequested 事件，触发相机震动。
                if (pool == null) // 当可复用对象池为空时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                var effect = pool.Get(); // 把 pool.Get 的返回值 保存为 effect。
                active.Add(effect); // 向 活动对象或有效标记 加入 effect。
                effect.transform.position = e.Damage.Point; // 更新 effect.transform.position，使用 e.Damage.Point。
                var particles = effect.GetComponentInChildren<ParticleSystem>(); // 把 effect.GetComponentInChildren<ParticleSystem> 的返回值 保存为 particles。
                if (particles != null) // 当particles不为空时，执行下方分支。
                    particles.Play(true); // 开始播放 true。
                AsyncHelper.Delay(this, 2, () => // 在组件上启动延时协程。
                { // 开始回调。
                    if (active.Remove(effect)) // 当条件 active.Remove(effect) 成立时，执行下方分支。
                        pool.Return(effect); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
                }); // 结束回调。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 退订并停止延时回收协程，再归还所有活动特效。
        /// </summary>
        private void OnDisable() // 退订并停止延时回收协程，再归还所有活动特效。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            StopAllCoroutines(); // 停止本组件启动的全部协程，随后由禁用清理统一回收活动对象。
            if (pool == null) // 当可复用对象池为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            foreach (var effect in active) // 遍历 活动对象或有效标记，每次把一个元素保存为 effect。
                pool.Return(effect); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            active.Clear(); // 清空 活动对象或有效标记 中保存的记录。
        } // 结束方法。

        /// <summary>
        /// 归还活动特效并释放空闲特效池。
        /// </summary>
        private void OnDestroy() // 归还活动特效并释放空闲特效池。
        { // 开始方法。
            if (pool == null) // 当可复用对象池为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            foreach (var effect in active) // 遍历 活动对象或有效标记，每次把一个元素保存为 effect。
                pool.Return(effect); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            active.Clear(); // 清空 活动对象或有效标记 中保存的记录。
            pool.Dispose(); // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
