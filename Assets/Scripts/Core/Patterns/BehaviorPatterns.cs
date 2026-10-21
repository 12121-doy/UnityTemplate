using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。

namespace Guisha.Core.Patterns // 将本文件类型归入 Guisha.Core.Patterns，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 声明一条可继续或停止的处理规则。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public interface IRule<in T> // 声明一条可继续或停止的处理规则。
    { // 开始接口。
        /// <summary>
        /// 执行规则并返回是否允许继续。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>true 表示规则链允许继续；false 表示已拒绝。</returns>
        bool Apply(T context); // 执行规则并返回是否允许继续。
    } // 结束接口。

    /// <summary>
    /// 用委托实现规则。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class DelegateRule<T> : IRule<T> // 用委托实现规则。
    { // 开始类型。
        /// <summary>
        /// 保存本次规则。
        /// </summary>
        private readonly Func<T, bool> rule; // 声明 本次规则，后续初始化。
        /// <summary>
        /// 构造 DelegateRule，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="rule">本次规则；由调用方注入具体行为。</param>
        public DelegateRule(Func<T, bool> rule) // 构造 DelegateRule，初始化其依赖与状态。
        { // 开始构造函数。
            this.rule = rule ?? throw new ArgumentNullException(nameof(rule)); // 更新 本次规则，使用 rule ?? throw new ArgumentNullException(nameof(rule))。
        } // 结束构造函数。

        /// <summary>
        /// 执行注入的规则委托并返回是否继续。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>true 表示规则链允许继续；false 表示已拒绝。</returns>
        public bool Apply(T context) => rule(context); // 执行注入的规则委托并返回是否继续。
    } // 结束类型。

    /// <summary>
    /// 顺序执行规则，首个 false 即停止。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class RuleChain<T> // 顺序执行规则，首个 false 即停止。
    { // 开始类型。
        /// <summary>
        /// 保存规则序列或规则快照。
        /// </summary>
        private readonly IRule<T>[] rules; // 声明 规则序列或规则快照，后续初始化。
        /// <summary>
        /// 构造 RuleChain，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="rules">规则序列或规则快照。</param>
        public RuleChain(params IRule<T>[] rules) // 构造 RuleChain，初始化其依赖与状态。
        { // 开始构造函数。
            this.rules = (IRule<T>[])(rules ?? throw new ArgumentNullException(nameof(rules))).Clone(); // 更新 规则序列或规则快照，使用 (IRule<T>[])(rules ?? throw new ArgumentNullException(nameof(rules))).Clone()。
            foreach (var rule in this.rules) // 遍历 规则序列或规则快照，每次把一个元素保存为 rule。
                if (rule == null) // 当本次规则为空时，执行下方分支。
                    throw new ArgumentException("Null rule."); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Null rule."。
        } // 结束构造函数。

        /// <summary>
        /// 按构造时复制的规则顺序执行，遇到 false 立即返回；全部通过则返回 true。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>true 表示规则链允许继续；false 表示已拒绝。</returns>
        public bool Apply(T context) // 按构造时复制的规则顺序执行，遇到 false 立即返回；全部通过则返回 true。
        { // 开始方法。
            foreach (var rule in rules) // 遍历 规则序列或规则快照，每次把一个元素保存为 rule。
                if (!rule.Apply(context)) // 当条件 rule.Apply(context) 不成立时，执行下方分支。
                    return false; // 返回 false，将当前分支的布尔结果交给调用方。
            return true; // 返回 true，将当前分支的布尔结果交给调用方。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 声明上下文到结果的可替换算法。
    /// </summary>
    /// <typeparam name="TContext">操作上下文类型。</typeparam>
    /// <typeparam name="TResult">处理结果类型。</typeparam>
    public interface IStrategy<in TContext, out TResult> // 声明上下文到结果的可替换算法。
    { // 开始接口。
        /// <summary>
        /// 根据当前节点或策略契约计算结果。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        TResult Evaluate(TContext context); // 根据当前节点或策略契约计算结果。
    } // 结束接口。

    /// <summary>
    /// 用委托实现策略。
    /// </summary>
    /// <typeparam name="TContext">操作上下文类型。</typeparam>
    /// <typeparam name="TResult">处理结果类型。</typeparam>
    public sealed class DelegateStrategy<TContext, TResult> : IStrategy<TContext, TResult> // 用委托实现策略。
    { // 开始类型。
        /// <summary>
        /// 保存策略求值委托。
        /// </summary>
        private readonly Func<TContext, TResult> evaluate; // 声明 策略求值委托，后续初始化。
        /// <summary>
        /// 构造 DelegateStrategy，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="evaluate">策略求值委托；由调用方注入具体行为。</param>
        public DelegateStrategy(Func<TContext, TResult> evaluate) // 构造 DelegateStrategy，初始化其依赖与状态。
        { // 开始构造函数。
            this.evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate)); // 更新 策略求值委托，使用 evaluate ?? throw new ArgumentNullException(nameof(evaluate))。
        } // 结束构造函数。

        /// <summary>
        /// 把上下文传入已注入的算法委托。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        public TResult Evaluate(TContext context) => evaluate(context); // 把上下文传入已注入的算法委托。
    } // 结束类型。

    /// <summary>
    /// 按键注册与选择策略。
    /// </summary>
    /// <typeparam name="TKey">查找键类型。</typeparam>
    /// <typeparam name="TContext">操作上下文类型。</typeparam>
    /// <typeparam name="TResult">处理结果类型。</typeparam>
    public sealed class StrategySelector<TKey, TContext, TResult> // 按键注册与选择策略。
    { // 开始类型。
        /// <summary>
        /// 保存策略注册表。
        /// </summary>
        private readonly Dictionary<TKey, IStrategy<TContext, TResult>> strategies = new Dictionary<TKey, IStrategy<TContext, TResult>>(); // 创建 Dictionary<TKey, IStrategy<TContext, TResult>>，保存为 策略注册表。
        /// <summary>
        /// 为键注册一个非空策略；重复键会报错。
        /// </summary>
        /// <param name="key">查找键。</param>
        /// <param name="strategy">算法策略。</param>
        public void Register(TKey key, IStrategy<TContext, TResult> strategy) // 为键注册一个非空策略；重复键会报错。
        { // 开始方法。
            if (strategy == null) // 当算法策略为空时，执行下方分支。
                throw new ArgumentNullException(nameof(strategy)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            strategies.Add(key, strategy); // 向 策略注册表 加入 key、strategy。
        } // 结束方法。

        /// <summary>
        /// 找到键对应的策略并求值，未注册时抛 KeyNotFoundException。
        /// </summary>
        /// <param name="key">查找键。</param>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        public TResult Evaluate(TKey key, TContext context) // 找到键对应的策略并求值，未注册时抛 KeyNotFoundException。
        { // 开始方法。
            if (!strategies.TryGetValue(key, out var strategy)) // 当条件 strategies.TryGetValue(key, out var strategy) 不成立时，执行下方分支。
                throw new KeyNotFoundException("Strategy is not registered."); // 抛出 KeyNotFoundException，拒绝无效参数或状态；错误信息为 "Strategy is not registered."。
            return strategy.Evaluate(context); // 根据当前节点或策略契约计算结果，并返回调用结果。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 单处理者请求/响应中介，校验响应类型，注册可以释放。
    /// </summary>
    public sealed class Mediator // 单处理者请求/响应中介，校验响应类型，注册可以释放。
    { // 开始类型。
        /// <summary>
        /// 中介者保存的响应类型与处理委托。
        /// </summary>
        private sealed class Binding // 中介者保存的响应类型与处理委托。
        { // 开始类型。
            /// <summary>
            /// 保存响应的运行时类型。
            /// </summary>
            public Type Response; // 声明 响应的运行时类型，后续初始化。
            /// <summary>
            /// 保存请求处理委托。
            /// </summary>
            public Func<object, object> Invoke; // 声明 请求处理委托，后续初始化。
        } // 结束类型。

        /// <summary>
        /// 可幂等释放的注册/订阅令牌。
        /// </summary>
        private sealed class Token : IDisposable // 可幂等释放的注册/订阅令牌。
        { // 开始类型。
            /// <summary>
            /// 保存退订或清理回调。
            /// </summary>
            private Action dispose; // 声明 退订或清理回调，后续初始化。
            /// <summary>
            /// 构造 Token，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="dispose">退订或清理回调；由调用方注入具体行为。</param>
            public Token(Action dispose) // 构造 Token，初始化其依赖与状态。
            { // 开始构造函数。
                this.dispose = dispose; // 更新 退订或清理回调，使用 退订或清理回调。
            } // 结束构造函数。

            /// <summary>
            /// 幂等释放自身持有的注册、订阅或资源。
            /// </summary>
            public void Dispose() // 幂等释放自身持有的注册、订阅或资源。
            { // 开始方法。
                var action = dispose; // 把 退订或清理回调 保存为 操作委托。
                dispose = null; // 清空 退订或清理回调 引用，表示当前不再持有该对象。
                action?.Invoke(); // 仅在 操作委托 非空时，执行 操作委托 回调；仅在回调非空时调用。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 保存请求类型到处理者的映射。
        /// </summary>
        private readonly Dictionary<Type, Binding> handlers = new Dictionary<Type, Binding>(); // 创建 Dictionary<Type, Binding>，保存为 请求类型到处理者的映射。
        /// <summary>
        /// 为请求类型注册唯一处理者，保存响应类型，返回可释放注册令牌。
        /// </summary>
        /// <typeparam name="TRequest">请求载荷类型。</typeparam>
        /// <typeparam name="TResponse">请求处理后返回的响应类型。</typeparam>
        /// <param name="handler">每个请求类型唯一的同步处理者；释放返回令牌即可取消注册。</param>
        /// <returns>注册/订阅令牌；不再需要时调用 Dispose 释放。</returns>
        public IDisposable Register<TRequest, TResponse>(Func<TRequest, TResponse> handler) // 为请求类型注册唯一处理者，保存响应类型，返回可释放注册令牌。
        { // 开始方法。
            if (handler == null) // 当请求处理委托为空时，执行下方分支。
                throw new ArgumentNullException(nameof(handler)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            var binding = new Binding // 创建 Binding，保存为 binding。
            { // 开始成员/集合初始化。
                Response = typeof(TResponse), // 更新 响应的运行时类型，使用 typeof(TResponse)。
                Invoke = request => handler((TRequest)request) // 为 请求处理委托 保存回调；此处仅注册，回调被调用时才执行内部代码。
            }; // 结束成员/集合初始化。
            handlers.Add(typeof(TRequest), binding); // 向 请求类型到处理者的映射 加入 typeof(TRequest)、binding。
            return new Token(() => // 返回 新建的 Token 对象。
            { // 开始回调。
                if (handlers.TryGetValue(typeof(TRequest), out var current) && ReferenceEquals(binding, current)) // 当（条件 handlers.TryGetValue(typeof(TRequest), out var current) 成立；且条件 ReferenceEquals(binding, current) 成立）时，执行下方分支。
                    handlers.Remove(typeof(TRequest)); // 从 请求类型到处理者的映射 移除 typeof(TRequest)。
            }); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 校验请求处理者和响应类型后同步执行，返回强类型结果。
        /// </summary>
        /// <typeparam name="TRequest">请求载荷类型。</typeparam>
        /// <typeparam name="TResponse">请求处理后返回的响应类型。</typeparam>
        /// <param name="request">收到的请求载荷。</param>
        /// <returns>TResponse 类型的处理结果。</returns>
        public TResponse Send<TRequest, TResponse>(TRequest request) // 校验请求处理者和响应类型后同步执行，返回强类型结果。
        { // 开始方法。
            if (!handlers.TryGetValue(typeof(TRequest), out var handler)) // 当条件 handlers.TryGetValue(typeof(TRequest), out var handler) 不成立时，执行下方分支。
                throw new KeyNotFoundException("Request has no handler."); // 抛出 KeyNotFoundException，拒绝无效参数或状态；错误信息为 "Request has no handler."。
            if (handler.Response != typeof(TResponse)) // 当handler.Response不等于typeof(TResponse)时，执行下方分支。
                throw new InvalidOperationException("Response type mismatch."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Response type mismatch."。
            return (TResponse)handler.Invoke(request); // 返回 (TResponse)handler.Invoke(request)。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 生成、校验归属并恢复复制快照。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class MementoStore<T> // 生成、校验归属并恢复复制快照。
    { // 开始类型。
        /// <summary>
        /// 保存快照归属与已复制的数据。
        /// </summary>
        public sealed class Snapshot // 保存快照归属与已复制的数据。
        { // 开始类型。
            /// <summary>
            /// 保存拥有者标识。
            /// </summary>
            internal readonly object Owner; // 声明 拥有者标识，后续初始化。
            /// <summary>
            /// 保存当前状态。
            /// </summary>
            internal readonly T State; // 声明 当前状态，后续初始化。
            /// <summary>
            /// 保存所属 store 和已复制状态；由 Capture 内部创建。
            /// </summary>
            /// <param name="owner">本次操作的拥有者。</param>
            /// <param name="state">目标状态或保存的状态数据。</param>
            internal Snapshot(object owner, T state) // 构造 Snapshot，初始化其依赖与状态。
            { // 开始构造函数。
                Owner = owner; // 更新 拥有者标识，使用 本次操作的拥有者。
                State = state; // 更新 当前状态，使用 目标状态或保存的状态数据。
            } // 结束构造函数。
        } // 结束类型。

        /// <summary>
        /// 保存本次操作的拥有者。
        /// </summary>
        private readonly object owner = new object (); // 创建 object，保存为 本次操作的拥有者。
        /// <summary>
        /// 保存深复制函数。
        /// </summary>
        private readonly Func<T, T> clone; // 声明 深复制函数，后续初始化。
        /// <summary>
        /// 构造 MementoStore，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="deepCopy">由调用方提供的深复制函数；由调用方注入具体行为。</param>
        public MementoStore(Func<T, T> deepCopy) // 构造 MementoStore，初始化其依赖与状态。
        { // 开始构造函数。
            clone = deepCopy ?? throw new ArgumentNullException(nameof(deepCopy)); // 更新 深复制函数，使用 deepCopy ?? throw new ArgumentNullException(nameof(deepCopy))。
        } // 结束构造函数。

        /// <summary>
        /// 调用复制函数保存快照，并记录当前 store 的归属标记。
        /// </summary>
        /// <param name="state">目标状态或保存的状态数据。</param>
        /// <returns>独立复制的当前状态快照。</returns>
        public Snapshot Capture(T state) => new Snapshot(owner, clone(state)); // 调用复制函数保存快照，并记录当前 store 的归属标记。
        /// <summary>
        /// 拒绝外来快照，再次复制快照数据后返回，避免调用方改写原快照。
        /// </summary>
        /// <param name="snapshot">所属 store 的状态快照。</param>
        /// <returns>T 类型的处理结果。</returns>
        public T Restore(Snapshot snapshot) // 拒绝外来快照，再次复制快照数据后返回，避免调用方改写原快照。
        { // 开始方法。
            if (snapshot == null || !ReferenceEquals(snapshot.Owner, owner)) // 当（所属 store 的状态快照为空；或条件 ReferenceEquals(snapshot.Owner, owner) 不成立）时，执行下方分支。
                throw new ArgumentException("Foreign snapshot."); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Foreign snapshot."。
            return clone(snapshot.State); // 执行注入的深复制函数，使用参数 snapshot.State，并返回调用结果。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 固定执行校验、前置、核心、后置和最终清理步骤。
    /// </summary>
    /// <typeparam name="TContext">操作上下文类型。</typeparam>
    /// <typeparam name="TResult">处理结果类型。</typeparam>
    public abstract class OperationTemplate<TContext, TResult> // 固定执行校验、前置、核心、后置和最终清理步骤。
    { // 开始类型。
        /// <summary>
        /// 按 Validate、Before、ExecuteCore、After 顺序执行；finally 中始终调用 Finish。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        public TResult Execute(TContext context) // 按 Validate、Before、ExecuteCore、After 顺序执行；finally 中始终调用 Finish。
        { // 开始方法。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                Validate(context); // 可覆盖的校验钩子；默认不处理。
                Before(context); // 可覆盖的前置钩子；默认不处理。
                var result = ExecuteCore(context); // 把 ExecuteCore 的返回值 保存为 本次操作结果。
                After(context, result); // 可覆盖的成功后置钩子，可以读取结果。
                return result; // 返回 本次操作结果。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                Finish(context); // 可覆盖的最终清理钩子，成功或失败都会进入。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 可覆盖的校验钩子；默认不处理。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        protected virtual void Validate(TContext context) // 可覆盖的校验钩子；默认不处理。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 可覆盖的前置钩子；默认不处理。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        protected virtual void Before(TContext context) // 可覆盖的前置钩子；默认不处理。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 必须实现的核心操作，返回 TResult。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <returns>TResult 类型的处理结果。</returns>
        protected abstract TResult ExecuteCore(TContext context); // 必须实现的核心操作，返回 TResult。
        /// <summary>
        /// 可覆盖的成功后置钩子，可以读取结果。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        /// <param name="result">本次操作结果。</param>
        protected virtual void After(TContext context, TResult result) // 可覆盖的成功后置钩子，可以读取结果。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 可覆盖的最终清理钩子，成功或失败都会进入。
        /// </summary>
        /// <param name="context">当前操作上下文。</param>
        protected virtual void Finish(TContext context) // 可覆盖的最终清理钩子，成功或失败都会进入。
        { // 开始方法。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 提供带去重的深度/广度优先迭代。
    /// </summary>
    public static class Traversal // 提供带去重的深度/广度优先迭代。
    { // 开始类型。
        /// <summary>
        /// 深度优先按子节点原顺序迭代，以比较器去重，跳过 null 子节点。
        /// </summary>
        /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
        /// <param name="root">根节点或根路径。</param>
        /// <param name="children">子节点集合；由调用方注入具体行为。</param>
        /// <param name="comparer">集合元素比较器。</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;T&gt; 类型的处理结果。</returns>
        public static IEnumerable<T> DepthFirst<T>(T root, Func<T, IEnumerable<T>> children, IEqualityComparer<T> comparer = null) // 深度优先按子节点原顺序迭代，以比较器去重，跳过 null 子节点。
            where T : class // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            if (children == null) // 当子节点集合为空时，执行下方分支。
                throw new ArgumentNullException(nameof(children)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            var seen = new HashSet<T>(comparer); // 创建 HashSet<T>，保存为 seen。
            var stack = new Stack<T>(); // 创建 Stack<T>，保存为 已经打开的面板栈。
            if (root != null) // 当根节点或根路径不为空时，执行下方分支。
                stack.Push(root); // 向 已经打开的面板栈 压入 root。
            while (stack.Count > 0) // 只要stack.Count大于0，就继续循环。
            { // 开始条件循环。
                var node = stack.Pop(); // 把 stack.Pop 的返回值 保存为 当前树节点或对白节点。
                if (!seen.Add(node)) // 当条件 seen.Add(node) 不成立时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                yield return node; // 向迭代器调用方产出当前节点，下一次枚举时继续。
                var list = new List<T>(children(node) ?? Array.Empty<T>()); // 创建 List<T>，保存为 list。
                for (int i = list.Count - 1; i >= 0; i--) // 按初始化与递增步骤循环；继续条件为 i >= 0。
                    if (list[i] != null) // 当list[i]不为空时，执行下方分支。
                        stack.Push(list[i]); // 向 已经打开的面板栈 压入 list[i]。
            } // 结束条件循环。
        } // 结束方法。

        /// <summary>
        /// 广度优先按队列迭代，以比较器去重，跳过 null 子节点。
        /// </summary>
        /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
        /// <param name="root">根节点或根路径。</param>
        /// <param name="children">子节点集合；由调用方注入具体行为。</param>
        /// <param name="comparer">集合元素比较器。</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;T&gt; 类型的处理结果。</returns>
        public static IEnumerable<T> BreadthFirst<T>(T root, Func<T, IEnumerable<T>> children, IEqualityComparer<T> comparer = null) // 广度优先按队列迭代，以比较器去重，跳过 null 子节点。
            where T : class // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            if (children == null) // 当子节点集合为空时，执行下方分支。
                throw new ArgumentNullException(nameof(children)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            var seen = new HashSet<T>(comparer); // 创建 HashSet<T>，保存为 seen。
            var queue = new Queue<T>(); // 创建 Queue<T>，保存为 queue。
            if (root != null) // 当根节点或根路径不为空时，执行下方分支。
                queue.Enqueue(root); // 向 queue 入队 root。
            while (queue.Count > 0) // 只要queue.Count大于0，就继续循环。
            { // 开始条件循环。
                var node = queue.Dequeue(); // 把 queue.Dequeue 的返回值 保存为 当前树节点或对白节点。
                if (!seen.Add(node)) // 当条件 seen.Add(node) 不成立时，执行下方分支。
                    continue; // 跳过本次循环剩余步骤，继续处理下一项。
                yield return node; // 向迭代器调用方产出当前节点，下一次枚举时继续。
                foreach (var child in children(node) ?? Array.Empty<T>()) // 遍历 children(node) ?? Array.Empty<T>()，每次把一个元素保存为 child。
                    if (child != null) // 当待挂接或移除的子节点不为空时，执行下方分支。
                        queue.Enqueue(child); // 向 queue 入队 child。
            } // 结束条件循环。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
