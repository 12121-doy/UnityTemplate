using Guisha.Rendering; // 使用与实际活动管线一致的材质、灯光和相机适配入口。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using Guisha.Gameplay; // 引入 Guisha.Gameplay 命名空间，使用其中的类型与 API。
using Guisha.UI; // 引入 Guisha.UI 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.AI; // 引入 UnityEngine.AI 命名空间，使用其中的类型与 API。
using UnityEngine.EventSystems; // 引入 UnityEngine.EventSystems 命名空间，使用其中的类型与 API。
using UnityEngine.UI; // 引入 UnityEngine.UI 命名空间，使用其中的类型与 API。

namespace Guisha.Samples // 将本文件类型归入 Guisha.Samples，避免类型命名冲突。
{ // 开始命名空间。
    // 生成场景只放一个构建器，所有灰盒对象都归当前场景拥有。
    /// <summary>
    /// 运行时构建灰盒地图、玩家、UI、交互对象和演示寻路。
    /// </summary>
    public sealed class DemoScene : MonoBehaviour // 运行时构建灰盒地图、玩家、UI、交互对象和演示寻路。
    { // 开始类型。
        /// <summary>
        /// 保存鬼器配置资产。
        /// </summary>
        public GhostArtifactItem artifact; // 声明 鬼器配置资产，后续初始化。
        /// <summary>
        /// 保存对白资产或当前对白。
        /// </summary>
        public DialogueAsset dialogue; // 声明 对白资产或当前对白，后续初始化。
        /// <summary>
        /// 保存鬼怪实体或鬼怪配置。
        /// </summary>
        public EntityDefinition ghost; // 声明 鬼怪实体或鬼怪配置，后续初始化。
        /// <summary>
        /// 保存当前副本编号。
        /// </summary>
        public int dungeon; // 声明 当前副本编号，后续初始化。
        /// <summary>
        /// 保存可选演示 UI 字体。
        /// </summary>
        public Font uiFont; // 声明 可选演示 UI 字体，后续初始化。
        /// <summary>
        /// 保存演示动画控制器。
        /// </summary>
        public RuntimeAnimatorController animationController; // 声明 演示动画控制器，后续初始化。
        /// <summary>
        /// 保存动画名称配置。
        /// </summary>
        public AnimationSettings animationSettings; // 声明 动画名称配置，后续初始化。
        /// <summary>
        /// 保存本演示创建的 NavMeshData。
        /// </summary>
        private NavMeshDataInstance navigation; // 声明 本演示创建的 NavMeshData，后续初始化。
        /// <summary>
        /// 保存本演示创建且需要销毁的材质。
        /// </summary>
        private readonly List<Material> materials = new List<Material>(); // 创建 List<Material>，保存为 本演示创建且需要销毁的材质。
        /// <summary>
        /// 保存按活动场景门控的内容子节点。
        /// </summary>
        private Transform content; // 声明 按活动场景门控的内容子节点，后续初始化。
        /// <summary>
        /// 保存界面字体。
        /// </summary>
        private Font font; // 声明 界面字体，后续初始化。
        /// <summary>
        /// 保存entered。
        /// </summary>
        private bool entered; // 声明 entered，后续初始化。
        /// <summary>
        /// 构建地图、寻路、玩家、相机、动画、交互、对白与 UI，再刷新活动场景门控。
        /// </summary>
        private void Start() // 构建地图、寻路、玩家、相机、动画、交互、对白与 UI，再刷新活动场景门控。
        { // 开始方法。
            font = uiFont != null ? uiFont : Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC", "Arial Unicode MS", "Arial" }, 24); // 更新 界面字体，使用 按条件 uiFont != null 选择的值。
            if (font == null) // 当界面字体为空时，执行下方分支。
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // 更新 界面字体，使用 Resources.GetBuiltinResource<Font> 的返回值。
            content = new GameObject("PlayableContent").transform; // 更新 按活动场景门控的内容子节点，使用 new GameObject("PlayableContent").transform。
            content.SetParent(transform); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            content.gameObject.SetActive(false); // 将 content.gameObject 的激活状态设为 false，同时影响其组件生命周期。
            var gate = gameObject.AddComponent<ActiveSceneContent>(); // 把 gameObject.AddComponent<ActiveSceneContent> 的返回值 保存为 gate。
            gate.content = content.gameObject; // 更新 gate.content，使用 content.gameObject。
            var ground = Box("Floor", new Vector3(0, -0.25f, 0), new Vector3(30, 0.5f, 30), new Color(0.16f, 0.18f, 0.2f)); // 把 Box 的返回值 保存为 ground。
            var walls = new List<GameObject> // 创建 List<GameObject>，保存为 walls。
            { // 开始成员/集合初始化。
                ground // 提供初始化/参数元素 ground，交给上方的对象或集合使用。
            }; // 结束成员/集合初始化。
            walls.Add(Box("WallNorth", new Vector3(0, 2, 15), new Vector3(30, 4, 0.5f), Color.gray)); // 向 walls 加入 Box("WallNorth", new Vector3(0, 2, 15), new Vector3(30, 4, 0.5f), Color.gray)。
            walls.Add(Box("WallSouth", new Vector3(0, 2, -15), new Vector3(30, 4, 0.5f), Color.gray)); // 向 walls 加入 Box("WallSouth", new Vector3(0, 2, -15), new Vector3(30, 4, 0.5f), Color.gray)。
            walls.Add(Box("WallEast", new Vector3(15, 2, 0), new Vector3(0.5f, 4, 30), Color.gray)); // 向 walls 加入 Box("WallEast", new Vector3(15, 2, 0), new Vector3(0.5f, 4, 30), Color.gray)。
            walls.Add(Box("WallWest", new Vector3(-15, 2, 0), new Vector3(0.5f, 4, 30), Color.gray)); // 向 walls 加入 Box("WallWest", new Vector3(-15, 2, 0), new Vector3(0.5f, 4, 30), Color.gray)。
            BuildNavigation(walls); // 以地板和墙体盒源生成并注册运行时 NavMeshData。
            var lightObject = new GameObject("Light"); // 创建 GameObject，保存为 lightObject。
            lightObject.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            var light = lightObject.AddComponent<Light>(); // 把 lightObject.AddComponent<Light> 的返回值 保存为 light。
            light.type = LightType.Directional; // 更新 light.type，使用 LightType.Directional。
            light.intensity = 0.8f; // 更新 light.intensity，使用 0.8f。
            light.transform.rotation = Quaternion.Euler(50, -30, 0); // 更新 light.transform.rotation，使用 Quaternion.Euler 的返回值。
            var player = new GameObject("宁秋水_Player"); // 创建 GameObject，保存为 玩家组件或玩家对象。
            player.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            player.transform.position = new Vector3(0, 0.1f, -8); // 更新 player.transform.position，使用 新建的 Vector3 对象。
            var capsule = player.AddComponent<CharacterController>(); // 把 player.AddComponent<CharacterController> 的返回值 保存为 capsule。
            capsule.height = 1.8f; // 更新 capsule.height，使用 1.8f。
            capsule.radius = 0.3f; // 更新 capsule.radius，使用 0.3f。
            capsule.center = Vector3.up * 0.9f; // 更新 capsule.center，使用 Vector3.up * 0.9f。
            player.AddComponent<PlayerEntity>(); // 给 玩家组件或玩家对象 添加 PlayerEntity 组件。
            player.AddComponent<PlayerStatus>(); // 给 玩家组件或玩家对象 添加 PlayerStatus 组件。
            var controller = player.AddComponent<PlayerController>(); // 把 player.AddComponent<PlayerController> 的返回值 保存为 控制器组件。
            player.AddComponent<SanSystem>(); // 给 玩家组件或玩家对象 添加 SanSystem 组件。
            var inventory = player.AddComponent<Inventory>(); // 把 player.AddComponent<Inventory> 的返回值 保存为 背包组件。
            inventory.catalog = new Item[] // 更新 inventory.catalog，使用 new Item[] {     artifact }。
            { // 开始成员/集合初始化。
                artifact // 更新 inventory.catalog，使用 new Item[] {     artifact }。
            }; // 结束成员/集合初始化。
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); // 把 GameObject.CreatePrimitive 的返回值 保存为 表现对象。
            visual.name = "PlayerVisual"; // 更新 visual.name，使用 "PlayerVisual"。
            visual.transform.SetParent(player.transform); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            visual.transform.localPosition = Vector3.up * 0.9f; // 更新 visual.transform.localPosition，使用 Vector3.up * 0.9f。
            visual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f); // 更新 visual.transform.localScale，使用 新建的 Vector3 对象。
            Destroy(visual.GetComponent<Collider>()); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
            if (animationController != null) // 当演示动画控制器不为空时，执行下方分支。
                visual.AddComponent<Animator>().runtimeAnimatorController = animationController; // 更新 visual.AddComponent<Animator>().runtimeAnimatorController，使用 演示动画控制器。
            player.AddComponent<EntityAnimator>().settings = animationSettings; // 更新 player.AddComponent<EntityAnimator>().settings，使用 动画名称配置。
            var cameraObject = new GameObject("Main Camera"); // 创建 GameObject，保存为 cameraObject。
            cameraObject.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            cameraObject.tag = "MainCamera"; // 更新 cameraObject.tag，使用 "MainCamera"。
            var camera = cameraObject.AddComponent<Camera>(); // 把 cameraObject.AddComponent<Camera> 的返回值 保存为 camera。
            camera.nearClipPlane = 0.05f; // 更新 camera.nearClipPlane，使用 0.05f。
            cameraObject.AddComponent<AudioListener>(); // 给 cameraObject 添加 AudioListener 组件。
            var follow = cameraObject.AddComponent<PlayerCamera>(); // 把 cameraObject.AddComponent<PlayerCamera> 的返回值 保存为 follow。
            follow.target = player.transform; // 更新 follow.target，使用 player.transform。
            controller.cameraTransform = camera.transform; // 更新 controller.cameraTransform，使用 camera.transform。
            player.AddComponent<PlayerInteractor>().view = camera; // 更新 player.AddComponent<PlayerInteractor>().view，使用 camera。
            BuildUI(); // 构建 HUD、暂停、音量、背包、对白、玩法选择面板和输入路由。
            var survivalObject = new GameObject("SurvivalDirector"); // 创建 GameObject，保存为 survivalObject。
            survivalObject.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            var survival = survivalObject.AddComponent<SurvivalDirector>(); // 把 survivalObject.AddComponent<SurvivalDirector> 的返回值 保存为 survival。
            survival.ghost = ghost; // 更新 survival.ghost，使用 鬼怪实体或鬼怪配置。
            survival.dungeon = dungeon; // 更新 survival.dungeon，使用 当前副本编号。
            content.gameObject.AddComponent<DialogueSystem>(); // 给 content.gameObject 添加 DialogueSystem 组件。
            content.gameObject.AddComponent<DamageSystem>(); // 给 content.gameObject 添加 DamageSystem 组件。
            var checkpoint = Box("Checkpoint_F5", new Vector3(-2, 0.7f, -5), Vector3.one, Color.green).AddComponent<Checkpoint>(); // 把 Box("Checkpoint_F5", new Vector3(-2, 0.7f, -5), Vector3.one, Color.green).AddComponent<Checkpoint> 的返回值 保存为 检查点标识。
            checkpoint.checkpointId = "demo." + dungeon + ".checkpoint"; // 更新 checkpoint.checkpointId，使用 "demo." + dungeon + ".checkpoint"。
            var note = Box("Clue", new Vector3(0, 0.7f, -3), new Vector3(0.8f, 0.2f, 0.8f), Color.white).AddComponent<ClueNote>(); // 把 Box("Clue", new Vector3(0, 0.7f, -3), new Vector3(0.8f, 0.2f, 0.8f), Color.white).AddComponent<ClueNote> 的返回值 保存为 note。
            note.clueId = "demo." + dungeon + ".clue"; // 更新 note.clueId，使用 "demo." + dungeon + ".clue"。
            note.text = "纸条：先按下红色机关，再推开门。别在鬼怪附近奔跑。"; // 更新 note.text，使用 "纸条：先按下红色机关，再推开门。别在鬼怪附近奔跑。"。
            var pickup = Box("Artifact", new Vector3(2, 0.7f, -4), Vector3.one * 0.6f, Color.cyan).AddComponent<PickupItem>(); // 把 Box("Artifact", new Vector3(2, 0.7f, -4), Vector3.one * 0.6f, Color.cyan).AddComponent<PickupItem> 的返回值 保存为 pickup。
            pickup.itemId = artifact.id; // 更新 pickup.itemId，使用 artifact.id。
            pickup.persistentId = "demo." + dungeon + ".artifact"; // 更新 pickup.persistentId，使用 "demo." + dungeon + ".artifact"。
            pickup.Refresh(); // 根据世界标记更新 collected，并显隐渲染与碰撞。
            var mechanism = Box("Mechanism", new Vector3(3, 0.8f, 0), Vector3.one, Color.red).AddComponent<Mechanism>(); // 把 Box("Mechanism", new Vector3(3, 0.8f, 0), Vector3.one, Color.red).AddComponent<Mechanism> 的返回值 保存为 mechanism。
            mechanism.mechanismId = "demo." + dungeon + ".mechanism"; // 更新 mechanism.mechanismId，使用 "demo." + dungeon + ".mechanism"。
            mechanism.requiredClueId = note.clueId; // 更新 mechanism.requiredClueId，使用 note.clueId。
            var doorObject = Box("Door", new Vector3(0, 1.5f, 2), new Vector3(2, 3, 0.3f), new Color(0.4f, 0.1f, 0.1f)); // 把 Box 的返回值 保存为 doorObject。
            var door = doorObject.AddComponent<Door>(); // 把 doorObject.AddComponent<Door> 的返回值 保存为 door。
            door.locked = true; // 更新 door.locked，使用 true（启用/满足）。
            door.unlockMechanismId = mechanism.mechanismId; // 更新 door.unlockMechanismId，使用 mechanism.mechanismId。
            door.persistentId = "demo." + dungeon + ".door"; // 更新 door.persistentId，使用 "demo." + dungeon + ".door"。
            door.Refresh(); // 按永久解锁标记和初始条件刷新 locked。
            var npc = Box("良言_NPC", new Vector3(-4, 1, -1), new Vector3(0.8f, 2, 0.8f), Color.yellow); // 把 Box 的返回值 保存为 npc。
            npc.SetActive(false); // 将 npc 的激活状态设为 false，同时影响其组件生命周期。
            var human = npc.AddComponent<HumanNPC>(); // 把 npc.AddComponent<HumanNPC> 的返回值 保存为 human。
            human.SetDialogue(dialogue); // 绑定对白资产。
            var triggerObject = new GameObject("DialogueVolume"); // 创建 GameObject，保存为 triggerObject。
            triggerObject.transform.SetParent(npc.transform, false); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            var trigger = triggerObject.AddComponent<BoxCollider>(); // 把 triggerObject.AddComponent<BoxCollider> 的返回值 保存为 trigger。
            trigger.isTrigger = true; // 更新 trigger.isTrigger，使用 true（启用/满足）。
            trigger.size = new Vector3(3, 2, 3); // 更新 trigger.size，使用 新建的 Vector3 对象。
            var story = triggerObject.AddComponent<StoryTrigger>(); // 把 triggerObject.AddComponent<StoryTrigger> 的返回值 保存为 story。
            story.dialogue = dialogue; // 更新 story.dialogue，使用 对白资产或当前对白。
            var rigidbody = npc.AddComponent<Rigidbody>(); // 把 npc.AddComponent<Rigidbody> 的返回值 保存为 rigidbody。
            rigidbody.isKinematic = true; // 更新 rigidbody.isKinematic，使用 true（启用/满足）。
            rigidbody.useGravity = false; // 更新 rigidbody.useGravity，使用 false（关闭/不满足）。
            npc.SetActive(true); // 将 npc 的激活状态设为 true，同时影响其组件生命周期。
            AddPortal(new Vector3(8, 1.5f, -4), dungeon == 0 ? 1 : 0); // 构建一个配置到指定演示场景的血门对象。
            AddPortal(new Vector3(-8, 1.5f, -4), dungeon == 2 ? 0 : 2); // 构建一个配置到指定演示场景的血门对象。
            var rendering = content.gameObject.AddComponent<PipelineSceneSetup>(); // 模板内容自行持有光照和曝光配置，附加场景切换时随内容停用。
            rendering.templateCamera = camera; // 绑定模板运行时创建的主相机。
            rendering.templateSun = light; // 绑定模板自己的方向光，避免改写美术场景灯光。
            RenderPipelineService.PrepareHierarchy(content.gameObject); // 同步准备玩家外观、NPC、交互物和相机。
            gate.Refresh(); // 按当前活动场景立即重算全部已注册内容根。
        } // 结束方法。

        /// <summary>
        /// 当前场景内容首次激活时广播副本编号、生成演示鬼怪并展示按键提示。
        /// </summary>
        private void Update() // 当前场景内容首次激活时广播副本编号、生成演示鬼怪并展示按键提示。
        { // 开始方法。
            if (entered || content == null || !content.gameObject.activeInHierarchy) // 当（entered为 true；或按活动场景门控的内容子节点为空；或条件 content.gameObject.activeInHierarchy 不成立）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            entered = true; // 更新 entered，使用 true（启用/满足）。
            EventBus.Publish(new DungeonEntered(dungeon)); // 广播 DungeonEntered 事件，更新当前副本编号。
            if (dungeon > 0 && ghost != null) // 当（当前副本编号大于0；且鬼怪实体或鬼怪配置不为空）时，执行下方分支。
                EventBus.Publish(new SpawnEntityRequested(ghost, new Vector3(0, 0.1f, 9), Quaternion.Euler(0, 180, 0), content)); // 广播 SpawnEntityRequested 事件，交给资源管理器异步生成配置实体。
            EventBus.Publish(new Notification("WASD 移动 | 鼠标视角 | Shift 跑 | Ctrl 蹲 | E 交互 | Tab 背包 | 1 鬼器 | Esc 暂停 | F5/F9 存读档")); // 广播 Notification 事件，让 HUD 显示短通知。
        } // 结束方法。

        /// <summary>
        /// 构建一个配置到指定演示场景的血门对象。
        /// </summary>
        /// <param name="position">世界位置。</param>
        /// <param name="destination">目标位置。</param>
        private void AddPortal(Vector3 position, int destination) // 构建一个配置到指定演示场景的血门对象。
        { // 开始方法。
            var portal = Box("BloodDoor_" + destination, position, new Vector3(1.5f, 3, 0.4f), new Color(0.5f, 0, 0.1f)).AddComponent<ScenePortal>(); // 把 Box("BloodDoor_" + destination, position, new Vector3(1.5f, 3, 0.4f), new Color(0.5f, 0, 0.1f)).AddComponent<ScenePortal> 的返回值 保存为 portal。
            portal.scenePath = "Assets/GuishaDemo/Scenes/" + (destination == 0 ? "Guisha" : destination == 1 ? "BloodVilla" : "RainVillage") + ".unity"; // 更新 portal.scenePath，使用 "Assets/GuishaDemo/Scenes/" + (destination == 0 ? "Guisha" : destination == 1 ? "BloodVilla" : "RainVillage") + ".unity"。
            portal.dungeonId = destination; // 更新 portal.dungeonId，使用 目标位置。
        } // 结束方法。

        /// <summary>
        /// 创建灰盒立方体、变换、材质并登记材质用于销毁清理。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="scale">物体缩放。</param>
        /// <param name="color">显示颜色。</param>
        /// <returns>UnityEngine.GameObject 类型的处理结果。</returns>
        private GameObject Box(string name, Vector3 position, Vector3 scale, Color color) // 创建灰盒立方体、变换、材质并登记材质用于销毁清理。
        { // 开始方法。
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); // 把 GameObject.CreatePrimitive 的返回值 保存为 go。
            go.name = name; // 更新 go.name，使用 名称。
            go.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            go.transform.position = position; // 更新 go.transform.position，使用 世界位置。
            go.transform.localScale = scale; // 更新 go.transform.localScale，使用 物体缩放。
            var material = RenderPipelineService.CreateMaterial(color); // 按真实活动管线创建材质，不能因安装了某个包就选错 Shader。
            materials.Add(material); // 向 本演示创建且需要销毁的材质 加入 material。
            go.GetComponent<Renderer>().sharedMaterial = material; // 更新 go.GetComponent<Renderer>().sharedMaterial，使用 material。
            return go; // 返回 go。
        } // 结束方法。

        /// <summary>
        /// 以地板和墙体盒源生成并注册运行时 NavMeshData。
        /// </summary>
        /// <param name="objects">待处理对象序列。</param>
        private void BuildNavigation(List<GameObject> objects) // 以地板和墙体盒源生成并注册运行时 NavMeshData。
        { // 开始方法。
            var sources = new List<NavMeshBuildSource>(); // 创建 List<NavMeshBuildSource>，保存为 sources。
            foreach (var obj in objects) // 遍历 待处理对象序列，每次把一个元素保存为 obj。
                sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box, transform = obj.transform.localToWorldMatrix, size = Vector3.one }); // 向 sources 加入 new NavMeshBuildSource {     shape = NavMeshBuildSourceShape.Box,     transform = obj.transform.localToWorldMatrix,     size = Vector3.one }。
            var settings = NavMesh.GetSettingsByID(0); // 把 NavMesh.GetSettingsByID 的返回值 保存为 框架或动画配置。
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(Vector3.zero, new Vector3(40, 10, 40)), Vector3.zero, Quaternion.identity); // 把 NavMeshBuilder.BuildNavMeshData 的返回值 保存为 待保存或已读取的数据。
            if (data != null) // 当待保存或已读取的数据不为空时，执行下方分支。
                navigation = NavMesh.AddNavMeshData(data); // 更新 本演示创建的 NavMeshData，使用 NavMesh.AddNavMeshData 的返回值。
        } // 结束方法。

        /// <summary>
        /// 创建 RectTransform 对象，绑定父节点、尺寸和屏幕锚定位置。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <param name="size">界面尺寸。</param>
        /// <param name="position">世界位置。</param>
        /// <returns>UnityEngine.GameObject 类型的处理结果。</returns>
        private GameObject UIObject(string name, Transform parent, Vector2 size, Vector2 position) // 创建 RectTransform 对象，绑定父节点、尺寸和屏幕锚定位置。
        { // 开始方法。
            var go = new GameObject(name, typeof(RectTransform)); // 创建 GameObject，保存为 go。
            go.transform.SetParent(parent, false); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            var rect = go.GetComponent<RectTransform>(); // 把 go.GetComponent<RectTransform> 的返回值 保存为 rect。
            rect.sizeDelta = size; // 更新 rect.sizeDelta，使用 界面尺寸。
            rect.anchoredPosition = position; // 更新 rect.anchoredPosition，使用 世界位置。
            return go; // 返回 go。
        } // 结束方法。

        /// <summary>
        /// 创建并设置文本标签。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <param name="value">当前处理值。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="size">界面尺寸。</param>
        /// <returns>UnityEngine.UI.Text 类型的处理结果。</returns>
        private Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size) // 创建并设置文本标签。
        { // 开始方法。
            var text = UIObject(name, parent, size, position).AddComponent<Text>(); // 把 UIObject(name, parent, size, position).AddComponent<Text> 的返回值 保存为 正文或待解析字符串。
            text.font = font; // 更新 text.font，使用 界面字体。
            text.text = value; // 更新 text.text，使用 当前处理值。
            text.fontSize = 24; // 更新 text.fontSize，使用 24。
            text.color = Color.white; // 更新 text.color，使用 Color.white。
            text.alignment = TextAnchor.MiddleCenter; // 更新 text.alignment，使用 TextAnchor.MiddleCenter。
            text.raycastTarget = false; // 更新 text.raycastTarget，使用 false（关闭/不满足）。
            return text; // 返回 正文或待解析字符串。
        } // 结束方法。

        /// <summary>
        /// 创建带底图和 CanvasGroup 的暂时关闭面板，供配置后激活。
        /// </summary>
        /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
        /// <param name="id">稳定标识。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <returns>T 类型的处理结果。</returns>
        private T Panel<T>(string id, Transform parent) // 创建带底图和 CanvasGroup 的暂时关闭面板，供配置后激活。
            where T : UIPanelBase // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            var go = UIObject(id, parent, new Vector2(850, 420), Vector2.zero); // 把 UIObject 的返回值 保存为 go。
            go.SetActive(false); // 将 go 的激活状态设为 false，同时影响其组件生命周期。
            var image = go.AddComponent<Image>(); // 把 go.AddComponent<Image> 的返回值 保存为 image。
            image.color = new Color(0.02f, 0.025f, 0.04f, 0.95f); // 更新 image.color，使用 新建的 Color 对象。
            go.AddComponent<CanvasGroup>(); // 给 go 添加 CanvasGroup 组件。
            var panel = go.AddComponent<T>(); // 把 go.AddComponent<T> 的返回值 保存为 panel。
            panel.panelId = id; // 更新 panel.panelId，使用 稳定标识。
            return panel; // 返回 panel。
        } // 结束方法。

        /// <summary>
        /// 创建按钮和标签并绑定点击动作。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="action">操作委托；由调用方注入具体行为。</param>
        /// <returns>UnityEngine.UI.Button 类型的处理结果。</returns>
        private Button Button(string name, Transform parent, Vector2 position, UnityEngine.Events.UnityAction action) // 创建按钮和标签并绑定点击动作。
        { // 开始方法。
            var go = UIObject(name, parent, new Vector2(220, 44), position); // 把 UIObject 的返回值 保存为 go。
            go.AddComponent<Image>().color = new Color(0.22f, 0.25f, 0.3f); // 更新 go.AddComponent<Image>().color，使用 新建的 Color 对象。
            var button = go.AddComponent<Button>(); // 把 go.AddComponent<Button> 的返回值 保存为 button。
            button.onClick.AddListener(action); // 给按钮/滑条注册 UnityEvent 回调，用户操作时再执行该动作。
            Label("Label", go.transform, name, Vector2.zero, new Vector2(220, 44)); // 创建并设置文本标签。
            return button; // 返回 button。
        } // 结束方法。

        /// <summary>
        /// 构建 HUD、暂停、音量、背包、对白、玩法选择面板和输入路由。
        /// </summary>
        private void BuildUI() // 构建 HUD、暂停、音量、背包、对白、玩法选择面板和输入路由。
        { // 开始方法。
            var canvasObject = new GameObject("Canvas", typeof(RectTransform)); // 创建 GameObject，保存为 canvasObject。
            canvasObject.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            var canvas = canvasObject.AddComponent<Canvas>(); // 把 canvasObject.AddComponent<Canvas> 的返回值 保存为 canvas。
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 更新 canvas.renderMode，使用 RenderMode.ScreenSpaceOverlay。
            var scaler = canvasObject.AddComponent<CanvasScaler>(); // 把 canvasObject.AddComponent<CanvasScaler> 的返回值 保存为 scaler。
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 更新 scaler.uiScaleMode，使用 CanvasScaler.ScaleMode.ScaleWithScreenSize。
            scaler.referenceResolution = new Vector2(1600, 900); // 更新 scaler.referenceResolution，使用 新建的 Vector2 对象。
            canvasObject.AddComponent<GraphicRaycaster>(); // 给 canvasObject 添加 GraphicRaycaster 组件。
            var events = new GameObject("EventSystem"); // 创建 GameObject，保存为 events。
            events.transform.SetParent(content); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
            events.AddComponent<EventSystem>(); // 给 events 添加 EventSystem 组件。
            events.AddComponent<StandaloneInputModule>(); // 给 events 添加 StandaloneInputModule 组件。
            var hud = canvasObject.AddComponent<HUD>(); // 把 canvasObject.AddComponent<HUD> 的返回值 保存为 hud。
            hud.modeText = Label("Mode", canvasObject.transform, "", new Vector2(0, 340), new Vector2(1000, 45)); // 更新 hud.modeText，使用 Label 的返回值。
            hud.statsText = Label("Stats", canvasObject.transform, "SAN 100 | 体力 100", new Vector2(0, 390), new Vector2(1000, 60)); // 更新 hud.statsText，使用 Label 的返回值。
            hud.promptText = Label("Prompt", canvasObject.transform, "", new Vector2(0, -180), new Vector2(1000, 60)); // 更新 hud.promptText，使用 Label 的返回值。
            hud.notificationText = Label("Notice", canvasObject.transform, "", new Vector2(0, -310), new Vector2(1450, 150)); // 更新 hud.notificationText，使用 Label 的返回值。
            hud.quickSlotsText = Label("QuickSlots", canvasObject.transform, "快捷栏 [1]", new Vector2(0, -420), new Vector2(1200, 50)); // 更新 hud.quickSlotsText，使用 Label 的返回值。
            Label("Crosshair", canvasObject.transform, "+", Vector2.zero, new Vector2(30, 30)); // 创建并设置文本标签。
            var menu = Panel<MenuPanel>("Pause", canvasObject.transform); // 把 Panel<MenuPanel> 的返回值 保存为 menu。
            Label("Title", menu.transform, "诡舍 · 暂停", new Vector2(0, 150), new Vector2(800, 60)); // 创建并设置文本标签。
            Button("继续", menu.transform, new Vector2(0, 50), menu.Resume); // 创建按钮和标签并绑定点击动作。
            Button("保存", menu.transform, new Vector2(0, -10), menu.Save); // 创建按钮和标签并绑定点击动作。
            Button("读取检查点", menu.transform, new Vector2(0, -70), menu.Load); // 创建按钮和标签并绑定点击动作。
            var volume = UIObject("Volume", menu.transform, new Vector2(220, 20), new Vector2(0, -145)).AddComponent<Slider>(); // 把 UIObject("Volume", menu.transform, new Vector2(220, 20), new Vector2(0, -145)).AddComponent<Slider> 的返回值 保存为 音量。
            var track = volume.gameObject.AddComponent<Image>(); // 把 volume.gameObject.AddComponent<Image> 的返回值 保存为 track。
            track.color = Color.gray; // 更新 track.color，使用 Color.gray。
            var fill = UIObject("Fill", volume.transform, new Vector2(220, 20), Vector2.zero); // 把 UIObject 的返回值 保存为 fill。
            fill.AddComponent<Image>().color = Color.cyan; // 更新 fill.AddComponent<Image>().color，使用 Color.cyan。
            volume.fillRect = fill.GetComponent<RectTransform>(); // 更新 volume.fillRect，使用 fill.GetComponent<RectTransform> 的返回值。
            volume.value = AudioListener.volume; // 更新 volume.value，使用 AudioListener.volume。
            volume.onValueChanged.AddListener(menu.SetMasterVolume); // 给按钮/滑条注册 UnityEvent 回调，用户操作时再执行该动作。
            menu.gameObject.SetActive(true); // 将 menu.gameObject 的激活状态设为 true，同时影响其组件生命周期。
            var inventory = Panel<InventoryUI>("Inventory", canvasObject.transform); // 把 Panel<InventoryUI> 的返回值 保存为 背包组件。
            inventory.itemsText = Label("Items", inventory.transform, "背包", new Vector2(0, 50), new Vector2(800, 280)); // 更新 inventory.itemsText，使用 Label 的返回值。
            Button("使用第一个鬼器", inventory.transform, new Vector2(0, -150), () => inventory.Use(0)); // 创建按钮和标签并绑定点击动作。
            inventory.gameObject.SetActive(true); // 将 inventory.gameObject 的激活状态设为 true，同时影响其组件生命周期。
            var dialoguePanel = Panel<DialoguePanel>("Dialogue", canvasObject.transform); // 把 Panel<DialoguePanel> 的返回值 保存为 dialoguePanel。
            dialoguePanel.speaker = Label("Speaker", dialoguePanel.transform, "", new Vector2(0, 150), new Vector2(800, 60)); // 更新 dialoguePanel.speaker，使用 Label 的返回值。
            dialoguePanel.body = Label("Body", dialoguePanel.transform, "", new Vector2(0, 40), new Vector2(800, 140)); // 更新 dialoguePanel.body，使用 Label 的返回值。
            dialoguePanel.choiceText = Label("Choices", dialoguePanel.transform, "", new Vector2(0, -110), new Vector2(800, 130)); // 更新 dialoguePanel.choiceText，使用 Label 的返回值。
            dialoguePanel.gameObject.SetActive(true); // 将 dialoguePanel.gameObject 的激活状态设为 true，同时影响其组件生命周期。
            var modes = Panel<GameModePanel>("Modes", canvasObject.transform); // 把 Panel<GameModePanel> 的返回值 保存为 玩法选择面板或配置数组。
            Label("Title", modes.transform, "选择玩法模式", new Vector2(0, 165), new Vector2(800, 45)); // 创建并设置文本标签。
            Button("剧情 · 轻度压力", modes.transform, new Vector2(0, 100), () => modes.Select(0)); // 创建按钮和标签并绑定点击动作。
            Button("探索 · 安全调查", modes.transform, new Vector2(0, 40), () => modes.Select(1)); // 创建按钮和标签并绑定点击动作。
            Button("挑战 · 限时解谜", modes.transform, new Vector2(0, -20), () => modes.Select(2)); // 创建按钮和标签并绑定点击动作。
            Button("生存 · 持续增援", modes.transform, new Vector2(0, -80), () => modes.Select(3)); // 创建按钮和标签并绑定点击动作。
            Button("返回", modes.transform, new Vector2(0, -150), modes.Back); // 创建按钮和标签并绑定点击动作。
            modes.gameObject.SetActive(true); // 将 modes.gameObject 的激活状态设为 true，同时影响其组件生命周期。
            Button("玩法模式", menu.transform, new Vector2(260, 50), () => EventBus.Publish(new PanelRequested("Modes", true))); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            canvasObject.AddComponent<UIInputRouter>().modes = modes; // 更新 canvasObject.AddComponent<UIInputRouter>().modes，使用 玩法选择面板或配置数组。
        } // 结束方法。

        /// <summary>
        /// 移除运行时寻路数据，并销毁本演示拥有的字体和材质。
        /// </summary>
        private void OnDestroy() // 移除运行时寻路数据，并销毁本演示拥有的字体和材质。
        { // 开始方法。
            if (navigation.valid) // 当navigation.valid为 true时，执行下方分支。
                navigation.Remove(); // 从 本演示创建的 NavMeshData 移除 当前元素。
            if (uiFont == null && font != null) // 当（可选演示 UI 字体为空；且界面字体不为空）时，执行下方分支。
                Destroy(font); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
            foreach (var material in materials) // 遍历 本演示创建且需要销毁的材质，每次把一个元素保存为 material。
                if (material != null) // 当material不为空时，执行下方分支。
                    Destroy(material); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
