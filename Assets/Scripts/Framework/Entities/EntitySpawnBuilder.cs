using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Threading; // 引入 System.Threading 命名空间，使用其中的类型与 API。
using System.Threading.Tasks; // 引入 System.Threading.Tasks 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 以链式配置包装 EntityFactory 的生成参数。
    /// </summary>
    public sealed class EntitySpawnBuilder // 以链式配置包装 EntityFactory 的生成参数。
    { // 开始类型。
        /// <summary>
        /// 保存创建产品或实体的工厂。
        /// </summary>
        private readonly EntityFactory factory; // 声明 创建产品或实体的工厂，后续初始化。
        /// <summary>
        /// 保存待生成实体配置。
        /// </summary>
        private EntityDefinition definition; // 声明 待生成实体配置，后续初始化。
        /// <summary>
        /// 保存世界位置。
        /// </summary>
        private Vector3 position; // 声明 世界位置，后续初始化。
        /// <summary>
        /// 保存世界旋转。
        /// </summary>
        private Quaternion rotation = Quaternion.identity; // 把 Quaternion.identity 保存为 世界旋转。
        /// <summary>
        /// 保存父级 Transform。
        /// </summary>
        private Transform parent; // 声明 父级 Transform，后续初始化。
        /// <summary>
        /// 构造 EntitySpawnBuilder，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="factory">创建产品或实体的工厂。</param>
        public EntitySpawnBuilder(EntityFactory factory) // 构造 EntitySpawnBuilder，初始化其依赖与状态。
        { // 开始构造函数。
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory)); // 更新 创建产品或实体的工厂，使用 factory ?? throw new ArgumentNullException(nameof(factory))。
        } // 结束构造函数。

        /// <summary>
        /// 指定实体配置并返回构建器。
        /// </summary>
        /// <param name="definition">待生成实体配置。</param>
        /// <returns>当前构建器，支持继续链式配置。</returns>
        public EntitySpawnBuilder Using(EntityDefinition definition) // 指定实体配置并返回构建器。
        { // 开始方法。
            this.definition = definition; // 更新 待生成实体配置，使用 待生成实体配置。
            return this; // 返回当前对象，支持继续链式调用。
        } // 结束方法。

        /// <summary>
        /// 设置世界位置和旋转并返回构建器。
        /// </summary>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <returns>当前构建器，支持继续链式配置。</returns>
        public EntitySpawnBuilder At(Vector3 position, Quaternion rotation) // 设置世界位置和旋转并返回构建器。
        { // 开始方法。
            this.position = position; // 更新 世界位置，使用 世界位置。
            this.rotation = rotation; // 更新 世界旋转，使用 世界旋转。
            return this; // 返回当前对象，支持继续链式调用。
        } // 结束方法。

        /// <summary>
        /// 设置所属场景父节点并返回构建器。
        /// </summary>
        /// <param name="parent">父级 Transform。</param>
        /// <returns>当前构建器，支持继续链式配置。</returns>
        public EntitySpawnBuilder Under(Transform parent) // 设置所属场景父节点并返回构建器。
        { // 开始方法。
            this.parent = parent; // 更新 父级 Transform，使用 父级 Transform。
            return this; // 返回当前对象，支持继续链式调用。
        } // 结束方法。

        /// <summary>
        /// 把已配置参数交给 EntityFactory，返回实体 Task。
        /// </summary>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.BaseEntity。</returns>
        public Task<BaseEntity> SpawnAsync(CancellationToken token = default) => factory.SpawnAsync(definition, position, rotation, token, parent); // 把已配置参数交给 EntityFactory，返回实体 Task。
    } // 结束类型。
} // 结束命名空间。
