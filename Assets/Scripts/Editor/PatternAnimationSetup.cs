using System;
using System.IO;
using Guisha.Framework;
using Guisha.Gameplay;
using Guisha.Samples;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Guisha.Editor
{
    public static class PatternAnimationSetup
    {
        [MenuItem("Guisha/Patterns/Run Examples")]
        public static void Examples() { Debug.Log(PatternExamples.Run()); }
        [MenuItem("Guisha/Setup Modes And Animation Demo")]
        public static void SetupProjectAssets()
        {
            const string root = "Assets/GuishaDemo";
            if (!Directory.Exists(root + "/Scenes")) throw new InvalidOperationException("Create Greybox Demo first.");
            var previous = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(previous.path)) throw new InvalidOperationException("Save the current scene before configuring demo assets.");
            Directory.CreateDirectory(root + "/Animation"); Directory.CreateDirectory(root + "/Config/Modes"); AssetDatabase.Refresh();
            var animation = AssetDatabase.LoadAssetAtPath<AnimationSettings>(root + "/Animation/Settings.asset");
            if (animation == null) { animation = ScriptableObject.CreateInstance<AnimationSettings>(); AssetDatabase.CreateAsset(animation, root + "/Animation/Settings.asset"); }
            var controller = CreateController(root + "/Animation/Entity.controller");
            var modes = new GameModeDefinition[4];
            for (int i = 0; i < modes.Length; i++)
            {
                string path = root + "/Config/Modes/" + (GameMode)i + ".asset";
                modes[i] = AssetDatabase.LoadAssetAtPath<GameModeDefinition>(path);
                if (modes[i] != null) continue;
                var rules = GameModeRules.Default((GameMode)i); var value = ScriptableObject.CreateInstance<GameModeDefinition>();
                value.mode = rules.Mode; value.playerDamage = rules.PlayerDamage; value.sanDrain = rules.SanDrain; value.staminaCost = rules.StaminaCost;
                value.enemySpeed = rules.EnemySpeed; value.hostileGhosts = rules.HostileGhosts; value.allowSave = rules.AllowSave; value.timeLimit = rules.TimeLimit;
                AssetDatabase.CreateAsset(value, path); modes[i] = value;
            }
            var catalog = AssetDatabase.LoadAssetAtPath<GameModeCatalog>(root + "/Config/Modes/Catalog.asset");
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<GameModeCatalog>(); catalog.definitions = modes; AssetDatabase.CreateAsset(catalog, root + "/Config/Modes/Catalog.asset"); }
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings");
            if (settings != null && settings.gameModes == null) { settings.gameModes = catalog; EditorUtility.SetDirty(settings); }
            foreach (string name in new[] { "Guisha", "BloodVilla", "RainVillage" })
            {
                string path = root + "/Scenes/" + name + ".unity"; var scene = SceneManager.GetSceneByPath(path); bool opened = !scene.isLoaded;
                if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                foreach (var go in scene.GetRootGameObjects())
                    foreach (var builder in go.GetComponentsInChildren<DemoScene>(true))
                    { builder.animationController = controller; builder.animationSettings = animation; EditorUtility.SetDirty(builder); }
                EditorSceneManager.SaveScene(scene); if (opened) EditorSceneManager.CloseScene(scene, true);
            }
            string prefabPath = root + "/Prefabs/RedGhost.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var visual = prefab.transform.GetChild(0); var animator = visual.GetComponent<Animator>();
                if (animator == null) animator = visual.gameObject.AddComponent<Animator>(); animator.runtimeAnimatorController = controller;
                var driver = prefab.GetComponent<EntityAnimator>(); if (driver == null) driver = prefab.AddComponent<EntityAnimator>();
                driver.settings = animation; driver.targetAnimator = animator;
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            if (previous.IsValid()) SceneManager.SetActiveScene(previous);
            AssetDatabase.SaveAssets(); Debug.Log("Guisha patterns, mode catalog and animation demo configured.");
        }
        private static AnimatorController CreateController(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path); if (existing != null) return existing;
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            foreach (string name in new[] { "Crouched", "Grounded", "Dead" }) controller.AddParameter(name, AnimatorControllerParameterType.Bool);
            foreach (string name in new[] { "Attack", "Hit", "Interact" }) controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
            var machine = controller.layers[0].stateMachine;
            AnimatorState idle = State(controller, machine, "Idle", 0.01f, true), walk = State(controller, machine, "Walk", 0.05f, true), run = State(controller, machine, "Run", 0.09f, true);
            var crouch = State(controller, machine, "Crouch", 0.01f, true); var dead = State(controller, machine, "Dead", 0, false);
            machine.defaultState = idle;
            Transition(idle, walk, AnimatorConditionMode.Greater, 0.1f, "Speed"); Transition(walk, idle, AnimatorConditionMode.Less, 0.1f, "Speed");
            Transition(walk, run, AnimatorConditionMode.Greater, 4, "Speed"); Transition(run, walk, AnimatorConditionMode.Less, 4, "Speed");
            foreach (var state in new[] { idle, walk, run }) Transition(state, crouch, AnimatorConditionMode.If, 0, "Crouched");
            Transition(crouch, idle, AnimatorConditionMode.IfNot, 0, "Crouched"); Transition(dead, idle, AnimatorConditionMode.IfNot, 0, "Dead");
            var death = machine.AddAnyStateTransition(dead); death.hasExitTime = false; death.canTransitionToSelf = false; death.AddCondition(AnimatorConditionMode.If, 0, "Dead");
            foreach (string name in new[] { "Attack", "Hit", "Interact" })
            {
                var state = State(controller, machine, name, 0.03f, false);
                var enter = machine.AddAnyStateTransition(state); enter.hasExitTime = false; enter.canTransitionToSelf = false;
                enter.AddCondition(AnimatorConditionMode.If, 0, name); enter.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
                var exit = state.AddTransition(idle); exit.hasExitTime = true; exit.exitTime = 0.9f; exit.duration = 0.1f;
            }
            return controller;
        }
        private static AnimatorState State(AnimatorController controller, AnimatorStateMachine machine, string name, float bob, bool loop)
        {
            var clip = new AnimationClip { name = name, frameRate = 30 };
            clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", AnimationCurve.EaseInOut(0, name == "Crouch" ? 0.6f : 0.9f, 0.5f, name == "Crouch" ? 0.6f + bob : 0.9f + bob));
            if (name == "Dead") clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", AnimationCurve.EaseInOut(0, 0, 0.5f, 75));
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip); clipSettings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            AssetDatabase.AddObjectToAsset(clip, controller); var state = machine.AddState(name); state.motion = clip; return state;
        }
        private static void Transition(AnimatorState source, AnimatorState destination, AnimatorConditionMode mode, float threshold, string parameter)
        { var transition = source.AddTransition(destination); transition.hasExitTime = false; transition.duration = 0.12f; transition.AddCondition(mode, threshold, parameter); }
    }
}
