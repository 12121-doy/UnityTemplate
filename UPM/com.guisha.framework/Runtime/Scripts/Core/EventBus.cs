using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    // 同步主线程事件总线；组件在 OnDisable 中释放订阅。
    /// <summary>
    /// 主线程同步事件总线，用类型区分频道，以订阅令牌管理生命周期。
    /// </summary>
    public static class EventBus // 主线程同步事件总线，用类型区分频道，以订阅令牌管理生命周期。
    { // 开始类型。
        /// <summary>
        /// 事件频道中的回调与活动状态记录。
        /// </summary>
        private sealed class Listener // 事件频道中的回调与活动状态记录。
        { // 开始类型。
            /// <summary>
            /// 保存事件回调。
            /// </summary>
            public Delegate Callback; // 声明 事件回调，后续初始化。
            /// <summary>
            /// 保存监听记录是否仍然有效。
            /// </summary>
            public bool Active = true; // 把 true（启用/满足） 保存为 监听记录是否仍然有效。
        } // 结束类型。

        /// <summary>
        /// 可幂等释放的注册/订阅令牌。
        /// </summary>
        private sealed class Token : IDisposable // 可幂等释放的注册/订阅令牌。
        { // 开始类型。
            /// <summary>
            /// 保存释放对象的回调。
            /// </summary>
            private Action release; // 声明 释放对象的回调，后续初始化。
            /// <summary>
            /// 构造 Token，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="release">释放对象的回调；由调用方注入具体行为。</param>
            public Token(Action release) // 构造 Token，初始化其依赖与状态。
            { // 开始构造函数。
                this.release = release; // 更新 释放对象的回调，使用 释放对象的回调。
            } // 结束构造函数。

            /// <summary>
            /// 幂等释放自身持有的注册、订阅或资源。
            /// </summary>
            public void Dispose() // 幂等释放自身持有的注册、订阅或资源。
            { // 开始方法。
                var action = release; // 把 释放对象的回调 保存为 操作委托。
                release = null; // 清空 释放对象的回调 引用，表示当前不再持有该对象。
                action?.Invoke(); // 仅在 操作委托 非空时，执行 操作委托 回调；仅在回调非空时调用。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 保存按事件类型分组的订阅表。
        /// </summary>
        private static readonly Dictionary<Type, List<Listener>> Channels = new Dictionary<Type, List<Listener>>(); // 创建 Dictionary<Type, List<Listener>>，保存为 按事件类型分组的订阅表。
        /// <summary>
        /// 保存事件异常处理器。
        /// </summary>
        public static Action<Exception> ExceptionHandler = Debug.LogException; // 把 Debug.LogException 保存为 事件异常处理器。
        /// <summary>
        /// 将回调加入类型频道，返回独立订阅令牌；释放令牌可退订，重复回调也各自拥有令牌。
        /// </summary>
        /// <typeparam name="T">事件载荷类型；相同 T 的发布和订阅共享一个频道。</typeparam>
        /// <param name="callback">完成时执行的回调；由调用方注入具体行为。</param>
        /// <returns>注册/订阅令牌；不再需要时调用 Dispose 释放。</returns>
        public static IDisposable Subscribe<T>(Action<T> callback) // 将回调加入类型频道，返回独立订阅令牌；释放令牌可退订，重复回调也各自拥有令牌。
        { // 开始方法。
            if (callback == null) // 当完成时执行的回调为空时，执行下方分支。
                throw new ArgumentNullException(nameof(callback)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            if (!Channels.TryGetValue(typeof(T), out var list)) // 当条件 Channels.TryGetValue(typeof(T), out var list) 不成立时，执行下方分支。
                Channels.Add(typeof(T), list = new List<Listener>()); // 向 按事件类型分组的订阅表 加入 typeof(T)、list = new List<Listener>()。
            var listener = new Listener // 创建 Listener，保存为 当前事件监听记录。
            { // 开始成员/集合初始化。
                Callback = callback // 更新 事件回调，使用 完成时执行的回调。
            }; // 结束成员/集合初始化。
            list.Add(listener); // 向 list 加入 listener。
            return new Token(() => // 返回 新建的 Token 对象。
            { // 开始回调。
                listener.Active = false; // 更新 listener.Active，使用 false（关闭/不满足）。
                list.Remove(listener); // 从 list 移除 listener。
            }); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 对频道快照同步派发，支持重入及派发中增删订阅；逐个隔离监听器异常。
        /// </summary>
        /// <typeparam name="T">事件载荷类型；相同 T 的发布和订阅共享一个频道。</typeparam>
        /// <param name="message">消息或事件载荷。</param>
        public static void Publish<T>(T message) // 对频道快照同步派发，支持重入及派发中增删订阅；逐个隔离监听器异常。
        { // 开始方法。
            if (!Channels.TryGetValue(typeof(T), out var list)) // 当条件 Channels.TryGetValue(typeof(T), out var list) 不成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            // 使用订阅快照，允许事件派发期间嵌套发布以及增加/取消订阅。
            var snapshot = list.ToArray(); // 复制当前监听器列表，隔离派发中增删订阅对本次遍历的影响。
            foreach (var listener in snapshot) // 遍历 所属 store 的状态快照，每次把一个元素保存为 listener。
            { // 开始遍历循环。
                if (!listener.Active) // 当条件 listener.Active 不成立时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                try // 进入异常保护范围；失败时由 catch/finally 配对清理。
                { // 开始受异常保护的操作。
                    ((Action<T>)listener.Callback)(message); // 按事件类型还原监听器委托，同步执行并传入当前载荷。
                } // 结束受异常保护的操作。
                catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
                { // 开始异常处理。
                    ExceptionHandler?.Invoke(error); // 仅在 事件异常处理器 非空时，执行 事件异常处理器 回调；仅在回调非空时调用。
                } // 结束异常处理。
            } // 结束遍历循环。
        } // 结束方法。

        /// <summary>
        /// 使已有监听记录失效并清空全部频道，连当前派发快照中的后续记录也会跳过。
        /// </summary>
        public static void Clear() // 使已有监听记录失效并清空全部频道，连当前派发快照中的后续记录也会跳过。
        { // 开始方法。
            foreach (var channel in Channels.Values) // 遍历 Channels.Values，每次把一个元素保存为 channel。
                foreach (var listener in channel) // 遍历 channel，每次把一个元素保存为 listener。
                    listener.Active = false; // 更新 listener.Active，使用 false（关闭/不满足）。
            Channels.Clear(); // 清空 按事件类型分组的订阅表 中保存的记录。
        } // 结束方法。

        /// <summary>
        /// SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 指定由 Unity 在配置的运行时初始化阶段自动调用。
        private static void Reset() // SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        { // 开始方法。
            Clear(); // 使已有监听记录失效并清空全部频道，连当前派发快照中的后续记录也会跳过。
            ExceptionHandler = Debug.LogException; // 设置事件监听异常的统一处理入口；值来自 Debug.LogException。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 保存一组 IDisposable，并在禁用或销毁时统一释放；释放后可再次收集订阅。
    /// </summary>
    public sealed class SubscriptionBag : IDisposable // 保存一组 IDisposable，并在禁用或销毁时统一释放；释放后可再次收集订阅。
    { // 开始类型。
        /// <summary>
        /// 保存背包物品列表。
        /// </summary>
        private readonly List<IDisposable> items = new List<IDisposable>(); // 创建 List<IDisposable>，保存为 背包物品列表。
        /// <summary>
        /// 把参数加入当前容器。
        /// </summary>
        /// <param name="item">当前对象。</param>
        public void Add(IDisposable item) // 把参数加入当前容器。
        { // 开始方法。
            if (item != null) // 当当前对象不为空时，执行下方分支。
                items.Add(item); // 向 背包物品列表 加入 item。
        } // 结束方法。

        /// <summary>
        /// 幂等释放自身持有的注册、订阅或资源。
        /// </summary>
        public void Dispose() // 幂等释放自身持有的注册、订阅或资源。
        { // 开始方法。
            foreach (var item in items) // 遍历 背包物品列表，每次把一个元素保存为 item。
                item.Dispose(); // 释放 当前对象 持有的资源或注册；具体拥有权由该对象负责。
            items.Clear(); // 清空 背包物品列表 中保存的记录。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
