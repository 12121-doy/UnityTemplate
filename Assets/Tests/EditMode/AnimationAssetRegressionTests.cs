using System; // 使用随机标识创建独立测试目录。
using System.Linq; // 检查重新导入后的参数集合。
using System.Reflection; // 调用编辑器生成器的控制器创建入口。
using Guisha.Editor; // 引用框架自带动画资源生成器。
using NUnit.Framework; // 声明 NUnit 测试与断言。
using UnityEditor; // 保存、重新导入并清理临时资产。
using UnityEditor.Animations; // 读取持久化后的 AnimatorController。
using UnityEngine; // 读取动画参数类型。

namespace Guisha.Tests // 将资产生成回归归入编辑器测试程序集。
{ // 开始测试命名空间。
    /// <summary>验证新项目生成的动画参数和状态在重新导入后仍然存在。</summary>
    public sealed class AnimationAssetRegressionTests // 首次生成时不能只依赖内存中的控制器状态。
    { // 开始测试类型。
        /// <summary>创建控制器并强制重新导入，检查 Speed、布尔、触发参数及状态机持久化。</summary>
        [Test] // 将此方法注册为编辑器回归测试。
        public void GeneratedControllerRetainsParametersAfterReimport() // 模拟新项目首次创建并重新加载动画资产。
        { // 开始测试方法。
            string folder = "GuishaAnimationTest_" + Guid.NewGuid().ToString("N"); // 使用唯一名称隔离本次测试资产。
            string directory = "Assets/" + folder; // 将临时目录限定在当前测试工程 Assets 内。
            string path = directory + "/Entity.controller"; // 指定本测试创建的控制器路径。
            AssetDatabase.CreateFolder("Assets", folder); // 创建唯一的临时资产目录。
            try // 保证断言失败时仍清理本测试资产。
            { // 开始资产验证范围。
                var create = typeof(PatternAnimationSetup).GetMethod("CreateController", BindingFlags.NonPublic | BindingFlags.Static); // 定位实际生成示例所用的方法。
                Assert.That(create, Is.Not.Null); // 确认测试没有绕过真正的生成入口。
                create.Invoke(null, new object[] { path }); // 从零创建控制器、参数、动画和转换。
                AssetDatabase.SaveAssets(); // 让测试读取磁盘持久化结果。
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport); // 强制重导入，避免只验证内存对象。
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path); // 重新读取导入后的控制器。
                Assert.That(controller, Is.Not.Null); // 必须生成可读取的资产。
                Assert.That(controller.parameters.Length, Is.EqualTo(7)); // Speed、三个布尔参数和三个触发参数都应保留。
                Assert.That(controller.parameters.Single(p => p.name == "Speed").type, Is.EqualTo(AnimatorControllerParameterType.Float)); // 运行时速度参数必须是浮点类型。
                Assert.That(controller.parameters.Single(p => p.name == "Dead").type, Is.EqualTo(AnimatorControllerParameterType.Bool)); // 死亡状态参数必须保留布尔类型。
                Assert.That(controller.parameters.Single(p => p.name == "Interact").type, Is.EqualTo(AnimatorControllerParameterType.Trigger)); // 交互动作必须保留触发参数。
                Assert.That(controller.layers[0].stateMachine.defaultState.name, Is.EqualTo("Idle")); // 默认动画状态必须持久化。
                Assert.That(controller.layers[0].stateMachine.states.Length, Is.EqualTo(8)); // 全部八个模板状态都应保存。
            } // 结束资产验证范围。
            finally // 成功和失败路径都移除临时资源。
            { // 开始配对清理。
                string assets = System.IO.Path.GetFullPath(Application.dataPath) + System.IO.Path.DirectorySeparatorChar; // 取得当前工程 Assets 的绝对边界。
                string resolved = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, folder)); // 解析唯一测试目录的绝对路径。
                Assert.That(resolved.StartsWith(assets, StringComparison.OrdinalIgnoreCase), Is.True); // 确认清理目标确实位于本工程 Assets 内。
                AssetDatabase.DeleteAsset(directory); // 只删除通过固定 Assets 前缀和随机名称构造的本次测试目录。
            } // 结束清理范围。
        } // 结束测试方法。
    } // 结束测试类型。
} // 结束测试命名空间。
