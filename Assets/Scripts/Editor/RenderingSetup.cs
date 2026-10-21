using System; // 引入 System 中使用的类型。
using System.IO; // 引入 System.IO 中使用的类型。
using System.Linq; // 引入 System.Linq 中使用的类型。
using System.Reflection; // 引入 System.Reflection 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using Guisha.Rendering; // 引入 Guisha.Rendering 中使用的类型。
using UnityEditor; // 引入 UnityEditor 中使用的类型。
using UnityEditor.Build; // 引入 UnityEditor.Build 中使用的类型。
using UnityEditor.Build.Reporting; // 引入 UnityEditor.Build.Reporting 中使用的类型。
using UnityEditor.PackageManager; // 引入 UnityEditor.PackageManager 中使用的类型。
using UnityEditor.PackageManager.Requests; // 引入 UnityEditor.PackageManager.Requests 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering; // 引入 UnityEngine.Rendering 中使用的类型。

namespace Guisha.Editor // 把渲染相关类型放入 Guisha.Editor 命名空间。
{ // 开始命名空间。
    /// <summary>提供三种管线的显式切换入口；只有用户选择菜单时才安装包或修改 Graphics 设置。</summary>
    public static class RenderingSetup // 声明 RenderingSetup，供当前模块使用。
    { // 开始类型。
        /// <summary>程序集重载后继续切换所用的 SessionState 键。</summary>
        private const string PendingKey = "Guisha.Rendering.Pending"; // 声明程序集重载后继续切换所用的 SessionState 键。
        /// <summary>模板管线资产的保存目录。</summary>
        private const string Root = "Assets/GuishaDemo/Rendering"; // 声明模板管线资产的保存目录。
        /// <summary>构建需要保留的 Shader 配置资产路径。</summary>
        private const string SettingsPath = "Assets/Resources/Guisha/RenderingSettings.asset"; // 声明构建需要保留的 Shader 配置资产路径。
        /// <summary>当前进行中的包安装请求。</summary>
        private static AddRequest request; // 声明当前进行中的包安装请求。
        /// <summary>安装包导致程序集重载后继续完成用户此前选择的管线配置。</summary>
        [InitializeOnLoadMethod] // 安装包导致程序集重载后继续完成用户此前选择的管线配置。
        private static void Resume() // 执行 private static void Resume()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            EditorApplication.delayCall += () => // 注册或解除上方生命周期回调，避免遗留重复监听。
            { // 开始当前代码块。
                int pending = SessionState.GetInt(PendingKey, -1); // 取得或创建尚待完成的目标管线，保存 SessionState.GetInt(PendingKey, -1) 的结果。
                if (pending >= 0 && FindPipelineType((RenderPipelineKind)pending) != null) // 仅当 (pending >= 0 && FindPipelineType((RenderPipelineKind)pending) != null) 成立时执行后续处理。
                { // 开始当前代码块。
                    SessionState.EraseInt(PendingKey); // 清除已完成或失败的切换请求，避免重复执行。
                    Activate((RenderPipelineKind)pending); // 执行 Activate((RenderPipelineKind)pending)，完成当前渲染适配步骤。
                } // 结束当前代码块。
            }; // 结束上方的回调、参数或初始化列表。
        } // 结束当前代码块。

