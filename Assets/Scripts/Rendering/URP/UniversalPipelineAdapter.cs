#if GUISHA_URP // 仅当对应管线包已安装且版本匹配时编译。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering.Universal; // 引入 UnityEngine.Rendering.Universal 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>仅在安装 URP 时编译的相机和光照适配器。</summary>
    public sealed class UniversalPipelineAdapter : IRenderPipelineAdapter // 声明 UniversalPipelineAdapter，供当前模块使用。
    { // 开始类型。
        /// <summary>此适配器支持的管线类别。</summary>
        public RenderPipelineKind Kind => RenderPipelineKind.Universal; // 声明此适配器支持的管线类别。

        /// <summary>运行前登记适配器；编辑器中同样可供预览和资源生成使用。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // 运行前登记适配器；编辑器中同样可供预览和资源生成使用。
#if UNITY_EDITOR // 仅在 Unity 编辑器中启用此初始化入口。
        [UnityEditor.InitializeOnLoadMethod] // 由 Unity/NUnit 属性声明创建菜单、初始化时机、组件约束或测试用例。
#endif // 结束上方条件编译范围。
        private static void Register() // 执行 private static void Register()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            RenderPipelineService.Register(new UniversalPipelineAdapter()); // 登记可选适配器，让核心服务通过接口使用当前管线。
        } // 结束当前代码块。

        /// <summary>补齐 URP 相机数据；只对模板相机设置背景和默认抗锯齿。</summary>
        public void PrepareCamera(Camera camera, bool templateDefaults, RenderingSettings settings) // 补齐 URP 相机数据；只对模板相机设置背景和默认抗锯齿。
        { // 开始当前代码块。
            var data = camera.GetUniversalAdditionalCameraData(); // 取得或创建管线附加数据组件，保存 camera.GetUniversalAdditionalCameraData() 的结果。
            if (!templateDefaults) // 仅当 (!templateDefaults) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            camera.allowHDR = true; // 更新 camera.allowHDR，取值为 true。
            camera.clearFlags = CameraClearFlags.SolidColor; // 更新 camera.clearFlags，取值为 CameraClearFlags.SolidColor。
            camera.backgroundColor = settings.background; // 更新 camera.backgroundColor，取值为 settings.background。
            data.renderPostProcessing = true; // 更新 data.renderPostProcessing，取值为 true。
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing; // 更新 data.antialiasing，取值为 AntialiasingMode.FastApproximateAntialiasing。
        } // 结束当前代码块。

        /// <summary>URP 方向光沿用模板配置的普通强度。</summary>
        public void ConfigureSun(Light sun, RenderingSettings settings) // URP 方向光沿用模板配置的普通强度。
        { // 开始当前代码块。
            sun.intensity = settings.universalSunIntensity; // 更新 sun.intensity，取值为 settings.universalSunIntensity。
        } // 结束当前代码块。

        /// <summary>URP 没有本适配器持有的附加灯光逻辑。</summary>
        public void ReleaseSun(Light sun) // URP 没有本适配器持有的附加灯光逻辑。
        { // 开始当前代码块。
        } // 结束当前代码块。

        /// <summary>URP 模板不需要额外固定曝光 Volume。</summary>
        public GameObject CreateEnvironment(Transform parent, RenderingSettings settings) => null; // URP 模板不需要额外固定曝光 Volume。
        /// <summary>基础 URP 材质状态由公共转换器写入；主贴图不需要额外关键字。</summary>
        public void ValidateMaterial(Material material) // 基础 URP 材质状态由公共转换器写入；主贴图不需要额外关键字。
        { // 开始当前代码块。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
#endif // 结束上方条件编译范围。
