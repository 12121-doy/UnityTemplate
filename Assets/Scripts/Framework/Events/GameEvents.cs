using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 枚举定义，可选值与实际源码一致。
    /// </summary>
    public enum GameState // 枚举定义，可选值与实际源码一致。
    { // 开始枚举。
        /// <summary>
        /// 正常游玩，允许按输入锁状态操作。
        /// </summary>
        Playing, // 正常游玩，允许按输入锁状态操作。
        /// <summary>
        /// 游戏暂停，缩放时间停止。
        /// </summary>
        Paused, // 游戏暂停，缩放时间停止。
        /// <summary>
        /// 场景或存档加载中。
        /// </summary>
        Loading, // 场景或存档加载中。
        /// <summary>
        /// 本局结束，停止缩放时间模拟。
        /// </summary>
        GameOver // 本局结束，停止缩放时间模拟。
    } // 结束枚举。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct GameStateChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存当前状态。
        /// </summary>
        public readonly GameState State; // 声明 当前状态，后续初始化。
        /// <summary>
        /// 创建 GameStateChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="state">目标状态或保存的状态数据。</param>
        public GameStateChanged(GameState state) // 构造 GameStateChanged，初始化其依赖与状态。
        { // 开始构造函数。
            State = state; // 更新 当前状态，使用 目标状态或保存的状态数据。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PauseRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存暂停标记。
        /// </summary>
        public readonly bool Pause; // 声明 暂停标记，后续初始化。
        /// <summary>
        /// 创建 PauseRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="pause">是否暂停。</param>
        public PauseRequested(bool pause) // 构造 PauseRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Pause = pause; // 更新 暂停标记，使用 是否暂停。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SaveRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct LoadRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SaveCapture // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存存档数据。
        /// </summary>
        public readonly SaveData Data; // 声明 存档数据，后续初始化。
        /// <summary>
        /// 创建 SaveCapture 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="data">待保存或已读取的数据。</param>
        public SaveCapture(SaveData data) // 构造 SaveCapture，初始化其依赖与状态。
        { // 开始构造函数。
            Data = data; // 更新 存档数据，使用 待保存或已读取的数据。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SaveLoaded // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存存档数据。
        /// </summary>
        public readonly SaveData Data; // 声明 存档数据，后续初始化。
        /// <summary>
        /// 创建 SaveLoaded 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="data">待保存或已读取的数据。</param>
        public SaveLoaded(SaveData data) // 构造 SaveLoaded，初始化其依赖与状态。
        { // 开始构造函数。
            Data = data; // 更新 存档数据，使用 待保存或已读取的数据。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct CheckpointReached // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public readonly string Id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 保存位置或解析偏移。
        /// </summary>
        public readonly Vector3 Position; // 声明 位置或解析偏移，后续初始化。
        /// <summary>
        /// 保存世界旋转。
        /// </summary>
        public readonly Quaternion Rotation; // 声明 世界旋转，后续初始化。
        /// <summary>
        /// 创建 CheckpointReached 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        public CheckpointReached(string id, Vector3 position, Quaternion rotation) // 构造 CheckpointReached，初始化其依赖与状态。
        { // 开始构造函数。
            Id = id; // 更新 稳定标识，使用 稳定标识。
            Position = position; // 更新 位置或解析偏移，使用 世界位置。
            Rotation = rotation; // 更新 世界旋转，使用 世界旋转。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SceneTransitionRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存场景资源路径。
        /// </summary>
        public readonly string ScenePath; // 声明 场景资源路径，后续初始化。
        /// <summary>
        /// 保存副本编号。
        /// </summary>
        public readonly int Dungeon; // 声明 副本编号，后续初始化。
        /// <summary>
        /// 保存完成结果回调。
        /// </summary>
        public readonly Action<bool> Completed; // 声明 完成结果回调，后续初始化。
        /// <summary>
        /// 创建 SceneTransitionRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="scenePath">场景的完整资源路径。</param>
        /// <param name="dungeon">当前副本编号。</param>
        /// <param name="completed">结果回调；由调用方注入具体行为。</param>
        public SceneTransitionRequested(string scenePath, int dungeon, Action<bool> completed = null) // 构造 SceneTransitionRequested，初始化其依赖与状态。
        { // 开始构造函数。
            ScenePath = scenePath; // 更新 场景资源路径，使用 场景的完整资源路径。
            Dungeon = dungeon; // 更新 副本编号，使用 当前副本编号。
            Completed = completed; // 更新 完成结果回调，使用 结果回调。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct DungeonEntered // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public readonly int Id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 创建 DungeonEntered 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        public DungeonEntered(int id) // 构造 DungeonEntered，初始化其依赖与状态。
        { // 开始构造函数。
            Id = id; // 更新 稳定标识，使用 稳定标识。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SceneTransitionStarted // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SceneTransitionFinished // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存场景资源路径。
        /// </summary>
        public readonly string ScenePath; // 声明 场景资源路径，后续初始化。
        /// <summary>
        /// 保存副本编号。
        /// </summary>
        public readonly int Dungeon; // 声明 副本编号，后续初始化。
        /// <summary>
        /// 保存操作成功标记。
        /// </summary>
        public readonly bool Success; // 声明 操作成功标记，后续初始化。
        /// <summary>
        /// 创建 SceneTransitionFinished 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="path">文件或资源路径。</param>
        /// <param name="dungeon">当前副本编号。</param>
        /// <param name="success">是否成功。</param>
        public SceneTransitionFinished(string path, int dungeon, bool success) // 构造 SceneTransitionFinished，初始化其依赖与状态。
        { // 开始构造函数。
            ScenePath = path; // 更新 场景资源路径，使用 文件或资源路径。
            Dungeon = dungeon; // 更新 副本编号，使用 当前副本编号。
            Success = success; // 更新 操作成功标记，使用 是否成功。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct InputLockChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存拥有者标识。
        /// </summary>
        public readonly object Owner; // 声明 拥有者标识，后续初始化。
        /// <summary>
        /// 保存输入锁开关。
        /// </summary>
        public readonly bool Locked; // 声明 输入锁开关，后续初始化。
        /// <summary>
        /// 创建 InputLockChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="owner">本次操作的拥有者。</param>
        /// <param name="locked">输入是否锁定。</param>
        public InputLockChanged(object owner, bool locked) // 构造 InputLockChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Owner = owner; // 更新 拥有者标识，使用 本次操作的拥有者。
            Locked = locked; // 更新 输入锁开关，使用 输入是否锁定。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct EntityDamaged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存事件对应的实体。
        /// </summary>
        public readonly BaseEntity Entity; // 声明 事件对应的实体，后续初始化。
        /// <summary>
        /// 保存Damage。
        /// </summary>
        public readonly DamageInfo Damage; // 声明 Damage，后续初始化。
        /// <summary>
        /// 创建 EntityDamaged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="entity">当前实体组件。</param>
        /// <param name="damage">伤害数据或承伤倍率。</param>
        public EntityDamaged(BaseEntity entity, DamageInfo damage) // 构造 EntityDamaged，初始化其依赖与状态。
        { // 开始构造函数。
            Entity = entity; // 更新 事件对应的实体，使用 当前实体组件。
            Damage = damage; // 更新 Damage，使用 伤害数据或承伤倍率。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct EntityDied // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存事件对应的实体。
        /// </summary>
        public readonly BaseEntity Entity; // 声明 事件对应的实体，后续初始化。
        /// <summary>
        /// 创建 EntityDied 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="entity">当前实体组件。</param>
        public EntityDied(BaseEntity entity) // 构造 EntityDied，初始化其依赖与状态。
        { // 开始构造函数。
            Entity = entity; // 更新 事件对应的实体，使用 当前实体组件。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct HealthChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存事件对应的实体。
        /// </summary>
        public readonly BaseEntity Entity; // 声明 事件对应的实体，后续初始化。
        /// <summary>
        /// 保存封装的数据值、上限。
        /// </summary>
        public readonly float Value, Max; // 声明 封装的数据值，后续初始化；声明 上限，后续初始化。
        /// <summary>
        /// 创建 HealthChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="entity">当前实体组件。</param>
        /// <param name="value">当前处理值。</param>
        /// <param name="max">最大值。</param>
        public HealthChanged(BaseEntity entity, float value, float max) // 构造 HealthChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Entity = entity; // 更新 事件对应的实体，使用 当前实体组件。
            Value = value; // 更新 封装的数据值，使用 当前处理值。
            Max = max; // 更新 上限，使用 最大值。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct EntitySpawned // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存事件对应的实体。
        /// </summary>
        public readonly BaseEntity Entity; // 声明 事件对应的实体，后续初始化。
        /// <summary>
        /// 创建 EntitySpawned 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="entity">当前实体组件。</param>
        public EntitySpawned(BaseEntity entity) // 构造 EntitySpawned，初始化其依赖与状态。
        { // 开始构造函数。
            Entity = entity; // 更新 事件对应的实体，使用 当前实体组件。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SpawnEntityRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存实体配置。
        /// </summary>
        public readonly EntityDefinition Definition; // 声明 实体配置，后续初始化。
        /// <summary>
        /// 保存位置或解析偏移。
        /// </summary>
        public readonly Vector3 Position; // 声明 位置或解析偏移，后续初始化。
        /// <summary>
        /// 保存世界旋转。
        /// </summary>
        public readonly Quaternion Rotation; // 声明 世界旋转，后续初始化。
        /// <summary>
        /// 保存父节点。
        /// </summary>
        public readonly Transform Parent; // 声明 父节点，后续初始化。
        /// <summary>
        /// 保存完成结果回调。
        /// </summary>
        public readonly Action<BaseEntity> Completed; // 声明 完成结果回调，后续初始化。
        /// <summary>
        /// 创建 SpawnEntityRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="definition">配置定义。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <param name="completed">结果回调；由调用方注入具体行为。</param>
        public SpawnEntityRequested(EntityDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null, Action<BaseEntity> completed = null) // 构造 SpawnEntityRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Definition = definition; // 更新 实体配置，使用 配置定义。
            Position = position; // 更新 位置或解析偏移，使用 世界位置。
            Rotation = rotation; // 更新 世界旋转，使用 世界旋转。
            Parent = parent; // 更新 父节点，使用 父级 Transform。
            Completed = completed; // 更新 完成结果回调，使用 结果回调。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct NoiseEmitted // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存位置或解析偏移。
        /// </summary>
        public readonly Vector3 Position; // 声明 位置或解析偏移，后续初始化。
        /// <summary>
        /// 保存范围半径。
        /// </summary>
        public readonly float Radius; // 声明 范围半径，后续初始化。
        /// <summary>
        /// 保存来源对象或音源。
        /// </summary>
        public readonly GameObject Source; // 声明 来源对象或音源，后续初始化。
        /// <summary>
        /// 创建 NoiseEmitted 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="position">世界位置。</param>
        /// <param name="radius">碰撞检测半径。</param>
        /// <param name="source">原始来源。</param>
        public NoiseEmitted(Vector3 position, float radius, GameObject source) // 构造 NoiseEmitted，初始化其依赖与状态。
        { // 开始构造函数。
            Position = position; // 更新 位置或解析偏移，使用 世界位置。
            Radius = radius; // 更新 范围半径，使用 碰撞检测半径。
            Source = source; // 更新 来源对象或音源，使用 原始来源。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct SpawnEntityByIdRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public readonly EntityId Id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 保存位置或解析偏移。
        /// </summary>
        public readonly Vector3 Position; // 声明 位置或解析偏移，后续初始化。
        /// <summary>
        /// 保存世界旋转。
        /// </summary>
        public readonly Quaternion Rotation; // 声明 世界旋转，后续初始化。
        /// <summary>
        /// 保存父节点。
        /// </summary>
        public readonly Transform Parent; // 声明 父节点，后续初始化。
        /// <summary>
        /// 创建 SpawnEntityByIdRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <param name="parent">父级 Transform。</param>
        public SpawnEntityByIdRequested(EntityId id, Vector3 position, Quaternion rotation, Transform parent = null) // 构造 SpawnEntityByIdRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Id = id; // 更新 稳定标识，使用 稳定标识。
            Position = position; // 更新 位置或解析偏移，使用 世界位置。
            Rotation = rotation; // 更新 世界旋转，使用 世界旋转。
            Parent = parent; // 更新 父节点，使用 父级 Transform。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PerceptionChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存感知者。
        /// </summary>
        public readonly GameObject Observer; // 声明 感知者，后续初始化。
        /// <summary>
        /// 保存目标实体。
        /// </summary>
        public readonly Transform Target; // 声明 目标实体，后续初始化。
        /// <summary>
        /// 保存位置或解析偏移。
        /// </summary>
        public readonly Vector3 Position; // 声明 位置或解析偏移，后续初始化。
        /// <summary>
        /// 保存Visual。
        /// </summary>
        public readonly bool Visual; // 声明 Visual，后续初始化。
        /// <summary>
        /// 创建 PerceptionChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="observer">感知者对象。</param>
        /// <param name="target">目标对象。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="visual">表现对象。</param>
        public PerceptionChanged(GameObject observer, Transform target, Vector3 position, bool visual) // 构造 PerceptionChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Observer = observer; // 更新 感知者，使用 observer。
            Target = target; // 更新 目标实体，使用 目标对象。
            Position = position; // 更新 位置或解析偏移，使用 世界位置。
            Visual = visual; // 更新 Visual，使用 表现对象。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PlayerStatsChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存玩家对象。
        /// </summary>
        public readonly GameObject Player; // 声明 玩家对象，后续初始化。
        /// <summary>
        /// 保存当前理智、理智上限、当前体力、体力上限。
        /// </summary>
        public readonly float San, MaxSan, Stamina, MaxStamina; // 声明 当前理智，后续初始化；声明 理智上限，后续初始化；声明 当前体力，后续初始化；声明 体力上限，后续初始化。
        /// <summary>
        /// 创建 PlayerStatsChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="player">玩家组件或玩家对象。</param>
        /// <param name="san">理智值或消耗倍率。</param>
        /// <param name="maxSan">理智上限。</param>
        /// <param name="stamina">体力值或消耗倍率。</param>
        /// <param name="maxStamina">体力上限。</param>
        public PlayerStatsChanged(GameObject player, float san, float maxSan, float stamina, float maxStamina) // 构造 PlayerStatsChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Player = player; // 更新 玩家对象，使用 玩家组件或玩家对象。
            San = san; // 更新 当前理智，使用 理智值或消耗倍率。
            MaxSan = maxSan; // 更新 理智上限，使用 理智上限。
            Stamina = stamina; // 更新 当前体力，使用 体力值或消耗倍率。
            MaxStamina = maxStamina; // 更新 体力上限，使用 体力上限。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct LowSanChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存有效状态标记。
        /// </summary>
        public readonly bool Active; // 声明 有效状态标记，后续初始化。
        /// <summary>
        /// 创建 LowSanChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="active">活动对象或有效标记。</param>
        public LowSanChanged(bool active) // 构造 LowSanChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Active = active; // 更新 有效状态标记，使用 活动对象或有效标记。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct InteractionPromptChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存显示文本。
        /// </summary>
        public readonly string Text; // 声明 显示文本，后续初始化。
        /// <summary>
        /// 创建 InteractionPromptChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="text">正文或待解析字符串。</param>
        public InteractionPromptChanged(string text) // 构造 InteractionPromptChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Text = text; // 更新 显示文本，使用 正文或待解析字符串。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct Notification // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存显示文本。
        /// </summary>
        public readonly string Text; // 声明 显示文本，后续初始化。
        /// <summary>
        /// 创建 Notification 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="text">正文或待解析字符串。</param>
        public Notification(string text) // 构造 Notification，初始化其依赖与状态。
        { // 开始构造函数。
            Text = text; // 更新 显示文本，使用 正文或待解析字符串。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PanelRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public readonly string Id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 保存面板打开状态。
        /// </summary>
        public readonly bool Open; // 声明 面板打开状态，后续初始化。
        /// <summary>
        /// 创建 PanelRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <param name="open">是否打开面板。</param>
        public PanelRequested(string id, bool open) // 构造 PanelRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Id = id; // 更新 稳定标识，使用 稳定标识。
            Open = open; // 更新 面板打开状态，使用 是否打开面板。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct InventoryChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存背包物品标识快照。
        /// </summary>
        public readonly string[] ItemIds; // 声明 背包物品标识快照，后续初始化。
        /// <summary>
        /// 创建 InventoryChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="ids">物品或世界标记 ID 数组。</param>
        public InventoryChanged(string[] ids) // 构造 InventoryChanged，初始化其依赖与状态。
        { // 开始构造函数。
            ItemIds = ids; // 更新 背包物品标识快照，使用 物品或世界标记 ID 数组。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct ItemPickupRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存交互发起对象。
        /// </summary>
        public readonly GameObject Actor; // 声明 交互发起对象，后续初始化。
        /// <summary>
        /// 保存物品标识。
        /// </summary>
        public readonly string ItemId; // 声明 物品标识，后续初始化。
        /// <summary>
        /// 保存完成结果回调。
        /// </summary>
        public readonly Action<bool> Completed; // 声明 完成结果回调，后续初始化。
        /// <summary>
        /// 创建 ItemPickupRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <param name="id">稳定标识。</param>
        /// <param name="completed">结果回调；由调用方注入具体行为。</param>
        public ItemPickupRequested(GameObject actor, string id, Action<bool> completed) // 构造 ItemPickupRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Actor = actor; // 更新 交互发起对象，使用 交互或使用物品的发起对象。
            ItemId = id; // 更新 物品标识，使用 稳定标识。
            Completed = completed; // 更新 完成结果回调，使用 结果回调。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct UseItemRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存背包槽位。
        /// </summary>
        public readonly int Slot; // 声明 背包槽位，后续初始化。
        /// <summary>
        /// 创建 UseItemRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="slot">传入的 slot，类型为 int。</param>
        public UseItemRequested(int slot) // 构造 UseItemRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Slot = slot; // 更新 背包槽位，使用 slot。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct ClueRead // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存稳定标识、显示文本。
        /// </summary>
        public readonly string Id, Text; // 声明 稳定标识，后续初始化；声明 显示文本，后续初始化。
        /// <summary>
        /// 创建 ClueRead 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <param name="text">正文或待解析字符串。</param>
        public ClueRead(string id, string text) // 构造 ClueRead，初始化其依赖与状态。
        { // 开始构造函数。
            Id = id; // 更新 稳定标识，使用 稳定标识。
            Text = text; // 更新 显示文本，使用 正文或待解析字符串。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct MechanismActivated // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        public readonly string Id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 创建 MechanismActivated 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        public MechanismActivated(string id) // 构造 MechanismActivated，初始化其依赖与状态。
        { // 开始构造函数。
            Id = id; // 更新 稳定标识，使用 稳定标识。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PlaySoundRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存音频片段。
        /// </summary>
        public readonly AudioClip Clip; // 声明 音频片段，后续初始化。
        /// <summary>
        /// 保存位置或解析偏移。
        /// </summary>
        public readonly Vector3 Position; // 声明 位置或解析偏移，后续初始化。
        /// <summary>
        /// 保存音量、2D/3D 空间混合比例。
        /// </summary>
        public readonly float Volume, SpatialBlend; // 声明 音量，后续初始化；声明 2D/3D 空间混合比例，后续初始化。
        /// <summary>
        /// 创建 PlaySoundRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="clip">音频或动画片段。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="volume">音量。</param>
        /// <param name="spatialBlend">空间音频混合比例。</param>
        public PlaySoundRequested(AudioClip clip, Vector3 position, float volume = 1, float spatialBlend = 1) // 构造 PlaySoundRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Clip = clip; // 更新 音频片段，使用 音频或动画片段。
            Position = position; // 更新 位置或解析偏移，使用 世界位置。
            Volume = volume; // 更新 音量，使用 音量。
            SpatialBlend = spatialBlend; // 更新 2D/3D 空间混合比例，使用 空间音频混合比例。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct BgmRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存音频片段。
        /// </summary>
        public readonly AudioClip Clip; // 声明 音频片段，后续初始化。
        /// <summary>
        /// 创建 BgmRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="clip">音频或动画片段。</param>
        public BgmRequested(AudioClip clip) // 构造 BgmRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Clip = clip; // 更新 音频片段，使用 音频或动画片段。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct CameraShakeRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存震动强度、持续秒数。
        /// </summary>
        public readonly float Strength, Duration; // 声明 震动强度，后续初始化；声明 持续秒数，后续初始化。
        /// <summary>
        /// 创建 CameraShakeRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="strength">强度。</param>
        /// <param name="duration">持续秒数。</param>
        public CameraShakeRequested(float strength, float duration) // 构造 CameraShakeRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Strength = strength; // 更新 震动强度，使用 强度。
            Duration = duration; // 更新 持续秒数，使用 持续秒数。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct DialogueRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存对白资产。
        /// </summary>
        public readonly DialogueAsset Dialogue; // 声明 对白资产，后续初始化。
        /// <summary>
        /// 创建 DialogueRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="dialogue">对白资产或当前对白。</param>
        public DialogueRequested(DialogueAsset dialogue) // 构造 DialogueRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Dialogue = dialogue; // 更新 对白资产，使用 对白资产或当前对白。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct DialogueLineChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存对白节点。
        /// </summary>
        public readonly DialogueNode Node; // 声明 对白节点，后续初始化。
        /// <summary>
        /// 创建 DialogueLineChanged 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="node">当前树节点或对白节点。</param>
        public DialogueLineChanged(DialogueNode node) // 构造 DialogueLineChanged，初始化其依赖与状态。
        { // 开始构造函数。
            Node = node; // 更新 对白节点，使用 当前树节点或对白节点。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct DialogueChoiceRequested // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存索引。
        /// </summary>
        public readonly int Index; // 声明 索引，后续初始化。
        /// <summary>
        /// 创建 DialogueChoiceRequested 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="index">当前索引。</param>
        public DialogueChoiceRequested(int index) // 构造 DialogueChoiceRequested，初始化其依赖与状态。
        { // 开始构造函数。
            Index = index; // 更新 索引，使用 当前索引。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct DialogueEnded // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
    } // 结束数据结构。
} // 结束命名空间。
