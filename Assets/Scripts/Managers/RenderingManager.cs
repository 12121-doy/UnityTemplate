using Guisha.Core; // 引入 Guisha.Core 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using Guisha.Rendering; // 引入 Guisha.Rendering 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering; // 引入 UnityEngine.Rendering 中使用的类型。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 中使用的类型。

namespace Guisha.Managers // 把渲染相关类型放入 Guisha.Managers 命名空间。
{ // 开始命名空间。
    /// <summary>跨场景的渲染适配入口；处理已加载场景、Addressables 实体和活动管线变化。</summary>
    public sealed class RenderingManager : MonoSingleton<RenderingManager> // 声明 RenderingManager，供当前模块使用。
    { // 开始类型。
        /// <summary>统一释放的框架事件订阅集合。</summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 声明统一释放的框架事件订阅集合。
        /// <summary>上一帧实际启用的管线资产。</summary>
        private RenderPipelineAsset previousAsset; // 声明上一帧实际启用的管线资产。
        /// <summary>上一帧识别的管线类别。</summary>
        private RenderPipelineKind previousKind; // 声明上一帧识别的管线类别。
        /// <summary>主实例订阅实体生成；场景中的对象由 sceneLoaded 统一准备。</summary>
        protected override void Awake() // 主实例订阅实体生成；场景中的对象由 sceneLoaded 统一准备。
        { // 开始当前代码块。
            base.Awake(); // 先完成 Mono 单例基类初始化，再判断本实例是否拥有管理职责。
            if (!IsPrimaryInstance) // 仅当 (!IsPrimaryInstance) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            previousAsset = GraphicsSettings.currentRenderPipeline; // 更新 上一帧实际启用的管线资产，取值为 GraphicsSettings.currentRenderPipeline。
            previousKind = RenderPipelineService.Current; // 更新 上一帧识别的管线类别，取值为 RenderPipelineService.Current。
            SceneManager.sceneLoaded += SceneLoaded; // 监听后续场景加载，为附加场景和切换后的场景准备渲染。
            subscriptions.Add(EventBus.Subscribe<EntitySpawned>(e => // 订阅对应类型事件，把模块连接到渲染适配流程，并保存退订令牌。
            { // 开始当前代码块。
                if (e.Entity != null) // 仅当 (e.Entity != null) 成立时执行后续处理。
                    RenderPipelineService.PrepareHierarchy(e.Entity.gameObject); // 为层级中的渲染组件和相机应用当前管线的适配。
            })); // 执行 }))，完成当前渲染适配步骤。
        } // 结束当前代码块。

        /// <summary>启动器运行顺序不影响首次场景准备，场景加载完成时所有适配器已注册。</summary>
        private void SceneLoaded(Scene scene, LoadSceneMode mode) // 启动器运行顺序不影响首次场景准备，场景加载完成时所有适配器已注册。
        { // 开始当前代码块。
            foreach (var root in scene.GetRootGameObjects()) // 遍历 scene.GetRootGameObjects()，逐个处理并保持资源拥有权。
                RenderPipelineService.PrepareHierarchy(root); // 为层级中的渲染组件和相机应用当前管线的适配。
        } // 结束当前代码块。

        /// <summary>检测质量等级或 Graphics 设置切换；只在资产实际变化时重新适配。</summary>
        private void Update() // 检测质量等级或 Graphics 设置切换；只在资产实际变化时重新适配。
        { // 开始当前代码块。
            if (previousAsset == GraphicsSettings.currentRenderPipeline) // 仅当 (previousAsset == GraphicsSettings.currentRenderPipeline) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            var old = previousKind; // 取得或创建切换前的管线类别，保存 previousKind 的结果。
            previousAsset = GraphicsSettings.currentRenderPipeline; // 更新 上一帧实际启用的管线资产，取值为 GraphicsSettings.currentRenderPipeline。
            previousKind = RenderPipelineService.Current; // 更新 上一帧识别的管线类别，取值为 RenderPipelineService.Current。
            EventBus.Publish(new RenderPipelineChanged(old, previousKind)); // 广播类型事件，由订阅模块完成生成或管线变化响应。
            for (int i = 0; i < SceneManager.sceneCount; i++) // 按 (int i = 0; i < SceneManager.sceneCount; i++) 中的初始化、继续条件和递增步骤循环。
            { // 开始当前代码块。
                var scene = SceneManager.GetSceneAt(i); // 取得或创建当前处理的场景，保存 SceneManager.GetSceneAt(i) 的结果。
                if (scene.isLoaded) // 仅当 (scene.isLoaded) 成立时执行后续处理。
                    SceneLoaded(scene, LoadSceneMode.Additive); // 执行 SceneLoaded(scene, LoadSceneMode.Additive)，完成当前渲染适配步骤。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>解除自己的 Unity/框架事件订阅，再清理单例引用。</summary>
        protected override void OnDestroy() // 解除自己的 Unity/框架事件订阅，再清理单例引用。
        { // 开始当前代码块。
            if (IsPrimaryInstance) // 仅当 (IsPrimaryInstance) 成立时执行后续处理。
                SceneManager.sceneLoaded -= SceneLoaded; // 解除本管理器的 Unity 场景加载监听。
            subscriptions.Dispose(); // 释放事件订阅令牌，防止组件销毁后继续收到回调。
            base.OnDestroy(); // 让单例基类清理其持有的主实例引用。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
