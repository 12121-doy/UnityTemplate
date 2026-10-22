using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 第三人称跟随、遮挡防穿墙和相机震动。
    /// </summary>
    public sealed class PlayerCamera : MonoBehaviour // 第三人称跟随、遮挡防穿墙和相机震动。
    { // 开始类型。
        /// <summary>
        /// 保存目标对象。
        /// </summary>
        public Transform target; // 声明 目标对象，后续初始化。
        /// <summary>
        /// 保存相机碰撞层掩码。
        /// </summary>
        public LayerMask collisionMask = ~0; // 把 ~0 保存为 相机碰撞层掩码。
        /// <summary>
        /// 保存相机跟随距离、相机目标高度、鼠标灵敏度、相机碰撞球半径。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float distance = 3, height = 1.6f, sensitivity = 2, radius = 0.2f; // 把 3 保存为 相机跟随距离；把 1.6f 保存为 相机目标高度；把 2 保存为 鼠标灵敏度；把 0.2f 保存为 相机碰撞球半径。
        /// <summary>
        /// 保存相机水平旋转角、相机俯仰角、剩余震动秒数、相机震动强度。
        /// </summary>
        private float yaw, pitch = 15, shakeTime, shakeStrength; // 声明 相机水平旋转角，后续初始化；把 15 保存为 相机俯仰角；声明 剩余震动秒数，后续初始化；声明 相机震动强度，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 订阅相机震动事件，保存持续时间与强度。
        /// </summary>
        private void OnEnable() // 订阅相机震动事件，保存持续时间与强度。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<CameraShakeRequested>(e => // 订阅 CameraShakeRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                shakeTime = e.Duration; // 更新 剩余震动秒数，使用 e.Duration。
                shakeStrength = e.Strength; // 更新 相机震动强度，使用 e.Strength。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 组件禁用时释放自身持有的事件订阅。
        /// </summary>
        private void OnDisable() // 组件禁用时释放自身持有的事件订阅。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 读取鼠标、计算绕目标的位置，以 SphereCast 处理墙体距离并叠加震动。
        /// </summary>
        private void LateUpdate() // 读取鼠标、计算绕目标的位置，以 SphereCast 处理墙体距离并叠加震动。
        { // 开始方法。
            if (target == null) // 当目标对象为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (GameSession.InputEnabled) // 当当前允许玩家输入时，执行下方分支。
            { // 开始条件分支。
                yaw += Input.GetAxis("Mouse X") * sensitivity; // 累加 相机水平旋转角，使用 Input.GetAxis("Mouse X") * sensitivity。
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -25, 70); // 更新 相机俯仰角，使用 Mathf.Clamp 的返回值。
            } // 结束条件分支。

            var rotation = Quaternion.Euler(pitch, yaw, 0); // 把 Quaternion.Euler 的返回值 保存为 世界旋转。
            var pivot = target.position + Vector3.up * height; // 把 target.position + Vector3.up * height 保存为 pivot。
            var direction = rotation * Vector3.back; // 把 rotation * Vector3.back 保存为 direction。
            float allowed = distance; // 把 相机跟随距离 保存为 allowed。
            // 即使检测层选中全部层，也过滤玩家自身层级。
            foreach (var hit in Physics.SphereCastAll(pivot, radius, direction, distance, collisionMask, QueryTriggerInteraction.Ignore)) // 遍历 Physics.SphereCastAll(pivot, radius, direction, distance, collisionMask, QueryTriggerInteraction.Ignore)，每次把一个元素保存为 hit。
                if (!hit.transform.IsChildOf(target)) // 当条件 hit.transform.IsChildOf(target) 不成立时，执行下方分支。
                    allowed = Mathf.Min(allowed, Mathf.Max(0.05f, hit.distance - 0.05f)); // 更新 allowed，使用 Mathf.Min 的返回值。
            var position = pivot + direction * allowed; // 把 pivot + direction * allowed 保存为 世界位置。
            if (shakeTime > 0 && GameSession.SimulationEnabled) // 当（剩余震动秒数大于0；且全局模拟允许状态为 true）时，执行下方分支。
            { // 开始条件分支。
                shakeTime -= Time.deltaTime; // 扣减 剩余震动秒数，使用 当前帧缩放后的秒数。
                position += Random.insideUnitSphere * shakeStrength; // 累加 世界位置，使用 Random.insideUnitSphere * shakeStrength。
            } // 结束条件分支。

            transform.SetPositionAndRotation(position, rotation); // 同时恢复世界位置与旋转。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
