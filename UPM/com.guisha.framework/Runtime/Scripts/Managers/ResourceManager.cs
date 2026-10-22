using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using System.Linq; // 引入 System.Linq 命名空间，使用其中的类型与 API。
using System.Threading; // 引入 System.Threading 命名空间，使用其中的类型与 API。
using System.Threading.Tasks; // 引入 System.Threading.Tasks 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.AddressableAssets; // 引入 UnityEngine.AddressableAssets 命名空间，使用其中的类型与 API。
using UnityEngine.ResourceManagement.AsyncOperations; // 引入 UnityEngine.ResourceManagement.AsyncOperations 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// Addressables 提供者，管理共享加载句柄和实例拥有权。
    /// </summary>
    public sealed class ResourceManager : MonoSingleton<ResourceManager>, IResourceProvider // Addressables 提供者，管理共享加载句柄和实例拥有权。
    { // 开始类型。
        /// <summary>
        /// 缓存中的加载句柄与使用者计数。
        /// </summary>
        private sealed class Entry // 缓存中的加载句柄与使用者计数。
        { // 开始类型。
            /// <summary>
            /// 保存Addressables 操作句柄。
            /// </summary>
            public AsyncOperationHandle Handle; // 声明 Addressables 操作句柄，后续初始化。
            /// <summary>
            /// 保存共享句柄引用数。
            /// </summary>
            public int Users; // 声明 共享句柄引用数，后续初始化。
        } // 结束类型。

        /// <summary>
        /// 保存缓存字典。
        /// </summary>
        private readonly Dictionary<string, Entry> cache = new Dictionary<string, Entry>(); // 创建 Dictionary<string, Entry>，保存为 缓存字典。
        /// <summary>
        /// 保存本管理器拥有的资源实例集合。
        /// </summary>
        private readonly HashSet<ResourceInstance> instances = new HashSet<ResourceInstance>(); // 创建 HashSet<ResourceInstance>，保存为 本管理器拥有的资源实例集合。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存组件生命周期取消源。
        /// </summary>
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource(); // 创建 CancellationTokenSource，保存为 组件生命周期取消源。
        /// <summary>
        /// 保存创建产品或实体的工厂。
        /// </summary>
        private EntityFactory factory; // 声明 创建产品或实体的工厂，后续初始化。
        /// <summary>
        /// 保存已释放标记。
        /// </summary>
        private bool disposed; // 声明 已释放标记，后续初始化。
        /// <summary>
        /// 先建立主单例，创建实体工厂并订阅两种实体生成请求。
        /// </summary>
        protected override void Awake() // 先建立主单例，创建实体工厂并订阅两种实体生成请求。
        { // 开始方法。
            base.Awake(); // 执行基类 Awake，保留基类初始化流程。
            if (Instance != this) // 当当前类型的主单例实例不等于this时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings"); // 把 Resources.Load<FrameworkSettings> 的返回值 保存为 框架或动画配置。
            factory = new EntityFactory(this, settings != null ? settings.entityCatalog : null); // 更新 创建产品或实体的工厂，使用 新建的 EntityFactory 对象。
            subscriptions.Add(EventBus.Subscribe<SpawnEntityRequested>(e => AsyncHelper.Forget(Spawn(e)))); // 订阅 SpawnEntityRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<SpawnEntityByIdRequested>(e => AsyncHelper.Forget(factory.SpawnAsync(e.Id, e.Position, e.Rotation, lifetime.Token, e.Parent)))); // 订阅 SpawnEntityByIdRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
        } // 结束方法。

        /// <summary>
        /// 处理定义式生成请求，成功或失败都通过 finally 回调完成结果。
        /// </summary>
        /// <param name="request">收到的请求载荷。</param>
        /// <returns>异步完成任务；应 await 或通过 AsyncHelper.Forget 观察异常。</returns>
        private async Task Spawn(SpawnEntityRequested request) // 处理定义式生成请求，成功或失败都通过 finally 回调完成结果。
        { // 开始方法。
            BaseEntity result = null; // 把 null（清空引用） 保存为 本次操作结果。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                result = await factory.SpawnAsync(request.Definition, request.Position, request.Rotation, lifetime.Token, request.Parent); // 更新 本次操作结果，使用 等待异步操作完成后的结果。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                request.Completed?.Invoke(result); // 无论生成成功或失败都通知完成；失败时 result 为 null，调用方据此归还生成配额。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 通过 Addressables Completed 回调等待完成，避开依赖 handle.Task。
        /// </summary>
        /// <param name="handle">异步操作句柄。</param>
        /// <returns>异步完成任务；应 await 或通过 AsyncHelper.Forget 观察异常。</returns>
        private static async Task Complete(AsyncOperationHandle handle) // 通过 Addressables Completed 回调等待完成，避开依赖 handle.Task。
        { // 开始方法。
            // 通过 Completed 回调等待完成，不依赖 AsyncOperationHandle.Task，也适用于 WebGL。
            if (handle.IsDone) // 当异步句柄完成状态为 true时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var completion = new TaskCompletionSource<bool>(); // 创建 TaskCompletionSource<bool>，保存为 等待操作完成的 Task 完成源。
            handle.Completed += _ => completion.TrySetResult(true); // 为 handle.Completed 注册事件回调；事件发生时执行右侧处理逻辑。
            await completion.Task; // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
        } // 结束方法。

        /// <summary>
        /// 按类型+地址合并加载；每个调用者取得独立 lease，最后一个释放后归还句柄。
        /// </summary>
        /// <typeparam name="T">要加载的 UnityEngine.Object 资产类型。</typeparam>
        /// <param name="address">Addressables 资源地址。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.AssetLease&lt;T&gt;。</returns>
        public async Task<AssetLease<T>> LoadAsync<T>(string address, CancellationToken token = default) // 按类型+地址合并加载；每个调用者取得独立 lease，最后一个释放后归还句柄。
            where T : UnityEngine.Object // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            if (disposed) // 当对象已经释放时，执行下方分支。
                throw new ObjectDisposedException(nameof(ResourceManager)); // 抛出 ObjectDisposedException，拒绝无效参数或状态。
            token.ThrowIfCancellationRequested(); // 观察取消状态，已取消时抛出取消异常。
            if (string.IsNullOrWhiteSpace(address)) // 当条件 string.IsNullOrWhiteSpace(address) 成立时，执行下方分支。
                throw new ArgumentException("Empty address.", nameof(address)); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Empty address."。
            string key = typeof(T).FullName + ":" + address; // 组合资源类型和地址作为缓存键，避免同地址不同类型混用句柄。
            if (!cache.TryGetValue(key, out var entry)) // 当条件 cache.TryGetValue(key, out var entry) 不成立时，执行下方分支。
            { // 开始条件分支。
                entry = new Entry // 更新 共享资源缓存项，使用 新建的 Entry 对象。
                { // 开始成员/集合初始化。
                    Handle = Addressables.LoadAssetAsync<T>(address) // 更新 Addressables 操作句柄，使用 Addressables.LoadAssetAsync<T> 的返回值。
                }; // 结束成员/集合初始化。
                cache.Add(key, entry); // 向 缓存字典 加入 key、entry。
            } // 结束条件分支。

            entry.Users++; // 为本次加载调用增加一份共享句柄引用。
            // 局部函数：LoadAsync 内部函数，减少本条目的 Users，并在最后一个引用释放时移除缓存/释放句柄。
            void Release() // LoadAsync 内部函数，减少本条目的 Users，并在最后一个引用释放时移除缓存/释放句柄。
            { // 开始局部函数。
                entry.Users--; // 释放当前 lease 拥有的一份引用；剩余引用归零后才能释放句柄。
                if (entry.Users == 0 && cache.TryGetValue(key, out var current) && ReferenceEquals(current, entry)) // 当（共享资源引用数等于0；且条件 cache.TryGetValue(key, out var current) 成立；且条件 ReferenceEquals(current, entry) 成立）时，执行下方分支。
                { // 开始条件分支。
                    cache.Remove(key); // 从 缓存字典 移除 key。
                    if (entry.Handle.IsValid()) // 当条件 entry.Handle.IsValid() 成立时，执行下方分支。
                        Addressables.Release(entry.Handle); // 释放 Addressables 操作句柄的一份拥有权。
                } // 结束条件分支。
            } // 结束局部函数。

            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                await Complete(entry.Handle); // 等待底层共享加载完成；逻辑取消仍需等句柄完成后配对清理。
                token.ThrowIfCancellationRequested(); // 观察取消状态，已取消时抛出取消异常。
                if (disposed) // 当对象已经释放时，执行下方分支。
                    throw new ObjectDisposedException(nameof(ResourceManager)); // 抛出 ObjectDisposedException，拒绝无效参数或状态。
                if (entry.Handle.Status != AsyncOperationStatus.Succeeded) // 当entry.Handle.Status不等于AsyncOperationStatus.Succeeded时，执行下方分支。
                    throw entry.Handle.OperationException ?? new InvalidOperationException("Addressables load failed: " + address); // 抛出当前操作的失败异常，交给调用方处理。
                return new AssetLease<T>((T)entry.Handle.Result, Release); // 返回包含资产与 Release 回调的独立 lease，让调用方显式拥有一份引用。
            } // 结束受异常保护的操作。
            catch // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                Release(); // LoadAsync 内部函数，减少本条目的 Users，并在最后一个引用释放时移除缓存/释放句柄。
                throw; // 重新抛出当前异常，让上层感知失败并保留原始堆栈。
            } // 结束异常处理。
        } // 结束方法。

        /// <summary>
        /// 使用 Addressables 实例化，成功后绑定 InstanceLifetime 并返回 ResourceInstance，失败/取消时配对释放。
        /// </summary>
        /// <param name="address">Addressables 资源地址。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="rotation">世界旋转。</param>
        /// <param name="parent">父级 Transform。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <returns>异步完成任务；完成后得到 Guisha.Framework.ResourceInstance。</returns>
        public async Task<ResourceInstance> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, CancellationToken token = default) // 使用 Addressables 实例化，成功后绑定 InstanceLifetime 并返回 ResourceInstance，失败/取消时配对释放。
        { // 开始方法。
            if (disposed) // 当对象已经释放时，执行下方分支。
                throw new ObjectDisposedException(nameof(ResourceManager)); // 抛出 ObjectDisposedException，拒绝无效参数或状态。
            token.ThrowIfCancellationRequested(); // 观察取消状态，已取消时抛出取消异常。
            var handle = Addressables.InstantiateAsync(address, position, rotation, parent, true); // 把 Addressables.InstantiateAsync 的返回值 保存为 异步操作句柄。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                await Complete(handle); // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
                token.ThrowIfCancellationRequested(); // 观察取消状态，已取消时抛出取消异常。
                if (disposed) // 当对象已经释放时，执行下方分支。
                    throw new ObjectDisposedException(nameof(ResourceManager)); // 抛出 ObjectDisposedException，拒绝无效参数或状态。
                if (handle.Status != AsyncOperationStatus.Succeeded) // 当异步操作状态不等于AsyncOperationStatus.Succeeded时，执行下方分支。
                    throw handle.OperationException ?? new InvalidOperationException("Instantiate failed: " + address); // 抛出当前操作的失败异常，交给调用方处理。
                ResourceInstance lease = null; // 把 null（清空引用） 保存为 资源拥有权令牌。
                lease = new ResourceInstance(handle.Result, () => // 更新 资源拥有权令牌，使用 新建的 ResourceInstance 对象。
                { // 开始回调。
                    instances.Remove(lease); // 从 本管理器拥有的资源实例集合 移除 lease。
                    if (handle.IsValid()) // 当条件 handle.IsValid() 成立时，执行下方分支。
                        Addressables.ReleaseInstance(handle); // 释放 Addressables 创建的实例及其拥有权。
                }); // 结束回调。
                instances.Add(lease); // 向 本管理器拥有的资源实例集合 加入 lease。
                handle.Result.AddComponent<InstanceLifetime>().Bind(lease); // 将实例 lease 绑定到 GameObject 销毁事件，场景卸载时可自动释放实例。
                return lease; // 返回 资源拥有权令牌。
            } // 结束受异常保护的操作。
            catch // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                if (handle.IsValid()) // 当条件 handle.IsValid() 成立时，执行下方分支。
                { // 开始条件分支。
                    if (handle.Status == AsyncOperationStatus.Succeeded) // 当异步操作状态等于AsyncOperationStatus.Succeeded时，执行下方分支。
                        Addressables.ReleaseInstance(handle); // 释放 Addressables 创建的实例及其拥有权。
                    else // 前面的条件不成立时，执行此备用分支。
                        Addressables.Release(handle); // 释放 Addressables 操作句柄的一份拥有权。
                } // 结束条件分支。

                throw; // 重新抛出当前异常，让上层感知失败并保留原始堆栈。
            } // 结束异常处理。
        } // 结束方法。

        /// <summary>
        /// 主实例停止接收请求、取消异步工作并释放已生成实例，最后清理单例引用。
        /// </summary>
        protected override void OnDestroy() // 主实例停止接收请求、取消异步工作并释放已生成实例，最后清理单例引用。
        { // 开始方法。
            if (Instance == this) // 当当前组件就是主单例时，执行下方分支。
            { // 开始条件分支。
                disposed = true; // 标记资源服务已经销毁，后续完成的异步操作进入失败清理路径。
                lifetime.Cancel(); // 通知组件拥有的异步工作取消。
                subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
                foreach (var instance in instances.ToArray()) // 遍历 instances.ToArray()，每次把一个元素保存为 instance。
                    instance.Dispose(); // 清空 GameObject 并幂等执行实例释放回调。
            // 进行中的加载保留句柄直到完成；已经发出的 lease 通过各自释放回调归还引用。
            } // 结束条件分支。

            lifetime.Dispose(); // 释放 组件生命周期取消源 持有的资源或注册；具体拥有权由该对象负责。
            base.OnDestroy(); // 执行基类销毁清理，保留单例引用等释放约定。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
