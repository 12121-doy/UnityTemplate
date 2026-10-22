using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。

namespace Guisha.Core.Patterns // 将本文件类型归入 Guisha.Core.Patterns，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 声明源类型到目标类型的转换契约。
    /// </summary>
    /// <typeparam name="TSource">适配前的源数据类型。</typeparam>
    /// <typeparam name="TResult">处理结果类型。</typeparam>
    public interface IAdapter<in TSource, out TResult> // 声明源类型到目标类型的转换契约。
    { // 开始接口。
        /// <summary>
        /// 执行源数据到目标数据的适配。
        /// </summary>
        /// <param name="source">原始来源。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        TResult Adapt(TSource source); // 执行源数据到目标数据的适配。
    } // 结束接口。

    /// <summary>
    /// 通过转换函数适配旧数据或不同接口的数据。
    /// </summary>
    /// <typeparam name="TSource">适配前的源数据类型。</typeparam>
    /// <typeparam name="TResult">处理结果类型。</typeparam>
    public sealed class DelegateAdapter<TSource, TResult> : IAdapter<TSource, TResult> // 通过转换函数适配旧数据或不同接口的数据。
    { // 开始类型。
        /// <summary>
        /// 保存类型转换委托。
        /// </summary>
        private readonly Func<TSource, TResult> convert; // 声明 类型转换委托，后续初始化。
        /// <summary>
        /// 构造 DelegateAdapter，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="convert">类型转换委托；由调用方注入具体行为。</param>
        public DelegateAdapter(Func<TSource, TResult> convert) // 构造 DelegateAdapter，初始化其依赖与状态。
        { // 开始构造函数。
            this.convert = convert ?? throw new ArgumentNullException(nameof(convert)); // 更新 类型转换委托，使用 convert ?? throw new ArgumentNullException(nameof(convert))。
        } // 结束构造函数。

        /// <summary>
        /// 把源数据交给转换委托并返回目标数据。
        /// </summary>
        /// <param name="source">原始来源。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        public TResult Adapt(TSource source) => convert(source); // 把源数据交给转换委托并返回目标数据。
    } // 结束类型。

    /// <summary>
    /// 消息输出后端契约。
    /// </summary>
    public interface IMessageSink // 消息输出后端契约。
    { // 开始接口。
        /// <summary>
        /// 向消息后端输出文本。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        void Write(string message); // 向消息后端输出文本。
    } // 结束接口。

    /// <summary>
    /// 将消息写入指定输出委托。
    /// </summary>
    public sealed class ActionMessageSink : IMessageSink // 将消息写入指定输出委托。
    { // 开始类型。
        /// <summary>
        /// 保存输出委托。
        /// </summary>
        private readonly Action<string> output; // 声明 输出委托，后续初始化。
        /// <summary>
        /// 构造 ActionMessageSink，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="output">输出委托；由调用方注入具体行为。</param>
        public ActionMessageSink(Action<string> output) // 构造 ActionMessageSink，初始化其依赖与状态。
        { // 开始构造函数。
            this.output = output ?? throw new ArgumentNullException(nameof(output)); // 更新 输出委托，使用 output ?? throw new ArgumentNullException(nameof(output))。
        } // 结束构造函数。

        /// <summary>
        /// 调用输出后端委托。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        public void Write(string message) => output(message); // 调用输出后端委托。
    } // 结束类型。

    // 桥接模式：消息格式层与输出后端可以独立替换。
    /// <summary>
    /// 桥接抽象，保留输出后端，由派生类实现消息格式。
    /// </summary>
    public abstract class MessageChannel // 桥接抽象，保留输出后端，由派生类实现消息格式。
    { // 开始类型。
        /// <summary>
        /// 获取消息输出后端。
        /// </summary>
        protected IMessageSink Sink { get; } // 获取消息输出后端。

        /// <summary>
        /// 构造 MessageChannel，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="sink">消息输出后端。</param>
        protected MessageChannel(IMessageSink sink) // 构造 MessageChannel，初始化其依赖与状态。
        { // 开始构造函数。
            Sink = sink ?? throw new ArgumentNullException(nameof(sink)); // 更新 消息输出后端，使用 sink ?? throw new ArgumentNullException(nameof(sink))。
        } // 结束构造函数。

        /// <summary>
        /// 向消息后端输出文本。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        public abstract void Write(string message); // 向消息后端输出文本。
    } // 结束类型。

    /// <summary>
    /// 为消息附加标签后写入后端。
    /// </summary>
    public sealed class TaggedMessageChannel : MessageChannel // 为消息附加标签后写入后端。
    { // 开始类型。
        /// <summary>
        /// 保存输出标签。
        /// </summary>
        private readonly string tag; // 声明 输出标签，后续初始化。
        /// <summary>
        /// 构造 TaggedMessageChannel，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="tag">输出标签。</param>
        /// <param name="sink">消息输出后端。</param>
        public TaggedMessageChannel(string tag, IMessageSink sink) : base(sink) // 构造 TaggedMessageChannel，初始化其依赖与状态。
        { // 开始构造函数。
            this.tag = tag; // 更新 输出标签，使用 输出标签。
        } // 结束构造函数。

        /// <summary>
        /// 在消息前添加 [tag]，再调用 Sink.Write。
        /// </summary>
        /// <param name="message">消息或事件载荷。</param>
        public override void Write(string message) => Sink.Write("[" + tag + "] " + message); // 在消息前添加 [tag]，再调用 Sink.Write。
    } // 结束类型。

    /// <summary>
    /// 为组合树声明进入/离开节点的访问行为。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public interface INodeVisitor<T> // 为组合树声明进入/离开节点的访问行为。
    { // 开始接口。
        /// <summary>
        /// 进入状态时初始化上下文；默认基类钩子可为空。
        /// </summary>
        /// <param name="node">当前树节点或对白节点。</param>
        void Enter(CompositeNode<T> node); // 进入状态时初始化上下文；默认基类钩子可为空。
        /// <summary>
        /// 访问者离开一个组合节点时调用的契约。
        /// </summary>
        /// <param name="node">当前树节点或对白节点。</param>
        void Leave(CompositeNode<T> node); // 访问者离开一个组合节点时调用的契约。
    } // 结束接口。

    /// <summary>
    /// 单父节点树结构，防成环，支持外部访问者。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class CompositeNode<T> // 单父节点树结构，防成环，支持外部访问者。
    { // 开始类型。
        /// <summary>
        /// 保存子节点集合。
        /// </summary>
        private readonly List<CompositeNode<T>> children = new List<CompositeNode<T>>(); // 创建 List<CompositeNode<T>>，保存为 子节点集合。
        /// <summary>
        /// 保存子节点只读视图。
        /// </summary>
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<CompositeNode<T>> view; // 声明 子节点只读视图，后续初始化。
        /// <summary>
        /// 获取封装的数据值。
        /// </summary>
        public T Value { get; } // 获取封装的数据值。
        /// <summary>
        /// 获取父节点。
        /// </summary>
        public CompositeNode<T> Parent { get; private set; } // 获取父节点。
        /// <summary>
        /// 获取只读子节点列表。
        /// </summary>
        public IReadOnlyList<CompositeNode<T>> Children => view; // 获取只读子节点列表。

        /// <summary>
        /// 构造 CompositeNode，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        public CompositeNode(T value) // 构造 CompositeNode，初始化其依赖与状态。
        { // 开始构造函数。
            Value = value; // 更新 封装的数据值，使用 当前处理值。
            view = children.AsReadOnly(); // 更新 子节点只读视图，使用 children.AsReadOnly 的返回值。
        } // 结束构造函数。

        /// <summary>
        /// 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
        /// </summary>
        /// <param name="child">待挂接或移除的子节点。</param>
        public void Add(CompositeNode<T> child) // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
        { // 开始方法。
            if (child == null) // 当待挂接或移除的子节点为空时，执行下方分支。
                throw new ArgumentNullException(nameof(child)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            for (var node = this; node != null; node = node.Parent) // 按初始化与递增步骤循环；继续条件为 node != null。
                if (ReferenceEquals(node, child)) // 当条件 ReferenceEquals(node, child) 成立时，执行下方分支。
                    throw new InvalidOperationException("Composite cycle."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Composite cycle."。
            if (child.Parent != null) // 当child.Parent不为空时，执行下方分支。
                throw new InvalidOperationException("Detach child before reparenting."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Detach child before reparenting."。
            children.Add(child); // 向 子节点集合 加入 child。
            child.Parent = this; // 更新 child.Parent，使用 this。
        } // 结束方法。

        /// <summary>
        /// 移除子节点并清空其 Parent，返回是否移除成功。
        /// </summary>
        /// <param name="child">待挂接或移除的子节点。</param>
        /// <returns>true 表示目标元素已成功移除。</returns>
        public bool Remove(CompositeNode<T> child) // 移除子节点并清空其 Parent，返回是否移除成功。
        { // 开始方法。
            if (!children.Remove(child)) // 当条件 children.Remove(child) 不成立时，执行下方分支。
                return false; // 返回 false，将当前分支的布尔结果交给调用方。
            child.Parent = null; // 清空 child.Parent 引用，表示当前不再持有该对象。
            return true; // 返回 true，将当前分支的布尔结果交给调用方。
        } // 结束方法。

        /// <summary>
        /// 先 Enter 当前节点，再按子节点快照递归访问，最后 Leave 当前节点。
        /// </summary>
        /// <param name="visitor">树节点访问者。</param>
        public void Accept(INodeVisitor<T> visitor) // 先 Enter 当前节点，再按子节点快照递归访问，最后 Leave 当前节点。
        { // 开始方法。
            if (visitor == null) // 当树节点访问者为空时，执行下方分支。
                throw new ArgumentNullException(nameof(visitor)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            visitor.Enter(this); // 进入状态时初始化上下文；默认基类钩子可为空。
            foreach (var child in children.ToArray()) // 遍历 children.ToArray()，每次把一个元素保存为 child。
                child.Accept(visitor); // 先 Enter 当前节点，再按子节点快照递归访问，最后 Leave 当前节点。
            visitor.Leave(this); // 访问者离开一个组合节点时调用的契约。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 声明输入与输出同类型的处理器契约。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public interface IProcessor<T> // 声明输入与输出同类型的处理器契约。
    { // 开始接口。
        /// <summary>
        /// 按处理器契约转换并返回数值。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        /// <returns>T 类型的处理结果。</returns>
        T Process(T value); // 按处理器契约转换并返回数值。
    } // 结束接口。

    /// <summary>
    /// 委托处理器。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class DelegateProcessor<T> : IProcessor<T> // 委托处理器。
    { // 开始类型。
        /// <summary>
        /// 保存处理算法委托。
        /// </summary>
        private readonly Func<T, T> process; // 声明 处理算法委托，后续初始化。
        /// <summary>
        /// 构造 DelegateProcessor，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="process">处理算法委托；由调用方注入具体行为。</param>
        public DelegateProcessor(Func<T, T> process) // 构造 DelegateProcessor，初始化其依赖与状态。
        { // 开始构造函数。
            this.process = process ?? throw new ArgumentNullException(nameof(process)); // 更新 处理算法委托，使用 process ?? throw new ArgumentNullException(nameof(process))。
        } // 结束构造函数。

        /// <summary>
        /// 调用注入的处理函数并返回结果。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        /// <returns>T 类型的处理结果。</returns>
        public T Process(T value) => process(value); // 调用注入的处理函数并返回结果。
    } // 结束类型。

    /// <summary>
    /// 保存内部处理器的装饰基类。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public abstract class ProcessorDecorator<T> : IProcessor<T> // 保存内部处理器的装饰基类。
    { // 开始类型。
        /// <summary>
        /// 获取被装饰的内部处理器。
        /// </summary>
        protected IProcessor<T> Inner { get; } // 获取被装饰的内部处理器。

        /// <summary>
        /// 构造 ProcessorDecorator，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="inner">被包装的内部实现。</param>
        protected ProcessorDecorator(IProcessor<T> inner) // 构造 ProcessorDecorator，初始化其依赖与状态。
        { // 开始构造函数。
            Inner = inner ?? throw new ArgumentNullException(nameof(inner)); // 更新 被装饰的内部处理器，使用 inner ?? throw new ArgumentNullException(nameof(inner))。
        } // 结束构造函数。

        /// <summary>
        /// 按处理器契约转换并返回数值。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        /// <returns>T 类型的处理结果。</returns>
        public abstract T Process(T value); // 按处理器契约转换并返回数值。
    } // 结束类型。

    /// <summary>
    /// 在内部处理前后增加转换。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class TransformDecorator<T> : ProcessorDecorator<T> // 在内部处理前后增加转换。
    { // 开始类型。
        /// <summary>
        /// 保存前置处理委托、后置处理委托。
        /// </summary>
        private readonly Func<T, T> before, after; // 声明 前置处理委托，后续初始化；声明 后置处理委托，后续初始化。
        /// <summary>
        /// 构造 TransformDecorator，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="inner">被包装的内部实现。</param>
        /// <param name="before">前置处理委托；由调用方注入具体行为。</param>
        /// <param name="after">后置处理委托；由调用方注入具体行为。</param>
        public TransformDecorator(IProcessor<T> inner, Func<T, T> before = null, Func<T, T> after = null) : base(inner) // 构造 TransformDecorator，初始化其依赖与状态。
        { // 开始构造函数。
            this.before = before; // 更新 前置处理委托，使用 前置处理委托。
            this.after = after; // 更新 后置处理委托，使用 后置处理委托。
        } // 结束构造函数。

        /// <summary>
        /// 可选 before 转换、内部处理、可选 after 转换依次执行。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        /// <returns>T 类型的处理结果。</returns>
        public override T Process(T value) // 可选 before 转换、内部处理、可选 after 转换依次执行。
        { // 开始方法。
            if (before != null) // 当前置处理委托不为空时，执行下方分支。
                value = before(value); // 更新 当前处理值，使用 before 的返回值。
            value = Inner.Process(value); // 更新 当前处理值，使用 Inner.Process 的返回值。
            return after != null ? after(value) : value; // 返回 按条件 after != null 选择的值。
        } // 结束方法。
    } // 结束类型。

    // 仅缓存不可变元数据；淘汰缓存项不会释放调用方仍在使用的对象。
    /// <summary>
    /// 有容量上限的共享元数据缓存；队列顺序淘汰，不承担 Unity 资源释放。
    /// </summary>
    /// <typeparam name="TKey">查找键类型。</typeparam>
    /// <typeparam name="TValue">共享缓存值类型。</typeparam>
    public sealed class FlyweightCache<TKey, TValue> // 有容量上限的共享元数据缓存；队列顺序淘汰，不承担 Unity 资源释放。
    { // 开始类型。
        /// <summary>
        /// 保存缓存字典。
        /// </summary>
        private readonly Dictionary<TKey, TValue> cache; // 声明 缓存字典，后续初始化。
        /// <summary>
        /// 保存缓存键的先进先出队列。
        /// </summary>
        private readonly Queue<TKey> order = new Queue<TKey>(); // 创建 Queue<TKey>，保存为 缓存键的先进先出队列。
        /// <summary>
        /// 保存创建对象的委托。
        /// </summary>
        private readonly Func<TKey, TValue> create; // 声明 创建对象的委托，后续初始化。
        /// <summary>
        /// 保存容器容量上限。
        /// </summary>
        private readonly int capacity; // 声明 容器容量上限，后续初始化。
        /// <summary>
        /// 获取当前元素数量。
        /// </summary>
        public int Count => cache.Count; // 获取当前元素数量。

        /// <summary>
        /// 构造 FlyweightCache，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="create">创建对象的委托；由调用方注入具体行为。</param>
        /// <param name="capacity">容器容量上限。</param>
        /// <param name="comparer">集合元素比较器。</param>
        public FlyweightCache(Func<TKey, TValue> create, int capacity = 256, IEqualityComparer<TKey> comparer = null) // 构造 FlyweightCache，初始化其依赖与状态。
        { // 开始构造函数。
            this.create = create ?? throw new ArgumentNullException(nameof(create)); // 更新 创建对象的委托，使用 create ?? throw new ArgumentNullException(nameof(create))。
            if (capacity < 1) // 当容器容量上限小于1时，执行下方分支。
                throw new ArgumentOutOfRangeException(nameof(capacity)); // 抛出 ArgumentOutOfRangeException，拒绝无效参数或状态。
            this.capacity = capacity; // 更新 容器容量上限，使用 容器容量上限。
            cache = new Dictionary<TKey, TValue>(comparer); // 更新 缓存字典，使用 新建的 Dictionary<TKey, TValue> 对象。
        } // 结束构造函数。

        /// <summary>
        /// 命中时返回共享值；未命中时创建，达到容量则按入队顺序移除最旧键。
        /// </summary>
        /// <param name="key">查找键。</param>
        /// <returns>TValue 类型的处理结果。</returns>
        public TValue Get(TKey key) // 命中时返回共享值；未命中时创建，达到容量则按入队顺序移除最旧键。
        { // 开始方法。
            if (cache.TryGetValue(key, out var value)) // 当条件 cache.TryGetValue(key, out var value) 成立时，执行下方分支。
                return value; // 返回 当前处理值。
            value = create(key); // 更新 当前处理值，使用 create 的返回值。
            if (cache.Count >= capacity) // 当cache.Count不小于容器容量上限时，执行下方分支。
                cache.Remove(order.Dequeue()); // 从 缓存字典 移除 order.Dequeue()。
            cache.Add(key, value); // 向 缓存字典 加入 key、value。
            order.Enqueue(key); // 向 缓存键的先进先出队列 入队 key。
            return value; // 返回 当前处理值。
        } // 结束方法。

        /// <summary>
        /// 只清除缓存和顺序队列，不释放使用者持有的值。
        /// </summary>
        public void Clear() // 只清除缓存和顺序队列，不释放使用者持有的值。
        { // 开始方法。
            cache.Clear(); // 清空 缓存字典 中保存的记录。
            order.Clear(); // 清空 缓存键的先进先出队列 中保存的记录。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 延迟创建并拥有被代理对象；首次访问 Value 才创建。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class LazyProxy<T> : IDisposable where T : class // 延迟创建并拥有被代理对象；首次访问 Value 才创建。
    { // 开始类型。
        /// <summary>
        /// 保存创建对象的委托。
        /// </summary>
        private readonly Func<T> create; // 声明 创建对象的委托，后续初始化。
        /// <summary>
        /// 保存释放对象的回调。
        /// </summary>
        private readonly Action<T> release; // 声明 释放对象的回调，后续初始化。
        /// <summary>
        /// 保存持有的对象实例。
        /// </summary>
        private T instance; // 声明 持有的对象实例，后续初始化。
        /// <summary>
        /// 保存已释放标记。
        /// </summary>
        private bool disposed; // 声明 已释放标记，后续初始化。
        /// <summary>
        /// 获取代理是否已经创建对象。
        /// </summary>
        public bool IsCreated => instance != null; // 获取代理是否已经创建对象。

        /// <summary>
        /// 构造 LazyProxy，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="create">创建对象的委托；由调用方注入具体行为。</param>
        /// <param name="release">释放对象的回调；由调用方注入具体行为。</param>
        public LazyProxy(Func<T> create, Action<T> release = null) // 构造 LazyProxy，初始化其依赖与状态。
        { // 开始构造函数。
            this.create = create ?? throw new ArgumentNullException(nameof(create)); // 更新 创建对象的委托，使用 create ?? throw new ArgumentNullException(nameof(create))。
            this.release = release; // 更新 释放对象的回调，使用 释放对象的回调。
        } // 结束构造函数。

        /// <summary>
        /// 首次访问时通过工厂创建对象；释放后访问抛出异常。
        /// </summary>
        public T Value // 首次访问时通过工厂创建对象；释放后访问抛出异常。
        { // 开始属性访问器列表。
            get // 读取属性时执行下方取值与状态校验。
            { // 开始属性读取。
                if (disposed) // 当对象已经释放时，执行下方分支。
                    throw new ObjectDisposedException(nameof(LazyProxy<T>)); // 抛出 ObjectDisposedException，拒绝无效参数或状态。
                if (instance == null) // 当持有的对象实例为空时，执行下方分支。
                    instance = create() ?? throw new InvalidOperationException("Proxy factory returned null."); // 更新 持有的对象实例，使用 create() ?? throw new InvalidOperationException("Proxy factory returned null.")。
                return instance; // 返回 持有的对象实例。
            } // 结束属性读取。
        } // 结束属性访问器列表。

        /// <summary>
        /// 幂等标记释放，已创建时调用 release 或 IDisposable.Dispose；未使用的代理不会为释放而创建对象。
        /// </summary>
        public void Dispose() // 幂等标记释放，已创建时调用 release 或 IDisposable.Dispose；未使用的代理不会为释放而创建对象。
        { // 开始方法。
            if (disposed) // 当对象已经释放时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            disposed = true; // 更新 已释放标记，使用 true（启用/满足）。
            var value = instance; // 把 持有的对象实例 保存为 当前处理值。
            instance = null; // 清空 持有的对象实例 引用，表示当前不再持有该对象。
            if (value == null) // 当当前处理值为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (release != null) // 当释放对象的回调不为空时，执行下方分支。
                release(value); // 执行注入的释放对象的回调，使用参数 value。
            else // 前面的条件不成立时，执行此备用分支。
                (value as IDisposable)?.Dispose(); // 仅在 (value as IDisposable) 非空时，释放 (value as IDisposable) 持有的资源或注册；具体拥有权由该对象负责。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