        /// <summary>恢复内置管线，并清除当前质量等级的覆盖。</summary>
        [MenuItem("Guisha/Rendering/Use Built-in")] // 恢复内置管线，并清除当前质量等级的覆盖。
        public static void BuiltIn() // 执行 public static void BuiltIn()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            Activate(RenderPipelineKind.BuiltIn); // 执行 Activate(RenderPipelineKind.BuiltIn)，完成当前渲染适配步骤。
        } // 结束当前代码块。

        /// <summary>安装所需 URP 包后创建/使用模板 URP 资产。</summary>
        [MenuItem("Guisha/Rendering/Use URP")] // 安装所需 URP 包后创建/使用模板 URP 资产。
        public static void Universal() // 执行 public static void Universal()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            Request(RenderPipelineKind.Universal); // 执行 Request(RenderPipelineKind.Universal)，完成当前渲染适配步骤。
        } // 结束当前代码块。

        /// <summary>安装所需 HDRP 包后创建/使用模板 HDRP 资产与全局设置。</summary>
        [MenuItem("Guisha/Rendering/Use HDRP")] // 安装所需 HDRP 包后创建/使用模板 HDRP 资产与全局设置。
        public static void HighDefinition() // 执行 public static void HighDefinition()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            Request(RenderPipelineKind.HighDefinition); // 执行 Request(RenderPipelineKind.HighDefinition)，完成当前渲染适配步骤。
        } // 结束当前代码块。

        /// <summary>显式请求切换，包缺失时使用 Package Manager 异步安装匹配 Unity 2022.3 的版本。</summary>
        private static void Request(RenderPipelineKind kind) // 显式请求切换，包缺失时使用 Package Manager 异步安装匹配 Unity 2022.3 的版本。
        { // 开始当前代码块。
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) // 仅当 (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) 成立时执行后续处理。
            { // 开始当前代码块。
                Debug.LogWarning("请先退出 Play 并等待编译完成，再切换渲染管线。"); // 向 Unity Console 报告配置状态或可操作的错误原因。
                return; // 当前条件无需后续处理，立即结束本次调用。
            } // 结束当前代码块。

            if (FindPipelineType(kind) != null) // 仅当 (FindPipelineType(kind) != null) 成立时执行后续处理。
            { // 开始当前代码块。
                Activate(kind); // 执行 Activate(kind)，完成当前渲染适配步骤。
                return; // 当前条件无需后续处理，立即结束本次调用。
            } // 结束当前代码块。

            if (request != null) // 仅当 (request != null) 成立时执行后续处理。
            { // 开始当前代码块。
                Debug.LogWarning("正在安装另一个渲染包，请等待完成。"); // 向 Unity Console 报告配置状态或可操作的错误原因。
                return; // 当前条件无需后续处理，立即结束本次调用。
            } // 结束当前代码块。

            SessionState.SetInt(PendingKey, (int)kind); // 保存未完成的用户切换请求，跨程序集重载继续处理。
            request = Client.Add("com.unity.render-pipelines." + (kind == RenderPipelineKind.Universal ? "universal" : "high-definition") + "@14.0.12"); // 将当前对象登记到由本模块管理的集合。
            EditorApplication.update += Poll; // 注册或解除上方生命周期回调，避免遗留重复监听。
        } // 结束当前代码块。

        /// <summary>观察安装失败；安装成功后在程序集重新加载完成时继续切换。</summary>
        private static void Poll() // 观察安装失败；安装成功后在程序集重新加载完成时继续切换。
        { // 开始当前代码块。
            if (request == null || !request.IsCompleted) // 仅当 (request == null || !request.IsCompleted) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            if (request.Status == StatusCode.Failure) // 仅当 (request.Status == StatusCode.Failure) 成立时执行后续处理。
            { // 开始当前代码块。
                SessionState.EraseInt(PendingKey); // 清除已完成或失败的切换请求，避免重复执行。
                Debug.LogError("渲染包安装失败：" + request.Error.message); // 向 Unity Console 报告配置状态或可操作的错误原因。
            } // 结束当前代码块。

            request = null; // 更新 当前进行中的包安装请求，取值为 null。
            EditorApplication.update -= Poll; // 注册或解除上方生命周期回调，避免遗留重复监听。
            Resume(); // 执行 Resume()，完成当前渲染适配步骤。
        } // 结束当前代码块。

        /// <summary>创建或复用管线资产，并赋给 Graphics；不批量覆盖其他质量等级的独立选择。</summary>
        /// <param name = "kind">目标管线，URP/HDRP 包须已完成安装。</param>
        public static void Activate(RenderPipelineKind kind) // 创建或复用管线资产，并赋给 Graphics；不批量覆盖其他质量等级的独立选择。
        { // 开始当前代码块。
            if (EditorApplication.isPlayingOrWillChangePlaymode) // 仅当 (EditorApplication.isPlayingOrWillChangePlaymode) 成立时执行后续处理。
                throw new InvalidOperationException("请在 Edit 模式切换管线。"); // 通过异常报告具体失败原因，终止本次无效调用。
            Directory.CreateDirectory(Root); // 确保资源或验证报告的目标目录已经存在。
            RenderPipelineAsset asset = null; // 取得或创建目标渲染管线资产，保存 null 的结果。
            if (kind != RenderPipelineKind.BuiltIn) // 仅当 (kind != RenderPipelineKind.BuiltIn) 成立时执行后续处理。
            { // 开始当前代码块。
                var type = FindPipelineType(kind) ?? throw new InvalidOperationException("所需管线包尚未安装。"); // 取得或创建反射取得的可选包类型，保存 FindPipelineType(kind) ?? throw new InvalidOperationException("所需管线包尚未安装。") 的结果。
                string path = Root + "/" + kind + ".asset"; // 取得或创建资源保存路径，保存 Root + "/" + kind + ".asset" 的结果。
                asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path); // 更新 目标渲染管线资产，取值为 AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path)。
                if (asset == null) // 仅当 (asset == null) 成立时执行后续处理。
                { // 开始当前代码块。
                    if (kind == RenderPipelineKind.Universal) // 仅当 (kind == RenderPipelineKind.Universal) 成立时执行后续处理。
                    { // 开始当前代码块。
                        var rendererType = FindType("UnityEngine.Rendering.Universal.UniversalRendererData"); // 取得或创建rendererType，保存 FindType("UnityEngine.Rendering.Universal.UniversalRendererData") 的结果。
                        string rendererPath = Root + "/UniversalRenderer.asset"; // 取得或创建URP Renderer 资产路径，保存 Root + "/UniversalRenderer.asset" 的结果。
                        var renderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>(rendererPath); // 取得或创建渲染组件或 Renderer 配置资产，保存 AssetDatabase.LoadAssetAtPath<ScriptableObject>(rendererPath) 的结果。
                        if (renderer == null) // 仅当 (renderer == null) 成立时执行后续处理。
                        { // 开始当前代码块。
                            renderer = ScriptableObject.CreateInstance(rendererType); // 更新 渲染组件或 Renderer 配置资产，取值为 ScriptableObject.CreateInstance(rendererType)。
                            AssetDatabase.CreateAsset(renderer, rendererPath); // 将新建的配置保存为可复用的 Unity 资产。
                        } // 结束当前代码块。

                        ReloadResources(renderer, "Packages/com.unity.render-pipelines.universal"); // 执行 ReloadResources(renderer, "Packages/com.unity.render-pipelines.universal")，完成当前渲染适配步骤。
                        EditorUtility.SetDirty(renderer); // 标记资源已修改，供 AssetDatabase 持久化保存。
                        asset = (RenderPipelineAsset)type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { renderer }); // 在编辑器中调用已安装包提供的初始化方法，核心运行时无需反射。
                    } // 结束当前代码块。
                    else // 前面的条件不成立时走此分支。
                        asset = (RenderPipelineAsset)ScriptableObject.CreateInstance(type); // 更新 目标渲染管线资产，取值为 (RenderPipelineAsset)ScriptableObject.CreateInstance(type)。
                    AssetDatabase.CreateAsset(asset, path); // 将新建的配置保存为可复用的 Unity 资产。
                } // 结束当前代码块。

                PlayerSettings.colorSpace = ColorSpace.Linear; // 更新 PlayerSettings.colorSpace，取值为 ColorSpace.Linear。
            } // 结束当前代码块。

            GraphicsSettings.defaultRenderPipeline = asset; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 asset。
            QualitySettings.renderPipeline = null; // 更新 QualitySettings.renderPipeline，取值为 null。
            if (kind == RenderPipelineKind.HighDefinition) // 仅当 (kind == RenderPipelineKind.HighDefinition) 成立时执行后续处理。
            { // 开始当前代码块。
                var globals = FindType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings"); // 取得或创建HDRP 全局设置类型，保存 FindType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings") 的结果。
                globals.GetMethod("Ensure", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { true }); // 在编辑器中调用已安装包提供的初始化方法，核心运行时无需反射。
            } // 结束当前代码块。

            RefreshShaderReferences(); // 刷新 Resources 中的显式 Shader 引用，供构建时保留使用。
            AssetDatabase.SaveAssets(); // 保存资源配置改动，确保下次打开工程仍能读取。
            Debug.Log("模板已配置：" + kind + "。其他 Quality 等级若有独立管线覆盖，仍按其配置运行。"); // 向 Unity Console 报告配置状态或可操作的错误原因。
        } // 结束当前代码块。

        /// <summary>把已安装管线的 Shader 写进 Resources 资产，防止运行时 Shader.Find 在构建中找不到。</summary>
        [MenuItem("Guisha/Rendering/Refresh Shader References")] // 把已安装管线的 Shader 写进 Resources 资产，防止运行时 Shader.Find 在构建中找不到。
        public static void RefreshShaderReferences() // 刷新 Resources 中的显式 Shader 引用，供构建时保留使用。
        { // 开始当前代码块。
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)); // 确保资源或验证报告的目标目录已经存在。
            var settings = AssetDatabase.LoadAssetAtPath<RenderingSettings>(SettingsPath); // 取得或创建加载或临时创建的渲染配置，保存 AssetDatabase.LoadAssetAtPath<RenderingSettings>(SettingsPath) 的结果。
            if (settings == null) // 仅当 (settings == null) 成立时执行后续处理。
            { // 开始当前代码块。
                settings = ScriptableObject.CreateInstance<RenderingSettings>(); // 更新 加载或临时创建的渲染配置，取值为 ScriptableObject.CreateInstance<RenderingSettings>()。
                AssetDatabase.CreateAsset(settings, SettingsPath); // 将新建的配置保存为可复用的 Unity 资产。
            } // 结束当前代码块。

            settings.builtInLit = Shader.Find("Standard"); // 更新 settings.builtInLit，取值为 Shader.Find("Standard")。
            settings.builtInUnlit = Shader.Find("Guisha/BuiltIn/Unlit"); // 更新 settings.builtInUnlit，取值为 Shader.Find("Guisha/BuiltIn/Unlit")。
            settings.universalLit = Shader.Find("Universal Render Pipeline/Lit"); // 更新 settings.universalLit，取值为 Shader.Find("Universal Render Pipeline/Lit")。
            settings.universalUnlit = Shader.Find("Universal Render Pipeline/Unlit"); // 更新 settings.universalUnlit，取值为 Shader.Find("Universal Render Pipeline/Unlit")。
            settings.highDefinitionLit = Shader.Find("HDRP/Lit"); // 更新 settings.highDefinitionLit，取值为 Shader.Find("HDRP/Lit")。
            settings.highDefinitionUnlit = Shader.Find("HDRP/Unlit"); // 更新 settings.highDefinitionUnlit，取值为 Shader.Find("HDRP/Unlit")。
            EditorUtility.SetDirty(settings); // 标记资源已修改，供 AssetDatabase 持久化保存。
            AssetDatabase.SaveAssets(); // 保存资源配置改动，确保下次打开工程仍能读取。
        } // 结束当前代码块。

        /// <summary>显示有效管线，覆盖层级以当前 Quality 优先于 Graphics 为准。</summary>
        [MenuItem("Guisha/Rendering/Validate Current Pipeline")] // 显示有效管线，覆盖层级以当前 Quality 优先于 Graphics 为准。
        public static void ValidateCurrent() // 执行 public static void ValidateCurrent()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            RefreshShaderReferences(); // 刷新 Resources 中的显式 Shader 引用，供构建时保留使用。
            var adapter = RenderPipelineService.RequireAdapter(); // 取得或创建当前管线的适配器，保存 RenderPipelineService.RequireAdapter() 的结果。
            Debug.Log("有效管线：" + adapter.Kind + "；资产：" + GraphicsSettings.currentRenderPipeline + "；模板 Shader 引用已刷新。"); // 向 Unity Console 报告配置状态或可操作的错误原因。
        } // 结束当前代码块。

        /// <summary>按完整类型名查找可选包类型；只在编辑器资源配置时使用反射。</summary>
        private static Type FindType(string name) // 按完整类型名查找可选包类型；只在编辑器资源配置时使用反射。
        { // 开始当前代码块。
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) // 遍历 AppDomain.CurrentDomain.GetAssemblies()，逐个处理并保持资源拥有权。
            { // 开始当前代码块。
                var type = assembly.GetType(name, false); // 取得或创建反射取得的可选包类型，保存 assembly.GetType(name, false) 的结果。
                if (type != null) // 仅当 (type != null) 成立时执行后续处理。
                    return type; // 返回 反射取得的可选包类型，交给调用方继续处理。
            } // 结束当前代码块。

            return null; // 返回 null，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>取得目标管线的资产类型；内置管线没有资产类型。</summary>
        private static Type FindPipelineType(RenderPipelineKind kind) // 取得目标管线的资产类型；内置管线没有资产类型。
        { // 开始当前代码块。
            return kind == RenderPipelineKind.Universal ? FindType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset") : kind == RenderPipelineKind.HighDefinition ? FindType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset") : null; // 返回 kind == RenderPipelineKind.Universal ? FindType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset") : kind == RenderPipelineKind.HighDefinition ? FindType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset") : null，交给调用方继续处理。
        } // 结束当前代码块。

        /// <summary>使用对应 Unity 版本的官方资源重载器补齐 URP Renderer 的内置 Shader 引用。</summary>
        private static void ReloadResources(UnityEngine.Object target, string packagePath) // 使用对应 Unity 版本的官方资源重载器补齐 URP Renderer 的内置 Shader 引用。
        { // 开始当前代码块。
            var type = FindType("UnityEngine.Rendering.ResourceReloader") ?? throw new InvalidOperationException("SRP Core 资源重载器未就绪。"); // 取得或创建反射取得的可选包类型，保存 FindType("UnityEngine.Rendering.ResourceReloader") ?? throw new InvalidOperationException("SRP Core 资源重载器未就绪。") 的结果。
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == "ReloadAllNullIn" && m.GetParameters().Length == 2); // 取得或创建反射找到的公共资源重载方法，保存 type.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == "ReloadAllNullIn" && m.GetParameters().Length == 2) 的结果。
            method.Invoke(null, new object[] { target, packagePath }); // 在编辑器中调用已安装包提供的初始化方法，核心运行时无需反射。
        } // 结束当前代码块。
    } // 结束类型。

    /// <summary>构建前刷新显式 Shader 引用；不安装包，也不替用户切换管线。</summary>
    public sealed class RenderingBuildCheck : IPreprocessBuildWithReport // 声明 RenderingBuildCheck，供当前模块使用。
    { // 开始类型。
        /// <summary>构建检查顺序，负值使本检查提前执行。</summary>
        public int callbackOrder => -100; // 声明构建检查顺序，负值使本检查提前执行。

        /// <summary>在 Player 构建流程开始时检查当前适配器与 Shader 引用。</summary>
        public void OnPreprocessBuild(BuildReport report) // 在 Player 构建流程开始时检查当前适配器与 Shader 引用。
        { // 开始当前代码块。
            RenderingSetup.RefreshShaderReferences(); // 刷新 Resources 中的显式 Shader 引用，供构建时保留使用。
            RenderPipelineService.RequireAdapter(); // 执行 RenderPipelineService.RequireAdapter()，完成当前渲染适配步骤。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
