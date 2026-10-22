using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.IO; // 引入 System.IO 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using Guisha.Gameplay; // 引入 Guisha.Gameplay 命名空间，使用其中的类型与 API。
using Guisha.Samples; // 引入 Guisha.Samples 命名空间，使用其中的类型与 API。
using UnityEditor; // 引入 UnityEditor 命名空间，使用其中的类型与 API。
using UnityEditor.Animations; // 引入 UnityEditor.Animations 命名空间，使用其中的类型与 API。
using UnityEditor.SceneManagement; // 引入 UnityEditor.SceneManagement 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 命名空间，使用其中的类型与 API。

namespace Guisha.Editor // 将本文件类型归入 Guisha.Editor，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 编辑器创建动画控制器、模式配置，并更新演示场景。
    /// </summary>
    public static class PatternAnimationSetup // 编辑器创建动画控制器、模式配置，并更新演示场景。
    { // 开始类型。
        /// <summary>
        /// 把 PatternExamples.Run 的结果写入 Console。
        /// </summary>
        [MenuItem("Guisha/Patterns/Run Examples")] // 将下方静态方法注册为 Unity 编辑器菜单入口。
        public static void Examples() // 把 PatternExamples.Run 的结果写入 Console。
        { // 开始方法。
            Debug.Log(PatternExamples.Run()); // 向 Unity Console 输出信息。
        } // 结束方法。

        /// <summary>
        /// 补建动画与四种玩法配置，更新演示场景和鬼怪预制体，已有配置优先保留。
        /// </summary>
        [MenuItem("Guisha/Setup Modes And Animation Demo")] // 将下方静态方法注册为 Unity 编辑器菜单入口。
        public static void SetupProjectAssets() // 补建动画与四种玩法配置，更新演示场景和鬼怪预制体，已有配置优先保留。
        { // 开始方法。
            const string root = "Assets/GuishaDemo"; // 把 "Assets/GuishaDemo" 保存为 演示资产根目录路径。
            if (!Directory.Exists(root + "/Scenes")) // 当条件 Directory.Exists(root + "/Scenes") 不成立时，执行下方分支。
                throw new InvalidOperationException("Create Greybox Demo first."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Create Greybox Demo first."。
            var previous = SceneManager.GetActiveScene(); // 把 SceneManager.GetActiveScene 的返回值 保存为 修改前的旧值。
            if (string.IsNullOrEmpty(previous.path)) // 当条件 string.IsNullOrEmpty(previous.path) 成立时，执行下方分支。
                throw new InvalidOperationException("Save the current scene before configuring demo assets."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Save the current scene before configuring demo assets."。
            Directory.CreateDirectory(root + "/Animation"); // 确保目标目录存在。
            Directory.CreateDirectory(root + "/Config/Modes"); // 确保目标目录存在。
            AssetDatabase.Refresh(); // 通知 Unity 刷新资源数据库。
            var animation = AssetDatabase.LoadAssetAtPath<AnimationSettings>(root + "/Animation/Settings.asset"); // 把 AssetDatabase.LoadAssetAtPath<AnimationSettings> 的返回值 保存为 旧版 Animation 组件。
            if (animation == null) // 当旧版 Animation 组件为空时，执行下方分支。
            { // 开始条件分支。
                animation = ScriptableObject.CreateInstance<AnimationSettings>(); // 更新 旧版 Animation 组件，使用 ScriptableObject.CreateInstance<AnimationSettings> 的返回值。
                AssetDatabase.CreateAsset(animation, root + "/Animation/Settings.asset"); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            } // 结束条件分支。

            var controller = CreateController(root + "/Animation/Entity.controller"); // 把 CreateController 的返回值 保存为 控制器组件。
            var modes = new GameModeDefinition[4]; // 把 new GameModeDefinition[4] 保存为 玩法选择面板或配置数组。
            for (int i = 0; i < modes.Length; i++) // 按初始化与递增步骤循环；继续条件为 i < modes.Length。
            { // 开始计数循环。
                string path = root + "/Config/Modes/" + (GameMode)i + ".asset"; // 把 root + "/Config/Modes/" + (GameMode)i + ".asset" 保存为 文件或资源路径。
                modes[i] = AssetDatabase.LoadAssetAtPath<GameModeDefinition>(path); // 更新 modes[i]，使用 AssetDatabase.LoadAssetAtPath<GameModeDefinition> 的返回值。
                if (modes[i] != null) // 当modes[i]不为空时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                var rules = GameModeRules.Default((GameMode)i); // 把 GameModeRules.Default 的返回值 保存为 规则序列或规则快照。
                var value = ScriptableObject.CreateInstance<GameModeDefinition>(); // 把 ScriptableObject.CreateInstance<GameModeDefinition> 的返回值 保存为 当前处理值。
                value.mode = rules.Mode; // 更新 value.mode，使用 rules.Mode。
                value.playerDamage = rules.PlayerDamage; // 更新 value.playerDamage，使用 rules.PlayerDamage。
                value.sanDrain = rules.SanDrain; // 更新 value.sanDrain，使用 rules.SanDrain。
                value.staminaCost = rules.StaminaCost; // 更新 value.staminaCost，使用 rules.StaminaCost。
                value.enemySpeed = rules.EnemySpeed; // 更新 value.enemySpeed，使用 rules.EnemySpeed。
                value.hostileGhosts = rules.HostileGhosts; // 更新 value.hostileGhosts，使用 rules.HostileGhosts。
                value.allowSave = rules.AllowSave; // 更新 value.allowSave，使用 rules.AllowSave。
                value.timeLimit = rules.TimeLimit; // 更新 value.timeLimit，使用 rules.TimeLimit。
                AssetDatabase.CreateAsset(value, path); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
                modes[i] = value; // 更新 modes[i]，使用 当前处理值。
            } // 结束计数循环。

            var catalog = AssetDatabase.LoadAssetAtPath<GameModeCatalog>(root + "/Config/Modes/Catalog.asset"); // 把 AssetDatabase.LoadAssetAtPath<GameModeCatalog> 的返回值 保存为 可用配置目录。
            if (catalog == null) // 当可用配置目录为空时，执行下方分支。
            { // 开始条件分支。
                catalog = ScriptableObject.CreateInstance<GameModeCatalog>(); // 更新 可用配置目录，使用 ScriptableObject.CreateInstance<GameModeCatalog> 的返回值。
                catalog.definitions = modes; // 更新 catalog.definitions，使用 玩法选择面板或配置数组。
                AssetDatabase.CreateAsset(catalog, root + "/Config/Modes/Catalog.asset"); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            } // 结束条件分支。

            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings"); // 把 Resources.Load<FrameworkSettings> 的返回值 保存为 框架或动画配置。
            if (settings != null && settings.gameModes == null) // 当（框架或动画配置不为空；且settings.gameModes为空）时，执行下方分支。
            { // 开始条件分支。
                settings.gameModes = catalog; // 更新 settings.gameModes，使用 可用配置目录。
                EditorUtility.SetDirty(settings); // 标记资产已修改，等待保存。
            } // 结束条件分支。

            foreach (string name in new[] // 遍历 new[] {     "Guisha",     "BloodVilla",     "RainVillage" }，每次把一个元素保存为 name。
            { // 开始成员/集合初始化。
                "Guisha", // 提供初始化/参数元素 "Guisha"，交给上方的对象或集合使用。
                "BloodVilla", // 提供初始化/参数元素 "BloodVilla"，交给上方的对象或集合使用。
                "RainVillage" // 提供初始化/参数元素 "RainVillage"，交给上方的对象或集合使用。
            } // 结束成员/集合初始化。

            ) // 结束上方跨行的参数/条件列表。
            { // 开始遍历循环。
                string path = root + "/Scenes/" + name + ".unity"; // 把 root + "/Scenes/" + name + ".unity" 保存为 文件或资源路径。
                var scene = SceneManager.GetSceneByPath(path); // 把 SceneManager.GetSceneByPath 的返回值 保存为 scene。
                bool opened = !scene.isLoaded; // 把 !scene.isLoaded 保存为 门当前是否打开。
                if (opened) // 当门当前是否打开为 true时，执行下方分支。
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); // 更新 scene，使用 EditorSceneManager.OpenScene 的返回值。
                foreach (var go in scene.GetRootGameObjects()) // 遍历 scene.GetRootGameObjects()，每次把一个元素保存为 go。
                    foreach (var builder in go.GetComponentsInChildren<DemoScene>(true)) // 遍历 go.GetComponentsInChildren<DemoScene>(true)，每次把一个元素保存为 builder。
                    { // 开始遍历循环。
                        builder.animationController = controller; // 更新 builder.animationController，使用 控制器组件。
                        builder.animationSettings = animation; // 更新 builder.animationSettings，使用 旧版 Animation 组件。
                        EditorUtility.SetDirty(builder); // 标记资产已修改，等待保存。
                    } // 结束遍历循环。

                EditorSceneManager.SaveScene(scene); // 调用 EditorSceneManager.SaveScene，提交编辑器资产/场景配置。
                if (opened) // 当门当前是否打开为 true时，执行下方分支。
                    EditorSceneManager.CloseScene(scene, true); // 关闭临时创建/编辑的场景，完成编辑器生成清理。
            } // 结束遍历循环。

            string prefabPath = root + "/Prefabs/RedGhost.prefab"; // 把 root + "/Prefabs/RedGhost.prefab" 保存为 prefabPath。
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath); // 把 PrefabUtility.LoadPrefabContents 的返回值 保存为 prefab。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                var visual = prefab.transform.GetChild(0); // 把 prefab.transform.GetChild 的返回值 保存为 表现对象。
                var animator = visual.GetComponent<Animator>(); // 把 visual.GetComponent<Animator> 的返回值 保存为 Animator 动画组件。
                if (animator == null) // 当Animator 动画组件为空时，执行下方分支。
                    animator = visual.gameObject.AddComponent<Animator>(); // 更新 Animator 动画组件，使用 visual.gameObject.AddComponent<Animator> 的返回值。
                animator.runtimeAnimatorController = controller; // 更新 animator.runtimeAnimatorController，使用 控制器组件。
                var driver = prefab.GetComponent<EntityAnimator>(); // 把 prefab.GetComponent<EntityAnimator> 的返回值 保存为 统一动画驱动。
                if (driver == null) // 当统一动画驱动为空时，执行下方分支。
                    driver = prefab.AddComponent<EntityAnimator>(); // 更新 统一动画驱动，使用 prefab.AddComponent<EntityAnimator> 的返回值。
                driver.settings = animation; // 更新 driver.settings，使用 旧版 Animation 组件。
                driver.targetAnimator = animator; // 更新 driver.targetAnimator，使用 Animator 动画组件。
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath); // 把当前对象层级保存为指定路径的预制体资产。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                PrefabUtility.UnloadPrefabContents(prefab); // 卸载编辑器临时打开的预制体内容，释放隔离编辑场景。
            } // 结束最终清理。

            if (previous.IsValid()) // 当条件 previous.IsValid() 成立时，执行下方分支。
                SceneManager.SetActiveScene(previous); // 切换 Unity 当前活动场景。
            AssetDatabase.SaveAssets(); // 把编辑器资产修改保存到磁盘。
            Debug.Log("Guisha patterns, mode catalog and animation demo configured."); // 向 Unity Console 输出信息。
        } // 结束方法。

        /// <summary>
        /// 控制器不存在时创建参数、八种动作状态、移动/蹲伏/死亡与动作过渡。
        /// </summary>
        /// <param name="path">文件或资源路径。</param>
        /// <returns>UnityEditor.Animations.AnimatorController 类型的处理结果。</returns>
        private static AnimatorController CreateController(string path) // 控制器不存在时创建参数、八种动作状态、移动/蹲伏/死亡与动作过渡。
        { // 开始方法。
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path); // 把 AssetDatabase.LoadAssetAtPath<AnimatorController> 的返回值 保存为 existing。
            if (existing != null) // 当existing不为空时，执行下方分支。
                return existing; // 返回 existing。
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path); // 把 AnimatorController.CreateAnimatorControllerAtPath 的返回值 保存为 控制器组件。
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float); // 调用 controller.AddParameter，提交当前对象的配置更新。
            foreach (string name in new[] // 遍历 new[] {     "Crouched",     "Grounded",     "Dead" }，每次把一个元素保存为 name。
            { // 开始成员/集合初始化。
                "Crouched", // 提供初始化/参数元素 "Crouched"，交给上方的对象或集合使用。
                "Grounded", // 提供初始化/参数元素 "Grounded"，交给上方的对象或集合使用。
                "Dead" // 提供初始化/参数元素 "Dead"，交给上方的对象或集合使用。
            } // 结束成员/集合初始化。

            ) // 结束上方跨行的参数/条件列表。
                controller.AddParameter(name, AnimatorControllerParameterType.Bool); // 调用 controller.AddParameter，提交当前对象的配置更新。
            foreach (string name in new[] // 遍历 new[] {     "Attack",     "Hit",     "Interact" }，每次把一个元素保存为 name。
            { // 开始成员/集合初始化。
                "Attack", // 提供初始化/参数元素 "Attack"，交给上方的对象或集合使用。
                "Hit", // 提供初始化/参数元素 "Hit"，交给上方的对象或集合使用。
                "Interact" // 提供初始化/参数元素 "Interact"，交给上方的对象或集合使用。
            } // 结束成员/集合初始化。

            ) // 结束上方跨行的参数/条件列表。
                controller.AddParameter(name, AnimatorControllerParameterType.Trigger); // 调用 controller.AddParameter，提交当前对象的配置更新。
            EditorUtility.SetDirty(controller); // 标记根控制器参数已修改，避免导入动画子资产时丢失尚未持久化的参数。
            AssetDatabase.SaveAssets(); // 在创建 AnimationClip 子资产之前先保存 Speed、布尔和触发参数。
            var machine = controller.layers[0].stateMachine; // 把 controller.layers[0].stateMachine 保存为 有限状态机。
            AnimatorState idle = State(controller, machine, "Idle", 0.01f, true), walk = State(controller, machine, "Walk", 0.05f, true), run = State(controller, machine, "Run", 0.09f, true); // 把 State 的返回值 保存为 空闲对象集合或待机状态；把 State 的返回值 保存为 walk；把 State 的返回值 保存为 run。
            var crouch = State(controller, machine, "Crouch", 0.01f, true); // 把 State 的返回值 保存为 crouch。
            var dead = State(controller, machine, "Dead", 0, false); // 把 State 的返回值 保存为 dead。
            machine.defaultState = idle; // 更新 machine.defaultState，使用 空闲对象集合或待机状态。
            Transition(idle, walk, AnimatorConditionMode.Greater, 0.1f, "Speed"); // 添加无退出时间的参数条件过渡。
            Transition(walk, idle, AnimatorConditionMode.Less, 0.1f, "Speed"); // 添加无退出时间的参数条件过渡。
            Transition(walk, run, AnimatorConditionMode.Greater, 4, "Speed"); // 添加无退出时间的参数条件过渡。
            Transition(run, walk, AnimatorConditionMode.Less, 4, "Speed"); // 添加无退出时间的参数条件过渡。
            foreach (var state in new[] // 遍历 new[] {     idle,     walk,     run }，每次把一个元素保存为 state。
            { // 开始成员/集合初始化。
                idle, // 提供初始化/参数元素 idle，交给上方的对象或集合使用。
                walk, // 提供初始化/参数元素 walk，交给上方的对象或集合使用。
                run // 提供初始化/参数元素 run，交给上方的对象或集合使用。
            } // 结束成员/集合初始化。

            ) // 结束上方跨行的参数/条件列表。
                Transition(state, crouch, AnimatorConditionMode.If, 0, "Crouched"); // 添加无退出时间的参数条件过渡。
            Transition(crouch, idle, AnimatorConditionMode.IfNot, 0, "Crouched"); // 添加无退出时间的参数条件过渡。
            Transition(dead, idle, AnimatorConditionMode.IfNot, 0, "Dead"); // 添加无退出时间的参数条件过渡。
            var death = machine.AddAnyStateTransition(dead); // 把 machine.AddAnyStateTransition 的返回值 保存为 death。
            death.hasExitTime = false; // 更新 death.hasExitTime，使用 false（关闭/不满足）。
            death.canTransitionToSelf = false; // 更新 death.canTransitionToSelf，使用 false（关闭/不满足）。
            death.AddCondition(AnimatorConditionMode.If, 0, "Dead"); // 调用 death.AddCondition，提交当前对象的配置更新。
            foreach (string name in new[] // 遍历 new[] {     "Attack",     "Hit",     "Interact" }，每次把一个元素保存为 name。
            { // 开始成员/集合初始化。
                "Attack", // 提供初始化/参数元素 "Attack"，交给上方的对象或集合使用。
                "Hit", // 提供初始化/参数元素 "Hit"，交给上方的对象或集合使用。
                "Interact" // 提供初始化/参数元素 "Interact"，交给上方的对象或集合使用。
            } // 结束成员/集合初始化。

            ) // 结束上方跨行的参数/条件列表。
            { // 开始遍历循环。
                var state = State(controller, machine, name, 0.03f, false); // 把 State 的返回值 保存为 Animator 动画状态对象。
                var enter = machine.AddAnyStateTransition(state); // 把 machine.AddAnyStateTransition 的返回值 保存为 进入状态时执行的测试委托。
                enter.hasExitTime = false; // 更新 enter.hasExitTime，使用 false（关闭/不满足）。
                enter.canTransitionToSelf = false; // 更新 enter.canTransitionToSelf，使用 false（关闭/不满足）。
                enter.AddCondition(AnimatorConditionMode.If, 0, name); // 调用 enter.AddCondition，提交当前对象的配置更新。
                enter.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead"); // 调用 enter.AddCondition，提交当前对象的配置更新。
                var exit = state.AddTransition(idle); // 把 state.AddTransition 的返回值 保存为 exit。
                exit.hasExitTime = true; // 更新 exit.hasExitTime，使用 true（启用/满足）。
                exit.exitTime = 0.9f; // 更新 exit.exitTime，使用 0.9f。
                exit.duration = 0.1f; // 更新 exit.duration，使用 0.1f。
            } // 结束遍历循环。

            EditorUtility.SetDirty(controller); // 保存创建后的根控制器和层设置。
            EditorUtility.SetDirty(machine); // 保存状态机的默认状态与转换关系。
            AssetDatabase.SaveAssets(); // 保证新项目关闭或重新导入后仍有完整动画配置。
            return controller; // 返回 控制器组件。
        } // 结束方法。

        /// <summary>
        /// 创建演示 AnimationClip 子资产与 AnimatorState。
        /// </summary>
        /// <param name="controller">控制器组件。</param>
        /// <param name="machine">有限状态机。</param>
        /// <param name="name">名称。</param>
        /// <param name="bob">演示动画的上下摆动幅度。</param>
        /// <param name="loop">是否循环播放。</param>
        /// <returns>UnityEditor.Animations.AnimatorState 类型的处理结果。</returns>
        private static AnimatorState State(AnimatorController controller, AnimatorStateMachine machine, string name, float bob, bool loop) // 创建演示 AnimationClip 子资产与 AnimatorState。
        { // 开始方法。
            var clip = new AnimationClip // 创建 AnimationClip，保存为 音频或动画片段。
            { // 开始成员/集合初始化。
                name = name, // 更新 名称，使用 名称。
                frameRate = 30 // 更新 frameRate，使用 30。
            }; // 结束成员/集合初始化。
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", AnimationCurve.EaseInOut(0, name == "Crouch" ? 0.6f : 0.9f, 0.5f, name == "Crouch" ? 0.6f + bob : 0.9f + bob)); // 为演示片段写入 Transform 动画曲线，驱动灰盒模型的位移或旋转。
            if (name == "Dead") // 当名称等于"Dead"时，执行下方分支。
                clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", AnimationCurve.EaseInOut(0, 0, 0.5f, 75)); // 为演示片段写入 Transform 动画曲线，驱动灰盒模型的位移或旋转。
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip); // 把 AnimationUtility.GetAnimationClipSettings 的返回值 保存为 clipSettings。
            clipSettings.loopTime = loop; // 更新 clipSettings.loopTime，使用 是否循环播放。
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings); // 把循环等导入/播放设置写回动画片段。
            AssetDatabase.AddObjectToAsset(clip, controller); // 调用 AssetDatabase.AddObjectToAsset，提交编辑器资产/场景配置。
            var state = machine.AddState(name); // 把 machine.AddState 的返回值 保存为 Animator 动画状态对象。
            state.motion = clip; // 更新 state.motion，使用 音频或动画片段。
            return state; // 返回 Animator 动画状态对象。
        } // 结束方法。

        /// <summary>
        /// 添加无退出时间的参数条件过渡。
        /// </summary>
        /// <param name="source">原始来源。</param>
        /// <param name="destination">目标位置。</param>
        /// <param name="mode">当前玩法。</param>
        /// <param name="threshold">比较阈值。</param>
        /// <param name="parameter">过渡条件的动画参数名。</param>
        private static void Transition(AnimatorState source, AnimatorState destination, AnimatorConditionMode mode, float threshold, string parameter) // 添加无退出时间的参数条件过渡。
        { // 开始方法。
            var transition = source.AddTransition(destination); // 把 source.AddTransition 的返回值 保存为 transition。
            transition.hasExitTime = false; // 更新 transition.hasExitTime，使用 false（关闭/不满足）。
            transition.duration = 0.12f; // 更新 transition.duration，使用 0.12f。
            transition.AddCondition(mode, threshold, parameter); // 调用 transition.AddCondition，提交当前对象的配置更新。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
