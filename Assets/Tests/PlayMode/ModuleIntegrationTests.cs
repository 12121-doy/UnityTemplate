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
    /// Mono 单例、玩法、动画和无尽生成的 PlayMode 集成测试。
    /// </summary>
    public sealed class ModuleIntegrationTests // Mono 单例、玩法、动画和无尽生成的 PlayMode 集成测试。
    { // 开始类型。
        /// <summary>
        /// 集成验证 Mono 单例保留主实例、拒绝重复项并在销毁时清空。
        /// </summary>
        /// <returns>协程枚举器，由 Unity 协程系统或 Test Runner 逐步推进。</returns>
        [UnityTest] // 把下方方法注册为 Unity/NUnit 测试用例。
        public IEnumerator MonoSingletonKeepsPrimaryAndClearsOnDestroy() // 集成验证 Mono 单例保留主实例、拒绝重复项并在销毁时清空。
        { // 开始方法。
            var go = new GameObject("SingletonPrimary"); // 创建 GameObject，保存为 go。
            var primary = go.AddComponent<SingletonTestManager>(); // 把 go.AddComponent<SingletonTestManager> 的返回值 保存为 primary。
            Assert.That(SingletonTestManager.TryGetInstance(out var found), Is.True); // 验证实际结果符合预期约束。
            Assert.That(found, Is.SameAs(primary)); // 验证实际结果符合预期约束。
            var duplicate = new GameObject("SingletonDuplicate"); // 创建 GameObject，保存为 duplicate。
            duplicate.AddComponent<SingletonTestManager>(); // 给 duplicate 添加 SingletonTestManager 组件。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(duplicate == null, Is.True); // 验证实际结果符合预期约束。
            Assert.That(SingletonTestManager.RequireInstance(), Is.SameAs(primary)); // 验证实际结果符合预期约束。
            var scene = SceneManager.CreateScene("SingletonSceneProbe"); // 把 SceneManager.CreateScene 的返回值 保存为 scene。
            SceneManager.SetActiveScene(scene); // 切换 Unity 当前活动场景。
            Assert.That(primary.gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad")); // 验证实际结果符合预期约束。
            Object.Destroy(go); // 调用 Object.Destroy 创建或销毁 Unity 对象。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(SingletonTestManager.Exists, Is.False); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 集成验证玩法、Animator 动作和无尽生成通过真实组件运行。
        /// </summary>
        /// <returns>协程枚举器，由 Unity 协程系统或 Test Runner 逐步推进。</returns>
        [UnityTest] // 把下方方法注册为 Unity/NUnit 测试用例。
        public IEnumerator ModesAnimationAndSurvivalUseRealGameplay() // 集成验证玩法、Animator 动作和无尽生成通过真实组件运行。
        { // 开始方法。
            yield return SceneManager.LoadSceneAsync("Assets/GuishaDemo/Scenes/Guisha.unity", LoadSceneMode.Single); // 把 SceneManager.LoadSceneAsync 的返回值 交给协程调度器/迭代器，暂时挂起当前枚举。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            EventBus.Publish(new GameModeRequested(GameMode.Story)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            var player = Object.FindObjectOfType<PlayerEntity>(); // 把 Object.FindObjectOfType<PlayerEntity> 的返回值 保存为 玩家组件或玩家对象。
            var animation = player.GetComponent<EntityAnimator>(); // 把 player.GetComponent<EntityAnimator> 的返回值 保存为 旧版 Animation 组件。
            Assert.That(animation, Is.Not.Null); // 验证实际结果符合预期约束。
            Assert.That(animation.Driver.Available, Is.True); // 验证实际结果符合预期约束。
            animation.Driver.Float("Speed", 2); // 设置浮点动画参数。
            Assert.That(animation.targetAnimator.GetFloat("Speed"), Is.EqualTo(2)); // 验证实际结果符合预期约束。
            EventBus.Publish(new EntityAnimationRequested(player, EntityAnimationState.Interact)); // 广播 EntityAnimationRequested 事件，让动画组件播放指定实体动作。
            yield return new WaitForSecondsRealtime(0.15f); // 暂停协程，等待指定帧/秒数或条件后继续。
            Assert.That(animation.targetAnimator.GetCurrentAnimatorStateInfo(0).IsName("Interact") || animation.targetAnimator.GetNextAnimatorStateInfo(0).IsName("Interact"), Is.True); // 验证实际结果符合预期约束。
            EventBus.Publish(new GameModeRequested(GameMode.Exploration)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            float health = player.Health; // 把 player.Health 保存为 保存的生命值。
            player.TakeDamage(new DamageInfo(100, null, player.transform.position)); // 校验伤害和存活状态，应用玩法倍率与本地伤害链，更新生命并广播受击/生命/死亡事件。
            Assert.That(player.Health, Is.EqualTo(health)); // 验证实际结果符合预期约束。
            var san = player.GetComponent<PlayerStatus>(); // 把 player.GetComponent<PlayerStatus> 的返回值 保存为 理智值或消耗倍率。
            float sanity = san.San; // 把 san.San 保存为 sanity。
            yield return new WaitForSecondsRealtime(0.3f); // 暂停协程，等待指定帧/秒数或条件后继续。
            Assert.That(san.San, Is.EqualTo(sanity)); // 验证实际结果符合预期约束。
            EventBus.Publish(new PanelRequested("Modes", true)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            Assert.That(GameSession.InputEnabled, Is.False); // 验证实际结果符合预期约束。
            EventBus.Publish(new PanelRequested("Modes", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            bool done = false; // 把 false（关闭/不满足） 保存为 done。
            EventBus.Publish(new SceneTransitionRequested("Assets/GuishaDemo/Scenes/BloodVilla.unity", 1, _ => done = true)); // 广播 SceneTransitionRequested 事件，交给场景管理器执行附加加载/切换。
            float timeout = Time.realtimeSinceStartup + 20; // 把 Time.realtimeSinceStartup + 20 保存为 timeout。
            while (!done && Time.realtimeSinceStartup < timeout) // 只要（条件 done 不成立；且Time.realtimeSinceStartup小于timeout），就继续循环。
                yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(done, Is.True); // 验证实际结果符合预期约束。
            EventBus.Publish(new GameModeRequested(GameMode.Endless)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            timeout = Time.realtimeSinceStartup + 20; // 更新 timeout，使用 Time.realtimeSinceStartup + 20。
            while ((GameModeRuntime.Wave < 1 || Object.FindObjectsOfType<GhostEntity>().Length < 2) && Time.realtimeSinceStartup < timeout) // 只要（（GameModeRuntime.Wave小于1；或Object.FindObjectsOfType小于GhostEntity>().Length < 2）；且Time.realtimeSinceStartup小于timeout），就继续循环。
                yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(GameModeRuntime.Wave, Is.GreaterThanOrEqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(Object.FindObjectsOfType<GhostEntity>().Length, Is.GreaterThanOrEqualTo(2)); // 验证实际结果符合预期约束。
            EventBus.Publish(new GameModeRequested(GameMode.Story)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            yield return null; // 暂停协程到下一帧，再从下一行继续。
            Assert.That(Object.FindObjectsOfType<GhostEntity>().Length, Is.LessThanOrEqualTo(1)); // 验证实际结果符合预期约束。
            var captured = new SaveData(); // 创建 SaveData，保存为 captured。
            EventBus.Publish(new SaveCapture(captured)); // 广播 SaveCapture 事件，让参与存档的组件填充共享数据。
            Assert.That(captured.gameMode, Is.EqualTo(GameMode.Story)); // 验证实际结果符合预期约束。
            EventBus.Publish(new GameModeRequested(GameMode.Exploration)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            EventBus.Publish(new SaveLoaded(captured)); // 广播 SaveLoaded 事件，让组件从存档恢复各自状态。
            Assert.That(GameModeRuntime.Rules.Mode, Is.EqualTo(GameMode.Story)); // 验证实际结果符合预期约束。
            EventBus.Publish(new GameModeRequested(GameMode.Challenge)); // 广播 GameModeRequested 事件，请求切换玩法规则。
            var mechanism = Object.FindObjectOfType<Mechanism>(); // 把 Object.FindObjectOfType<Mechanism> 的返回值 保存为 mechanism。
            WorldState.Set(mechanism.requiredClueId); // 首次加入标记时广播 WorldStateChanged。
            mechanism.Interact(Object.FindObjectOfType<PlayerEntity>().gameObject); // 解析/检查条件表达式和旧线索条件，通过后设置标记，广播 MechanismActivated 并提示。
            Assert.That(GameModeRuntime.Rules.Mode, Is.EqualTo(GameMode.Story), "Challenge should finish at the configured mechanism"); // 验证实际结果符合预期约束。
            var activePlayer = Object.FindObjectOfType<PlayerEntity>(); // 把 Object.FindObjectOfType<PlayerEntity> 的返回值 保存为 activePlayer。
            var saved = new SaveData(); // 创建 SaveData，保存为 saved。
            EventBus.Publish(new SaveCapture(saved)); // 广播 SaveCapture 事件，让参与存档的组件填充共享数据。
            activePlayer.SetHealth(1); // 钳制生命到 0-MaxHealth，并广播 HealthChanged；此入口本身不广播 EntityDied。
            activePlayer.TakeDamage(new DamageInfo(100, null, activePlayer.transform.position)); // 校验伤害和存活状态，应用玩法倍率与本地伤害链，更新生命并广播受击/生命/死亡事件。
            Assert.That(GameSession.State, Is.EqualTo(GameState.GameOver)); // 验证实际结果符合预期约束。
            yield return new WaitForSecondsRealtime(0.4f); // 暂停协程，等待指定帧/秒数或条件后继续。
            var activeAnimation = activePlayer.GetComponent<EntityAnimator>(); // 把 activePlayer.GetComponent<EntityAnimator> 的返回值 保存为 activeAnimation。
            Assert.That(activeAnimation.targetAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dead") || activeAnimation.targetAnimator.GetNextAnimatorStateInfo(0).IsName("Dead"), Is.True, "Death animation should advance while gameplay is frozen"); // 验证实际结果符合预期约束。
            Time.timeScale = 1; // 设置全局缩放时间，控制暂停与恢复；值来自 1。
            GameSession.SetState(GameState.Playing); // 更新共享流程状态并发布 GameStateChanged。
            EventBus.Publish(new SaveLoaded(saved)); // 广播 SaveLoaded 事件，让组件从存档恢复各自状态。
            Assert.That(activeAnimation.targetAnimator.updateMode, Is.EqualTo(AnimatorUpdateMode.Normal)); // 验证实际结果符合预期约束。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
