using Guisha.Rendering; // 过场相机启用前补齐当前管线要求的附加相机数据。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.Playables; // 引入 UnityEngine.Playables 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// PlayableDirector 过场入口、镜头与输入锁管理。
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))] // 声明本组件必须同时具备的依赖组件。
    public sealed class CutscenePlayer : MonoBehaviour // PlayableDirector 过场入口、镜头与输入锁管理。
    { // 开始类型。
        /// <summary>
        /// 保存过场镜头对象。
        /// </summary>
        public GameObject cutsceneCamera; // 声明 过场镜头对象，后续初始化。
        /// <summary>
        /// 保存PlayableDirector 过场组件。
        /// </summary>
        private PlayableDirector director; // 声明 PlayableDirector 过场组件，后续初始化。
        /// <summary>
        /// 保存当前是否播放过场。
        /// </summary>
        private bool playing; // 声明 当前是否播放过场，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 缓存当前对象的 PlayableDirector。
        /// </summary>
        private void Awake() // 缓存当前对象的 PlayableDirector。
        { // 开始方法。
            director = GetComponent<PlayableDirector>(); // 更新 PlayableDirector 过场组件，使用 GetComponent<PlayableDirector> 的返回值。
        } // 结束方法。

        /// <summary>
        /// 订阅 Director 停止及读档事件，确保过场结束后释放输入锁。
        /// </summary>
        private void OnEnable() // 订阅 Director 停止及读档事件，确保过场结束后释放输入锁。
        { // 开始方法。
            director.stopped += Stopped; // 为 director.stopped 注册事件回调；事件发生时执行右侧处理逻辑。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (playing) // 当当前是否播放过场为 true时，执行下方分支。
                    director.Stop(); // 停止正在播放的过场。
                Release(); // 关闭过场相机、清空播放标记并释放自身输入锁。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 退订、停止正在播放的 Director，并释放过场相机与输入锁。
        /// </summary>
        private void OnDisable() // 退订、停止正在播放的 Director，并释放过场相机与输入锁。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            director.stopped -= Stopped; // 从 director.stopped 退订事件回调，避免生命周期结束后继续调用。
            if (playing) // 当当前是否播放过场为 true时，执行下方分支。
                director.Stop(); // 停止正在播放的过场。
            Release(); // 关闭过场相机、清空播放标记并释放自身输入锁。
        } // 结束方法。

        /// <summary>
        /// 在未播放且有资产时加输入锁，启用过场相机并播放 Director。
        /// </summary>
        public void Play() // 在未播放且有资产时加输入锁，启用过场相机并播放 Director。
        { // 开始方法。
            if (playing || director.playableAsset == null) // 当（当前是否播放过场为 true；或director.playableAsset为空）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            playing = true; // 更新 当前是否播放过场，使用 true（启用/满足）。
            EventBus.Publish(new InputLockChanged(this, true)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
            if (cutsceneCamera != null) // 当过场镜头对象不为空时，执行下方分支。
            { // 先准备相机，再启用镜头，避免首帧缺少管线配置。
                RenderPipelineService.PrepareHierarchy(cutsceneCamera); // 原有相机与 Volume 美术参数保持由场景配置。
                cutsceneCamera.SetActive(true); // 启用已经准备好的过场相机。
            } // 完成过场镜头启用。
            director.Play(); // 开始播放配置的 Timeline/过场资产。
        } // 结束方法。

        /// <summary>
        /// Director 停止回调，转入 Release。
        /// </summary>
        /// <param name="_">接口保留的未使用参数。</param>
        private void Stopped(PlayableDirector _) // Director 停止回调，转入 Release。
        { // 开始方法。
            Release(); // 关闭过场相机、清空播放标记并释放自身输入锁。
        } // 结束方法。

        /// <summary>
        /// 关闭过场相机、清空播放标记并释放自身输入锁。
        /// </summary>
        private void Release() // 关闭过场相机、清空播放标记并释放自身输入锁。
        { // 开始方法。
            playing = false; // 更新 当前是否播放过场，使用 false（关闭/不满足）。
            if (cutsceneCamera != null) // 当过场镜头对象不为空时，执行下方分支。
                cutsceneCamera.SetActive(false); // 将 过场镜头对象 的激活状态设为 false，同时影响其组件生命周期。
            EventBus.Publish(new InputLockChanged(this, false)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
