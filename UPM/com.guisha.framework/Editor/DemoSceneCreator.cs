using System.IO; // 引入 System.IO 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using Guisha.Gameplay; // 引入 Guisha.Gameplay 命名空间，使用其中的类型与 API。
using Guisha.Samples; // 引入 Guisha.Samples 命名空间，使用其中的类型与 API。
using UnityEditor; // 引入 UnityEditor 命名空间，使用其中的类型与 API。
using UnityEditor.SceneManagement; // 引入 UnityEditor.SceneManagement 命名空间，使用其中的类型与 API。
using UnityEditor.AddressableAssets; // 引入 UnityEditor.AddressableAssets 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.AI; // 引入 UnityEngine.AI 命名空间，使用其中的类型与 API。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 命名空间，使用其中的类型与 API。

namespace Guisha.Editor // 将本文件类型归入 Guisha.Editor，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 编辑器灰盒场景、预制体与 Addressables 配置生成入口。
    /// </summary>
    public static class DemoSceneCreator // 编辑器灰盒场景、预制体与 Addressables 配置生成入口。
    { // 开始类型。
        /// <summary>
        /// 菜单入口，处理场景保存提示，生成后打开主演示场景并显示说明。
        /// </summary>
        [MenuItem("Guisha/Create Greybox Demo")] // 将下方静态方法注册为 Unity 编辑器菜单入口。
        public static void Create() // 菜单入口，处理场景保存提示，生成后打开主演示场景并显示说明。
        { // 开始方法。
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 当条件 EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (CreateProjectAssets()) // 当条件 CreateProjectAssets() 成立时，执行下方分支。
            { // 开始条件分支。
                EditorSceneManager.OpenScene("Assets/GuishaDemo/Scenes/Guisha.unity"); // 调用 EditorSceneManager.OpenScene，提交编辑器资产/场景配置。
                EditorUtility.DisplayDialog("演示场景已生成", "输入模式为 Input Manager (Old) 或 Both 时可直接 Play。打包前构建 Addressables 内容。", "确定"); // 弹出编辑器生成结果提示，向使用者说明后续操作。
            } // 结束条件分支。
        } // 结束方法。

        // 可在已经打开的工程中运行；生成过程保留用户当前打开的场景。
        /// <summary>
        /// 保留当前场景，创建演示资产、场景、实体 Catalog、Addressables 和框架配置，随后配置动画/玩法。
        /// </summary>
        /// <returns>true 表示演示工程资产已成功创建。</returns>
        public static bool CreateProjectAssets() // 保留当前场景，创建演示资产、场景、实体 Catalog、Addressables 和框架配置，随后配置动画/玩法。
        { // 开始方法。
            const string root = "Assets/GuishaDemo"; // 把 "Assets/GuishaDemo" 保存为 演示资产根目录路径。
            if (Directory.Exists(root)) // 当条件 Directory.Exists(root) 成立时，执行下方分支。
            { // 开始条件分支。
                Debug.LogWarning("GuishaDemo already exists; generation skipped."); // 向 Unity Console 输出警告。
                return false; // 返回 false，将当前分支的布尔结果交给调用方。
            } // 结束条件分支。

            var originalScene = SceneManager.GetActiveScene(); // 把 SceneManager.GetActiveScene 的返回值 保存为 originalScene。
            Directory.CreateDirectory(root + "/Scenes"); // 确保目标目录存在。
            Directory.CreateDirectory(root + "/Config"); // 确保目标目录存在。
            Directory.CreateDirectory(root + "/Prefabs"); // 确保目标目录存在。
            AssetDatabase.Refresh(); // 通知 Unity 刷新资源数据库。
            var artifact = ScriptableObject.CreateInstance<GhostArtifactItem>(); // 把 ScriptableObject.CreateInstance<GhostArtifactItem> 的返回值 保存为 鬼器配置资产。
            artifact.id = "ghost_artifact"; // 更新 artifact.id，使用 "ghost_artifact"。
            artifact.displayName = "安神鬼器"; // 更新 artifact.displayName，使用 "安神鬼器"。
            AssetDatabase.CreateAsset(artifact, root + "/Config/Artifact.asset"); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            var dialogue = ScriptableObject.CreateInstance<DialogueAsset>(); // 把 ScriptableObject.CreateInstance<DialogueAsset> 的返回值 保存为 对白资产或当前对白。
            dialogue.nodes = new[] // 更新 dialogue.nodes，使用 new[] {     new DialogueNode     {         speaker = "良言",         text = "宁秋水，血门后面的规则需要你自己找。",         next = 1     },     new DialogueNode     {         speaker = "宁秋水",         text = "先调查哪里？",         choices = new[]         {             new DialogueChoice             {                 text = "先找纸条线索",                 next = 2             },             new DialogueChoice             {                 text = "保存检查点",                 next = 3             }         }     },     new DialogueNode     {         speaker = "良言",         text = "纸条会告诉你机关的条件。",         next = -1     },     new DialogueNode     {         speaker = "良言",         text = "绿色方块可以保存；F9 可以读取。",         next = -1     } }。
            { // 开始成员/集合初始化。
                new DialogueNode // 创建 DialogueNode 并传入该对象需要的依赖/数据。
                { // 开始成员/集合初始化。
                    speaker = "良言", // 更新 说话角色名，使用 "良言"。
                    text = "宁秋水，血门后面的规则需要你自己找。", // 更新 正文或待解析字符串，使用 "宁秋水，血门后面的规则需要你自己找。"。
                    next = 1 // 更新 下一项，使用 1。
                }, // 结束成员/集合初始化。
                new DialogueNode // 创建 DialogueNode 并传入该对象需要的依赖/数据。
                { // 开始成员/集合初始化。
                    speaker = "宁秋水", // 更新 说话角色名，使用 "宁秋水"。
                    text = "先调查哪里？", // 更新 正文或待解析字符串，使用 "先调查哪里？"。
                    choices = new[] // 更新 对白选项数据，使用 new[] {     new DialogueChoice     {         text = "先找纸条线索",         next = 2     },     new DialogueChoice     {         text = "保存检查点",         next = 3     } }。
                    { // 开始成员/集合初始化。
                        new DialogueChoice // 创建 DialogueChoice 并传入该对象需要的依赖/数据。
                        { // 开始成员/集合初始化。
                            text = "先找纸条线索", // 更新 正文或待解析字符串，使用 "先找纸条线索"。
                            next = 2 // 更新 下一项，使用 2。
                        }, // 结束成员/集合初始化。
                        new DialogueChoice // 创建 DialogueChoice 并传入该对象需要的依赖/数据。
                        { // 开始成员/集合初始化。
                            text = "保存检查点", // 更新 正文或待解析字符串，使用 "保存检查点"。
                            next = 3 // 更新 下一项，使用 3。
                        } // 结束成员/集合初始化。
                    } // 结束成员/集合初始化。
                }, // 结束成员/集合初始化。
                new DialogueNode // 创建 DialogueNode 并传入该对象需要的依赖/数据。
                { // 开始成员/集合初始化。
                    speaker = "良言", // 更新 说话角色名，使用 "良言"。
                    text = "纸条会告诉你机关的条件。", // 更新 正文或待解析字符串，使用 "纸条会告诉你机关的条件。"。
                    next = -1 // 更新 下一项，使用 -1。
                }, // 结束成员/集合初始化。
                new DialogueNode // 创建 DialogueNode 并传入该对象需要的依赖/数据。
                { // 开始成员/集合初始化。
                    speaker = "良言", // 更新 说话角色名，使用 "良言"。
                    text = "绿色方块可以保存；F9 可以读取。", // 更新 正文或待解析字符串，使用 "绿色方块可以保存；F9 可以读取。"。
                    next = -1 // 更新 下一项，使用 -1。
                } // 结束成员/集合初始化。
            }; // 结束成员/集合初始化。
            AssetDatabase.CreateAsset(dialogue, root + "/Config/Dialogue.asset"); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            var ghost = ScriptableObject.CreateInstance<EntityDefinition>(); // 把 ScriptableObject.CreateInstance<EntityDefinition> 的返回值 保存为 鬼怪实体或鬼怪配置。
            ghost.id = EntityId.RedGhost; // 更新 ghost.id，使用 EntityId.RedGhost。
            ghost.type = EntityType.Ghost; // 更新 ghost.type，使用 EntityType.Ghost。
            ghost.prefabAddress = "entities/red_ghost"; // 更新 ghost.prefabAddress，使用 "entities/red_ghost"。
            ghost.moveSpeed = 2.5f; // 更新 ghost.moveSpeed，使用 2.5f。
            ghost.attackDamage = 15; // 更新 ghost.attackDamage，使用 15。
            AssetDatabase.CreateAsset(ghost, root + "/Config/RedGhost.asset"); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            var catalog = ScriptableObject.CreateInstance<EntityCatalog>(); // 把 ScriptableObject.CreateInstance<EntityCatalog> 的返回值 保存为 可用配置目录。
            catalog.definitions = new[] // 更新 catalog.definitions，使用 new[] {     ghost }。
            { // 开始成员/集合初始化。
                ghost // 更新 catalog.definitions，使用 new[] {     ghost }。
            }; // 结束成员/集合初始化。
            AssetDatabase.CreateAsset(catalog, root + "/Config/EntityCatalog.asset"); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            var prefabScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); // 把 EditorSceneManager.NewScene 的返回值 保存为 prefabScene。
            SceneManager.SetActiveScene(prefabScene); // 切换 Unity 当前活动场景。
            var source = new GameObject("RedGhost"); // 创建 GameObject，保存为 待保存的临时预制体根对象。
            var collision = source.AddComponent<CapsuleCollider>(); // 把 source.AddComponent<CapsuleCollider> 的返回值 保存为 collision。
            collision.height = 1.8f; // 更新 collision.height，使用 1.8f。
            collision.radius = 0.3f; // 更新 collision.radius，使用 0.3f。
            collision.center = Vector3.up * 0.9f; // 更新 collision.center，使用 Vector3.up * 0.9f。
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); // 把 GameObject.CreatePrimitive 的返回值 保存为 表现对象。
            visual.transform.SetParent(source.transform); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            visual.transform.localPosition = Vector3.up * 0.9f; // 更新 visual.transform.localPosition，使用 Vector3.up * 0.9f。
            visual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f); // 更新 visual.transform.localScale，使用 新建的 Vector3 对象。
            Object.DestroyImmediate(visual.GetComponent<Collider>()); // 在编辑器中立即销毁不再需要的临时对象或组件。
            source.AddComponent<GhostEntity>(); // 给 待保存的临时预制体根对象 添加 GhostEntity 组件。
            source.AddComponent<AIPerception>(); // 给 待保存的临时预制体根对象 添加 AIPerception 组件。
            source.AddComponent<NavMeshAgent>(); // 给 待保存的临时预制体根对象 添加 NavMeshAgent 组件。
            source.AddComponent<GhostBrain>(); // 给 待保存的临时预制体根对象 添加 GhostBrain 组件。
            PrefabUtility.SaveAsPrefabAsset(source, root + "/Prefabs/RedGhost.prefab"); // 把当前对象层级保存为指定路径的预制体资产。
            Object.DestroyImmediate(source); // 在编辑器中立即销毁不再需要的临时对象或组件。
            EditorSceneManager.CloseScene(prefabScene, true); // 关闭临时创建/编辑的场景，完成编辑器生成清理。
            if (originalScene.IsValid()) // 当条件 originalScene.IsValid() 成立时，执行下方分支。
                SceneManager.SetActiveScene(originalScene); // 切换 Unity 当前活动场景。
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true); // 把 AddressableAssetSettingsDefaultObject.GetSettings 的返回值 保存为 框架或动画配置。
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(root + "/Prefabs/RedGhost.prefab"), settings.DefaultGroup); // 把 settings.CreateOrMoveEntry 的返回值 保存为 共享资源缓存项。
            entry.address = ghost.prefabAddress; // 更新 entry.address，使用 ghost.prefabAddress。
            EditorUtility.SetDirty(settings); // 标记资产已修改，等待保存。
            var existingScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); // 创建 System.Collections.Generic.List<EditorBuildSettingsScene>，保存为 existingScenes。
            var names = new[] // 把 new[] {     "Guisha",     "BloodVilla",     "RainVillage" } 保存为 names。
            { // 开始成员/集合初始化。
                "Guisha", // 提供初始化/参数元素 "Guisha"，交给上方的对象或集合使用。
                "BloodVilla", // 提供初始化/参数元素 "BloodVilla"，交给上方的对象或集合使用。
                "RainVillage" // 提供初始化/参数元素 "RainVillage"，交给上方的对象或集合使用。
            }; // 结束成员/集合初始化。
            for (int i = 0; i < names.Length; i++) // 按初始化与递增步骤循环；继续条件为 i < names.Length。
            { // 开始计数循环。
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); // 把 EditorSceneManager.NewScene 的返回值 保存为 scene。
                SceneManager.SetActiveScene(scene); // 切换 Unity 当前活动场景。
                var builder = new GameObject("DemoScene").AddComponent<DemoScene>(); // 把 new GameObject("DemoScene").AddComponent<DemoScene> 的返回值 保存为 builder。
                builder.artifact = artifact; // 更新 builder.artifact，使用 鬼器配置资产。
                builder.dialogue = dialogue; // 更新 builder.dialogue，使用 对白资产或当前对白。
                builder.ghost = ghost; // 更新 builder.ghost，使用 鬼怪实体或鬼怪配置。
                builder.dungeon = i; // 更新 builder.dungeon，使用 循环索引。
                var path = root + "/Scenes/" + names[i] + ".unity"; // 把 root + "/Scenes/" + names[i] + ".unity" 保存为 文件或资源路径。
                EditorSceneManager.SaveScene(scene, path); // 调用 EditorSceneManager.SaveScene，提交编辑器资产/场景配置。
                existingScenes.Add(new EditorBuildSettingsScene(path, true)); // 向 existingScenes 加入 new EditorBuildSettingsScene(path, true)。
                EditorSceneManager.CloseScene(scene, true); // 关闭临时创建/编辑的场景，完成编辑器生成清理。
            } // 结束计数循环。

            if (originalScene.IsValid()) // 当条件 originalScene.IsValid() 成立时，执行下方分支。
                SceneManager.SetActiveScene(originalScene); // 切换 Unity 当前活动场景。
            const string frameworkSettingsPath = "Assets/Resources/Guisha/FrameworkSettings.asset"; // 把 "Assets/Resources/Guisha/FrameworkSettings.asset" 保存为 frameworkSettingsPath。
            if (!File.Exists(frameworkSettingsPath)) // 当条件 File.Exists(frameworkSettingsPath) 不成立时，执行下方分支。
            { // 开始条件分支。
                Directory.CreateDirectory("Assets/Resources/Guisha"); // 确保目标目录存在。
                AssetDatabase.Refresh(); // 通知 Unity 刷新资源数据库。
                var frameworkSettings = ScriptableObject.CreateInstance<FrameworkSettings>(); // 把 ScriptableObject.CreateInstance<FrameworkSettings> 的返回值 保存为 frameworkSettings。
                frameworkSettings.entityCatalog = catalog; // 更新 frameworkSettings.entityCatalog，使用 可用配置目录。
                frameworkSettings.persistentScenePath = root + "/Scenes/Guisha.unity"; // 更新 frameworkSettings.persistentScenePath，使用 root + "/Scenes/Guisha.unity"。
                AssetDatabase.CreateAsset(frameworkSettings, frameworkSettingsPath); // 调用 AssetDatabase.CreateAsset，提交编辑器资产/场景配置。
            } // 结束条件分支。

            EditorBuildSettings.scenes = existingScenes.ToArray(); // 更新 EditorBuildSettings.scenes，使用 existingScenes.ToArray 的返回值。
            AssetDatabase.SaveAssets(); // 把编辑器资产修改保存到磁盘。
            PatternAnimationSetup.SetupProjectAssets(); // 补建动画与四种玩法配置，更新演示场景和鬼怪预制体，已有配置优先保留。
            return true; // 返回 true，将当前分支的布尔结果交给调用方。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
