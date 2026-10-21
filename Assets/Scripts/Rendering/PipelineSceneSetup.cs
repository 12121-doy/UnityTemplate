using Guisha.Core; // 引入 Guisha.Core 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>仅管理模板自己的相机、方向光与曝光环境，不覆盖美术场景已有的 Volume 配置。</summary>
    public sealed class PipelineSceneSetup : MonoBehaviour // 声明 PipelineSceneSetup，供当前模块使用。
    { // 开始类型。
        /// <summary>允许使用模板默认配置的主相机。</summary>
        public Camera templateCamera; // 声明允许使用模板默认配置的主相机。
        /// <summary>允许使用模板光照配置的方向光。</summary>
        public Light templateSun; // 声明允许使用模板光照配置的方向光。
        /// <summary>本次适配创建的曝光环境对象。</summary>
        private GameObject environment; // 声明本次适配创建的曝光环境对象。
        /// <summary>最近一次配置模板灯光的适配器。</summary>
        private IRenderPipelineAdapter activeAdapter; // 声明最近一次配置模板灯光的适配器。
        /// <summary>管线切换事件的退订令牌。</summary>
        private System.IDisposable subscription; // 声明管线切换事件的退订令牌。
        /// <summary>内容根激活时应用当前管线，并监听后续切换。</summary>
        private void OnEnable() // 内容根激活时应用当前管线，并监听后续切换。
        { // 开始当前代码块。
            Apply(); // 执行 Apply()，完成当前渲染适配步骤。
            subscription = EventBus.Subscribe<RenderPipelineChanged>(_ => Apply()); // 订阅对应类型事件，把模块连接到渲染适配流程，并保存退订令牌。
        } // 结束当前代码块。

        /// <summary>解除订阅；曝光子对象随内容根一起停用。</summary>
        private void OnDisable() // 解除订阅；曝光子对象随内容根一起停用。
        { // 开始当前代码块。
            subscription?.Dispose(); // 释放事件订阅令牌，防止组件销毁后继续收到回调。
            subscription = null; // 更新 管线切换事件的退订令牌，取值为 null。
        } // 结束当前代码块。

        /// <summary>重建模板环境并按实际管线设置相机和光照单位。</summary>
        public void Apply() // 重建模板环境并按实际管线设置相机和光照单位。
        { // 开始当前代码块。
            if (environment != null) // 仅当 (environment != null) 成立时执行后续处理。
            { // 开始当前代码块。
                environment.SetActive(false); // 切换对象激活状态，同时触发其组件和子对象的启停生命周期。
                Destroy(environment); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            } // 结束当前代码块。

            if (templateSun != null) // 仅当 (templateSun != null) 成立时执行后续处理。
                activeAdapter?.ReleaseSun(templateSun); // 停用上一个管线留下的附加灯光逻辑，再由新管线配置强度。
            var adapter = RenderPipelineService.RequireAdapter(); // 取得或创建当前管线的适配器，保存 RenderPipelineService.RequireAdapter() 的结果。
            activeAdapter = adapter; // 更新 最近一次配置模板灯光的适配器，取值为 adapter。
            var settings = RenderPipelineService.Settings; // 取得或创建加载或临时创建的渲染配置，保存 RenderPipelineService.Settings 的结果。
            if (templateCamera != null) // 仅当 (templateCamera != null) 成立时执行后续处理。
                adapter.PrepareCamera(templateCamera, true, settings); // 交给活动适配器补齐相机附加数据，并遵守模板默认值开关。
            if (templateSun != null) // 仅当 (templateSun != null) 成立时执行后续处理。
                adapter.ConfigureSun(templateSun, settings); // 按活动管线配置模板方向光的正确单位和强度。
            environment = adapter.CreateEnvironment(transform, settings); // 更新 本次适配创建的曝光环境对象，取值为 adapter.CreateEnvironment(transform, settings)。
        } // 结束当前代码块。

        /// <summary>释放运行时生成的环境对象。</summary>
        private void OnDestroy() // 释放运行时生成的环境对象。
        { // 开始当前代码块。
            subscription?.Dispose(); // 释放事件订阅令牌，防止组件销毁后继续收到回调。
            if (environment != null) // 仅当 (environment != null) 成立时执行后续处理。
                Destroy(environment); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
