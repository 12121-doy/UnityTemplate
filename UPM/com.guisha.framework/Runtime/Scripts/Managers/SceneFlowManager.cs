using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Threading.Tasks; // 引入 System.Threading.Tasks 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 串行附加场景切换，激活成功后卸载旧副本并保留主场景。
    /// </summary>
    public sealed class SceneFlowManager : MonoSingleton<SceneFlowManager> // 串行附加场景切换，激活成功后卸载旧副本并保留主场景。
    { // 开始类型。
        /// <summary>
        /// 保存保留加载的主场景路径。
        /// </summary>
        [Tooltip("留空时保留首次载入的场景作为诡舍主场景。")] // 为 Inspector 配置字段提供悬停说明。
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private string persistentScenePath; // 声明 保留加载的主场景路径，后续初始化。
        /// <summary>
        /// 保存当前副本场景。
        /// </summary>
        private string currentDungeonScene; // 声明 当前副本场景，后续初始化。
        /// <summary>
        /// 保存场景切换进行中标记。
        /// </summary>
        private bool busy; // 声明 场景切换进行中标记，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 先建立主单例，读取常驻主场景路径并订阅场景切换请求。
        /// </summary>
        protected override void Awake() // 先建立主单例，读取常驻主场景路径并订阅场景切换请求。
        { // 开始方法。
            base.Awake(); // 执行基类 Awake，保留基类初始化流程。
            if (Instance != this) // 当当前类型的主单例实例不等于this时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings"); // 把 Resources.Load<FrameworkSettings> 的返回值 保存为 框架或动画配置。
            if (settings != null) // 当框架或动画配置不为空时，执行下方分支。
                persistentScenePath = settings.persistentScenePath; // 更新 保留加载的主场景路径，使用 settings.persistentScenePath。
            subscriptions.Add(EventBus.Subscribe<SceneTransitionRequested>(e => AsyncHelper.Forget(Transition(e)))); // 订阅 SceneTransitionRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
        } // 结束方法。

        /// <summary>
        /// 串行附加加载与激活，等待新场景订阅就绪后卸载旧副本；错误尝试回滚并返回失败。
        /// </summary>
        /// <param name="request">收到的请求载荷。</param>
        /// <returns>异步完成任务；应 await 或通过 AsyncHelper.Forget 观察异常。</returns>
        private async Task Transition(SceneTransitionRequested request) // 串行附加加载与激活，等待新场景订阅就绪后卸载旧副本；错误尝试回滚并返回失败。
        { // 开始方法。
            if (busy) // 当已有场景切换正在进行时，执行下方分支。
            { // 开始条件分支。
                request.Completed?.Invoke(false); // 仅在 request.Completed 非空时，执行 request.Completed 回调；仅在回调非空时调用。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            if (string.IsNullOrWhiteSpace(request.ScenePath)) // 当条件 string.IsNullOrWhiteSpace(request.ScenePath) 成立时，执行下方分支。
            { // 开始条件分支。
                request.Completed?.Invoke(false); // 仅在 request.Completed 非空时，执行 request.Completed 回调；仅在回调非空时调用。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            busy = true; // 更新 场景切换进行中标记，使用 true（启用/满足）。
            bool success = false; // 把 false（关闭/不满足） 保存为 是否成功。
            if (string.IsNullOrEmpty(persistentScenePath)) // 当条件 string.IsNullOrEmpty(persistentScenePath) 成立时，执行下方分支。
                persistentScenePath = SceneManager.GetActiveScene().path; // 更新 保留加载的主场景路径，使用 SceneManager.GetActiveScene().path。
            EventBus.Publish(new SceneTransitionStarted()); // 广播 SceneTransitionStarted 事件，通知场景切换开始。
            Scene loaded = default; // 把 default 保存为 loaded。
            bool added = false; // 把 false（关闭/不满足） 保存为 added。
            var previous = SceneManager.GetActiveScene(); // 把 SceneManager.GetActiveScene 的返回值 保存为 修改前的旧值。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                loaded = SceneManager.GetSceneByPath(request.ScenePath); // 更新 loaded，使用 SceneManager.GetSceneByPath 的返回值。
                if (!loaded.isLoaded) // 当条件 loaded.isLoaded 不成立时，执行下方分支。
                { // 开始条件分支。
                    await AsyncHelper.Await(SceneManager.LoadSceneAsync(request.ScenePath, LoadSceneMode.Additive)); // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
                    loaded = SceneManager.GetSceneByPath(request.ScenePath); // 更新 loaded，使用 SceneManager.GetSceneByPath 的返回值。
                    added = true; // 更新 added，使用 true（启用/满足）。
                } // 结束条件分支。

                if (!loaded.IsValid() || !loaded.isLoaded || !SceneManager.SetActiveScene(loaded)) // 当（条件 loaded.IsValid() 不成立；或条件 loaded.isLoaded 不成立；或条件 SceneManager.SetActiveScene(loaded) 不成立）时，执行下方分支。
                    throw new InvalidOperationException("Scene did not become active. Use full Assets/...unity paths in Build Settings."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Scene did not become active. Use full Assets/...unity paths in Build Settings."。
                // 等待一帧，让新场景完成 Start/订阅建立，再恢复存档。
                await Task.Yield(); // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
                if (!string.IsNullOrEmpty(currentDungeonScene) && currentDungeonScene != request.ScenePath && currentDungeonScene != persistentScenePath) // 当（条件 string.IsNullOrEmpty(currentDungeonScene) 不成立；且当前副本场景不等于request.ScenePath；且当前副本场景不等于保留加载的主场景路径）时，执行下方分支。
                { // 开始条件分支。
                    var old = SceneManager.GetSceneByPath(currentDungeonScene); // 把 SceneManager.GetSceneByPath 的返回值 保存为 old。
                    if (old.isLoaded) // 当old.isLoaded为 true时，执行下方分支。
                        await AsyncHelper.Await(SceneManager.UnloadSceneAsync(old)); // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
                } // 结束条件分支。

                currentDungeonScene = request.ScenePath == persistentScenePath ? null : request.ScenePath; // 更新 当前副本场景，使用 按条件 request.ScenePath == persistentScenePath 选择的值。
                success = true; // 更新 是否成功，使用 true（启用/满足）。
            } // 结束受异常保护的操作。
            catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                Guisha.Core.Logger.Error(error.ToString()); // 按 ERROR 级别输出错误，并按开关写文件。
                if (previous.isLoaded) // 当previous.isLoaded为 true时，执行下方分支。
                    SceneManager.SetActiveScene(previous); // 切换 Unity 当前活动场景。
                if (added && loaded.isLoaded) // 当（added为 true；且loaded.isLoaded为 true）时，执行下方分支。
                    await AsyncHelper.Await(SceneManager.UnloadSceneAsync(loaded)); // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
            } // 结束异常处理。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                busy = false; // 更新 场景切换进行中标记，使用 false（关闭/不满足）。
                EventBus.Publish(new SceneTransitionFinished(request.ScenePath, request.Dungeon, success)); // 广播 SceneTransitionFinished 事件，通知场景切换完成或失败。
                request.Completed?.Invoke(success); // 仅在 request.Completed 非空时，执行 request.Completed 回调；仅在回调非空时调用。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 释放自身事件订阅，最后调用基类清理主单例引用。
        /// </summary>
        protected override void OnDestroy() // 释放自身事件订阅，最后调用基类清理主单例引用。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            base.OnDestroy(); // 执行基类销毁清理，保留单例引用等释放约定。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
