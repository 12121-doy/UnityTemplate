using System;
using System.IO;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Guisha.Managers
{
    public sealed class GameManager : MonoSingleton<GameManager>
    {
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private string checkpoint = "";
        private Vector3 checkpointPosition;
        private Quaternion checkpointRotation = Quaternion.identity;
        private bool hasCheckpoint;
        private bool loadingSave;
        public int CurrentDungeon { get; private set; }
        public GameState State => GameSession.State;
        public string SavePath => Path.Combine(Application.persistentDataPath, "Saves", "slot1.json");
        protected override void Awake()
        {
            base.Awake(); if (Instance != this) return;
            Time.timeScale = 1;
            subscriptions.Add(EventBus.Subscribe<PauseRequested>(e => { if (State == GameState.Playing || State == GameState.Paused) SetState(e.Pause ? GameState.Paused : GameState.Playing); }));
            subscriptions.Add(EventBus.Subscribe<InputLockChanged>(e => GameSession.SetLock(e.Owner, e.Locked)));
            subscriptions.Add(EventBus.Subscribe<SaveRequested>(_ => Save()));
            subscriptions.Add(EventBus.Subscribe<LoadRequested>(_ => Load()));
            subscriptions.Add(EventBus.Subscribe<ModeRoundEnded>(e => { SetState(GameState.GameOver); EventBus.Publish(new Notification(e.Reason + "，按 F9 读取检查点")); }));
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => { if (e.Entity.Type == EntityType.Player) SetState(GameState.GameOver); }));
            subscriptions.Add(EventBus.Subscribe<CheckpointReached>(e =>
            { checkpoint = e.Id; checkpointPosition = e.Position; checkpointRotation = e.Rotation; hasCheckpoint = true; Save(); }));
            subscriptions.Add(EventBus.Subscribe<DungeonEntered>(e => CurrentDungeon = e.Id));
            subscriptions.Add(EventBus.Subscribe<SceneTransitionStarted>(_ => SetState(GameState.Loading)));
            subscriptions.Add(EventBus.Subscribe<SceneTransitionFinished>(e =>
            { if (e.Success) { CurrentDungeon = e.Dungeon; hasCheckpoint = false; checkpoint = ""; } SetState(GameState.Playing); }));
            SetState(GameState.Playing);
        }
        private void SetState(GameState state)
        {
            Time.timeScale = state == GameState.Paused || state == GameState.GameOver ? 0 : 1;
            GameSession.SetState(state);
        }
        public void Save()
        {
            if (State == GameState.Loading || loadingSave) return;
            if (!GameModeRuntime.Rules.AllowSave) { EventBus.Publish(new Notification("当前模式禁用保存")); return; }
            var data = new SaveData { scenePath = SceneManager.GetActiveScene().path, dungeon = CurrentDungeon, checkpointId = checkpoint };
            data.worldFlags = WorldState.Snapshot();
            EventBus.Publish(new SaveCapture(data));
            if (!data.hasPlayer) { EventBus.Publish(new Notification("保存失败：场景中没有玩家。")); return; }
            if (hasCheckpoint) { data.position = checkpointPosition; data.rotation = checkpointRotation; }
            try { SerializeHelper.Save(SavePath, data); EventBus.Publish(new Notification("检查点已保存")); }
            catch (Exception error) { Guisha.Core.Logger.Error(error.ToString()); EventBus.Publish(new Notification("保存失败")); }
        }
        public void Load()
        {
            if (State == GameState.Loading || loadingSave) return;
            if (!SerializeHelper.TryLoad<SaveData>(SavePath, out var data) || data.version != 1 || !data.hasPlayer)
            { EventBus.Publish(new Notification("没有有效的存档")); return; }
            loadingSave = true;
            var previousFlags = WorldState.Snapshot();
            WorldState.Restore(data.worldFlags);
            void Restore(bool success)
            {
                loadingSave = false;
                if (!success) { WorldState.Restore(previousFlags); EventBus.Publish(new Notification("存档场景加载失败")); return; }
                CurrentDungeon = data.dungeon; checkpoint = data.checkpointId;
                hasCheckpoint = !string.IsNullOrEmpty(checkpoint); checkpointPosition = data.position; checkpointRotation = data.rotation;
                SetState(GameState.Playing);
                EventBus.Publish(new PanelRequested("Pause", false));
                EventBus.Publish(new SaveLoaded(data));
                EventBus.Publish(new Notification("存档已恢复"));
            }
            if (data.scenePath == SceneManager.GetActiveScene().path) Restore(true);
            else EventBus.Publish(new SceneTransitionRequested(data.scenePath, data.dungeon, Restore));
        }
        protected override void OnDestroy() { subscriptions.Dispose(); base.OnDestroy(); }
    }
}
