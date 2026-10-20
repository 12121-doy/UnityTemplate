using System.Collections.Generic;
using Guisha.Core;
using Guisha.Framework;
using Guisha.Gameplay;
using Guisha.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Guisha.Samples
{
    // Generated scene has this one builder; all greybox objects are scene-owned.
    public sealed class DemoScene : MonoBehaviour
    {
        public GhostArtifactItem artifact;
        public DialogueAsset dialogue;
        public EntityDefinition ghost;
        public int dungeon;
        public Font uiFont;
        public RuntimeAnimatorController animationController;
        public AnimationSettings animationSettings;
        private NavMeshDataInstance navigation;
        private readonly List<Material> materials = new List<Material>();
        private Transform content;
        private Font font;
        private bool entered;
        private void Start()
        {
            font = uiFont != null ? uiFont : Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC", "Arial Unicode MS", "Arial" }, 24);
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            content = new GameObject("PlayableContent").transform; content.SetParent(transform);
            content.gameObject.SetActive(false);
            var gate = gameObject.AddComponent<ActiveSceneContent>(); gate.content = content.gameObject;
            var ground = Box("Floor", new Vector3(0, -0.25f, 0), new Vector3(30, 0.5f, 30), new Color(0.16f, 0.18f, 0.2f));
            var walls = new List<GameObject> { ground };
            walls.Add(Box("WallNorth", new Vector3(0, 2, 15), new Vector3(30, 4, 0.5f), Color.gray));
            walls.Add(Box("WallSouth", new Vector3(0, 2, -15), new Vector3(30, 4, 0.5f), Color.gray));
            walls.Add(Box("WallEast", new Vector3(15, 2, 0), new Vector3(0.5f, 4, 30), Color.gray));
            walls.Add(Box("WallWest", new Vector3(-15, 2, 0), new Vector3(0.5f, 4, 30), Color.gray));
            BuildNavigation(walls);
            var lightObject = new GameObject("Light"); lightObject.transform.SetParent(content);
            var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 0.8f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            var player = new GameObject("宁秋水_Player"); player.transform.SetParent(content); player.transform.position = new Vector3(0, 0.1f, -8);
            var capsule = player.AddComponent<CharacterController>(); capsule.height = 1.8f; capsule.radius = 0.3f; capsule.center = Vector3.up * 0.9f;
            player.AddComponent<PlayerEntity>(); player.AddComponent<PlayerStatus>();
            var controller = player.AddComponent<PlayerController>();
            player.AddComponent<SanSystem>(); var inventory = player.AddComponent<Inventory>(); inventory.catalog = new Item[] { artifact };
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); visual.name = "PlayerVisual";
            visual.transform.SetParent(player.transform); visual.transform.localPosition = Vector3.up * 0.9f; visual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            Destroy(visual.GetComponent<Collider>());
            if (animationController != null) visual.AddComponent<Animator>().runtimeAnimatorController = animationController;
            player.AddComponent<EntityAnimator>().settings = animationSettings;
            var cameraObject = new GameObject("Main Camera"); cameraObject.transform.SetParent(content); cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>(); camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>(); var follow = cameraObject.AddComponent<PlayerCamera>(); follow.target = player.transform;
            controller.cameraTransform = camera.transform; player.AddComponent<PlayerInteractor>().view = camera;
            BuildUI();
            var survivalObject = new GameObject("SurvivalDirector"); survivalObject.transform.SetParent(content);
            var survival = survivalObject.AddComponent<SurvivalDirector>(); survival.ghost = ghost; survival.dungeon = dungeon;
            content.gameObject.AddComponent<DialogueSystem>(); content.gameObject.AddComponent<DamageSystem>();
            var checkpoint = Box("Checkpoint_F5", new Vector3(-2, 0.7f, -5), Vector3.one, Color.green).AddComponent<Checkpoint>();
            checkpoint.checkpointId = "demo." + dungeon + ".checkpoint";
            var note = Box("Clue", new Vector3(0, 0.7f, -3), new Vector3(0.8f, 0.2f, 0.8f), Color.white).AddComponent<ClueNote>();
            note.clueId = "demo." + dungeon + ".clue"; note.text = "纸条：先按下红色机关，再推开门。别在鬼怪附近奔跑。";
            var pickup = Box("Artifact", new Vector3(2, 0.7f, -4), Vector3.one * 0.6f, Color.cyan).AddComponent<PickupItem>();
            pickup.itemId = artifact.id; pickup.persistentId = "demo." + dungeon + ".artifact"; pickup.Refresh();
            var mechanism = Box("Mechanism", new Vector3(3, 0.8f, 0), Vector3.one, Color.red).AddComponent<Mechanism>();
            mechanism.mechanismId = "demo." + dungeon + ".mechanism"; mechanism.requiredClueId = note.clueId;
            var doorObject = Box("Door", new Vector3(0, 1.5f, 2), new Vector3(2, 3, 0.3f), new Color(0.4f, 0.1f, 0.1f));
            var door = doorObject.AddComponent<Door>(); door.locked = true; door.unlockMechanismId = mechanism.mechanismId; door.persistentId = "demo." + dungeon + ".door"; door.Refresh();
            var npc = Box("良言_NPC", new Vector3(-4, 1, -1), new Vector3(0.8f, 2, 0.8f), Color.yellow);
            npc.SetActive(false); var human = npc.AddComponent<HumanNPC>(); human.SetDialogue(dialogue);
            var triggerObject = new GameObject("DialogueVolume"); triggerObject.transform.SetParent(npc.transform, false);
            var trigger = triggerObject.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(3, 2, 3);
            var story = triggerObject.AddComponent<StoryTrigger>(); story.dialogue = dialogue;
            var rigidbody = npc.AddComponent<Rigidbody>(); rigidbody.isKinematic = true; rigidbody.useGravity = false; npc.SetActive(true);
            AddPortal(new Vector3(8, 1.5f, -4), dungeon == 0 ? 1 : 0);
            AddPortal(new Vector3(-8, 1.5f, -4), dungeon == 2 ? 0 : 2);
            gate.Refresh();
        }
        private void Update()
        {
            if (entered || content == null || !content.gameObject.activeInHierarchy) return;
            entered = true;
            EventBus.Publish(new DungeonEntered(dungeon));
            if (dungeon > 0 && ghost != null) EventBus.Publish(new SpawnEntityRequested(ghost, new Vector3(0, 0.1f, 9), Quaternion.Euler(0, 180, 0), content));
            EventBus.Publish(new Notification("WASD 移动 | 鼠标视角 | Shift 跑 | Ctrl 蹲 | E 交互 | Tab 背包 | 1 鬼器 | Esc 暂停 | F5/F9 存读档"));
        }
        private void AddPortal(Vector3 position, int destination)
        {
            var portal = Box("BloodDoor_" + destination, position, new Vector3(1.5f, 3, 0.4f), new Color(0.5f, 0, 0.1f)).AddComponent<ScenePortal>();
            portal.scenePath = "Assets/GuishaDemo/Scenes/" + (destination == 0 ? "Guisha" : destination == 1 ? "BloodVilla" : "RainVillage") + ".unity";
            portal.dungeonId = destination;
        }
        private GameObject Box(string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(content);
            go.transform.position = position; go.transform.localScale = scale;
            var shader = Shader.Find("Universal Render Pipeline/Lit"); if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader); material.color = color; materials.Add(material); go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        private void BuildNavigation(List<GameObject> objects)
        {
            var sources = new List<NavMeshBuildSource>();
            foreach (var obj in objects)
                sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box, transform = obj.transform.localToWorldMatrix, size = Vector3.one });
            var settings = NavMesh.GetSettingsByID(0);
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(Vector3.zero, new Vector3(40, 10, 40)), Vector3.zero, Quaternion.identity);
            if (data != null) navigation = NavMesh.AddNavMeshData(data);
        }
        private GameObject UIObject(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position; return go;
        }
        private Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size)
        {
            var text = UIObject(name, parent, size, position).AddComponent<Text>(); text.font = font;
            text.text = value; text.fontSize = 24; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false; return text;
        }
        private T Panel<T>(string id, Transform parent) where T : UIPanelBase
        {
            var go = UIObject(id, parent, new Vector2(850, 420), Vector2.zero); go.SetActive(false);
            var image = go.AddComponent<Image>(); image.color = new Color(0.02f, 0.025f, 0.04f, 0.95f);
            go.AddComponent<CanvasGroup>(); var panel = go.AddComponent<T>(); panel.panelId = id; return panel;
        }
        private Button Button(string name, Transform parent, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = UIObject(name, parent, new Vector2(220, 44), position);
            go.AddComponent<Image>().color = new Color(0.22f, 0.25f, 0.3f);
            var button = go.AddComponent<Button>(); button.onClick.AddListener(action);
            Label("Label", go.transform, name, Vector2.zero, new Vector2(220, 44)); return button;
        }
        private void BuildUI()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform)); canvasObject.transform.SetParent(content);
            var canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900);
            canvasObject.AddComponent<GraphicRaycaster>();
            var events = new GameObject("EventSystem"); events.transform.SetParent(content); events.AddComponent<EventSystem>(); events.AddComponent<StandaloneInputModule>();
            var hud = canvasObject.AddComponent<HUD>();
            hud.modeText = Label("Mode", canvasObject.transform, "", new Vector2(0, 340), new Vector2(1000, 45));
            hud.statsText = Label("Stats", canvasObject.transform, "SAN 100 | 体力 100", new Vector2(0, 390), new Vector2(1000, 60));
            hud.promptText = Label("Prompt", canvasObject.transform, "", new Vector2(0, -180), new Vector2(1000, 60));
            hud.notificationText = Label("Notice", canvasObject.transform, "", new Vector2(0, -310), new Vector2(1450, 150));
            hud.quickSlotsText = Label("QuickSlots", canvasObject.transform, "快捷栏 [1]", new Vector2(0, -420), new Vector2(1200, 50));
            Label("Crosshair", canvasObject.transform, "+", Vector2.zero, new Vector2(30, 30));
            var menu = Panel<MenuPanel>("Pause", canvasObject.transform);
            Label("Title", menu.transform, "诡舍 · 暂停", new Vector2(0, 150), new Vector2(800, 60));
            Button("继续", menu.transform, new Vector2(0, 50), menu.Resume);
            Button("保存", menu.transform, new Vector2(0, -10), menu.Save);
            Button("读取检查点", menu.transform, new Vector2(0, -70), menu.Load);
            var volume = UIObject("Volume", menu.transform, new Vector2(220, 20), new Vector2(0, -145)).AddComponent<Slider>();
            var track = volume.gameObject.AddComponent<Image>(); track.color = Color.gray;
            var fill = UIObject("Fill", volume.transform, new Vector2(220, 20), Vector2.zero); fill.AddComponent<Image>().color = Color.cyan;
            volume.fillRect = fill.GetComponent<RectTransform>(); volume.value = AudioListener.volume; volume.onValueChanged.AddListener(menu.SetMasterVolume);
            menu.gameObject.SetActive(true);
            var inventory = Panel<InventoryUI>("Inventory", canvasObject.transform);
            inventory.itemsText = Label("Items", inventory.transform, "背包", new Vector2(0, 50), new Vector2(800, 280));
            Button("使用第一个鬼器", inventory.transform, new Vector2(0, -150), () => inventory.Use(0)); inventory.gameObject.SetActive(true);
            var dialoguePanel = Panel<DialoguePanel>("Dialogue", canvasObject.transform);
            dialoguePanel.speaker = Label("Speaker", dialoguePanel.transform, "", new Vector2(0, 150), new Vector2(800, 60));
            dialoguePanel.body = Label("Body", dialoguePanel.transform, "", new Vector2(0, 40), new Vector2(800, 140));
            dialoguePanel.choiceText = Label("Choices", dialoguePanel.transform, "", new Vector2(0, -110), new Vector2(800, 130)); dialoguePanel.gameObject.SetActive(true);
            var modes = Panel<GameModePanel>("Modes", canvasObject.transform);
            Label("Title", modes.transform, "选择玩法模式", new Vector2(0, 165), new Vector2(800, 45));
            Button("剧情 · 轻度压力", modes.transform, new Vector2(0, 100), () => modes.Select(0));
            Button("探索 · 安全调查", modes.transform, new Vector2(0, 40), () => modes.Select(1));
            Button("挑战 · 限时解谜", modes.transform, new Vector2(0, -20), () => modes.Select(2));
            Button("生存 · 持续增援", modes.transform, new Vector2(0, -80), () => modes.Select(3));
            Button("返回", modes.transform, new Vector2(0, -150), modes.Back); modes.gameObject.SetActive(true);
            Button("玩法模式", menu.transform, new Vector2(260, 50), () => EventBus.Publish(new PanelRequested("Modes", true)));
            canvasObject.AddComponent<UIInputRouter>().modes = modes;
        }
        private void OnDestroy()
        { if (navigation.valid) navigation.Remove(); if (uiFont == null && font != null) Destroy(font); foreach (var material in materials) if (material != null) Destroy(material); }
    }
}
