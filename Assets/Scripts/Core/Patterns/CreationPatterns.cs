using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。

namespace Guisha.Core.Patterns // 将本文件类型归入 Guisha.Core.Patterns，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 声明单产品创建契约。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public interface IFactory<out T> // 声明单产品创建契约。
    { // 开始接口。
        /// <summary>
        /// 按工厂契约创建并返回产品。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        T Create(); // 按工厂契约创建并返回产品。
    } // 结束接口。

    /// <summary>
    /// 工厂方法基类；Create 组织创建流程，CreateProduct 由派生类提供。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public abstract class Factory<T> : IFactory<T> // 工厂方法基类；Create 组织创建流程，CreateProduct 由派生类提供。
    { // 开始类型。
        /// <summary>
        /// 调用 CreateProduct 生成产品，再调用 OnCreated，最后返回产品。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        public T Create() // 调用 CreateProduct 生成产品，再调用 OnCreated，最后返回产品。
        { // 开始方法。
            var product = CreateProduct(); // 把 CreateProduct 的返回值 保存为 刚创建的产品。
            OnCreated(product); // 产品创建后的可选钩子，默认不处理。
            return product; // 返回 刚创建的产品。
        } // 结束方法。

        /// <summary>
        /// 必须由派生工厂实现的产品创建钩子。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        protected abstract T CreateProduct(); // 必须由派生工厂实现的产品创建钩子。
        /// <summary>
        /// 产品创建后的可选钩子，默认不处理。
        /// </summary>
        /// <param name="product">刚创建的产品。</param>
        protected virtual void OnCreated(T product) // 产品创建后的可选钩子，默认不处理。
        { // 开始方法。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 通过委托提供具体产品的工厂。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class DelegateFactory<T> : Factory<T> // 通过委托提供具体产品的工厂。
    { // 开始类型。
        /// <summary>
        /// 保存创建对象的委托。
        /// </summary>
        private readonly Func<T> create; // 声明 创建对象的委托，后续初始化。
        /// <summary>
        /// 构造 DelegateFactory，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="create">创建对象的委托；由调用方注入具体行为。</param>
        public DelegateFactory(Func<T> create) // 构造 DelegateFactory，初始化其依赖与状态。
        { // 开始构造函数。
            this.create = create ?? throw new ArgumentNullException(nameof(create)); // 更新 创建对象的委托，使用 create ?? throw new ArgumentNullException(nameof(create))。
        } // 结束构造函数。

        /// <summary>
        /// 调用构造时注入的 create 委托创建产品。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        protected override T CreateProduct() => create(); // 调用构造时注入的 create 委托创建产品。
    } // 结束类型。

    /// <summary>
    /// 声明成套创建两类相关产品的契约。
    /// </summary>
    /// <typeparam name="TA">产品族中的 A 产品类型。</typeparam>
    /// <typeparam name="TB">产品族中的 B 产品类型。</typeparam>
    public interface IAbstractFactory<out TA, out TB> // 声明成套创建两类相关产品的契约。
    { // 开始接口。
        /// <summary>
        /// 创建产品族的 A 产品。
        /// </summary>
        /// <returns>本次创建或复制的 TA 对象。</returns>
        TA CreateA(); // 创建产品族的 A 产品。
        /// <summary>
        /// 创建产品族的 B 产品。
        /// </summary>
        /// <returns>本次创建或复制的 TB 对象。</returns>
        TB CreateB(); // 创建产品族的 B 产品。
    } // 结束接口。

    /// <summary>
    /// 组合两个工厂，创建同一产品族的两个产品。
    /// </summary>
    /// <typeparam name="TA">产品族中的 A 产品类型。</typeparam>
    /// <typeparam name="TB">产品族中的 B 产品类型。</typeparam>
    public sealed class ProductFamilyFactory<TA, TB> : IAbstractFactory<TA, TB> // 组合两个工厂，创建同一产品族的两个产品。
    { // 开始类型。
        /// <summary>
        /// 保存A 产品工厂。
        /// </summary>
        private readonly IFactory<TA> a; // 声明 A 产品工厂，后续初始化。
        /// <summary>
        /// 保存B 产品工厂。
        /// </summary>
        private readonly IFactory<TB> b; // 声明 B 产品工厂，后续初始化。
        /// <summary>
        /// 构造 ProductFamilyFactory，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="a">A 产品工厂。</param>
        /// <param name="b">B 产品工厂。</param>
        public ProductFamilyFactory(IFactory<TA> a, IFactory<TB> b) // 构造 ProductFamilyFactory，初始化其依赖与状态。
        { // 开始构造函数。
            this.a = a ?? throw new ArgumentNullException(nameof(a)); // 更新 A 产品工厂，使用 a ?? throw new ArgumentNullException(nameof(a))。
            this.b = b ?? throw new ArgumentNullException(nameof(b)); // 更新 B 产品工厂，使用 b ?? throw new ArgumentNullException(nameof(b))。
        } // 结束构造函数。

        /// <summary>
        /// 通过 A 工厂创建产品族中的 A 产品。
        /// </summary>
        /// <returns>本次创建或复制的 TA 对象。</returns>
        public TA CreateA() => a.Create(); // 通过 A 工厂创建产品族中的 A 产品。
        /// <summary>
        /// 通过 B 工厂创建产品族中的 B 产品。
        /// </summary>
        /// <returns>本次创建或复制的 TB 对象。</returns>
        public TB CreateB() => b.Create(); // 通过 B 工厂创建产品族中的 B 产品。
    } // 结束类型。

    // 每次 Build 都从新产品开始；配置构建器不会修改先前已经创建的产品。
    /// <summary>
    /// 记录配置步骤，每次 Build 都先创建新对象，再执行配置。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class ObjectBuilder<T> // 记录配置步骤，每次 Build 都先创建新对象，再执行配置。
    { // 开始类型。
        /// <summary>
        /// 保存创建产品或实体的工厂。
        /// </summary>
        private readonly IFactory<T> factory; // 声明 创建产品或实体的工厂，后续初始化。
        /// <summary>
        /// 保存构建配置步骤列表。
        /// </summary>
        private readonly List<Action<T>> steps = new List<Action<T>>(); // 创建 List<Action<T>>，保存为 构建配置步骤列表。
        /// <summary>
        /// 构造 ObjectBuilder，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="factory">创建产品或实体的工厂。</param>
        public ObjectBuilder(IFactory<T> factory) // 构造 ObjectBuilder，初始化其依赖与状态。
        { // 开始构造函数。
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory)); // 更新 创建产品或实体的工厂，使用 factory ?? throw new ArgumentNullException(nameof(factory))。
        } // 结束构造函数。

        /// <summary>
        /// 添加一个配置步骤并返回自身，支持链式调用。
        /// </summary>
        /// <param name="configure">产品配置步骤；由调用方注入具体行为。</param>
        /// <returns>当前构建器，支持继续链式配置。</returns>
        public ObjectBuilder<T> With(Action<T> configure) // 添加一个配置步骤并返回自身，支持链式调用。
        { // 开始方法。
            steps.Add(configure ?? throw new ArgumentNullException(nameof(configure))); // 向 构建配置步骤列表 加入 configure ?? throw new ArgumentNullException(nameof(configure))。
            return this; // 返回当前对象，支持继续链式调用。
        } // 结束方法。

        /// <summary>
        /// 创建新产品，按步骤快照逐一配置；重复 Build 创建独立产品。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        public T Build() // 创建新产品，按步骤快照逐一配置；重复 Build 创建独立产品。
        { // 开始方法。
            var product = factory.Create(); // 把 factory.Create 的返回值 保存为 刚创建的产品。
            foreach (var step in steps.ToArray()) // 遍历 steps.ToArray()，每次把一个元素保存为 step。
                step(product); // 对新产品执行本条构建配置步骤。
            return product; // 返回 刚创建的产品。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 声明原型复制契约。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public interface IPrototype<out T> // 声明原型复制契约。
    { // 开始接口。
        /// <summary>
        /// 复制模板并返回副本。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        T Clone(); // 复制模板并返回副本。
    } // 结束接口。

    /// <summary>
    /// 保存模板与复制函数，由复制函数定义复制深度。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class Prototype<T> : IPrototype<T> // 保存模板与复制函数，由复制函数定义复制深度。
    { // 开始类型。
        /// <summary>
        /// 保存原型模板。
        /// </summary>
        private readonly T template; // 声明 原型模板，后续初始化。
        /// <summary>
        /// 保存复制函数。
        /// </summary>
        private readonly Func<T, T> copy; // 声明 复制函数，后续初始化。
        /// <summary>
        /// 构造 Prototype，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="template">原型模板。</param>
        /// <param name="deepCopy">由调用方提供的深复制函数；由调用方注入具体行为。</param>
        public Prototype(T template, Func<T, T> deepCopy) // 构造 Prototype，初始化其依赖与状态。
        { // 开始构造函数。
            this.template = template; // 更新 原型模板，使用 原型模板。
            copy = deepCopy ?? throw new ArgumentNullException(nameof(deepCopy)); // 更新 复制函数，使用 deepCopy ?? throw new ArgumentNullException(nameof(deepCopy))。
        } // 结束构造函数。

        /// <summary>
        /// 对保存的模板调用 deepCopy 委托；复制深度由该委托保证。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        public T Clone() => copy(template); // 对保存的模板调用 deepCopy 委托；复制深度由该委托保证。
    } // 结束类型。
} // 结束命名空间。
