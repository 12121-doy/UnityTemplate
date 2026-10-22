using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 区域触发对白、刷怪与过场，支持一次性世界标记。
    /// </summary>
    [RequireComponent(typeof(Collider))] // 声明本组件必须同时具备的依赖组件。
    public sealed class StoryTrigger : MonoBehaviour // 区域触发对白、刷怪与过场，支持一次性世界标记。
    { // 开始类型。
        /// <summary>
        /// 保存是否只触发一次。
        /// </summary>
        public bool once = true; // 把 true（启用/满足） 保存为 是否只触发一次。
        /// <summary>
        /// 保存持久世界标记 ID。
        /// </summary>
        public string persistentId; // 声明 持久世界标记 ID，后续初始化。
        /// <summary>
        /// 保存对白资产或当前对白。
        /// </summary>
        public DialogueAsset dialogue; // 声明 对白资产或当前对白，后续初始化。
        /// <summary>
        /// 保存待生成实体配置。
        /// </summary>
        public EntityDefinition spawn; // 声明 待生成实体配置，后续初始化。
        /// <summary>
        /// 保存实体生成位置。
        /// </summary>
        public Transform spawnPoint; // 声明 实体生成位置，后续初始化。
        /// <summary>
        /// 保存过场播放器。
        /// </summary>
        public CutscenePlayer cutscene; // 声明 过场播放器，后续初始化。
        /// <summary>
        /// 保存触发器已使用标记。
        /// </summary>
        private bool used; // 声明 触发器已使用标记，后续初始化。
        /// <summary>
        /// Unity 编辑器 Reset 回调，把本 Collider 设为触发器。
        /// </summary>
        private void Reset() // Unity 编辑器 Reset 回调，把本 Collider 设为触发器。
        { // 开始方法。
            GetComponent<Collider>().isTrigger = true; // 更新 GetComponent<Collider>().isTrigger，使用 true（启用/满足）。
        } // 结束方法。

        /// <summary>
        /// 玩家进入且未消耗一次性触发时，记录标记并触发配置的对白、实体生成和过场。
        /// </summary>
        /// <param name="other">待比较对象。</param>
        private void OnTriggerEnter(Collider other) // 玩家进入且未消耗一次性触发时，记录标记并触发配置的对白、实体生成和过场。
        { // 开始方法。
            bool alreadyUsed = string.IsNullOrEmpty(persistentId) ? used : WorldState.Has(persistentId); // 把 按条件 string.IsNullOrEmpty(persistentId) 选择的值 保存为 alreadyUsed。
            if ((once && alreadyUsed) || other.GetComponentInParent<PlayerEntity>() == null) // 当（（是否只触发一次为 true；且alreadyUsed为 true）；或other.GetComponentInParent小于PlayerEntity>() == null）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            used = true; // 更新 触发器已使用标记，使用 true（启用/满足）。
            WorldState.Set(persistentId); // 首次加入标记时广播 WorldStateChanged。
            if (dialogue != null) // 当对白资产或当前对白不为空时，执行下方分支。
                EventBus.Publish(new DialogueRequested(dialogue)); // 广播 DialogueRequested 事件，启动指定对白树。
            if (spawn != null) // 当待生成实体配置不为空时，执行下方分支。
            { // 开始条件分支。
                var point = spawnPoint != null ? spawnPoint : transform; // 把 按条件 spawnPoint != null 选择的值 保存为 待检测位置。
                EventBus.Publish(new SpawnEntityRequested(spawn, point.position, point.rotation, transform)); // 广播 SpawnEntityRequested 事件，交给资源管理器异步生成配置实体。
            } // 结束条件分支。

            if (cutscene != null) // 当过场播放器不为空时，执行下方分支。
                cutscene.Play(); // 在未播放且有资产时加输入锁，启用过场相机并播放 Director。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
