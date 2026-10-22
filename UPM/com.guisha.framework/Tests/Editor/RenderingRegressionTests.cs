using System; // 引入 System 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using Guisha.Rendering; // 引入 Guisha.Rendering 中使用的类型。
using NUnit.Framework; // 引入 NUnit.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering; // 引入 UnityEngine.Rendering 中使用的类型。

namespace Guisha.Tests // 把渲染相关类型放入 Guisha.Tests 命名空间。
{ // 开始命名空间。
    /// <summary>验证管线检测、材质选择和源资产拥有权，不依赖安装全部 SRP 包。</summary>
    public sealed class RenderingRegressionTests // 声明 RenderingRegressionTests，供当前模块使用。
    { // 开始类型。
        /// <summary>未知资产用于证明质量等级覆盖优先于 Graphics 中的默认管线。</summary>
        private sealed class OtherPipelineAsset : RenderPipelineAsset // 声明 OtherPipelineAsset，供当前模块使用。
        { // 开始类型。
            /// <summary>测试专用资产不创建真正的渲染实例，只用于识别未知 SRP。</summary>
            protected override RenderPipeline CreatePipeline() => null; // 测试专用资产不创建真正的渲染实例，只用于识别未知 SRP。
        } // 结束类型。

        /// <summary>未分配管线资产就是内置管线，未知 SRP 不能被默认为内置。</summary>
        [Test] // 未分配管线资产就是内置管线，未知 SRP 不能被默认为内置。
        public void NullMeansBuiltInAndUnknownAssetIsExplicit() // 执行 public void NullMeansBuiltInAndUnknownAssetIsExplicit()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            Assert.That(RenderPipelineService.Classify(null), Is.EqualTo(RenderPipelineKind.BuiltIn)); // 断言 RenderPipelineService.Classify(null), Is.EqualTo(RenderPipelineKind.BuiltIn))，验证实际行为符合预期。
            var other = ScriptableObject.CreateInstance<OtherPipelineAsset>(); // 取得或创建测试专用的未知管线资产，保存 ScriptableObject.CreateInstance<OtherPipelineAsset>() 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                Assert.That(RenderPipelineService.Classify(other), Is.EqualTo(RenderPipelineKind.Unsupported)); // 断言 RenderPipelineService.Classify(other), Is.EqualTo(RenderPipelineKind.Unsupported))，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                UnityEngine.Object.DestroyImmediate(other); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>检测实际活动资产，不能只看包、默认 Graphics 或 Shader 是否存在。</summary>
        [Test] // 检测实际活动资产，不能只看包、默认 Graphics 或 Shader 是否存在。
        public void QualityOverrideWinsOverGraphicsDefault() // 执行 public void QualityOverrideWinsOverGraphicsDefault()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            var graphics = GraphicsSettings.defaultRenderPipeline; // 取得或创建修改前的 Graphics 默认管线，保存 GraphicsSettings.defaultRenderPipeline 的结果。
            var quality = QualitySettings.renderPipeline; // 取得或创建修改前的当前质量等级覆盖，保存 QualitySettings.renderPipeline 的结果。
            var other = ScriptableObject.CreateInstance<OtherPipelineAsset>(); // 取得或创建测试专用的未知管线资产，保存 ScriptableObject.CreateInstance<OtherPipelineAsset>() 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                GraphicsSettings.defaultRenderPipeline = null; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 null。
                QualitySettings.renderPipeline = other; // 更新 QualitySettings.renderPipeline，取值为 other。
                Assert.That(RenderPipelineService.Current, Is.EqualTo(RenderPipelineKind.Unsupported)); // 断言 RenderPipelineService.Current, Is.EqualTo(RenderPipelineKind.Unsupported))，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                QualitySettings.renderPipeline = quality; // 更新 QualitySettings.renderPipeline，取值为 quality。
                GraphicsSettings.defaultRenderPipeline = graphics; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 graphics。
                UnityEngine.Object.DestroyImmediate(other); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>即使工程安装了 URP/HDRP，内置管线仍必须创建 Standard 材质。</summary>
        [Test] // 即使工程安装了 URP/HDRP，内置管线仍必须创建 Standard 材质。
        public void BuiltInNeverSelectsAnInstalledSrpShader() // 执行 public void BuiltInNeverSelectsAnInstalledSrpShader()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            var graphics = GraphicsSettings.defaultRenderPipeline; // 取得或创建修改前的 Graphics 默认管线，保存 GraphicsSettings.defaultRenderPipeline 的结果。
            var quality = QualitySettings.renderPipeline; // 取得或创建修改前的当前质量等级覆盖，保存 QualitySettings.renderPipeline 的结果。
            Material material = null; // 取得或创建当前处理的材质，保存 null 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                GraphicsSettings.defaultRenderPipeline = null; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 null。
                QualitySettings.renderPipeline = null; // 更新 QualitySettings.renderPipeline，取值为 null。
                material = RenderPipelineService.CreateMaterial(Color.cyan); // 更新 当前处理的材质，取值为 RenderPipelineService.CreateMaterial(Color.cyan)。
                Assert.That(material.shader.name, Is.EqualTo("Standard")); // 断言 material.shader.name, Is.EqualTo("Standard"))，验证实际行为符合预期。
                Assert.That(material.color, Is.EqualTo(Color.cyan)); // 断言 material.color, Is.EqualTo(Color.cyan))，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                if (material != null) // 仅当 (material != null) 成立时执行后续处理。
                    UnityEngine.Object.DestroyImmediate(material); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
                QualitySettings.renderPipeline = quality; // 更新 QualitySettings.renderPipeline，取值为 quality。
                GraphicsSettings.defaultRenderPipeline = graphics; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 graphics。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>转换以副本承接目标 Shader，保留主颜色，不改写原材质。</summary>
        [Test] // 转换以副本承接目标 Shader，保留主颜色，不改写原材质。
        public void AdaptationOwnsCopyAndPreservesColor() // 执行 public void AdaptationOwnsCopyAndPreservesColor()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            var graphics = GraphicsSettings.defaultRenderPipeline; // 取得或创建修改前的 Graphics 默认管线，保存 GraphicsSettings.defaultRenderPipeline 的结果。
            var quality = QualitySettings.renderPipeline; // 取得或创建修改前的当前质量等级覆盖，保存 QualitySettings.renderPipeline 的结果。
            var source = new Material(Shader.Find("Unlit/Color")); // 取得或创建转换前的源材质，保存 new Material(Shader.Find("Unlit/Color")) 的结果。
            Material adapted = null; // 取得或创建独立的转换结果材质，保存 null 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                GraphicsSettings.defaultRenderPipeline = null; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 null。
                QualitySettings.renderPipeline = null; // 更新 QualitySettings.renderPipeline，取值为 null。
                source.color = Color.red; // 更新 source.color，取值为 Color.red。
                adapted = RenderPipelineService.AdaptMaterial(source); // 更新 独立的转换结果材质，取值为 RenderPipelineService.AdaptMaterial(source)。
                Assert.That(adapted, Is.Not.SameAs(source)); // 断言 adapted, Is.Not.SameAs(source))，验证实际行为符合预期。
                Assert.That(adapted.shader.name, Is.EqualTo("Guisha/BuiltIn/Unlit")); // 断言 adapted.shader.name, Is.EqualTo("Guisha/BuiltIn/Unlit"))，验证实际行为符合预期。
                Assert.That(adapted.color, Is.EqualTo(Color.red)); // 断言 adapted.color, Is.EqualTo(Color.red))，验证实际行为符合预期。
                Assert.That(source.shader.name, Is.EqualTo("Unlit/Color")); // 断言 source.shader.name, Is.EqualTo("Unlit/Color"))，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                if (adapted != null) // 仅当 (adapted != null) 成立时执行后续处理。
                    UnityEngine.Object.DestroyImmediate(adapted); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
                UnityEngine.Object.DestroyImmediate(source); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
                QualitySettings.renderPipeline = quality; // 更新 QualitySettings.renderPipeline，取值为 quality。
                GraphicsSettings.defaultRenderPipeline = graphics; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 graphics。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>从旧版无光照纹理材质迁移时保留贴图对象、重复比例和偏移。</summary>
        [Test] // 从旧版无光照纹理材质迁移时保留贴图对象、重复比例和偏移。
        public void AdaptationPreservesTextureTransform() // 执行 public void AdaptationPreservesTextureTransform()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            var graphics = GraphicsSettings.defaultRenderPipeline; // 取得或创建修改前的 Graphics 默认管线，保存 GraphicsSettings.defaultRenderPipeline 的结果。
            var quality = QualitySettings.renderPipeline; // 取得或创建修改前的当前质量等级覆盖，保存 QualitySettings.renderPipeline 的结果。
            var source = new Material(Shader.Find("Unlit/Texture")); // 取得或创建转换前的源材质，保存 new Material(Shader.Find("Unlit/Texture")) 的结果。
            var texture = new Texture2D(2, 2); // 取得或创建用于检查贴图引用保留的测试纹理，保存 new Texture2D(2, 2) 的结果。
            Material adapted = null; // 取得或创建独立的转换结果材质，保存 null 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                GraphicsSettings.defaultRenderPipeline = null; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 null。
                QualitySettings.renderPipeline = null; // 更新 QualitySettings.renderPipeline，取值为 null。
                source.mainTexture = texture; // 更新 source.mainTexture，取值为 texture。
                source.mainTextureScale = new Vector2(2, 3); // 更新 source.mainTextureScale，取值为 new Vector2(2, 3)。
                source.mainTextureOffset = new Vector2(0.2f, 0.4f); // 更新 source.mainTextureOffset，取值为 new Vector2(0.2f, 0.4f)。
                adapted = RenderPipelineService.AdaptMaterial(source); // 更新 独立的转换结果材质，取值为 RenderPipelineService.AdaptMaterial(source)。
                Assert.That(adapted.mainTexture, Is.SameAs(texture)); // 断言 adapted.mainTexture, Is.SameAs(texture))，验证实际行为符合预期。
                Assert.That(adapted.mainTextureScale, Is.EqualTo(source.mainTextureScale)); // 断言 adapted.mainTextureScale, Is.EqualTo(source.mainTextureScale))，验证实际行为符合预期。
                Assert.That(adapted.mainTextureOffset, Is.EqualTo(source.mainTextureOffset)); // 断言 adapted.mainTextureOffset, Is.EqualTo(source.mainTextureOffset))，验证实际行为符合预期。
                Assert.That(source.shader.name, Is.EqualTo("Unlit/Texture")); // 断言 source.shader.name, Is.EqualTo("Unlit/Texture"))，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                if (adapted != null) // 仅当 (adapted != null) 成立时执行后续处理。
                    UnityEngine.Object.DestroyImmediate(adapted); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
                UnityEngine.Object.DestroyImmediate(source); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
                UnityEngine.Object.DestroyImmediate(texture); // 在 EditMode 中立即销毁测试创建的对象，避免污染项目资源。
                QualitySettings.renderPipeline = quality; // 更新 QualitySettings.renderPipeline，取值为 quality。
                GraphicsSettings.defaultRenderPipeline = graphics; // 更新 GraphicsSettings.defaultRenderPipeline，取值为 graphics。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>三种管线的 Lit 和 Unlit 都有独立且确定的 Shader 映射。</summary>
        [TestCase(RenderPipelineKind.BuiltIn, "Standard", "Guisha/BuiltIn/Unlit")] // 三种管线的 Lit 和 Unlit 都有独立且确定的 Shader 映射。
        [TestCase(RenderPipelineKind.Universal, "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit")] // 由 Unity/NUnit 属性声明创建菜单、初始化时机、组件约束或测试用例。
        [TestCase(RenderPipelineKind.HighDefinition, "HDRP/Lit", "HDRP/Unlit")] // 由 Unity/NUnit 属性声明创建菜单、初始化时机、组件约束或测试用例。
        public void ShaderMappingIsExplicit(RenderPipelineKind kind, string lit, string unlit) // 执行 public void ShaderMappingIsExplicit(RenderPipelineKind kind, string lit, string unlit)，完成当前渲染适配步骤。
        { // 开始当前代码块。
            Assert.That(RenderPipelineService.ShaderName(kind, false), Is.EqualTo(lit)); // 断言 RenderPipelineService.ShaderName(kind, false), Is.EqualTo(lit))，验证实际行为符合预期。
            Assert.That(RenderPipelineService.ShaderName(kind, true), Is.EqualTo(unlit)); // 断言 RenderPipelineService.ShaderName(kind, true), Is.EqualTo(unlit))，验证实际行为符合预期。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
