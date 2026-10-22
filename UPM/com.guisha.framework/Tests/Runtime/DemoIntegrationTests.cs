using System.Collections; // 引入 System.Collections 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using Guisha.Gameplay; // 引入 Guisha.Gameplay 命名空间，使用其中的类型与 API。
using Guisha.Managers; // 引入 Guisha.Managers 命名空间，使用其中的类型与 API。
using Guisha.UI; // 引入 Guisha.UI 命名空间，使用其中的类型与 API。
using NUnit.Framework; // 引入 NUnit.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 命名空间，使用其中的类型与 API。
using UnityEngine.TestTools; // 引入 UnityEngine.TestTools 命名空间，使用其中的类型与 API。

namespace Guisha.Validation // 将本文件类型归入 Guisha.Validation，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 灰盒场景与存档/场景切换的 PlayMode 集成测试。
    /// </summary>
    public sealed class DemoIntegrationTests // 灰盒场景与存档/场景切换的 PlayMode 集成测试。
    { // 开始类型。
        /// <summary>
        /// 保存主相机引用。
        /// </summary>
        private const string Main = "Assets/GuishaDemo/Scenes/Guisha.unity"; // 把 "Assets/GuishaDemo/Scenes/Guisha.unity" 保存为 主相机引用。
        /// <summary>
        /// 保存副本编号。
        /// </summary>
        private const string Dungeon = "Assets/GuishaDemo/Scenes/BloodVilla.unity"; // 把 "Assets/GuishaDemo/Scenes/BloodVilla.unity" 保存为 副本编号。
        /// <summary>
        /// 集成验证灰盒加载、玩家/背包存档恢复与附加副本切换。
        /// </summary>
        /// <returns>协程枚举器，由 Unity 协程系统或 Test Runner 逐步推进。</returns>
        [UnityTest] // 把下方方法注册为 Unity/NUnit 测试用例。
        public IEnumerator GreyboxLoadsRestoresStateAndSwitchesDungeon() // 集成验证灰盒加载、玩家/背包存档恢复与附加副本切换。
        { // 开始方法。
            yield return SceneManager.LoadSceneAsync(Main, LoadSceneMode.Single); // 把 SceneManager.LoadSceneAsync 的返回值 交给协程调度器/迭代器，暂时挂起当前枚举。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(GameManager.Exists && ResourceManager.Exists, Is.True); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectsOfType<PlayerEntity>().Length, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectsOfType<AudioListener>().Length, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectOfType<HUD>(), Is.Not.Null); // 验证实际结果符合预期约束。
            var player = Object.FindObjectOfType<PlayerEntity>(); // 把 Object.FindObjectOfType<PlayerEntity> 的返回值 保存为 玩家组件或玩家对象。
            var status = player.GetComponent<PlayerStatus>(); // 把 player.GetComponent<PlayerStatus> 的返回值 保存为 玩家状态组件。
            var inventory = player.GetComponent<Inventory>(); // 把 player.GetComponent<Inventory> 的返回值 保存为 背包组件。
            status.ChangeSan(-30); // 按 delta 改变 San，钳制到有效范围，数值变化时广播。
            Assert.That(inventory.TryAdd("ghost_artifact"), Is.True); // 验证实际结果符合预期约束。
            var data = new SaveData(); // 创建 SaveData，保存为 待保存或已读取的数据。
            EventBus.Publish(new SaveCapture(data)); // 广播 SaveCapture 事件，让参与存档的组件填充共享数据。
            Assert.That(data.hasPlayer, Is.True); // 验证实际结果符合预期约束。
            inventory.Use(0); // 检查模拟状态与槽位，成功使用后按 consumable 移除，并广播快照。
            Assert.That(inventory.Items.Count, Is.EqualTo(0)); // 验证实际结果符合预期约束。
            EventBus.Publish(new SaveLoaded(data)); // 广播 SaveLoaded 事件，让组件从存档恢复各自状态。
            Assert.That(status.San, Is.EqualTo(data.san).Within(0.01f)); // 验证实际结果符合预期约束。
            Assert.That(inventory.Items.Count, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            EventBus.Publish(new PanelRequested("Inventory", true)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            Assert.That(GameSession.InputEnabled, Is.False); // 验证实际结果符合预期约束。
            EventBus.Publish(new PanelRequested("Inventory", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            Assert.That(GameSession.InputEnabled, Is.True); // 验证实际结果符合预期约束。
            bool done = false, success = false; // 把 false（关闭/不满足） 保存为 done；把 false（关闭/不满足） 保存为 是否成功。
            EventBus.Publish(new SceneTransitionRequested(Dungeon, 1, ok => // 广播 SceneTransitionRequested 事件，交给场景管理器执行附加加载/切换。
            { // 开始回调。
                success = ok; // 更新 是否成功，使用 ok。
                done = true; // 更新 done，使用 true（启用/满足）。
            })); // 结束回调。
            float timeout = Time.realtimeSinceStartup + 20; // 把 Time.realtimeSinceStartup + 20 保存为 timeout。
            while (!done && Time.realtimeSinceStartup < timeout) // 只要（条件 done 不成立；且Time.realtimeSinceStartup小于timeout），就继续循环。
                yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(done && success, Is.True, "Dungeon transition timed out or failed"); // 验证实际结果符合预期约束。
            timeout = Time.realtimeSinceStartup + 20; // 更新 timeout，使用 Time.realtimeSinceStartup + 20。
            while (Object.FindObjectOfType<GhostEntity>() == null && Time.realtimeSinceStartup < timeout) // 只要（Object.FindObjectOfType小于GhostEntity>() == null；且Time.realtimeSinceStartup小于timeout），就继续循环。
                yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(Object.FindObjectsOfType<PlayerEntity>().Length, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectsOfType<AudioListener>().Length, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            var ghost = Object.FindObjectOfType<GhostEntity>(); // 把 Object.FindObjectOfType<GhostEntity> 的返回值 保存为 鬼怪实体或鬼怪配置。
            Assert.That(ghost, Is.Not.Null, "Addressables ghost not spawned"); // 验证实际结果符合预期约束。
            Assert.That(ghost.Definition.id, Is.EqualTo(EntityId.RedGhost)); // 验证实际结果符合预期约束。
            Assert.That(ghost.gameObject.scene.path, Is.EqualTo(Dungeon)); // 验证实际结果符合预期约束。
            Assert.That(ghost.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh, Is.True); // 验证实际结果符合预期约束。
            done = false; // 更新 done，使用 false（关闭/不满足）。
            success = false; // 更新 是否成功，使用 false（关闭/不满足）。
            EventBus.Publish(new SceneTransitionRequested(Main, 0, ok => // 广播 SceneTransitionRequested 事件，交给场景管理器执行附加加载/切换。
            { // 开始回调。
                success = ok; // 更新 是否成功，使用 ok。
                done = true; // 更新 done，使用 true（启用/满足）。
            })); // 结束回调。
            timeout = Time.realtimeSinceStartup + 20; // 更新 timeout，使用 Time.realtimeSinceStartup + 20。
            while (!done && Time.realtimeSinceStartup < timeout) // 只要（条件 done 不成立；且Time.realtimeSinceStartup小于timeout），就继续循环。
                yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(done && success, Is.True); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectsOfType<PlayerEntity>().Length, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectsOfType<AudioListener>().Length, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(SceneManager.GetSceneByPath(Dungeon).isLoaded, Is.False); // 验证实际结果符合预期约束。
            EventBus.Publish(new PanelRequested("Inventory", true)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            Assert.That(GameSession.InputEnabled, Is.False, "Main-scene UI did not re-register"); // 验证实际结果符合预期约束。
            EventBus.Publish(new PanelRequested("Inventory", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            Assert.That(GameSession.InputEnabled, Is.True); // 验证实际结果符合预期约束。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
