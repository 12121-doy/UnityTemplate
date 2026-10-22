using System; // 引入 System 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 常驻玩法管理器，切换规则、计时、处理挑战完成与模式存档。
    /// </summary>
    public sealed class GameModeManager : MonoSingleton<GameModeManager> // 常驻玩法管理器，切换规则、计时、处理挑战完成与模式存档。
    { // 开始类型。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存可用配置目录。
        /// </summary>
        private GameModeCatalog catalog; // 声明 可用配置目录，后续初始化。
        /// <summary>
        /// 保存当前副本编号。
        /// </summary>
        private int dungeon; // 声明 当前副本编号，后续初始化。
        /// <summary>
        /// 保存下次模式进度广播时间。
        /// </summary>
        private float nextProgress; // 声明 下次模式进度广播时间，后续初始化。
        /// <summary>
        /// 保存本轮是否已经结束。
        /// </summary>
        private bool ended; // 声明 本轮是否已经结束，后续初始化。
        /// <summary>
        /// 先建立主单例，读取玩法目录并应用默认规则，然后订阅模式/存档/挑战事件。
        /// </summary>
        protected override void Awake() // 先建立主单例，读取玩法目录并应用默认规则，然后订阅模式/存档/挑战事件。
        { // 开始方法。
            base.Awake(); // 执行基类 Awake，保留基类初始化流程。
            if (Instance != this) // 当当前类型的主单例实例不等于this时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings"); // 把 Resources.Load<FrameworkSettings> 的返回值 保存为 框架或动画配置。
            catalog = settings != null ? settings.gameModes : null; // 更新 可用配置目录，使用 按条件 settings != null 选择的值。
            Apply(settings != null ? settings.defaultGameMode : GameMode.Story); // 重设结束标记，应用 Catalog/默认规则并发布进度。
            subscriptions.Add(EventBus.Subscribe<GameModeRequested>(e => // 广播 Notification 事件，让 HUD 显示短通知。
            { // 开始回调。
                if (GameSession.State == GameState.Loading || GameSession.State == GameState.GameOver || !Enum.IsDefined(typeof(GameMode), e.Mode)) // 当（GameSession.State等于GameState.Loading；或GameSession.State等于GameState.GameOver；或条件 Enum.IsDefined(typeof(GameMode), e.Mode) 不成立）时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                Apply(e.Mode); // 重设结束标记，应用 Catalog/默认规则并发布进度。
                EventBus.Publish(new Notification("已切换：" + GameModeRuntime.Label(e.Mode))); // 广播 Notification 事件，让 HUD 显示短通知。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<DungeonEntered>(e => dungeon = e.Id)); // 订阅 DungeonEntered 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<MechanismActivated>(e => // 广播 ChallengeCompleted 事件，通知限时挑战完成。
            { // 开始回调。
                if (dungeon <= 0 || !GameSession.SimulationEnabled || GameModeRuntime.Rules.Mode != GameMode.Challenge) // 当（当前副本编号不大于0；或当前不推进游戏模拟；或GameModeRuntime.Rules.Mode不等于GameMode.Challenge）时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                string objective = null; // 把 null（清空引用） 保存为 objective。
                if (catalog != null) // 当可用配置目录不为空时，执行下方分支。
                    foreach (var definition in catalog.definitions) // 遍历 catalog.definitions，每次把一个元素保存为 definition。
                        if (definition != null && definition.mode == GameMode.Challenge) // 当（当前遍历的玩法定义不为空；且definition.mode等于GameMode.Challenge）时，执行下方分支。
                        { // 开始条件分支。
                            objective = definition.objectiveMechanismId; // 更新 objective，使用 definition.objectiveMechanismId。
                            break; // 结束当前循环或 switch 分支。
                        } // 结束条件分支。

                if (!string.IsNullOrEmpty(objective) && objective != e.Id) // 当（条件 string.IsNullOrEmpty(objective) 不成立；且objective不等于e.Id）时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                float seconds = GameModeRuntime.Elapsed; // 把 GameModeRuntime.Elapsed 保存为 延时秒数。
                Apply(GameMode.Story); // 重设结束标记，应用 Catalog/默认规则并发布进度。
                EventBus.Publish(new ChallengeCompleted(seconds, e.Id)); // 广播 ChallengeCompleted 事件，通知限时挑战完成。
                EventBus.Publish(new Notification("限时挑战成功，已返回剧情模式")); // 广播 Notification 事件，让 HUD 显示短通知。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SceneTransitionFinished>(e => // 订阅 SceneTransitionFinished 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (e.Success) // 当e.Success为 true时，执行下方分支。
                { // 开始条件分支。
                    dungeon = e.Dungeon; // 更新 当前副本编号，使用 e.Dungeon。
                    GameModeRuntime.SetProgress(0, 0); // 写入非负计时和波次，非有限计时回退为 0。
                    ended = false; // 更新 本轮是否已经结束，使用 false（关闭/不满足）。
                } // 结束条件分支。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SurvivalWaveStarted>(e => GameModeRuntime.SetProgress(GameModeRuntime.Elapsed, e.Wave))); // 订阅 SurvivalWaveStarted 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SaveCapture>(e => // 订阅 SaveCapture 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                e.Data.gameMode = GameModeRuntime.Rules.Mode; // 更新 e.Data.gameMode，使用 GameModeRuntime.Rules.Mode。
                e.Data.modeElapsed = GameModeRuntime.Elapsed; // 更新 e.Data.modeElapsed，使用 GameModeRuntime.Elapsed。
                e.Data.survivalWave = GameModeRuntime.Wave; // 更新 e.Data.survivalWave，使用 GameModeRuntime.Wave。
            })); // 结束回调。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(e => // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                Apply(Enum.IsDefined(typeof(GameMode), e.Data.gameMode) ? e.Data.gameMode : GameMode.Story); // 重设结束标记，应用 Catalog/默认规则并发布进度。
                GameModeRuntime.SetProgress(e.Data.modeElapsed, e.Data.survivalWave); // 写入非负计时和波次，非有限计时回退为 0。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 重设结束标记，应用 Catalog/默认规则并发布进度。
        /// </summary>
        /// <param name="mode">当前玩法。</param>
        private void Apply(GameMode mode) // 重设结束标记，应用 Catalog/默认规则并发布进度。
        { // 开始方法。
            ended = false; // 更新 本轮是否已经结束，使用 false（关闭/不满足）。
            GameModeRuntime.Apply(catalog != null ? catalog.Resolve(mode) : GameModeRules.Default(mode)); // 替换规则，清空计时/波次并广播 GameModeChanged。
            Publish(); // 广播模式经过时间、剩余时间和波次。
        } // 结束方法。

        /// <summary>
        /// 广播模式经过时间、剩余时间和波次。
        /// </summary>
        private void Publish() // 广播模式经过时间、剩余时间和波次。
        { // 开始方法。
            EventBus.Publish(new GameModeProgress(GameModeRuntime.Elapsed, GameModeRuntime.Remaining, GameModeRuntime.Wave)); // 广播 GameModeProgress 事件，刷新模式计时和波次 UI。
        } // 结束方法。

        /// <summary>
        /// 在副本中推进未结束计时，定期广播进度，超时时发送结束请求。
        /// </summary>
        private void Update() // 在副本中推进未结束计时，定期广播进度，超时时发送结束请求。
        { // 开始方法。
            if (!GameSession.SimulationEnabled || dungeon <= 0 || ended) // 当（当前不推进游戏模拟；或当前副本编号不大于0；或本轮是否已经结束为 true）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            GameModeRuntime.SetProgress(GameModeRuntime.Elapsed + Time.deltaTime, GameModeRuntime.Wave); // 写入非负计时和波次，非有限计时回退为 0。
            if (Time.unscaledTime >= nextProgress) // 当当前非缩放时间不小于下次模式进度广播时间时，执行下方分支。
            { // 开始条件分支。
                nextProgress = Time.unscaledTime + 0.25f; // 更新 下次模式进度广播时间，使用 Time.unscaledTime + 0.25f。
                Publish(); // 广播模式经过时间、剩余时间和波次。
            } // 结束条件分支。

            if (GameModeRuntime.Rules.TimeLimit > 0 && GameModeRuntime.Remaining <= 0) // 当（GameModeRuntime.Rules.TimeLimit大于0；且GameModeRuntime.Remaining不大于0）时，执行下方分支。
            { // 开始条件分支。
                ended = true; // 更新 本轮是否已经结束，使用 true（启用/满足）。
                EventBus.Publish(new ModeRoundEnded("挑战时间耗尽")); // 广播 ModeRoundEnded 事件，处理本轮结束。
            } // 结束条件分支。
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
