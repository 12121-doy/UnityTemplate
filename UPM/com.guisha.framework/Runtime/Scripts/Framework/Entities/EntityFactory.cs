using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using System.Threading; // 引入 System.Threading 命名空间，使用其中的类型与 API。
using System.Threading.Tasks; // 引入 System.Threading.Tasks 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 通过资源提供者按配置或实体 ID 异步创建实体。
    /// </summary>
    public sealed class EntityFactory // 通过资源提供者按配置或实体 ID 异步创建实体。
    { // 开始类型。
        /// <summary>
        /// 保存资源提供者。
        /// </summary>
        private readonly IResourceProvider resources; // 声明 资源提供者，后续初始化。
        /// <summary>
        /// 保存可用配置目录。
        /// </summary>
        private readonly Dictionary<EntityId, EntityDefinition> catalog = new Dictionary<EntityId, EntityDefinition>(); // 创建 Dictionary<EntityId, EntityDefinition>，保存为 可用配置目录。
        /// <summary>
        /// 构造 EntityFactory，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="resources">资源提供者。</param>
        /// <param name="definitions">实体配置列表。</param>
        public EntityFactory(IResourceProvider resources, EntityCatalog definitions = null) // 构造 EntityFactory，初始化其依赖与状态。
        { // 开始构造函数。
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources)); // 更新 资源提供者，使用 resources ?? throw new ArgumentNullException(nameof(resources))。
            if (definitions == null) // 当实体配置列表为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            foreach (var entry in definitions.definitions) // 遍历 definitions.definitions，每次把一个元素保存为 entry。
            { // 开始遍历循环。
                if (entry == null) // 当共享资源缓存项为空时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                if (catalog.ContainsKey(entry.id)) // 当条件 catalog.ContainsKey(entry.id) 成立时，执行下方分支。
                    throw new ArgumentException("Duplicate entity ID: " + entry.id); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Duplicate entity ID: " + entry.id。
                catalog.Add(entry.id, entry); // 向 可用配置目录 加入 entry.id、entry。
            } // 结束遍历循环。
        } // 结束构造函数。

        /// <summary>
        /// 按 ID 从 Catalog 解析配置，或直接用配置异步实例化；校验 BaseEntity/类型后配置并广播 EntitySpawned。
        /// </summary>
        /// <param name="id">在 EntityCatalog 中查找的实体枚举 ID。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <param name="parent">实例所属的场景内容父节点；取消或父节点失效时清理结果。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.BaseEntity。</returns>
        public Task<BaseEntity> SpawnAsync(EntityId id, Vector3 position, Quaternion rotation, CancellationToken token = default, Transform parent = null) // 按 ID 从 Catalog 解析配置，或直接用配置异步实例化；校验 BaseEntity/类型后配置并广播 EntitySpawned。
        { // 开始方法。
            if (!catalog.TryGetValue(id, out var definition)) // 当条件 catalog.TryGetValue(id, out var definition) 不成立时，执行下方分支。
                throw new KeyNotFoundException("Entity not registered: " + id); // 抛出 KeyNotFoundException，拒绝无效参数或状态；错误信息为 "Entity not registered: " + id。
            return SpawnAsync(definition, position, rotation, token, parent); // 按 ID 从 Catalog 解析配置，或直接用配置异步实例化；校验 BaseEntity/类型后配置并广播 EntitySpawned，并返回调用结果。
        } // 结束方法。

        /// <summary>
        /// 按 ID 从 Catalog 解析配置，或直接用配置异步实例化；校验 BaseEntity/类型后配置并广播 EntitySpawned。
        /// </summary>
        /// <param name="definition">实体生成配置。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <param name="parent">实例所属的场景内容父节点；取消或父节点失效时清理结果。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.BaseEntity。</returns>
        public async Task<BaseEntity> SpawnAsync(EntityDefinition definition, Vector3 position, Quaternion rotation, CancellationToken token = default, Transform parent = null) // 按 ID 从 Catalog 解析配置，或直接用配置异步实例化；校验 BaseEntity/类型后配置并广播 EntitySpawned。
        { // 开始方法。
            if (definition == null || string.IsNullOrWhiteSpace(definition.prefabAddress)) // 当（实体生成配置为空；或条件 string.IsNullOrWhiteSpace(definition.prefabAddress) 成立）时，执行下方分支。
                throw new ArgumentException("Entity definition requires an Addressables address."); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Entity definition requires an Addressables address."。
            bool hasParent = parent != null; // 把 parent != null 保存为 hasParent。
            var lease = await resources.InstantiateAsync(definition.prefabAddress, position, rotation, parent, token); // 等待异步操作并把结果保存为 资源拥有权令牌。
            if (hasParent && parent == null) // 当（hasParent为 true；且父级 Transform为空）时，执行下方分支。
            { // 开始条件分支。
                lease.Dispose(); // 清空 GameObject 并幂等执行实例释放回调。
                throw new OperationCanceledException("Spawn owner scene was unloaded."); // 抛出 OperationCanceledException，拒绝无效参数或状态；错误信息为 "Spawn owner scene was unloaded."。
            } // 结束条件分支。

            var entity = lease.GameObject.GetComponent<BaseEntity>(); // 把 lease.GameObject.GetComponent<BaseEntity> 的返回值 保存为 当前实体组件。
            if (entity == null || entity.Type != definition.type) // 当（当前实体组件为空；或entity.Type不等于definition.type）时，执行下方分支。
            { // 开始条件分支。
                lease.Dispose(); // 清空 GameObject 并幂等执行实例释放回调。
                throw new InvalidOperationException("Entity prefab missing BaseEntity or has a mismatched type."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Entity prefab missing BaseEntity or has a mismatched type."。
            } // 结束条件分支。

            entity.Configure(definition); // 绑定新的实体配置，并将生命重设为新上限。
            EventBus.Publish(new EntitySpawned(entity)); // 广播 EntitySpawned 事件，通知生成后的实体已经就绪。
            return entity; // 返回 当前实体组件。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
