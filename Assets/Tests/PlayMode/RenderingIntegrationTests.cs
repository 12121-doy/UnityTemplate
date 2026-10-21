using System; // 引入 System 中使用的类型。
using System.Collections; // 引入 System.Collections 中使用的类型。
using System.IO; // 引入 System.IO 中使用的类型。
using System.Linq; // 引入 System.Linq 中使用的类型。
using Guisha.Core; // 引入 Guisha.Core 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using Guisha.Gameplay; // 引入 Guisha.Gameplay 中使用的类型。
using Guisha.Managers; // 引入 Guisha.Managers 中使用的类型。
using Guisha.Rendering; // 引入 Guisha.Rendering 中使用的类型。
using NUnit.Framework; // 引入 NUnit.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 中使用的类型。
using UnityEngine.TestTools; // 引入 UnityEngine.TestTools 中使用的类型。

namespace Guisha.Validation // 把渲染相关类型放入 Guisha.Validation 命名空间。
{ // 开始命名空间。
    /// <summary>在真实活动管线中验证模板场景、Addressables 外观及离屏渲染结果。</summary>
    public sealed class RenderingIntegrationTests // 声明 RenderingIntegrationTests，供当前模块使用。
    { // 开始类型。
        /// <summary>反复刷新与对象池启停复用同一材质副本，销毁绑定时释放副本并恢复源材质。</summary>
        [UnityTest] // 反复刷新与对象池启停复用同一材质副本，销毁绑定时释放副本并恢复源材质。
        public IEnumerator MaterialBindingReusesAndReleasesOwnedCopies() // 执行 public IEnumerator MaterialBindingReusesAndReleasesOwnedCopies()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            var source = new Material(Shader.Find("Unlit/Color")); // 取得或创建转换前的源材质，保存 new Material(Shader.Find("Unlit/Color")) 的结果。
            var owner = new GameObject("MaterialBindingLifetime"); // 取得或创建测试对象或生命周期拥有者，保存 new GameObject("MaterialBindingLifetime") 的结果。
            owner.SetActive(false); // 切换对象激活状态，同时触发其组件和子对象的启停生命周期。
            var renderer = owner.AddComponent<MeshRenderer>(); // 取得或创建渲染组件或 Renderer 配置资产，保存 owner.AddComponent<MeshRenderer>() 的结果。
            renderer.sharedMaterial = source; // 更新 renderer.sharedMaterial，取值为 source。
            var binding = owner.AddComponent<PipelineMaterialBinding>(); // 取得或创建持有材质副本的绑定组件，保存 owner.AddComponent<PipelineMaterialBinding>() 的结果。
            owner.SetActive(true); // 切换对象激活状态，同时触发其组件和子对象的启停生命周期。
            var owned = renderer.sharedMaterial; // 取得或创建本组件创建且需要释放的材质副本，保存 renderer.sharedMaterial 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                Assert.That(owned, Is.Not.SameAs(source)); // 断言 owned, Is.Not.SameAs(source))，验证实际行为符合预期。
                binding.Refresh(); // 仅在需要时重建适配材质，重复调用复用已有结果。
                Assert.That(renderer.sharedMaterial, Is.SameAs(owned)); // 断言 renderer.sharedMaterial, Is.SameAs(owned))，验证实际行为符合预期。
                owner.SetActive(false); // 切换对象激活状态，同时触发其组件和子对象的启停生命周期。
                owner.SetActive(true); // 切换对象激活状态，同时触发其组件和子对象的启停生命周期。
                Assert.That(renderer.sharedMaterial, Is.SameAs(owned)); // 断言 renderer.sharedMaterial, Is.SameAs(owned))，验证实际行为符合预期。
                UnityEngine.Object.Destroy(binding); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
                yield return null; // 把协程挂起到下一帧，等待异步加载、渲染或延迟销毁完成。
                yield return null; // 把协程挂起到下一帧，等待异步加载、渲染或延迟销毁完成。
                Assert.That(owned == null, Is.True, "绑定生成的材质必须配对销毁。"); // 断言 owned == null, Is.True, "绑定生成的材质必须配对销毁。")，验证实际行为符合预期。
                Assert.That(renderer.sharedMaterial, Is.SameAs(source)); // 断言 renderer.sharedMaterial, Is.SameAs(source))，验证实际行为符合预期。
                Assert.That(source.shader.name, Is.EqualTo("Unlit/Color")); // 断言 source.shader.name, Is.EqualTo("Unlit/Color"))，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                UnityEngine.Object.Destroy(owner); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
                UnityEngine.Object.Destroy(source); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            } // 结束当前代码块。
        } // 结束当前代码块。

        /// <summary>加载模板、生成鬼怪并读取主相机渲染，检查错误粉色和有效画面。</summary>
        [UnityTest, Timeout(300000)] // 加载模板、生成鬼怪并读取主相机渲染，检查错误粉色和有效画面。
        public IEnumerator ActivePipelineRendersTemplateAndSpawnedEntity() // 执行 public IEnumerator ActivePipelineRendersTemplateAndSpawnedEntity()，完成当前渲染适配步骤。
        { // 开始当前代码块。
            yield return SceneManager.LoadSceneAsync("Assets/GuishaDemo/Scenes/Guisha.unity"); // 等待 SceneManager.LoadSceneAsync("Assets/GuishaDemo/Scenes/Guisha.unity") 完成后继续测试。
            for (int i = 0; i < 8; i++) // 按 (int i = 0; i < 8; i++) 中的初始化、继续条件和递增步骤循环。
                yield return null; // 把协程挂起到下一帧，等待异步加载、渲染或延迟销毁完成。
            var kind = RenderPipelineService.Current; // 取得或创建目标管线类别，保存 RenderPipelineService.Current 的结果。
            Assert.That(kind, Is.Not.EqualTo(RenderPipelineKind.Unsupported)); // 断言 kind, Is.Not.EqualTo(RenderPipelineKind.Unsupported))，验证实际行为符合预期。
            Assert.That(RenderPipelineService.RequireAdapter().Kind, Is.EqualTo(kind)); // 断言 RenderPipelineService.RequireAdapter().Kind, Is.EqualTo(kind))，验证实际行为符合预期。
            var player = UnityEngine.Object.FindObjectOfType<PlayerEntity>(); // 取得或创建场景中的玩家实体，保存 UnityEngine.Object.FindObjectOfType<PlayerEntity>() 的结果。
            var camera = Camera.main; // 取得或创建当前处理的相机，保存 Camera.main 的结果。
            Assert.That(player, Is.Not.Null); // 断言 player, Is.Not.Null)，验证实际行为符合预期。
            Assert.That(camera, Is.Not.Null); // 断言 camera, Is.Not.Null)，验证实际行为符合预期。
            Assert.That(UnityEngine.Object.FindObjectOfType<Guisha.UI.HUD>(), Is.Not.Null); // 断言 UnityEngine.Object.FindObjectOfType<Guisha.UI.HUD>(), Is.Not.Null)，验证实际行为符合预期。
            Assert.That(RenderingManager.Exists, Is.True); // 断言 RenderingManager.Exists, Is.True)，验证实际行为符合预期。
            var definition = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings").entityCatalog.definitions.First(d => d.type == EntityType.Ghost); // 取得或创建鬼怪实体配置，保存 Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings").entityCatalog.definitions.First(d => d.type == EntityType.Ghost) 的结果。
            BaseEntity spawned = null; // 取得或创建异步生成的鬼怪实体，保存 null 的结果。
            bool complete = false; // 取得或创建异步生成回调是否完成，保存 false 的结果。
            EventBus.Publish(new SpawnEntityRequested(definition, new Vector3(1, 0.1f, 2), Quaternion.identity, player.transform.parent, entity => // 广播类型事件，由订阅模块完成生成或管线变化响应。
            { // 开始当前代码块。
                spawned = entity; // 更新 异步生成的鬼怪实体，取值为 entity。
                complete = true; // 更新 异步生成回调是否完成，取值为 true。
            })); // 执行 }))，完成当前渲染适配步骤。
            float deadline = Time.realtimeSinceStartup + 30; // 取得或创建等待实体生成的超时时刻，保存 Time.realtimeSinceStartup + 30 的结果。
            while (!complete && Time.realtimeSinceStartup < deadline) // 在 (!complete && Time.realtimeSinceStartup < deadline) 条件内继续等待，包含超时限制。
                yield return null; // 把协程挂起到下一帧，等待异步加载、渲染或延迟销毁完成。
            Assert.That(complete, Is.True); // 断言 complete, Is.True)，验证实际行为符合预期。
            Assert.That(spawned, Is.Not.Null); // 断言 spawned, Is.Not.Null)，验证实际行为符合预期。
            foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>()) // 遍历 UnityEngine.Object.FindObjectsOfType<Renderer>()，逐个处理并保持资源拥有权。
                foreach (var material in renderer.sharedMaterials) // 遍历 renderer.sharedMaterials，逐个处理并保持资源拥有权。
                { // 开始当前代码块。
                    Assert.That(material, Is.Not.Null); // 断言 material, Is.Not.Null)，验证实际行为符合预期。
                    Assert.That(material.shader.isSupported, Is.True, material.shader.name); // 断言 material.shader.isSupported, Is.True, material.shader.name)，验证实际行为符合预期。
                    Assert.That(material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader")); // 断言 material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"))，验证实际行为符合预期。
                    if (RenderPipelineService.IsTemplateShader(material.shader.name, out var unlit)) // 仅当 (RenderPipelineService.IsTemplateShader(material.shader.name, out var unlit)) 成立时执行后续处理。
                        Assert.That(material.shader.name, Is.EqualTo(RenderPipelineService.ShaderName(kind, unlit))); // 断言 material.shader.name, Is.EqualTo(RenderPipelineService.ShaderName(kind, unlit)))，验证实际行为符合预期。
                } // 结束当前代码块。

            var target = new RenderTexture(320, 200, 24, RenderTextureFormat.ARGB32); // 取得或创建绑定的 Renderer 组件，保存 new RenderTexture(320, 200, 24, RenderTextureFormat.ARGB32) 的结果。
            var image = new Texture2D(320, 200, TextureFormat.RGB24, false); // 取得或创建读取相机像素的测试纹理，保存 new Texture2D(320, 200, TextureFormat.RGB24, false) 的结果。
            var previousTarget = camera.targetTexture; // 取得或创建相机原有的输出纹理，保存 camera.targetTexture 的结果。
            var previousActive = RenderTexture.active; // 取得或创建原有的全局活动 RenderTexture，保存 RenderTexture.active 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                target.Create(); // 为离屏渲染测试分配 RenderTexture 的 GPU 资源。
                camera.targetTexture = target; // 更新 camera.targetTexture，取值为 target。
                for (int i = 0; i < 20; i++) // 按 (int i = 0; i < 20; i++) 中的初始化、继续条件和递增步骤循环。
                    yield return null; // 把协程挂起到下一帧，等待异步加载、渲染或延迟销毁完成。
                RenderTexture.active = target; // 更新 RenderTexture.active，取值为 target。
                image.ReadPixels(new Rect(0, 0, 320, 200), 0, 0); // 读取真实主相机的 GPU 输出，检查黑屏和错误粉色。
                image.Apply(); // 应用读取的像素数据，供统计和 PNG 编码使用。
                var pixels = image.GetPixels(); // 取得或创建相机输出的像素数组，保存 image.GetPixels() 的结果。
                float brightest = pixels.Max(c => Mathf.Max(c.r, Mathf.Max(c.g, c.b))); // 取得或创建画面最亮颜色通道，保存 pixels.Max(c => Mathf.Max(c.r, Mathf.Max(c.g, c.b))) 的结果。
                int pink = pixels.Count(c => c.r > 0.8f && c.b > 0.8f && c.g < 0.15f); // 取得或创建疑似错误 Shader 粉色的像素数量，保存 pixels.Count(c => c.r > 0.8f && c.b > 0.8f && c.g < 0.15f) 的结果。
                var directory = Environment.GetEnvironmentVariable("GUISHA_RENDERING_REPORT"); // 取得或创建验证报告的输出目录，保存 Environment.GetEnvironmentVariable("GUISHA_RENDERING_REPORT") 的结果。
                if (!string.IsNullOrEmpty(directory)) // 仅当 (!string.IsNullOrEmpty(directory)) 成立时执行后续处理。
                { // 开始当前代码块。
                    Directory.CreateDirectory(directory); // 确保资源或验证报告的目标目录已经存在。
                    File.WriteAllBytes(Path.Combine(directory, kind + ".png"), image.EncodeToPNG()); // 写出真实测试产生的图片或像素统计，便于核验画面。
                    File.WriteAllText(Path.Combine(directory, kind + "-pixels.txt"), "brightest=" + brightest + " pink=" + pink + " gpu=" + SystemInfo.graphicsDeviceName); // 写出真实测试产生的图片或像素统计，便于核验画面。
                } // 结束当前代码块。

                Assert.That(brightest, Is.GreaterThan(0.08f), "主相机没有产生可见画面。"); // 断言 brightest, Is.GreaterThan(0.08f), "主相机没有产生可见画面。")，验证实际行为符合预期。
                Assert.That(pink, Is.LessThan(pixels.Length / 200), "画面中存在大量错误 Shader 粉色。"); // 断言 pink, Is.LessThan(pixels.Length / 200), "画面中存在大量错误 Shader 粉色。")，验证实际行为符合预期。
            } // 结束当前代码块。
            finally // 无论测试或转换成功与否，都恢复原设置并清理拥有的对象。
            { // 开始当前代码块。
                camera.targetTexture = previousTarget; // 更新 camera.targetTexture，取值为 previousTarget。
                RenderTexture.active = previousActive; // 更新 RenderTexture.active，取值为 previousActive。
                target.Release(); // 释放测试用 RenderTexture 的 GPU 资源。
                UnityEngine.Object.Destroy(target); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
                UnityEngine.Object.Destroy(image); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
                if (spawned != null) // 仅当 (spawned != null) 成立时执行后续处理。
                    UnityEngine.Object.Destroy(spawned.gameObject); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            } // 结束当前代码块。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
