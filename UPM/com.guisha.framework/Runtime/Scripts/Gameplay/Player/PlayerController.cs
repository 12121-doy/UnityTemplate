using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// CharacterController 移动、蹲伏、奔跑消耗、脚步噪声与光标控制。
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerStatus))] // 声明本组件必须同时具备的依赖组件。
    public sealed class PlayerController : MonoBehaviour // CharacterController 移动、蹲伏、奔跑消耗、脚步噪声与光标控制。
    { // 开始类型。
        /// <summary>
        /// 保存控制移动方向的相机 Transform。
        /// </summary>
        public Transform cameraTransform; // 声明 控制移动方向的相机 Transform，后续初始化。
        /// <summary>
        /// 保存步行速度、奔跑速度、蹲伏速度、重力加速度。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float walkSpeed = 3, runSpeed = 5, crouchSpeed = 1.5f, gravity = -20; // 把 3 保存为 步行速度；把 5 保存为 奔跑速度；把 1.5f 保存为 蹲伏速度；把 -20 保存为 重力加速度。
        /// <summary>
        /// 保存站起时检测头顶障碍的层掩码。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private LayerMask ceilingMask = ~0; // 把 ~0 保存为 站起时检测头顶障碍的层掩码。
        /// <summary>
        /// 保存CharacterController 胶囊控制器。
        /// </summary>
        private CharacterController controller; // 声明 CharacterController 胶囊控制器，后续初始化。
        /// <summary>
        /// 保存玩家状态组件。
        /// </summary>
        private PlayerStatus status; // 声明 玩家状态组件，后续初始化。
        /// <summary>
        /// 保存当前实体组件。
        /// </summary>
        private PlayerEntity entity; // 声明 当前实体组件，后续初始化。
        /// <summary>
        /// 保存竖直运动速度、站立时胶囊高度、下次脚步噪声时间。
        /// </summary>
        private float vertical, standingHeight, nextStep; // 声明 竖直运动速度，后续初始化；声明 站立时胶囊高度，后续初始化；声明 下次脚步噪声时间，后续初始化。
        /// <summary>
        /// 保存站立时胶囊中心。
        /// </summary>
        private Vector3 standingCenter; // 声明 站立时胶囊中心，后续初始化。
        /// <summary>
        /// 保存当前蹲伏状态。
        /// </summary>
        private bool crouched; // 声明 当前蹲伏状态，后续初始化。
        /// <summary>
        /// 获取角色是否处于蹲伏状态。
        /// </summary>
        public bool IsCrouched => crouched; // 获取角色是否处于蹲伏状态。

        /// <summary>
        /// 缓存 CharacterController、PlayerStatus、PlayerEntity，并保存站立胶囊尺寸。
        /// </summary>
        private void Awake() // 缓存 CharacterController、PlayerStatus、PlayerEntity，并保存站立胶囊尺寸。
        { // 开始方法。
            controller = GetComponent<CharacterController>(); // 更新 CharacterController 胶囊控制器，使用 GetComponent<CharacterController> 的返回值。
            status = GetComponent<PlayerStatus>(); // 更新 玩家状态组件，使用 GetComponent<PlayerStatus> 的返回值。
            entity = GetComponent<PlayerEntity>(); // 更新 当前实体组件，使用 GetComponent<PlayerEntity> 的返回值。
            standingHeight = controller.height; // 记录站立胶囊高度，后续从蹲伏恢复时使用。
            standingCenter = controller.center; // 记录站立胶囊中心，改变高度时据此维持脚底位置。
        } // 结束方法。

        /// <summary>
        /// 按输入锁、角色生命和旧输入系统计算移动/奔跑/蹲伏、重力与噪声，并调用 CharacterController.Move。
        /// </summary>
        private void Update() // 按输入锁、角色生命和旧输入系统计算移动/奔跑/蹲伏、重力与噪声，并调用 CharacterController.Move。
        { // 开始方法。
            Cursor.lockState = GameSession.InputEnabled ? CursorLockMode.Locked : CursorLockMode.None; // 按输入状态设置鼠标锁定模式；值来自 按条件 GameSession.InputEnabled 选择的值。
            Cursor.visible = !GameSession.InputEnabled; // 按输入状态控制鼠标指针显隐；值来自 !GameSession.InputEnabled。
            if (!GameSession.InputEnabled || !entity.IsAlive) // 当（当前不允许玩家输入；或条件 entity.IsAlive 不成立）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (cameraTransform == null && Camera.main != null) // 当（控制移动方向的相机 Transform为空；且主相机不为空）时，执行下方分支。
                cameraTransform = Camera.main.transform; // 更新 控制移动方向的相机 Transform，使用 Camera.main.transform。
            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl); // 读取左 Ctrl 是否按住，作为本帧期望的蹲伏状态。
            if (wantsCrouch != crouched) // 当输入期望的蹲伏状态不等于当前蹲伏状态时，执行下方分支。
            { // 开始条件分支。
                bool blocked = false; // 先假定站立空间没有障碍，再通过胶囊重叠检测修正。
                if (!wantsCrouch) // 当玩家正在尝试站起时，执行下方分支。
                { // 开始条件分支。
                    var bottom = transform.position + standingCenter + Vector3.down * (standingHeight * 0.5f - controller.radius); // 计算完整站立胶囊的底部球心，作为站起空间检测的起点。
                    var top = bottom + Vector3.up * (standingHeight - 2 * controller.radius); // 根据站立高度与半径计算顶部球心，覆盖角色站起后的完整空间。
                    foreach (var hit in Physics.OverlapCapsule(bottom, top, controller.radius * 0.95f, ceilingMask, QueryTriggerInteraction.Ignore)) // 遍历 Physics.OverlapCapsule(bottom, top, controller.radius * 0.95f, ceilingMask, QueryTriggerInteraction.Ignore)，每次把一个元素保存为 hit。
                        if (!hit.transform.IsChildOf(transform)) // 当条件 hit.transform.IsChildOf(transform) 不成立时，执行下方分支。
                        { // 开始条件分支。
                            blocked = true; // 发现不属于玩家自身层级的障碍，本次禁止站起。
                            break; // 结束当前循环或 switch 分支。
                        } // 结束条件分支。
                } // 结束条件分支。

                if (!blocked) // 当站立空间没有被头顶障碍占用时，执行下方分支。
                { // 开始条件分支。
                    crouched = wantsCrouch; // 空间检测通过后提交蹲伏/站立状态。
                    controller.height = standingHeight * (crouched ? 0.55f : 1); // 按蹲伏状态调整胶囊高度；值来自 standingHeight * (crouched ? 0.55f : 1)。
                    controller.center = standingCenter - Vector3.up * (standingHeight - controller.height) * 0.5f; // 同步胶囊中心，保持角色脚底位置；值来自 standingCenter - Vector3.up * (standingHeight - controller.height) * 0.5f。
                } // 结束条件分支。
            } // 结束条件分支。

            Vector3 input = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1); // 读取横纵移动轴并限制向量长度，避免斜向移动比直行更快。
            Vector3 forward = cameraTransform != null ? MathUtility.Planar(cameraTransform.forward).normalized : Vector3.forward; // 将相机前向投影到水平面；没有相机时使用世界前向。
            Vector3 right = Vector3.Cross(Vector3.up, forward); // 用世界上方向与水平前方向叉乘，得到水平右方向。
            Vector3 move = forward * input.z + right * input.x; // 把玩家横纵输入组合成相对于相机的世界移动方向。
            bool running = !crouched && input.sqrMagnitude > 0 && Input.GetKey(KeyCode.LeftShift) && status.ConsumeStamina(20 * Time.deltaTime); // 仅在未蹲伏、有移动输入、按住 Shift 且体力消耗成功时启用奔跑；体力按秒消耗。
            float speed = crouched ? crouchSpeed : running ? runSpeed : walkSpeed; // 按蹲伏、奔跑、步行的优先级选择移动速度。
            if (status.Injured) // 当status.Injured为 true时，执行下方分支。
                speed *= 0.7f; // 受伤时把本帧移动速度降低到原来的 70%。
            if (controller.isGrounded && vertical < 0) // 当（角色控制器当前接地；且竖直运动速度小于0）时，执行下方分支。
                vertical = -2; // 接地且正在下落时保留轻微向下速度，帮助控制器贴地。
            vertical += gravity * Time.deltaTime; // 按帧时长累积重力，更新竖直速度。
            controller.Move((move * speed + Vector3.up * vertical) * Time.deltaTime); // 将本帧位移交给 CharacterController 进行碰撞移动。
            if (move.sqrMagnitude > 0.01f) // 当move.sqrMagnitude大于0.01f时，执行下方分支。
            { // 开始条件分支。
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 12 * Time.deltaTime); // 平滑旋转角色朝向；值来自 Quaternion.Slerp 的返回值。
                if (controller.isGrounded && Time.time >= nextStep) // 当（角色控制器当前接地；且已经到达下一次允许发出脚步噪声的时间）时，执行下方分支。
                { // 开始条件分支。
                    nextStep = Time.time + (running ? 0.3f : 0.55f); // 设置下一次脚步噪声时间；奔跑的脚步间隔更短。
                    EventBus.Publish(new NoiseEmitted(transform.position, crouched ? 2 : running ? 12 : 6, gameObject)); // 广播 NoiseEmitted 事件，让听觉感知组件记录声音位置。
                } // 结束条件分支。
            } // 结束条件分支。
        } // 结束方法。

        /// <summary>
        /// 解除鼠标锁定并显示指针，使玩家可以操作界面。
        /// </summary>
        private void OnDisable() // 解除鼠标锁定并显示指针，使玩家可以操作界面。
        { // 开始方法。
            Cursor.lockState = CursorLockMode.None; // 按输入状态设置鼠标锁定模式；值来自 CursorLockMode.None。
            Cursor.visible = true; // 按输入状态控制鼠标指针显隐；值来自 true（启用/满足）。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
