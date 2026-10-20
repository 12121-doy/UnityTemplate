using System;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Managers
{
    public sealed class GameModeManager : MonoSingleton<GameModeManager>
    {
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private GameModeCatalog catalog;
        private int dungeon;
        private float nextProgress;
        private bool ended;
        protected override void Awake()
        {
            base.Awake(); if (Instance != this) return;
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings");
            catalog = settings != null ? settings.gameModes : null;
            Apply(settings != null ? settings.defaultGameMode : GameMode.Story);
            subscriptions.Add(EventBus.Subscribe<GameModeRequested>(e =>
            {
                if (GameSession.State == GameState.Loading || GameSession.State == GameState.GameOver || !Enum.IsDefined(typeof(GameMode), e.Mode)) return;
                Apply(e.Mode); EventBus.Publish(new Notification("已切换：" + GameModeRuntime.Label(e.Mode)));
            }));
            subscriptions.Add(EventBus.Subscribe<DungeonEntered>(e => dungeon = e.Id));
            subscriptions.Add(EventBus.Subscribe<MechanismActivated>(e =>
            {
                if (dungeon <= 0 || !GameSession.SimulationEnabled || GameModeRuntime.Rules.Mode != GameMode.Challenge) return;
                string objective = null;
                if (catalog != null) foreach (var definition in catalog.definitions)
                    if (definition != null && definition.mode == GameMode.Challenge) { objective = definition.objectiveMechanismId; break; }
                if (!string.IsNullOrEmpty(objective) && objective != e.Id) return;
                float seconds = GameModeRuntime.Elapsed;
                Apply(GameMode.Story);
                EventBus.Publish(new ChallengeCompleted(seconds, e.Id));
                EventBus.Publish(new Notification("限时挑战成功，已返回剧情模式"));
            }));
            subscriptions.Add(EventBus.Subscribe<SceneTransitionFinished>(e =>
            { if (e.Success) { dungeon = e.Dungeon; GameModeRuntime.SetProgress(0, 0); ended = false; } }));
            subscriptions.Add(EventBus.Subscribe<SurvivalWaveStarted>(e => GameModeRuntime.SetProgress(GameModeRuntime.Elapsed, e.Wave)));
            subscriptions.Add(EventBus.Subscribe<SaveCapture>(e =>
            { e.Data.gameMode = GameModeRuntime.Rules.Mode; e.Data.modeElapsed = GameModeRuntime.Elapsed; e.Data.survivalWave = GameModeRuntime.Wave; }));
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(e =>
            { Apply(Enum.IsDefined(typeof(GameMode), e.Data.gameMode) ? e.Data.gameMode : GameMode.Story); GameModeRuntime.SetProgress(e.Data.modeElapsed, e.Data.survivalWave); }));
        }
        private void Apply(GameMode mode)
        { ended = false; GameModeRuntime.Apply(catalog != null ? catalog.Resolve(mode) : GameModeRules.Default(mode)); Publish(); }
        private void Publish() { EventBus.Publish(new GameModeProgress(GameModeRuntime.Elapsed, GameModeRuntime.Remaining, GameModeRuntime.Wave)); }
        private void Update()
        {
            if (!GameSession.SimulationEnabled || dungeon <= 0 || ended) return;
            GameModeRuntime.SetProgress(GameModeRuntime.Elapsed + Time.deltaTime, GameModeRuntime.Wave);
            if (Time.unscaledTime >= nextProgress) { nextProgress = Time.unscaledTime + 0.25f; Publish(); }
            if (GameModeRuntime.Rules.TimeLimit > 0 && GameModeRuntime.Remaining <= 0)
            { ended = true; EventBus.Publish(new ModeRoundEnded("挑战时间耗尽")); }
        }
        protected override void OnDestroy() { subscriptions.Dispose(); base.OnDestroy(); }
    }
}
