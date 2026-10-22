using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.AI; // 引入 UnityEngine.AI 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 副本生存波次导演，限制自己生成的鬼怪并清理过期生成。
    /// </summary>
    public sealed class SurvivalDirector : MonoBehaviour // 副本生存波次导演，限制自己生成的鬼怪并清理过期生成。
    { // 开始类型。
        /// <summary>
        /// 保存鬼怪实体或鬼怪配置。
        /// </summary>
        public EntityDefinition ghost; // 声明 鬼怪实体或鬼怪配置，后续初始化。
        /// <summary>
        /// 保存当前副本编号。
        /// </summary>
        public int dungeon; // 声明 当前副本编号，后续初始化。
        /// <summary>
        /// 保存本导演最多同时持有的鬼怪数。
        /// </summary>
        [Min(1)] // 约束 Inspector 中可编辑的数值范围。
        public int maxActive = 8; // 把 8 保存为 本导演最多同时持有的鬼怪数。
        /// <summary>
        /// 保存两波增援的秒数间隔。
        /// </summary>
        [Min(1)] // 约束 Inspector 中可编辑的数值范围。
        public float waveInterval = 30; // 把 30 保存为 两波增援的秒数间隔。
        /// <summary>
        /// 保存本导演拥有的鬼怪列表。
        /// </summary>
        private readonly List<BaseEntity> owned = new List<BaseEntity>(); // 创建 List<BaseEntity>，保存为 本导演拥有的鬼怪列表。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存下次生存增援时刻。
        /// </summary>
        private float nextWave; // 声明 下次生存增援时刻，后续初始化。
        /// <summary>
        /// 保存当前生存波次、尚未返回结果的生成请求数、生成请求代次，过滤过期回调。
        /// </summary>
        private int wave, pending, generation; // 声明 当前生存波次，后续初始化；声明 尚未返回结果的生成请求数，后续初始化；声明 生成请求代次，过滤过期回调，后续初始化。
        /// <summary>
        /// 重设波次计时并订阅玩法切换/实体死亡，维护本导演生成的鬼怪。
        /// </summary>
        private void OnEnable() // 重设波次计时并订阅玩法切换/实体死亡，维护本导演生成的鬼怪。
        { // 开始方法。
            nextWave = Time.time; // 更新 下次生存增援时刻，使用 当前缩放时间。
            wave = 0; // 更新 当前生存波次，使用 0。
            subscriptions.Add(EventBus.Subscribe<GameModeChanged>(_ => Clear())); // 订阅 GameModeChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => // 订阅 EntityDied 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (owned.Remove(e.Entity)) // 当条件 owned.Remove(e.Entity) 成立时，执行下方分支。
                    Destroy(e.Entity.gameObject); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 无尽副本按间隔开启波次，在配额内采样 NavMesh 并发出带完成回调的刷怪请求。
        /// </summary>
        private void Update() // 无尽副本按间隔开启波次，在配额内采样 NavMesh 并发出带完成回调的刷怪请求。
        { // 开始方法。
            owned.RemoveAll(e => e == null); // 移除已失效的持有项，使后续配额统计反映当前有效对象。
            if (dungeon <= 0 || ghost == null || !GameSession.SimulationEnabled || GameModeRuntime.Rules.Mode != GameMode.Endless || Time.time < nextWave) // 当（当前副本编号不大于0；或鬼怪实体或鬼怪配置为空；或当前不推进游戏模拟；或GameModeRuntime.Rules.Mode不等于GameMode.Endless；或当前缩放时间小于下次生存增援时刻）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            nextWave = Time.time + waveInterval; // 更新 下次生存增援时刻，使用 Time.time + waveInterval。
            int count = Mathf.Min(1 + wave / 2, maxActive - owned.Count - pending); // 把 Mathf.Min 的返回值 保存为 count。
            if (count <= 0) // 当count不大于0时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            wave++; // 递增 当前生存波次。
            EventBus.Publish(new SurvivalWaveStarted(wave)); // 广播 SurvivalWaveStarted 事件，通知无尽生存开启新波次。
            for (int i = 0; i < count; i++) // 按初始化与递增步骤循环；继续条件为 i < count。
            { // 开始计数循环。
                var point = transform.position + Quaternion.Euler(0, i * 137 + wave * 47, 0) * Vector3.forward * 10; // 把 transform.position + Quaternion.Euler(0, i * 137 + wave * 47, 0) * Vector3.forward * 10 保存为 待检测位置。
                if (!NavMesh.SamplePosition(point, out var location, 5, NavMesh.AllAreas)) // 当条件 NavMesh.SamplePosition(point, out var location, 5, NavMesh.AllAreas) 不成立时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                // EntityFactory 通过事件报告生成成功；地址失败由 ResourceManager 记录日志。
                pending++; // 递增 尚未返回结果的生成请求数。
                int ticket = generation; // 把 生成请求代次，过滤过期回调 保存为 ticket。
                EventBus.Publish(new SpawnEntityRequested(ghost, location.position, Quaternion.identity, transform, entity => // 广播 SpawnEntityRequested 事件，交给资源管理器异步生成配置实体。
                { // 开始回调。
                    if (ticket != generation) // 当ticket不等于生成请求代次，过滤过期回调时，执行下方分支。
                    { // 开始条件分支。
                        if (entity != null) // 当当前实体组件不为空时，执行下方分支。
                            Destroy(entity.gameObject); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
                        return; // 提前结束当前方法/回调，跳过后续处理。
                    } // 结束条件分支。

                    pending = Mathf.Max(0, pending - 1); // 更新 尚未返回结果的生成请求数，使用 Mathf.Max 的返回值。
                    if (entity != null) // 当当前实体组件不为空时，执行下方分支。
                        owned.Add(entity); // 向 本导演拥有的鬼怪列表 加入 entity。
                })); // 结束回调。
            } // 结束计数循环。
        } // 结束方法。

        /// <summary>
        /// 推进代次，销毁本导演生成的鬼怪，清空计数并重设下次波次时间；晚到结果按旧代次丢弃。
        /// </summary>
        private void Clear() // 推进代次，销毁本导演生成的鬼怪，清空计数并重设下次波次时间；晚到结果按旧代次丢弃。
        { // 开始方法。
            generation++; // 推进代次；旧请求即使稍后成功返回，也会被识别为过期结果。
            foreach (var entity in owned) // 遍历 本导演拥有的鬼怪列表，每次把一个元素保存为 entity。
                if (entity != null) // 当当前实体组件不为空时，执行下方分支。
                    Destroy(entity.gameObject); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
            owned.Clear(); // 清空 本导演拥有的鬼怪列表 中保存的记录。
            pending = 0; // 更新 尚未返回结果的生成请求数，使用 0。
            wave = 0; // 更新 当前生存波次，使用 0。
            nextWave = Time.time; // 更新 下次生存增援时刻，使用 当前缩放时间。
        } // 结束方法。

        /// <summary>
        /// 释放事件订阅并清理本导演持有的鬼怪与过期生成代次。
        /// </summary>
        private void OnDisable() // 释放事件订阅并清理本导演持有的鬼怪与过期生成代次。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            Clear(); // 推进代次，销毁本导演生成的鬼怪，清空计数并重设下次波次时间；晚到结果按旧代次丢弃。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
