using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    // 对象池供主线程使用；按引用身份核验拥有权，避免自定义 Equals 干扰。
    /// <summary>
    /// 按引用身份管理借出与空闲对象，检查重复/外来归还。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class ObjectPool<T> : IDisposable where T : class // 按引用身份管理借出与空闲对象，检查重复/外来归还。
    { // 开始类型。
        /// <summary>
        /// 对象池使用的引用身份比较器。
        /// </summary>
        private sealed class Identity : IEqualityComparer<T> // 对象池使用的引用身份比较器。
        { // 开始类型。
            /// <summary>
            /// 比较两个对象是否是同一引用。
            /// </summary>
            /// <param name="a">第一个比较或组合参数。</param>
            /// <param name="b">第二个比较或组合参数。</param>
            /// <returns>当前相等性比较的结果。</returns>
            public bool Equals(T a, T b) => ReferenceEquals(a, b); // 比较两个对象是否是同一引用。
            /// <summary>
            /// 取得对象的运行时引用身份哈希。
            /// </summary>
            /// <param name="value">当前处理值。</param>
            /// <returns>int 类型的处理结果。</returns>
            public int GetHashCode(T value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value); // 取得对象的运行时引用身份哈希。
        } // 结束类型。

        /// <summary>
        /// 保存空闲对象集合或待机状态。
        /// </summary>
        private readonly Stack<T> idle = new Stack<T>(); // 创建 Stack<T>，保存为 空闲对象集合或待机状态。
        /// <summary>
        /// 保存当前借出对象集合。
        /// </summary>
        private readonly HashSet<T> rented = new HashSet<T>(new Identity()); // 创建 HashSet<T>，保存为 当前借出对象集合。
        /// <summary>
        /// 保存创建对象的委托。
        /// </summary>
        private readonly Func<T> create; // 声明 创建对象的委托，后续初始化。
        /// <summary>
        /// 保存借出时执行的回调、归还时执行的回调、销毁池对象的回调。
        /// </summary>
        private readonly Action<T> onGet, onReturn, onDestroy; // 声明 借出时执行的回调，后续初始化；声明 归还时执行的回调，后续初始化；声明 销毁池对象的回调，后续初始化。
        /// <summary>
        /// 保存容器容量上限。
        /// </summary>
        private readonly int capacity; // 声明 容器容量上限，后续初始化。
        /// <summary>
        /// 保存已释放标记。
        /// </summary>
        private bool disposed; // 声明 已释放标记，后续初始化。
        /// <summary>
        /// 获取空闲对象数量。
        /// </summary>
        public int IdleCount => idle.Count; // 获取空闲对象数量。
        /// <summary>
        /// 获取借出对象数量。
        /// </summary>
        public int ActiveCount => rented.Count; // 获取借出对象数量。

        /// <summary>
        /// 构造 ObjectPool，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="create">创建对象的委托；由调用方注入具体行为。</param>
        /// <param name="onGet">借出时执行的回调；由调用方注入具体行为。</param>
        /// <param name="onReturn">归还时执行的回调；由调用方注入具体行为。</param>
        /// <param name="onDestroy">销毁池对象的回调；由调用方注入具体行为。</param>
        /// <param name="capacity">允许保留的空闲对象上限；不是借出对象总数限制。</param>
        public ObjectPool(Func<T> create, Action<T> onGet = null, Action<T> onReturn = null, Action<T> onDestroy = null, int capacity = 64) // 构造 ObjectPool，初始化其依赖与状态。
        { // 开始构造函数。
            this.create = create ?? throw new ArgumentNullException(nameof(create)); // 更新 创建对象的委托，使用 create ?? throw new ArgumentNullException(nameof(create))。
            this.onGet = onGet; // 更新 借出时执行的回调，使用 借出时执行的回调。
            this.onReturn = onReturn; // 更新 归还时执行的回调，使用 归还时执行的回调。
            this.onDestroy = onDestroy; // 更新 销毁池对象的回调，使用 销毁池对象的回调。
            this.capacity = Math.Max(0, capacity); // 更新 容器容量上限，使用 Math.Max 的返回值。
        } // 结束构造函数。

        /// <summary>
        /// 优先取空闲对象，否则调用工厂创建；登记借出身份，执行 onGet 并返回对象。
        /// </summary>
        /// <returns>已经登记为借出的可复用对象；使用完通过同一对象池 Return。</returns>
        public T Get() // 优先取空闲对象，否则调用工厂创建；登记借出身份，执行 onGet 并返回对象。
        { // 开始方法。
            if (disposed) // 当对象已经释放时，执行下方分支。
                throw new ObjectDisposedException(nameof(ObjectPool<T>)); // 抛出 ObjectDisposedException，拒绝无效参数或状态。
            var item = idle.Count > 0 ? idle.Pop() : create(); // 把 按条件 idle.Count > 0 选择的值 保存为 当前对象。
            if (ReferenceEquals(item, null) || !rented.Add(item)) // 当（条件 ReferenceEquals(item, null) 成立；或条件 rented.Add(item) 不成立）时，执行下方分支。
                throw new InvalidOperationException("Pool factory returned an invalid object."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Pool factory returned an invalid object."。
            onGet?.Invoke(item); // 仅在 借出时执行的回调 非空时，执行 借出时执行的回调 回调；仅在回调非空时调用。
            return item; // 返回 当前对象。
        } // 结束方法。

        /// <summary>
        /// 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
        /// </summary>
        /// <param name="item">当前对象。</param>
        public void Return(T item) // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
        { // 开始方法。
            if (ReferenceEquals(item, null) || !rented.Remove(item)) // 当（条件 ReferenceEquals(item, null) 成立；或条件 rented.Remove(item) 不成立）时，执行下方分支。
                throw new InvalidOperationException("Foreign object or duplicate pool return."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Foreign object or duplicate pool return."。
            onReturn?.Invoke(item); // 仅在 归还时执行的回调 非空时，执行 归还时执行的回调 回调；仅在回调非空时调用。
            if (!disposed && idle.Count < capacity) // 当（对象仍未释放；且idle.Count小于容器容量上限）时，执行下方分支。
                idle.Push(item); // 向 空闲对象集合或待机状态 压入 item。
            else // 前面的条件不成立时，执行此备用分支。
                onDestroy?.Invoke(item); // 仅在 销毁池对象的回调 非空时，执行 销毁池对象的回调 回调；仅在回调非空时调用。
        } // 结束方法。

        /// <summary>
        /// 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
        /// </summary>
        public void Dispose() // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
        { // 开始方法。
            if (disposed) // 当对象已经释放时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            disposed = true; // 更新 已释放标记，使用 true（启用/满足）。
            while (idle.Count > 0) // 只要idle.Count大于0，就继续循环。
            { // 开始条件循环。
                var item = idle.Pop(); // 把 idle.Pop 的返回值 保存为 当前对象。
                onDestroy?.Invoke(item); // 仅在 销毁池对象的回调 非空时，执行 销毁池对象的回调 回调；仅在回调非空时调用。
            } // 结束条件循环。
        // 借出对象在 Return 前仍由调用方使用，释放池时不要提前销毁这些对象。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
