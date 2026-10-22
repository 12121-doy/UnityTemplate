#if GUISHA_HDRP // 仅当对应管线包已安装且版本匹配时编译。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering; // 引入 UnityEngine.Rendering 中使用的类型。
using UnityEngine.Rendering.HighDefinition; // 引入 UnityEngine.Rendering.HighDefinition 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>仅在安装 HDRP 时编译，处理附加相机、Lux 光照、固定曝光与材质关键字。</summary>
    public sealed class HighDefinitionPipelineAdapter : IRenderPipelineAdapter // 声明 HighDefinitionPipelineAdapter，供当前模块使用。
    { // 开始类型。
        /// <summary>此适配器支持的管线类别。</summary>
        public RenderPipelineKind Kind => RenderPipelineKind.HighDefinition; // 声明此适配器支持的管线类别。

        /// <summary>运行/编辑器初始化时登记 HDRP 实现，不让业务程序集直接依赖 HDRP。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // 运行/编辑器初始化时登记 HDRP 实现，不让业务程序集直接依赖 HDRP。
#if UNITY_EDITOR // 仅在 Unity 编辑器中启用此初始化入口。
        [UnityEditor.InitializeOnLoadMethod] // 由 Unity/NUnit 属性声明创建菜单、初始化时机、组件约束或测试用例。
#endif // 结束上方条件编译范围。
        private static void Register() // 执行 private static void Register()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            RenderPipelineService.Register(new HighDefinitionPipelineAdapter()); // 登记可选适配器，让核心服务通过接口使用当前管线。
        } // 结束当前代码块。

        /// <summary>补齐 HDRP 相机数据；模板镜头使用确定的背景色与曝光 Volume 层。</summary>
        public void PrepareCamera(Camera camera, bool templateDefaults, RenderingSettings settings) // 补齐 HDRP 相机数据；模板镜头使用确定的背景色与曝光 Volume 层。
        { // 开始当前代码块。
            var data = camera.GetComponent<HDAdditionalCameraData>(); // 取得或创建管线附加数据组件，保存 camera.GetComponent<HDAdditionalCameraData>() 的结果。
            if (data == null) // 仅当 (data == null) 成立时执行后续处理。
                data = camera.gameObject.AddComponent<HDAdditionalCameraData>(); // 更新 管线附加数据组件，取值为 camera.gameObject.AddComponent<HDAdditionalCameraData>()。
            if (!templateDefaults) // 仅当 (!templateDefaults) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            camera.allowHDR = true; // 更新 camera.allowHDR，取值为 true。
            data.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color; // 更新 data.clearColorMode，取值为 HDAdditionalCameraData.ClearColorMode.Color。
            data.backgroundColorHDR = settings.background; // 更新 data.backgroundColorHDR，取值为 settings.background。
            data.volumeLayerMask = ~0; // 更新 data.volumeLayerMask，取值为 ~0。
            data.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing; // 更新 data.antialiasing，取值为 HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing。
        } // 结束当前代码块。

        /// <summary>显式设置 HDRP 的 Lux 单位，避免把内置的 0.8 强度当作 0.8 Lux。</summary>
        public void ConfigureSun(Light sun, RenderingSettings settings) // 显式设置 HDRP 的 Lux 单位，避免把内置的 0.8 强度当作 0.8 Lux。
        { // 开始当前代码块。
            var data = sun.GetComponent<HDAdditionalLightData>(); // 取得或创建管线附加数据组件，保存 sun.GetComponent<HDAdditionalLightData>() 的结果。
            if (data == null) // 仅当 (data == null) 成立时执行后续处理。
                data = sun.gameObject.AddComponent<HDAdditionalLightData>(); // 更新 管线附加数据组件，取值为 sun.gameObject.AddComponent<HDAdditionalLightData>()。
            data.enabled = true; // 更新 data.enabled，取值为 true。
            data.SetIntensity(settings.highDefinitionSunLux, LightUnit.Lux); // 使用 Lux 配置 HDRP 方向光，保持模板的可见照明。
        } // 结束当前代码块。

        /// <summary>切换离开 HDRP 时停用附加灯光更新，再由新适配器设置普通强度。</summary>
        public void ReleaseSun(Light sun) // 切换离开 HDRP 时停用附加灯光更新，再由新适配器设置普通强度。
        { // 开始当前代码块。
            var data = sun.GetComponent<HDAdditionalLightData>(); // 取得或创建管线附加数据组件，保存 sun.GetComponent<HDAdditionalLightData>() 的结果。
            if (data != null) // 仅当 (data != null) 成立时执行后续处理。
                data.enabled = false; // 更新 data.enabled，取值为 false。
        } // 结束当前代码块。

        /// <summary>创建仅供模板内容使用的固定曝光，防止 HDRP 默认曝光造成灰盒过暗。</summary>
        public GameObject CreateEnvironment(Transform parent, RenderingSettings settings) // 创建仅供模板内容使用的固定曝光，防止 HDRP 默认曝光造成灰盒过暗。
        { // 开始当前代码块。
            var root = new GameObject("Guisha_HDRP_Exposure"); // 取得或创建待适配的对象根节点，保存 new GameObject("Guisha_HDRP_Exposure") 的结果。
            root.transform.SetParent(parent, false); // 执行 root.transform.SetParent(parent, false)，完成当前渲染适配步骤。
            var owner = root.AddComponent<HighDefinitionEnvironment>(); // 取得或创建测试对象或生命周期拥有者，保存 root.AddComponent<HighDefinitionEnvironment>() 的结果。
            owner.Initialize(settings.highDefinitionExposureEV); // 执行 owner.Initialize(settings.highDefinitionExposureEV)，完成当前渲染适配步骤。
            return root; // 返回 待适配的对象根节点，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>通过 HDRP 公共 API 更新关键字、渲染队列和 Pass 状态。</summary>
        public void ValidateMaterial(Material material) // 通过 HDRP 公共 API 更新关键字、渲染队列和 Pass 状态。
        { // 开始当前代码块。
            HDMaterial.ValidateMaterial(material); // 调用 HDRP 公共材质校验接口，更新关键字、Pass 和渲染队列。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
#endif // 结束上方条件编译范围。
