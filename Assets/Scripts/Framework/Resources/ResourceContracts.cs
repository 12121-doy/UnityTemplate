using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Threading; // 引入 System.Threading 命名空间，使用其中的类型与 API。
using System.Threading.Tasks; // 引入 System.Threading.Tasks 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 资源资产的拥有权令牌，Dispose 后释放一份引用。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class AssetLease<T> : IDisposable where T : UnityEngine.Object // 资源资产的拥有权令牌，Dispose 后释放一份引用。
    { // 开始类型。
        /// <summary>
        /// 保存释放对象的回调。
        /// </summary>
        private Action release; // 声明 释放对象的回调，后续初始化。
        /// <summary>
        /// 获取已加载资源。
        /// </summary>
        public T Asset { get; private set; } // 获取已加载资源。

        /// <summary>
        /// 构造 AssetLease，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="asset">已经加载的 Unity 资产。</param>
        /// <param name="release">释放对象的回调；由调用方注入具体行为。</param>
        public AssetLease(T asset, Action release) // 构造 AssetLease，初始化其依赖与状态。
        { // 开始构造函数。
            Asset = asset; // 更新 已加载资源，使用 已经加载的 Unity 资产。
            this.release = release; // 更新 释放对象的回调，使用 释放对象的回调。
        } // 结束构造函数。

        /// <summary>
        /// 清空 Asset 并幂等执行释放回调。
        /// </summary>
        public void Dispose() // 清空 Asset 并幂等执行释放回调。
        { // 开始方法。
            var action = release; // 把 释放对象的回调 保存为 操作委托。
            release = null; // 清空 释放对象的回调 引用，表示当前不再持有该对象。
            Asset = null; // 清空 已加载资源 引用，表示当前不再持有该对象。
            action?.Invoke(); // 仅在 操作委托 非空时，执行 操作委托 回调；仅在回调非空时调用。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 可释放的 Addressables 实例拥有权对象。
    /// </summary>
    public sealed class ResourceInstance : IDisposable // 可释放的 Addressables 实例拥有权对象。
    { // 开始类型。
        /// <summary>
        /// 保存释放对象的回调。
        /// </summary>
        private Action release; // 声明 释放对象的回调，后续初始化。
        /// <summary>
        /// 获取实例化对象。
        /// </summary>
        public GameObject GameObject { get; private set; } // 获取实例化对象。

        /// <summary>
        /// 构造 ResourceInstance，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="instance">持有的对象实例。</param>
        /// <param name="release">释放对象的回调；由调用方注入具体行为。</param>
        public ResourceInstance(GameObject instance, Action release) // 构造 ResourceInstance，初始化其依赖与状态。
        { // 开始构造函数。
            GameObject = instance; // 更新 实例化对象，使用 持有的对象实例。
            this.release = release; // 更新 释放对象的回调，使用 释放对象的回调。
        } // 结束构造函数。

        /// <summary>
        /// 清空 GameObject 并幂等执行实例释放回调。
        /// </summary>
        public void Dispose() // 清空 GameObject 并幂等执行实例释放回调。
        { // 开始方法。
            var action = release; // 把 释放对象的回调 保存为 操作委托。
            release = null; // 清空 释放对象的回调 引用，表示当前不再持有该对象。
            GameObject = null; // 清空 实例化对象 引用，表示当前不再持有该对象。
            action?.Invoke(); // 仅在 操作委托 非空时，执行 操作委托 回调；仅在回调非空时调用。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 声明资产加载和预制体实例化异步契约。
    /// </summary>
    public interface IResourceProvider // 声明资产加载和预制体实例化异步契约。
    { // 开始接口。
        /// <summary>
        /// 加载并返回可释放的资产 lease。
        /// </summary>
        /// <typeparam name="T">要加载的 UnityEngine.Object 资产类型。</typeparam>
        /// <param name="address">Addressables 资源地址。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.AssetLease&lt;T&gt;。</returns>
        Task<AssetLease<T>> LoadAsync<T>(string address, CancellationToken token = default) // 加载并返回可释放的资产 lease。
            where T : UnityEngine.Object; // 限定泛型类型必须满足此处的继承、接口或构造约束。
        /// <summary>
        /// 实例化并返回可释放的资源实例。
        /// </summary>
        /// <param name="address">Addressables 资源地址。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.ResourceInstance。</returns>
        Task<ResourceInstance> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, CancellationToken token = default); // 实例化并返回可释放的资源实例。
    } // 结束接口。
} // 结束命名空间。
