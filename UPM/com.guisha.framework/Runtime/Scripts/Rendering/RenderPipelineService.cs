using System; // 引入 System 中使用的类型。
using System.Collections.Generic; // 引入 System.Collections.Generic 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering; // 引入 UnityEngine.Rendering 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>可选管线适配器契约；核心程序集不引用 URP/HDRP 包。</summary>
    public interface IRenderPipelineAdapter // 声明 IRenderPipelineAdapter，供当前模块使用。
    { // 开始适配契约。
        /// <summary>此适配器支持的管线类别。</summary>
        RenderPipelineKind Kind { get; } // 声明此适配器支持的管线类别。

        /// <summary>补齐相机的管线附加组件，只有模板相机才使用模板默认值。</summary>
        void PrepareCamera(Camera camera, bool templateDefaults, RenderingSettings settings); // 补齐相机的管线附加组件，只有模板相机才使用模板默认值。
        /// <summary>按该管线的光度单位配置模板方向光。</summary>
        void ConfigureSun(Light sun, RenderingSettings settings); // 按该管线的光度单位配置模板方向光。
        /// <summary>离开当前管线前停用本适配器的灯光附加逻辑。</summary>
        void ReleaseSun(Light sun); // 离开当前管线前停用本适配器的灯光附加逻辑。
        /// <summary>为模板创建必要的环境对象，返回由调用方销毁的根对象。</summary>
        GameObject CreateEnvironment(Transform parent, RenderingSettings settings); // 为模板创建必要的环境对象，返回由调用方销毁的根对象。
        /// <summary>刷新新建材质的管线专属关键字。</summary>
        void ValidateMaterial(Material material); // 刷新新建材质的管线专属关键字。
    } // 结束适配契约。

    /// <summary>检测真实活动管线，选择可选适配器，并统一准备材质和对象层级。</summary>
    public static class RenderPipelineService // 声明 RenderPipelineService，供当前模块使用。
    { // 开始类型。
        /// <summary>管线类别到可选适配器的注册表。</summary>
        private static readonly Dictionary<RenderPipelineKind, IRenderPipelineAdapter> Adapters = new Dictionary<RenderPipelineKind, IRenderPipelineAdapter>(); // 声明管线类别到可选适配器的注册表。
        /// <summary>加载或临时创建的渲染配置。</summary>
        private static RenderingSettings settings; // 声明加载或临时创建的渲染配置。
        /// <summary>缺少 Resources 资产时使用的临时配置。</summary>
        private static RenderingSettings defaults; // 声明缺少 Resources 资产时使用的临时配置。
        /// <summary>GraphicsSettings.currentRenderPipeline 已包含当前质量等级的管线覆盖。</summary>
        public static RenderPipelineKind Current => Classify(GraphicsSettings.currentRenderPipeline); // 声明当前实际活动的管线类别。

        /// <summary>读取用户配置；没有配置资产时创建仅在运行时存在的默认配置。</summary>
        public static RenderingSettings Settings // 声明缓存的渲染设置入口。
        { // 开始当前代码块。
            get // 读取属性时检查缓存，必要时加载配置。
            { // 开始当前代码块。
                if (settings != null) // 仅当 (settings != null) 成立时执行后续处理。
                    return settings; // 返回 加载或临时创建的渲染配置，交给调用方继续处理。
                settings = Resources.Load<RenderingSettings>("Guisha/RenderingSettings"); // 更新 加载或临时创建的渲染配置，取值为 Resources.Load<RenderingSettings>("Guisha/RenderingSettings")。
                if (settings == null) // 仅当 (settings == null) 成立时执行后续处理。
                { // 开始当前代码块。
                    if (defaults == null) // 仅当 (defaults == null) 成立时执行后续处理。
                    { // 开始当前代码块。
                        defaults = ScriptableObject.CreateInstance<RenderingSettings>(); // 更新 缺少 Resources 资产时使用的临时配置，取值为 ScriptableObject.CreateInstance<RenderingSettings>()。
                        defaults.hideFlags = HideFlags.HideAndDontSave; // 更新 defaults.hideFlags，取值为 HideFlags.HideAndDontSave。
                    } // 结束当前代码块。

                    settings = defaults; // 更新 加载或临时创建的渲染配置，取值为 defaults。
                } // 结束当前代码块。

                return settings; // 返回 加载或临时创建的渲染配置，交给调用方继续处理。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>按资产类型及其基类识别管线，允许用户派生管线资产。</summary>
        /// <param name = "asset">实际启用的资产；null 表示内置管线。</param>
        /// <returns>内置、URP、HDRP 或未知 SRP。</returns>
        public static RenderPipelineKind Classify(RenderPipelineAsset asset) // 按资产类型及其基类识别管线，允许用户派生管线资产。
        { // 开始当前代码块。
            if (asset == null) // 仅当 (asset == null) 成立时执行后续处理。
                return RenderPipelineKind.BuiltIn; // 返回 RenderPipelineKind.BuiltIn，交给调用方继续处理。
            for (var type = asset.GetType(); type != null; type = type.BaseType) // 按 (var type = asset.GetType(); type != null; type = type.BaseType) 中的初始化、继续条件和递增步骤循环。
            { // 开始当前代码块。
                if (type.FullName == "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset") // 仅当 (type.FullName == "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset") 成立时执行后续处理。
                    return RenderPipelineKind.Universal; // 返回 RenderPipelineKind.Universal，交给调用方继续处理。
                if (type.FullName == "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset") // 仅当 (type.FullName == "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset") 成立时执行后续处理。
                    return RenderPipelineKind.HighDefinition; // 返回 RenderPipelineKind.HighDefinition，交给调用方继续处理。
            } // 结束当前代码块。

            return RenderPipelineKind.Unsupported; // 返回 RenderPipelineKind.Unsupported，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>由已安装包对应的可选程序集登记适配器；后注册的同类实现替换旧实现。</summary>
        /// <param name = "adapter">已经初始化的管线适配器。</param>
        public static void Register(IRenderPipelineAdapter adapter) // 由已安装包对应的可选程序集登记适配器；后注册的同类实现替换旧实现。
        { // 开始当前代码块。
            if (adapter == null) // 仅当 (adapter == null) 成立时执行后续处理。
                throw new ArgumentNullException(nameof(adapter)); // 通过异常报告具体失败原因，终止本次无效调用。
            Adapters[adapter.Kind] = adapter; // 更新 Adapters[adapter.Kind]，取值为 adapter。
        } // 结束当前代码块。

        /// <summary>取得当前管线适配器；未知管线或缺少可选程序集时明确报错。</summary>
        /// <returns>能够处理当前活动管线的实现。</returns>
        public static IRenderPipelineAdapter RequireAdapter() // 取得当前管线适配器；未知管线或缺少可选程序集时明确报错。
        { // 开始当前代码块。
            if (Current == RenderPipelineKind.BuiltIn) // 仅当 (Current == RenderPipelineKind.BuiltIn) 成立时执行后续处理。
                return BuiltInAdapter.Instance; // 返回 BuiltInAdapter.Instance，交给调用方继续处理。
            if (Adapters.TryGetValue(Current, out var adapter)) // 仅当 (Adapters.TryGetValue(Current, out var adapter)) 成立时执行后续处理。
                return adapter; // 返回 当前管线的适配器，交给调用方继续处理。
            throw new InvalidOperationException("缺少活动管线适配器：" + Current + "。请安装 Unity 2022.3 对应的 URP/HDRP 14.x 包并等待编译完成。"); // 通过异常报告具体失败原因，终止本次无效调用。
        } // 结束当前代码块。

        /// <summary>返回目标管线的 Lit/Unlit Shader 名称，不按包是否安装猜测管线。</summary>
        public static string ShaderName(RenderPipelineKind kind, bool unlit) // 返回目标管线的 Lit/Unlit Shader 名称，不按包是否安装猜测管线。
        { // 开始当前代码块。
            switch (kind) // 按指定管线类别选择对应实现。
            { // 开始当前代码块。
                case RenderPipelineKind.BuiltIn: // 匹配 RenderPipelineKind.BuiltIn 时使用此分支。
                    return unlit ? "Guisha/BuiltIn/Unlit" : "Standard"; // 返回 unlit ? "Guisha/BuiltIn/Unlit" : "Standard"，交给调用方继续处理。
                case RenderPipelineKind.Universal: // 匹配 RenderPipelineKind.Universal 时使用此分支。
                    return unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"; // 返回 unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"，交给调用方继续处理。
                case RenderPipelineKind.HighDefinition: // 匹配 RenderPipelineKind.HighDefinition 时使用此分支。
                    return unlit ? "HDRP/Unlit" : "HDRP/Lit"; // 返回 unlit ? "HDRP/Unlit" : "HDRP/Lit"，交给调用方继续处理。
                default: // 处理未匹配的管线类别。
                    throw new NotSupportedException("未适配此自定义渲染管线。"); // 通过异常报告具体失败原因，终止本次无效调用。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>创建当前管线的独立模板材质；调用方拥有并负责销毁。</summary>
        public static Material CreateMaterial(Color color, bool unlit = false) // 创建当前管线的独立模板材质；调用方拥有并负责销毁。
        { // 开始当前代码块。
            var shader = Settings.GetShader(Current, unlit); // 取得或创建当前管线所需的 Shader，保存 Settings.GetShader(Current, unlit) 的结果。
            if (shader == null) // 仅当 (shader == null) 成立时执行后续处理。
                shader = Shader.Find(ShaderName(Current, unlit)); // 更新 当前管线所需的 Shader，取值为 Shader.Find(ShaderName(Current, unlit))。
            if (shader == null || !shader.isSupported) // 仅当 (shader == null || !shader.isSupported) 成立时执行后续处理。
                throw new InvalidOperationException("当前管线 Shader 不可用：" + ShaderName(Current, unlit) + "。请运行 Guisha/Rendering/Refresh Shader References 后重新构建。"); // 通过异常报告具体失败原因，终止本次无效调用。
            var result = new Material(shader) // 执行 var result = new Material(shader)，完成当前渲染适配步骤。
            { // 开始当前代码块。
                name = "Guisha_" + Current + (unlit ? "_Unlit" : "_Lit") // 执行 name = "Guisha_" + Current + (unlit ? "_Unlit" : "_Lit")，完成当前渲染适配步骤。
            }; // 结束上方的回调、参数或初始化列表。
            SetColor(result, color); // 执行 SetColor(result, color)，完成当前渲染适配步骤。
            RequireAdapter().ValidateMaterial(result); // 在颜色、纹理与表面属性设置后刷新材质的管线状态。
            return result; // 返回 新创建的目标材质，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>识别框架可自动迁移的标准 Lit/Unlit 材质；UI 和自定义 Shader 保持原有实现。</summary>
        public static bool IsTemplateShader(string shaderName, out bool unlit) // 识别框架可自动迁移的标准 Lit/Unlit 材质；UI 和自定义 Shader 保持原有实现。
        { // 开始当前代码块。
            unlit = shaderName == "Guisha/BuiltIn/Unlit" || shaderName == "Unlit/Color" || shaderName == "Unlit/Texture" || shaderName == "Universal Render Pipeline/Unlit" || shaderName == "HDRP/Unlit"; // 更新 是否使用无光照材质，取值为 shaderName == "Guisha/BuiltIn/Unlit" || shaderName == "Unlit/Color" || shaderName == "Unlit/Texture" || shaderName == "Universal Render Pipeline/Unlit" || shaderName == "HDRP/Unlit"。
            return unlit || shaderName == "Standard" || shaderName == "Universal Render Pipeline/Lit" || shaderName == "HDRP/Lit"; // 返回 unlit || shaderName == "Standard" || shaderName == "Universal Render Pipeline/Lit" || shaderName == "HDRP/Lit"，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>创建适配副本，保留基本颜色、主纹理、金属度和平滑度；不修改共享源资产。</summary>
        /// <returns>转换后的独立材质；无需转换时返回源材质。</returns>
        public static Material AdaptMaterial(Material source) // 创建适配副本，保留基本颜色、主纹理、金属度和平滑度；不修改共享源资产。
        { // 开始当前代码块。
            if (source == null) // 仅当 (source == null) 成立时执行后续处理。
                return CreateMaterial(Color.white); // 返回 CreateMaterial(Color.white)，交给调用方继续处理。
            if (!IsTemplateShader(source.shader.name, out var unlit)) // 仅当 (!IsTemplateShader(source.shader.name, out var unlit)) 成立时执行后续处理。
                return source; // 返回 转换前的源材质，交给调用方继续处理。
            if (source.shader.name == ShaderName(Current, unlit)) // 仅当 (source.shader.name == ShaderName(Current, unlit)) 成立时执行后续处理。
                return source; // 返回 转换前的源材质，交给调用方继续处理。
            var colorName = source.HasProperty("_BaseColor") ? "_BaseColor" : source.HasProperty("_UnlitColor") ? "_UnlitColor" : "_Color"; // 取得或创建源材质的主颜色属性名，保存 source.HasProperty("_BaseColor") ? "_BaseColor" : source.HasProperty("_UnlitColor") ? "_UnlitColor" : "_Color" 的结果。
            var target = CreateMaterial(source.HasProperty(colorName) ? source.GetColor(colorName) : Color.white, unlit); // 取得或创建绑定的 Renderer 组件，保存 CreateMaterial(source.HasProperty(colorName) ? source.GetColor(colorName) : Color.white, unlit) 的结果。
            var sourceMap = FirstProperty(source, "_BaseColorMap", "_BaseMap", "_UnlitColorMap", "_MainTex"); // 取得或创建源材质的主贴图属性名，保存 FirstProperty(source, "_BaseColorMap", "_BaseMap", "_UnlitColorMap", "_MainTex") 的结果。
            var targetMap = FirstProperty(target, "_BaseColorMap", "_BaseMap", "_UnlitColorMap", "_MainTex"); // 取得或创建目标材质的主贴图属性名，保存 FirstProperty(target, "_BaseColorMap", "_BaseMap", "_UnlitColorMap", "_MainTex") 的结果。
            if (sourceMap != null && targetMap != null) // 仅当 (sourceMap != null && targetMap != null) 成立时执行后续处理。
            { // 开始当前代码块。
                target.SetTexture(targetMap, source.GetTexture(sourceMap)); // 保留源主贴图引用，避免为切换管线重复复制纹理。
                target.SetTextureScale(targetMap, source.GetTextureScale(sourceMap)); // 把源贴图的重复比例复制到目标管线的主贴图属性。
                target.SetTextureOffset(targetMap, source.GetTextureOffset(sourceMap)); // 把源贴图的 UV 偏移复制到目标管线的主贴图属性。
            } // 结束当前代码块。

            if (source.HasProperty("_Metallic") && target.HasProperty("_Metallic")) // 仅当 (source.HasProperty("_Metallic") && target.HasProperty("_Metallic")) 成立时执行后续处理。
                target.SetFloat("_Metallic", source.GetFloat("_Metallic")); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
            var smooth = FirstProperty(source, "_Smoothness", "_Glossiness"); // 取得或创建源材质的平滑度属性名，保存 FirstProperty(source, "_Smoothness", "_Glossiness") 的结果。
            var targetSmooth = FirstProperty(target, "_Smoothness", "_Glossiness"); // 取得或创建目标材质的平滑度属性名，保存 FirstProperty(target, "_Smoothness", "_Glossiness") 的结果。
            if (smooth != null && targetSmooth != null) // 仅当 (smooth != null && targetSmooth != null) 成立时执行后续处理。
                target.SetFloat(targetSmooth, source.GetFloat(smooth)); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
            var cutoff = FirstProperty(source, "_AlphaCutoff", "_Cutoff"); // 取得或创建源材质的透明裁剪阈值属性名，保存 FirstProperty(source, "_AlphaCutoff", "_Cutoff") 的结果。
            var targetCutoff = FirstProperty(target, "_AlphaCutoff", "_Cutoff"); // 取得或创建目标材质的透明裁剪阈值属性名，保存 FirstProperty(target, "_AlphaCutoff", "_Cutoff") 的结果。
            if (cutoff != null && targetCutoff != null) // 仅当 (cutoff != null && targetCutoff != null) 成立时执行后续处理。
                target.SetFloat(targetCutoff, source.GetFloat(cutoff)); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
            bool transparent = (source.HasProperty("_Surface") && source.GetFloat("_Surface") > 0) || (source.HasProperty("_SurfaceType") && source.GetFloat("_SurfaceType") > 0) || (source.HasProperty("_Mode") && source.GetFloat("_Mode") >= 2); // 取得或创建源材质是否为透明表面，保存 (source.HasProperty("_Surface") && source.GetFloat("_Surface") > 0) || (source.HasProperty("_SurfaceType") && source.GetFloat("_SurfaceType") > 0) || (source.HasProperty("_Mode") && source.GetFloat("_Mode") >= 2) 的结果。
            if (transparent) // 仅当 (transparent) 成立时执行后续处理。
            { // 开始当前代码块。
                foreach (var property in new[] // 遍历 new[，逐个处理并保持资源拥有权。
                { // 开始当前代码块。
                    "_Surface", // 执行 "_Surface",，完成当前渲染适配步骤。
                    "_SurfaceType" // 执行 "_SurfaceType"，完成当前渲染适配步骤。
                } // 结束当前代码块。

                ) // 执行 )，完成当前渲染适配步骤。
                    if (target.HasProperty(property)) // 仅当 (target.HasProperty(property)) 成立时执行后续处理。
                        target.SetFloat(property, 1); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                if (target.HasProperty("_Mode")) // 仅当 (target.HasProperty("_Mode")) 成立时执行后续处理。
                    target.SetFloat("_Mode", 2); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                if (target.HasProperty("_SrcBlend")) // 仅当 (target.HasProperty("_SrcBlend")) 成立时执行后续处理。
                    target.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                if (target.HasProperty("_DstBlend")) // 仅当 (target.HasProperty("_DstBlend")) 成立时执行后续处理。
                    target.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                if (target.HasProperty("_ZWrite")) // 仅当 (target.HasProperty("_ZWrite")) 成立时执行后续处理。
                    target.SetFloat("_ZWrite", 0); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                target.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); // 启用材质特性关键字，使 Shader 编译分支与表面类型匹配。
                target.EnableKeyword("_ALPHABLEND_ON"); // 启用材质特性关键字，使 Shader 编译分支与表面类型匹配。
                target.renderQueue = (int)RenderQueue.Transparent; // 更新 target.renderQueue，取值为 (int)RenderQueue.Transparent。
            } // 结束当前代码块。

            if (source.IsKeywordEnabled("_ALPHATEST_ON") || (source.HasProperty("_AlphaCutoffEnable") && source.GetFloat("_AlphaCutoffEnable") > 0)) // 仅当 (source.IsKeywordEnabled("_ALPHATEST_ON") || (source.HasProperty("_AlphaCutoffEnable") && source.GetFloat("_AlphaCutoffEnable") > 0)) 成立时执行后续处理。
            { // 开始当前代码块。
                if (target.HasProperty("_Mode")) // 仅当 (target.HasProperty("_Mode")) 成立时执行后续处理。
                    target.SetFloat("_Mode", 1); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                if (target.HasProperty("_AlphaClip")) // 仅当 (target.HasProperty("_AlphaClip")) 成立时执行后续处理。
                    target.SetFloat("_AlphaClip", 1); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                if (target.HasProperty("_AlphaCutoffEnable")) // 仅当 (target.HasProperty("_AlphaCutoffEnable")) 成立时执行后续处理。
                    target.SetFloat("_AlphaCutoffEnable", 1); // 写入目标 Shader 支持的表面、混合、深度或裁剪属性。
                target.EnableKeyword("_ALPHATEST_ON"); // 启用材质特性关键字，使 Shader 编译分支与表面类型匹配。
                target.renderQueue = (int)RenderQueue.AlphaTest; // 更新 target.renderQueue，取值为 (int)RenderQueue.AlphaTest。
            } // 结束当前代码块。

            RequireAdapter().ValidateMaterial(target); // 在颜色、纹理与表面属性设置后刷新材质的管线状态。
            return target; // 返回 绑定的 Renderer 组件，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>按 Shader 的主颜色属性写入颜色，涵盖三条管线的 Lit/Unlit。</summary>
        public static void SetColor(Material material, Color color) // 按 Shader 的主颜色属性写入颜色，涵盖三条管线的 Lit/Unlit。
        { // 开始当前代码块。
            var name = FirstProperty(material, "_BaseColor", "_UnlitColor", "_Color"); // 取得或创建待查找的名称，保存 FirstProperty(material, "_BaseColor", "_UnlitColor", "_Color") 的结果。
            if (name != null) // 仅当 (name != null) 成立时执行后续处理。
                material.SetColor(name, color); // 通过目标 Shader 支持的属性写入基础颜色。
        } // 结束当前代码块。

        /// <summary>准备动态生成的角色、拾取物、特效与相机；包含未启用子对象。</summary>
        public static void PrepareHierarchy(GameObject root) // 准备动态生成的角色、拾取物、特效与相机；包含未启用子对象。
        { // 开始当前代码块。
            if (root == null) // 仅当 (root == null) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) // 遍历 root.GetComponentsInChildren<Renderer>(true)，逐个处理并保持资源拥有权。
            { // 开始当前代码块。
                var binding = renderer.GetComponent<PipelineMaterialBinding>(); // 取得或创建持有材质副本的绑定组件，保存 renderer.GetComponent<PipelineMaterialBinding>() 的结果。
                if (binding == null) // 仅当 (binding == null) 成立时执行后续处理。
                    binding = renderer.gameObject.AddComponent<PipelineMaterialBinding>(); // 更新 持有材质副本的绑定组件，取值为 renderer.gameObject.AddComponent<PipelineMaterialBinding>()。
                binding.Refresh(); // 仅在需要时重建适配材质，重复调用复用已有结果。
            } // 结束当前代码块。

            foreach (var camera in root.GetComponentsInChildren<Camera>(true)) // 遍历 root.GetComponentsInChildren<Camera>(true)，逐个处理并保持资源拥有权。
                RequireAdapter().PrepareCamera(camera, false, Settings); // 交给活动适配器补齐相机附加数据，并遵守模板默认值开关。
        } // 结束当前代码块。

        /// <summary>查找材质实际提供的第一个属性，屏蔽不同管线的属性名差异。</summary>
        private static string FirstProperty(Material material, params string[] names) // 查找材质实际提供的第一个属性，屏蔽不同管线的属性名差异。
        { // 开始当前代码块。
            foreach (var name in names) // 遍历 names，逐个处理并保持资源拥有权。
                if (material.HasProperty(name)) // 仅当 (material.HasProperty(name)) 成立时执行后续处理。
                    return name; // 返回 待查找的名称，交给调用方继续处理。
            return null; // 返回 null，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>进入 Play 时清空静态注册和默认配置，支持禁用 Domain Reload。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 进入 Play 时清空静态注册和默认配置，支持禁用 Domain Reload。
        private static void Reset() // 执行 private static void Reset()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            Adapters.Clear(); // 清空已失效的缓存或拥有权记录。
            settings = null; // 更新 加载或临时创建的渲染配置，取值为 null。
            if (defaults != null) // 仅当 (defaults != null) 成立时执行后续处理。
                UnityEngine.Object.Destroy(defaults); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            defaults = null; // 更新 缺少 Resources 资产时使用的临时配置，取值为 null。
        } // 结束当前代码块。

        /// <summary>内置管线不需要 SRP 的附加相机数据或曝光 Volume。</summary>
        private sealed class BuiltInAdapter : IRenderPipelineAdapter // 声明 BuiltInAdapter，供当前模块使用。
        { // 开始类型。
            /// <summary>内置管线适配器的共享实例。</summary>
            public static readonly BuiltInAdapter Instance = new BuiltInAdapter(); // 声明内置管线适配器的共享实例。
            /// <summary>此适配器支持的管线类别。</summary>
            public RenderPipelineKind Kind => RenderPipelineKind.BuiltIn; // 声明此适配器支持的管线类别。

            /// <summary>补齐当前管线需要的相机设置；只在允许模板默认值时覆盖外观配置。</summary>
            public void PrepareCamera(Camera camera, bool templateDefaults, RenderingSettings options) // 补齐相机的管线附加组件，只有模板相机才使用模板默认值。
            { // 开始当前代码块。
                if (templateDefaults) // 仅当 (templateDefaults) 成立时执行后续处理。
                { // 开始当前代码块。
                    camera.clearFlags = CameraClearFlags.SolidColor; // 更新 camera.clearFlags，取值为 CameraClearFlags.SolidColor。
                    camera.backgroundColor = options.background; // 更新 camera.backgroundColor，取值为 options.background。
                    camera.allowHDR = true; // 更新 camera.allowHDR，取值为 true。
                } // 结束当前代码块。
            } // 结束当前代码块。

            /// <summary>使用当前管线的强度单位配置模板方向光。</summary>
            public void ConfigureSun(Light sun, RenderingSettings options) // 按该管线的光度单位配置模板方向光。
            { // 开始当前代码块。
                sun.intensity = options.builtInSunIntensity; // 更新 sun.intensity，取值为 options.builtInSunIntensity。
            } // 结束当前代码块。

            /// <summary>离开当前管线时释放或停用其灯光附加逻辑；内置管线无需处理。</summary>
            public void ReleaseSun(Light sun) // 离开当前管线前停用本适配器的灯光附加逻辑。
            { // 开始当前代码块。
            } // 结束当前代码块。

            /// <summary>创建当前管线需要的模板环境；无需额外环境时返回 null。</summary>
            public GameObject CreateEnvironment(Transform parent, RenderingSettings options) => null; // 为模板创建必要的环境对象，返回由调用方销毁的根对象。
            /// <summary>刷新管线需要的材质状态；内置 Shader 无需额外处理。</summary>
            public void ValidateMaterial(Material material) // 刷新新建材质的管线专属关键字。
            { // 开始当前代码块。
            } // 结束当前代码块。
        } // 结束类型。
    } // 结束类型。
} // 结束命名空间。
