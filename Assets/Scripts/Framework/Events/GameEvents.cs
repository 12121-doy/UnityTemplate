using System;
using UnityEngine;
namespace Guisha.Framework
{
    public enum GameState { Playing, Paused, Loading, GameOver }
    public readonly struct GameStateChanged { public readonly GameState State; public GameStateChanged(GameState state) { State = state; } }
    public readonly struct PauseRequested { public readonly bool Pause; public PauseRequested(bool pause) { Pause = pause; } }
    public readonly struct SaveRequested { }
    public readonly struct LoadRequested { }
    public readonly struct SaveCapture { public readonly SaveData Data; public SaveCapture(SaveData data) { Data = data; } }
    public readonly struct SaveLoaded { public readonly SaveData Data; public SaveLoaded(SaveData data) { Data = data; } }
    public readonly struct CheckpointReached
    {
        public readonly string Id; public readonly Vector3 Position; public readonly Quaternion Rotation;
        public CheckpointReached(string id, Vector3 position, Quaternion rotation) { Id = id; Position = position; Rotation = rotation; }
    }
    public readonly struct SceneTransitionRequested
    {
        public readonly string ScenePath; public readonly int Dungeon; public readonly Action<bool> Completed;
        public SceneTransitionRequested(string scenePath, int dungeon, Action<bool> completed = null)
        { ScenePath = scenePath; Dungeon = dungeon; Completed = completed; }
    }
    public readonly struct DungeonEntered { public readonly int Id; public DungeonEntered(int id) { Id = id; } }
    public readonly struct SceneTransitionStarted { }
    public readonly struct SceneTransitionFinished
    {
        public readonly string ScenePath; public readonly int Dungeon; public readonly bool Success;
        public SceneTransitionFinished(string path, int dungeon, bool success) { ScenePath = path; Dungeon = dungeon; Success = success; }
    }
    public readonly struct InputLockChanged
    {
        public readonly object Owner; public readonly bool Locked;
        public InputLockChanged(object owner, bool locked) { Owner = owner; Locked = locked; }
    }
    public readonly struct EntityDamaged { public readonly BaseEntity Entity; public readonly DamageInfo Damage; public EntityDamaged(BaseEntity entity, DamageInfo damage) { Entity = entity; Damage = damage; } }
    public readonly struct EntityDied { public readonly BaseEntity Entity; public EntityDied(BaseEntity entity) { Entity = entity; } }
    public readonly struct HealthChanged { public readonly BaseEntity Entity; public readonly float Value, Max; public HealthChanged(BaseEntity entity, float value, float max) { Entity = entity; Value = value; Max = max; } }
    public readonly struct EntitySpawned { public readonly BaseEntity Entity; public EntitySpawned(BaseEntity entity) { Entity = entity; } }
    public readonly struct SpawnEntityRequested
    {
        public readonly EntityDefinition Definition; public readonly Vector3 Position; public readonly Quaternion Rotation; public readonly Transform Parent; public readonly Action<BaseEntity> Completed;
        public SpawnEntityRequested(EntityDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null, Action<BaseEntity> completed = null)
        { Definition = definition; Position = position; Rotation = rotation; Parent = parent; Completed = completed; }
    }
    public readonly struct NoiseEmitted
    {
        public readonly Vector3 Position; public readonly float Radius; public readonly GameObject Source;
        public NoiseEmitted(Vector3 position, float radius, GameObject source) { Position = position; Radius = radius; Source = source; }
    }
    public readonly struct SpawnEntityByIdRequested
    {
        public readonly EntityId Id;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Transform Parent;
        public SpawnEntityByIdRequested(EntityId id, Vector3 position, Quaternion rotation, Transform parent = null)
        { Id = id; Position = position; Rotation = rotation; Parent = parent; }
    }
    public readonly struct PerceptionChanged
    {
        public readonly GameObject Observer;
        public readonly Transform Target;
        public readonly Vector3 Position;
        public readonly bool Visual;
        public PerceptionChanged(GameObject observer, Transform target, Vector3 position, bool visual)
        { Observer = observer; Target = target; Position = position; Visual = visual; }
    }
    public readonly struct PlayerStatsChanged
    {
        public readonly GameObject Player; public readonly float San, MaxSan, Stamina, MaxStamina;
        public PlayerStatsChanged(GameObject player, float san, float maxSan, float stamina, float maxStamina)
        { Player = player; San = san; MaxSan = maxSan; Stamina = stamina; MaxStamina = maxStamina; }
    }
    public readonly struct LowSanChanged { public readonly bool Active; public LowSanChanged(bool active) { Active = active; } }
    public readonly struct InteractionPromptChanged { public readonly string Text; public InteractionPromptChanged(string text) { Text = text; } }
    public readonly struct Notification { public readonly string Text; public Notification(string text) { Text = text; } }
    public readonly struct PanelRequested { public readonly string Id; public readonly bool Open; public PanelRequested(string id, bool open) { Id = id; Open = open; } }
    public readonly struct InventoryChanged { public readonly string[] ItemIds; public InventoryChanged(string[] ids) { ItemIds = ids; } }
    public readonly struct ItemPickupRequested
    {
        public readonly GameObject Actor; public readonly string ItemId; public readonly Action<bool> Completed;
        public ItemPickupRequested(GameObject actor, string id, Action<bool> completed) { Actor = actor; ItemId = id; Completed = completed; }
    }
    public readonly struct UseItemRequested { public readonly int Slot; public UseItemRequested(int slot) { Slot = slot; } }
    public readonly struct ClueRead { public readonly string Id, Text; public ClueRead(string id, string text) { Id = id; Text = text; } }
    public readonly struct MechanismActivated { public readonly string Id; public MechanismActivated(string id) { Id = id; } }
    public readonly struct PlaySoundRequested
    {
        public readonly AudioClip Clip; public readonly Vector3 Position; public readonly float Volume, SpatialBlend;
        public PlaySoundRequested(AudioClip clip, Vector3 position, float volume = 1, float spatialBlend = 1)
        { Clip = clip; Position = position; Volume = volume; SpatialBlend = spatialBlend; }
    }
    public readonly struct BgmRequested { public readonly AudioClip Clip; public BgmRequested(AudioClip clip) { Clip = clip; } }
    public readonly struct CameraShakeRequested { public readonly float Strength, Duration; public CameraShakeRequested(float strength, float duration) { Strength = strength; Duration = duration; } }
    public readonly struct DialogueRequested { public readonly DialogueAsset Dialogue; public DialogueRequested(DialogueAsset dialogue) { Dialogue = dialogue; } }
    public readonly struct DialogueLineChanged { public readonly DialogueNode Node; public DialogueLineChanged(DialogueNode node) { Node = node; } }
    public readonly struct DialogueChoiceRequested { public readonly int Index; public DialogueChoiceRequested(int index) { Index = index; } }
    public readonly struct DialogueEnded { }
}
