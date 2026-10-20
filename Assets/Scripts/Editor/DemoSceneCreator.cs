using System.IO;
using Guisha.Framework;
using Guisha.Gameplay;
using Guisha.Samples;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
namespace Guisha.Editor
{
    public static class DemoSceneCreator
    {
        [MenuItem("Guisha/Create Greybox Demo")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (CreateProjectAssets())
            {
                EditorSceneManager.OpenScene("Assets/GuishaDemo/Scenes/Guisha.unity");
                EditorUtility.DisplayDialog("演示场景已生成", "输入模式为 Input Manager (Old) 或 Both 时可直接 Play。打包前构建 Addressables 内容。", "确定");
            }
        }
        // Can run in an already-open project without closing or saving the user's scenes.
        public static bool CreateProjectAssets()
        {
            const string root = "Assets/GuishaDemo";
            if (Directory.Exists(root))
            { Debug.LogWarning("GuishaDemo already exists; generation skipped."); return false; }
            var originalScene = SceneManager.GetActiveScene();
            Directory.CreateDirectory(root + "/Scenes"); Directory.CreateDirectory(root + "/Config"); Directory.CreateDirectory(root + "/Prefabs"); AssetDatabase.Refresh();
            var artifact = ScriptableObject.CreateInstance<GhostArtifactItem>(); artifact.id = "ghost_artifact"; artifact.displayName = "安神鬼器";
            AssetDatabase.CreateAsset(artifact, root + "/Config/Artifact.asset");
            var dialogue = ScriptableObject.CreateInstance<DialogueAsset>();
            dialogue.nodes = new[]
            {
                new DialogueNode { speaker = "良言", text = "宁秋水，血门后面的规则需要你自己找。", next = 1 },
                new DialogueNode { speaker = "宁秋水", text = "先调查哪里？", choices = new[]
                    { new DialogueChoice { text = "先找纸条线索", next = 2 }, new DialogueChoice { text = "保存检查点", next = 3 } } },
                new DialogueNode { speaker = "良言", text = "纸条会告诉你机关的条件。", next = -1 },
                new DialogueNode { speaker = "良言", text = "绿色方块可以保存；F9 可以读取。", next = -1 }
            };
            AssetDatabase.CreateAsset(dialogue, root + "/Config/Dialogue.asset");
            var ghost = ScriptableObject.CreateInstance<EntityDefinition>(); ghost.id = EntityId.RedGhost; ghost.type = EntityType.Ghost;
            ghost.prefabAddress = "entities/red_ghost"; ghost.moveSpeed = 2.5f; ghost.attackDamage = 15;
            AssetDatabase.CreateAsset(ghost, root + "/Config/RedGhost.asset");
            var catalog = ScriptableObject.CreateInstance<EntityCatalog>(); catalog.definitions = new[] { ghost };
            AssetDatabase.CreateAsset(catalog, root + "/Config/EntityCatalog.asset");
            var prefabScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(prefabScene);
            var source = new GameObject("RedGhost");
            var collision = source.AddComponent<CapsuleCollider>(); collision.height = 1.8f; collision.radius = 0.3f; collision.center = Vector3.up * 0.9f;
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); visual.transform.SetParent(source.transform);
            visual.transform.localPosition = Vector3.up * 0.9f; visual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f); Object.DestroyImmediate(visual.GetComponent<Collider>());
            source.AddComponent<GhostEntity>(); source.AddComponent<AIPerception>(); source.AddComponent<NavMeshAgent>(); source.AddComponent<GhostBrain>();
            PrefabUtility.SaveAsPrefabAsset(source, root + "/Prefabs/RedGhost.prefab"); Object.DestroyImmediate(source);
            EditorSceneManager.CloseScene(prefabScene, true);
            if (originalScene.IsValid()) SceneManager.SetActiveScene(originalScene);
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(root + "/Prefabs/RedGhost.prefab"), settings.DefaultGroup);
            entry.address = ghost.prefabAddress; EditorUtility.SetDirty(settings);
            var existingScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var names = new[] { "Guisha", "BloodVilla", "RainVillage" };
            for (int i = 0; i < names.Length; i++)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                var builder = new GameObject("DemoScene").AddComponent<DemoScene>();
                builder.artifact = artifact; builder.dialogue = dialogue; builder.ghost = ghost; builder.dungeon = i;
                var path = root + "/Scenes/" + names[i] + ".unity"; EditorSceneManager.SaveScene(scene, path);
                existingScenes.Add(new EditorBuildSettingsScene(path, true));
                EditorSceneManager.CloseScene(scene, true);
            }
            if (originalScene.IsValid()) SceneManager.SetActiveScene(originalScene);
            const string frameworkSettingsPath = "Assets/Resources/Guisha/FrameworkSettings.asset";
            if (!File.Exists(frameworkSettingsPath))
            {
                Directory.CreateDirectory("Assets/Resources/Guisha"); AssetDatabase.Refresh();
                var frameworkSettings = ScriptableObject.CreateInstance<FrameworkSettings>();
                frameworkSettings.entityCatalog = catalog;
                frameworkSettings.persistentScenePath = root + "/Scenes/Guisha.unity";
                AssetDatabase.CreateAsset(frameworkSettings, frameworkSettingsPath);
            }
            EditorBuildSettings.scenes = existingScenes.ToArray(); AssetDatabase.SaveAssets();
            PatternAnimationSetup.SetupProjectAssets();
            return true;
        }
    }
}
