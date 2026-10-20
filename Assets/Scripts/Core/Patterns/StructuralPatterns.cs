using System;
using System.Collections.Generic;
namespace Guisha.Core.Patterns
{
    public interface IAdapter<in TSource, out TResult> { TResult Adapt(TSource source); }
    public sealed class DelegateAdapter<TSource, TResult> : IAdapter<TSource, TResult>
    {
        private readonly Func<TSource, TResult> convert;
        public DelegateAdapter(Func<TSource, TResult> convert) { this.convert = convert ?? throw new ArgumentNullException(nameof(convert)); }
        public TResult Adapt(TSource source) => convert(source);
    }
    public interface IMessageSink { void Write(string message); }
    public sealed class ActionMessageSink : IMessageSink
    {
        private readonly Action<string> output;
        public ActionMessageSink(Action<string> output) { this.output = output ?? throw new ArgumentNullException(nameof(output)); }
        public void Write(string message) => output(message);
    }
    // Bridge: formatting hierarchy and output backend can vary independently.
    public abstract class MessageChannel
    {
        protected IMessageSink Sink { get; }
        protected MessageChannel(IMessageSink sink) { Sink = sink ?? throw new ArgumentNullException(nameof(sink)); }
        public abstract void Write(string message);
    }
    public sealed class TaggedMessageChannel : MessageChannel
    {
        private readonly string tag;
        public TaggedMessageChannel(string tag, IMessageSink sink) : base(sink) { this.tag = tag; }
        public override void Write(string message) => Sink.Write("[" + tag + "] " + message);
    }
    public interface INodeVisitor<T>
    {
        void Enter(CompositeNode<T> node);
        void Leave(CompositeNode<T> node);
    }
    public sealed class CompositeNode<T>
    {
        private readonly List<CompositeNode<T>> children = new List<CompositeNode<T>>();
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<CompositeNode<T>> view;
        public T Value { get; }
        public CompositeNode<T> Parent { get; private set; }
        public IReadOnlyList<CompositeNode<T>> Children => view;
        public CompositeNode(T value) { Value = value; view = children.AsReadOnly(); }
        public void Add(CompositeNode<T> child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            for (var node = this; node != null; node = node.Parent)
                if (ReferenceEquals(node, child)) throw new InvalidOperationException("Composite cycle.");
            if (child.Parent != null) throw new InvalidOperationException("Detach child before reparenting.");
            children.Add(child); child.Parent = this;
        }
        public bool Remove(CompositeNode<T> child)
        { if (!children.Remove(child)) return false; child.Parent = null; return true; }
        public void Accept(INodeVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            visitor.Enter(this); foreach (var child in children.ToArray()) child.Accept(visitor); visitor.Leave(this);
        }
    }
    public interface IProcessor<T> { T Process(T value); }
    public sealed class DelegateProcessor<T> : IProcessor<T>
    {
        private readonly Func<T, T> process;
        public DelegateProcessor(Func<T, T> process) { this.process = process ?? throw new ArgumentNullException(nameof(process)); }
        public T Process(T value) => process(value);
    }
    public abstract class ProcessorDecorator<T> : IProcessor<T>
    {
        protected IProcessor<T> Inner { get; }
        protected ProcessorDecorator(IProcessor<T> inner) { Inner = inner ?? throw new ArgumentNullException(nameof(inner)); }
        public abstract T Process(T value);
    }
    public sealed class TransformDecorator<T> : ProcessorDecorator<T>
    {
        private readonly Func<T, T> before, after;
        public TransformDecorator(IProcessor<T> inner, Func<T, T> before = null, Func<T, T> after = null) : base(inner)
        { this.before = before; this.after = after; }
        public override T Process(T value) { if (before != null) value = before(value); value = Inner.Process(value); return after != null ? after(value) : value; }
    }
    // Cache immutable metadata only. Eviction never disposes objects still used by callers.
    public sealed class FlyweightCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> cache;
        private readonly Queue<TKey> order = new Queue<TKey>();
        private readonly Func<TKey, TValue> create;
        private readonly int capacity;
        public int Count => cache.Count;
        public FlyweightCache(Func<TKey, TValue> create, int capacity = 256, IEqualityComparer<TKey> comparer = null)
        {
            this.create = create ?? throw new ArgumentNullException(nameof(create));
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            this.capacity = capacity; cache = new Dictionary<TKey, TValue>(comparer);
        }
        public TValue Get(TKey key)
        {
            if (cache.TryGetValue(key, out var value)) return value;
            value = create(key);
            if (cache.Count >= capacity) cache.Remove(order.Dequeue());
            cache.Add(key, value); order.Enqueue(key); return value;
        }
        public void Clear() { cache.Clear(); order.Clear(); }
    }
    public sealed class LazyProxy<T> : IDisposable where T : class
    {
        private readonly Func<T> create;
        private readonly Action<T> release;
        private T instance;
        private bool disposed;
        public bool IsCreated => instance != null;
        public LazyProxy(Func<T> create, Action<T> release = null)
        { this.create = create ?? throw new ArgumentNullException(nameof(create)); this.release = release; }
        public T Value
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(nameof(LazyProxy<T>));
                if (instance == null) instance = create() ?? throw new InvalidOperationException("Proxy factory returned null.");
                return instance;
            }
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            var value = instance; instance = null;
            if (value == null) return;
            if (release != null) release(value); else (value as IDisposable)?.Dispose();
        }
    }
}
