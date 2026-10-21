using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.IO; // 引入 System.IO 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 维护流程状态、存读档、检查点与副本编号。
    /// </summary>
    public sealed class GameManager : MonoSingleton<GameManager> // 维护流程状态、存读档、检查点与副本编号。
    { // 开始类型。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存检查点标识。
        /// </summary>
        private string checkpoint = ""; // 把 "" 保存为 检查点标识。
        /// <summary>
        /// 保存检查点位置。
        /// </summary>
        private Vector3 checkpointPosition; // 声明 检查点位置，后续初始化。
        /// <summary>
        /// 保存检查点旋转。
        /// </summary>
        private Quaternion checkpointRotation = Quaternion.identity; // 把 Quaternion.identity 保存为 检查点旋转。
        /// <summary>
        /// 保存是否已有检查点。
        /// </summary>
        private bool hasCheckpoint; // 声明 是否已有检查点，后续初始化。
        /// <summary>
        /// 保存正在读取存档的标记。
        /// </summary>
        private bool loadingSave; // 声明 正在读取存档的标记，后续初始化。
        /// <summary>
        /// 获取当前副本编号。
        /// </summary>
        public int CurrentDungeon { get; private set; } // 获取当前副本编号。
        /// <summary>
        /// 获取当前状态。
        /// </summary>
        public GameState State => GameSession.State; // 获取当前状态。
        /// <summary>
        /// 获取当前存档槽的文件路径。
        /// </summary>
        public string SavePath => Path.Combine(Application.persistentDataPath, "Saves", "slot1.json"); // 获取当前存档槽的文件路径。

        /// <summary>
        /// 先建立主单例并恢复时间缩放，再订阅流程、输入锁、检查点和存读档事件。
        /// </summary>
        protected override void Awake() // 先建立主单例并恢复时间缩放，再订阅流程、输入锁、检查点和存读档事件。
        { // 开始方法。
            base.Awake(); // 执行基类 Awake，保留基类初始化流程。
            if (Instance != this) // 当当前类型的主单例实例不等于this时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            Time.timeScale = 1; // 设置全局缩放时间，控制暂停与恢复；值来自 1。
            subscriptions.Add(EventBus.Subscribe<PauseRequested>(e => // 订阅 PauseRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (State == GameState.Playing || State == GameState.Paused) // 当（当前状态等于GameState.Playing；或当前状态等于GameState.Paused）时，执行下方分支。
                    SetState(e.Pause ? GameState.Paused : GameState.Playing); // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<InputLockChanged>(e => GameSession.SetLock(e.Owner, e.Locked))); // 订阅 InputLockChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SaveRequested>(_ => Save())); // 订阅 SaveRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<LoadRequested>(_ => Load())); // 订阅 LoadRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<ModeRoundEnded>(e => // 广播 Notification 事件，让 HUD 显示短通知。
            { // 开始回调。
                SetState(GameState.GameOver); // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
                EventBus.Publish(new Notification(e.Reason + "，按 F9 读取检查点")); // 广播 Notification 事件，让 HUD 显示短通知。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => // 订阅 EntityDied 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Entity.Type == EntityType.Player) // 当e.Entity.Type等于EntityType.Player时，执行下方分支。
                    SetState(GameState.GameOver); // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<CheckpointReached>(e => // 订阅 CheckpointReached 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                checkpoint = e.Id; // 更新 检查点标识，使用 e.Id。
                checkpointPosition = e.Position; // 更新 检查点位置，使用 e.Position。
                checkpointRotation = e.Rotation; // 更新 检查点旋转，使用 e.Rotation。
                hasCheckpoint = true; // 更新 是否已有检查点，使用 true（启用/满足）。
                Save(); // 在允许保存时采集角色/背包/模式和世界数据，按检查点替换出生位置并写文件。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<DungeonEntered>(e => CurrentDungeon = e.Id)); // 订阅 DungeonEntered 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SceneTransitionStarted>(_ => SetState(GameState.Loading))); // 订阅 SceneTransitionStarted 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SceneTransitionFinished>(e => // 订阅 SceneTransitionFinished 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Success) // 当e.Success为 true时，执行下方分支。
                { // 开始条件分支。
                    CurrentDungeon = e.Dungeon; // 更新 当前副本编号，使用 e.Dungeon。
                    hasCheckpoint = false; // 更新 是否已有检查点，使用 false（关闭/不满足）。
                    checkpoint = ""; // 更新 检查点标识，使用 ""。
                } // 结束条件分支。

                SetState(GameState.Playing); // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
            })); // 结束回调。
            SetState(GameState.Playing); // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
        } // 结束方法。

        /// <summary>
        /// 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
        /// </summary>
        /// <param name="state">目标状态或保存的状态数据。</param>
        private void SetState(GameState state) // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
        { // 开始方法。
            Time.timeScale = state == GameState.Paused || state == GameState.GameOver ? 0 : 1; // 设置全局缩放时间，控制暂停与恢复；值来自 按条件 state == GameState.Paused || state == GameState.GameOver 选择的值。
            GameSession.SetState(state); // 更新共享流程状态并发布 GameStateChanged。
        } // 结束方法。

        /// <summary>
        /// 在允许保存时采集角色/背包/模式和世界数据，按检查点替换出生位置并写文件。
        /// </summary>
        public void Save() // 在允许保存时采集角色/背包/模式和世界数据，按检查点替换出生位置并写文件。
        { // 开始方法。
            if (State == GameState.Loading || loadingSave) // 当（当前状态等于GameState.Loading；或正在读取存档的标记为 true）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (!GameModeRuntime.Rules.AllowSave) // 当条件 GameModeRuntime.Rules.AllowSave 不成立时，执行下方分支。
            { // 开始条件分支。
                EventBus.Publish(new Notification("当前模式禁用保存")); // 广播 Notification 事件，让 HUD 显示短通知。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            var data = new SaveData // 创建 SaveData，保存为 待保存或已读取的数据。
            { // 开始成员/集合初始化。
                scenePath = SceneManager.GetActiveScene().path, // 更新 场景的完整资源路径，使用 SceneManager.GetActiveScene().path。
                dungeon = CurrentDungeon, // 更新 当前副本编号，使用 当前副本编号。
                checkpointId = checkpoint // 更新 保存的检查点标识，使用 检查点标识。
            }; // 结束成员/集合初始化。
            data.worldFlags = WorldState.Snapshot(); // 更新 data.worldFlags，使用 WorldState.Snapshot 的返回值。
            EventBus.Publish(new SaveCapture(data)); // 广播 SaveCapture 事件，让参与存档的组件填充共享数据。
            if (!data.hasPlayer) // 当条件 data.hasPlayer 不成立时，执行下方分支。
            { // 开始条件分支。
                EventBus.Publish(new Notification("保存失败：场景中没有玩家。")); // 广播 Notification 事件，让 HUD 显示短通知。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            if (hasCheckpoint) // 当是否已有检查点为 true时，执行下方分支。
            { // 开始条件分支。
                data.position = checkpointPosition; // 更新 data.position，使用 检查点位置。
                data.rotation = checkpointRotation; // 更新 data.rotation，使用 检查点旋转。
            } // 结束条件分支。

            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                SerializeHelper.Save(SavePath, data); // 写临时文件，保留已有文件为 .bak，再复制覆盖目标文件。
                EventBus.Publish(new Notification("检查点已保存")); // 广播 Notification 事件，让 HUD 显示短通知。
            } // 结束受异常保护的操作。
            catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                Guisha.Core.Logger.Error(error.ToString()); // 按 ERROR 级别输出错误，并按开关写文件。
                EventBus.Publish(new Notification("保存失败")); // 广播 Notification 事件，让 HUD 显示短通知。
            } // 结束异常处理。
        } // 结束方法。

        /// <summary>
        /// 读取有效版本存档，必要时先切场景；成功后恢复流程、角色与各模块。
        /// </summary>
        public void Load() // 读取有效版本存档，必要时先切场景；成功后恢复流程、角色与各模块。
        { // 开始方法。
            if (State == GameState.Loading || loadingSave) // 当（当前状态等于GameState.Loading；或正在读取存档的标记为 true）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (!SerializeHelper.TryLoad<SaveData>(SavePath, out var data) || data.version != 1 || !data.hasPlayer) // 当（!SerializeHelper.TryLoad小于SaveData>(SavePath, out var data)；或data.version不等于1；或条件 data.hasPlayer 不成立）时，执行下方分支。
            { // 开始条件分支。
                EventBus.Publish(new Notification("没有有效的存档")); // 广播 Notification 事件，让 HUD 显示短通知。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            loadingSave = true; // 更新 正在读取存档的标记，使用 true（启用/满足）。
            var previousFlags = WorldState.Snapshot(); // 把 WorldState.Snapshot 的返回值 保存为 previousFlags。
            WorldState.Restore(data.worldFlags); // 替换全部标记，忽略空值，随后广播变化。
            // 局部函数：Load 内部的完成回调：失败恢复旧世界标记，成功发布 SaveLoaded 并关闭暂停面板。
            // 参数 success：是否成功。
            void Restore(bool success) // Load 内部的完成回调：失败恢复旧世界标记，成功发布 SaveLoaded 并关闭暂停面板。
            { // 开始局部函数。
                loadingSave = false; // 更新 正在读取存档的标记，使用 false（关闭/不满足）。
                if (!success) // 当条件 success 不成立时，执行下方分支。
                { // 开始条件分支。
                    WorldState.Restore(previousFlags); // 替换全部标记，忽略空值，随后广播变化。
                    EventBus.Publish(new Notification("存档场景加载失败")); // 广播 Notification 事件，让 HUD 显示短通知。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束条件分支。

                CurrentDungeon = data.dungeon; // 更新 当前副本编号，使用 data.dungeon。
                checkpoint = data.checkpointId; // 更新 检查点标识，使用 data.checkpointId。
                hasCheckpoint = !string.IsNullOrEmpty(checkpoint); // 更新 是否已有检查点，使用 !string.IsNullOrEmpty(checkpoint)。
                checkpointPosition = data.position; // 更新 检查点位置，使用 data.position。
                checkpointRotation = data.rotation; // 更新 检查点旋转，使用 data.rotation。
                SetState(GameState.Playing); // 更新 Time.timeScale，暂停/结束时设为 0，再写入 GameSession。
                EventBus.Publish(new PanelRequested("Pause", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
                EventBus.Publish(new SaveLoaded(data)); // 广播 SaveLoaded 事件，让组件从存档恢复各自状态。
                EventBus.Publish(new Notification("存档已恢复")); // 广播 Notification 事件，让 HUD 显示短通知。
            } // 结束局部函数。

            if (data.scenePath == SceneManager.GetActiveScene().path) // 当data.scenePath等于SceneManager.GetActiveScene().path时，执行下方分支。
                Restore(true); // Load 内部的完成回调：失败恢复旧世界标记，成功发布 SaveLoaded 并关闭暂停面板。
            else // 前面的条件不成立时，执行此备用分支。
                EventBus.Publish(new SceneTransitionRequested(data.scenePath, data.dungeon, Restore)); // 广播 SceneTransitionRequested 事件，交给场景管理器执行附加加载/切换。
        } // 结束方法。

        /// <summary>
        /// 释放自身事件订阅，最后调用基类清理主单例引用。
        /// </summary>
        protected override void OnDestroy() // 释放自身事件订阅，最后调用基类清理主单例引用。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            base.OnDestroy(); // 执行基类销毁清理，保留单例引用等释放约定。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
