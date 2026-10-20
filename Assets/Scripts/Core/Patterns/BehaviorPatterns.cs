using System;
using System.Collections.Generic;
namespace Guisha.Core.Patterns
{
    public interface IRule<in T> { bool Apply(T context); }
    public sealed class DelegateRule<T> : IRule<T>
    {
        private readonly Func<T, bool> rule;
        public DelegateRule(Func<T, bool> rule) { this.rule = rule ?? throw new ArgumentNullException(nameof(rule)); }
        public bool Apply(T context) => rule(context);
    }
    public sealed class RuleChain<T>
    {
        private readonly IRule<T>[] rules;
        public RuleChain(params IRule<T>[] rules)
        {
            this.rules = (IRule<T>[])(rules ?? throw new ArgumentNullException(nameof(rules))).Clone();
            foreach (var rule in this.rules) if (rule == null) throw new ArgumentException("Null rule.");
        }
        public bool Apply(T context) { foreach (var rule in rules) if (!rule.Apply(context)) return false; return true; }
    }
    public interface IStrategy<in TContext, out TResult> { TResult Evaluate(TContext context); }
    public sealed class DelegateStrategy<TContext, TResult> : IStrategy<TContext, TResult>
    {
        private readonly Func<TContext, TResult> evaluate;
        public DelegateStrategy(Func<TContext, TResult> evaluate) { this.evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate)); }
        public TResult Evaluate(TContext context) => evaluate(context);
    }
    public sealed class StrategySelector<TKey, TContext, TResult>
    {
        private readonly Dictionary<TKey, IStrategy<TContext, TResult>> strategies = new Dictionary<TKey, IStrategy<TContext, TResult>>();
        public void Register(TKey key, IStrategy<TContext, TResult> strategy)
        { if (strategy == null) throw new ArgumentNullException(nameof(strategy)); strategies.Add(key, strategy); }
        public TResult Evaluate(TKey key, TContext context)
        { if (!strategies.TryGetValue(key, out var strategy)) throw new KeyNotFoundException("Strategy is not registered."); return strategy.Evaluate(context); }
    }
    public sealed class Mediator
    {
        private sealed class Binding { public Type Response; public Func<object, object> Invoke; }
        private sealed class Token : IDisposable
        {
            private Action dispose;
            public Token(Action dispose) { this.dispose = dispose; }
            public void Dispose() { var action = dispose; dispose = null; action?.Invoke(); }
        }
        private readonly Dictionary<Type, Binding> handlers = new Dictionary<Type, Binding>();
        public IDisposable Register<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var binding = new Binding { Response = typeof(TResponse), Invoke = request => handler((TRequest)request) };
            handlers.Add(typeof(TRequest), binding);
            return new Token(() => { if (handlers.TryGetValue(typeof(TRequest), out var current) && ReferenceEquals(binding, current)) handlers.Remove(typeof(TRequest)); });
        }
        public TResponse Send<TRequest, TResponse>(TRequest request)
        {
            if (!handlers.TryGetValue(typeof(TRequest), out var handler)) throw new KeyNotFoundException("Request has no handler.");
            if (handler.Response != typeof(TResponse)) throw new InvalidOperationException("Response type mismatch.");
            return (TResponse)handler.Invoke(request);
        }
    }
    public sealed class MementoStore<T>
    {
        public sealed class Snapshot
        {
            internal readonly object Owner;
            internal readonly T State;
            internal Snapshot(object owner, T state) { Owner = owner; State = state; }
        }
        private readonly object owner = new object();
        private readonly Func<T, T> clone;
        public MementoStore(Func<T, T> deepCopy) { clone = deepCopy ?? throw new ArgumentNullException(nameof(deepCopy)); }
        public Snapshot Capture(T state) => new Snapshot(owner, clone(state));
        public T Restore(Snapshot snapshot)
        {
            if (snapshot == null || !ReferenceEquals(snapshot.Owner, owner)) throw new ArgumentException("Foreign snapshot.");
            return clone(snapshot.State);
        }
    }
    public abstract class OperationTemplate<TContext, TResult>
    {
        public TResult Execute(TContext context)
        {
            try { Validate(context); Before(context); var result = ExecuteCore(context); After(context, result); return result; }
            finally { Finish(context); }
        }
        protected virtual void Validate(TContext context) { }
        protected virtual void Before(TContext context) { }
        protected abstract TResult ExecuteCore(TContext context);
        protected virtual void After(TContext context, TResult result) { }
        protected virtual void Finish(TContext context) { }
    }
    public static class Traversal
    {
        public static IEnumerable<T> DepthFirst<T>(T root, Func<T, IEnumerable<T>> children, IEqualityComparer<T> comparer = null) where T : class
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            var seen = new HashSet<T>(comparer); var stack = new Stack<T>(); if (root != null) stack.Push(root);
            while (stack.Count > 0)
            {
                var node = stack.Pop(); if (!seen.Add(node)) continue; yield return node;
                var list = new List<T>(children(node) ?? Array.Empty<T>());
                for (int i = list.Count - 1; i >= 0; i--) if (list[i] != null) stack.Push(list[i]);
            }
        }
        public static IEnumerable<T> BreadthFirst<T>(T root, Func<T, IEnumerable<T>> children, IEqualityComparer<T> comparer = null) where T : class
        {
            if (children == null) throw new ArgumentNullException(nameof(children));
            var seen = new HashSet<T>(comparer); var queue = new Queue<T>(); if (root != null) queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue(); if (!seen.Add(node)) continue; yield return node;
                foreach (var child in children(node) ?? Array.Empty<T>()) if (child != null) queue.Enqueue(child);
            }
        }
    }
}
