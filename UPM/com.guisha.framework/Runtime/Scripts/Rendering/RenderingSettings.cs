using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>三种管线的模板材质与曝光配置；Resources 中的显式 Shader 引用避免构建时被剥离。</summary>
    [CreateAssetMenu(menuName = "Guisha/Rendering Settings")] // 声明 RenderingSettings，供当前模块使用。
    public sealed class RenderingSettings : ScriptableObject // 执行 public sealed class RenderingSettings : ScriptableObject，完成当前渲染适配步骤。
    { // 开始类型。
        /// <summary>内置 Standard Shader 引用。</summary>
        public Shader builtInLit; // 声明内置 Standard Shader 引用。
        /// <summary>保留主颜色与贴图的内置 Unlit Shader 引用。</summary>
        public Shader builtInUnlit; // 声明保留主颜色与贴图的内置 Unlit Shader 引用。
        /// <summary>URP Lit Shader 引用。</summary>
        public Shader universalLit; // 声明URP Lit Shader 引用。
        /// <summary>URP Unlit Shader 引用。</summary>
        public Shader universalUnlit; // 声明URP Unlit Shader 引用。
        /// <summary>HDRP Lit Shader 引用。</summary>
        public Shader highDefinitionLit; // 声明HDRP Lit Shader 引用。
        /// <summary>HDRP Unlit Shader 引用。</summary>
        public Shader highDefinitionUnlit; // 声明HDRP Unlit Shader 引用。
        /// <summary>内置管线模板方向光强度。</summary>
        [Min(0)] // 声明内置管线模板方向光强度。
        public float builtInSunIntensity = 0.8f; // 执行 public float builtInSunIntensity = 0.8f，完成当前渲染适配步骤。
        /// <summary>URP 模板方向光强度。</summary>
        [Min(0)] // 声明URP 模板方向光强度。
        public float universalSunIntensity = 0.8f; // 执行 public float universalSunIntensity = 0.8f，完成当前渲染适配步骤。
        /// <summary>HDRP 模板方向光照度，单位 Lux。</summary>
        [Min(0)] // 声明HDRP 模板方向光照度，单位 Lux。
        public float highDefinitionSunLux = 10000; // 执行 public float highDefinitionSunLux = 10000，完成当前渲染适配步骤。
        /// <summary>HDRP 模板固定曝光 EV100。</summary>
        public float highDefinitionExposureEV = 10; // 声明HDRP 模板固定曝光 EV100。
        /// <summary>模板相机的背景颜色。</summary>
        public Color background = new Color(0.035f, 0.04f, 0.055f); // 声明模板相机的背景颜色。
        /// <summary>按实际活动管线取得构建中保留的 Shader 引用。</summary>
        /// <param name = "kind">当前活动管线。</param>
        /// <param name = "unlit">是否需要不受光材质。</param>
        /// <returns>配置的 Shader；未配置时返回 null，由服务显式查找。</returns>
        public Shader GetShader(RenderPipelineKind kind, bool unlit) // 按实际活动管线取得构建中保留的 Shader 引用。
        { // 开始当前代码块。
            switch (kind) // 按指定管线类别选择对应实现。
            { // 开始当前代码块。
                case RenderPipelineKind.BuiltIn: // 匹配 RenderPipelineKind.BuiltIn 时使用此分支。
                    return unlit ? builtInUnlit : builtInLit; // 返回 unlit ? builtInUnlit : builtInLit，交给调用方继续处理。
                case RenderPipelineKind.Universal: // 匹配 RenderPipelineKind.Universal 时使用此分支。
                    return unlit ? universalUnlit : universalLit; // 返回 unlit ? universalUnlit : universalLit，交给调用方继续处理。
                case RenderPipelineKind.HighDefinition: // 匹配 RenderPipelineKind.HighDefinition 时使用此分支。
                    return unlit ? highDefinitionUnlit : highDefinitionLit; // 返回 unlit ? highDefinitionUnlit : highDefinitionLit，交给调用方继续处理。
                default: // 处理未匹配的管线类别。
                    return null; // 返回 null，交给调用方继续处理。
            } // 结束当前代码块。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
