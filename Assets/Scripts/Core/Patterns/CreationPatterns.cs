using System;
using System.Collections.Generic;
namespace Guisha.Core.Patterns
{
    public interface IFactory<out T> { T Create(); }
    public abstract class Factory<T> : IFactory<T>
    {
        public T Create() { var product = CreateProduct(); OnCreated(product); return product; }
        protected abstract T CreateProduct();
        protected virtual void OnCreated(T product) { }
    }
    public sealed class DelegateFactory<T> : Factory<T>
    {
        private readonly Func<T> create;
        public DelegateFactory(Func<T> create) { this.create = create ?? throw new ArgumentNullException(nameof(create)); }
        protected override T CreateProduct() => create();
    }
    public interface IAbstractFactory<out TA, out TB> { TA CreateA(); TB CreateB(); }
    public sealed class ProductFamilyFactory<TA, TB> : IAbstractFactory<TA, TB>
    {
        private readonly IFactory<TA> a;
        private readonly IFactory<TB> b;
        public ProductFamilyFactory(IFactory<TA> a, IFactory<TB> b)
        { this.a = a ?? throw new ArgumentNullException(nameof(a)); this.b = b ?? throw new ArgumentNullException(nameof(b)); }
        public TA CreateA() => a.Create();
        public TB CreateB() => b.Create();
    }
    // Every Build starts from a fresh factory product; configuring the builder never mutates a live object.
    public sealed class ObjectBuilder<T>
    {
        private readonly IFactory<T> factory;
        private readonly List<Action<T>> steps = new List<Action<T>>();
        public ObjectBuilder(IFactory<T> factory) { this.factory = factory ?? throw new ArgumentNullException(nameof(factory)); }
        public ObjectBuilder<T> With(Action<T> configure)
        { steps.Add(configure ?? throw new ArgumentNullException(nameof(configure))); return this; }
        public T Build() { var product = factory.Create(); foreach (var step in steps.ToArray()) step(product); return product; }
    }
    public interface IPrototype<out T> { T Clone(); }
    public sealed class Prototype<T> : IPrototype<T>
    {
        private readonly T template;
        private readonly Func<T, T> copy;
        public Prototype(T template, Func<T, T> deepCopy)
        { this.template = template; copy = deepCopy ?? throw new ArgumentNullException(nameof(deepCopy)); }
        public T Clone() => copy(template);
    }
}
