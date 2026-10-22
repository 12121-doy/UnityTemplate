using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using System.Linq; // 引入 System.Linq 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Core.Patterns; // 引入 Guisha.Core.Patterns 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using NUnit.Framework; // 引入 NUnit.Framework 命名空间，使用其中的类型与 API。

namespace Guisha.Tests // 将本文件类型归入 Guisha.Tests，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 23 种设计模式、动画驱动和玩法默认值的 EditMode 回归测试。
    /// </summary>
    public sealed class PatternRegressionTests // 23 种设计模式、动画驱动和玩法默认值的 EditMode 回归测试。
    { // 开始类型。
        /// <summary>
        /// 原型、工厂等模式测试使用的可变数据盒。
        /// </summary>
        private sealed class Box // 原型、工厂等模式测试使用的可变数据盒。
        { // 开始类型。
            /// <summary>
            /// 保存封装的数据值。
            /// </summary>
            public int Value; // 声明 封装的数据值，后续初始化。
        } // 结束类型。

        /// <summary>
        /// 记录组合树访问顺序的测试访问者。
        /// </summary>
        private sealed class Visitor : INodeVisitor<string> // 记录组合树访问顺序的测试访问者。
        { // 开始类型。
            /// <summary>
            /// 保存遍历顺序记录。
            /// </summary>
            public readonly List<string> Visits = new List<string>(); // 创建 List<string>，保存为 遍历顺序记录。
            /// <summary>
            /// 记录测试中的状态进入或节点进入回调。
            /// </summary>
            /// <param name="node">当前树节点或对白节点。</param>
            public void Enter(CompositeNode<string> node) // 记录测试中的状态进入或节点进入回调。
            { // 开始方法。
                Visits.Add("+" + node.Value); // 向 遍历顺序记录 加入 "+" + node.Value。
            } // 结束方法。

            /// <summary>
            /// 记录访问者离开节点的顺序。
            /// </summary>
            /// <param name="node">当前树节点或对白节点。</param>
            public void Leave(CompositeNode<string> node) // 记录访问者离开节点的顺序。
            { // 开始方法。
                Visits.Add("-" + node.Value); // 向 遍历顺序记录 加入 "-" + node.Value。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 验证模板方法清理顺序的测试操作。
        /// </summary>
        private sealed class Operation : OperationTemplate<int, int> // 验证模板方法清理顺序的测试操作。
        { // 开始类型。
            /// <summary>
            /// 保存最终清理调用次数。
            /// </summary>
            public int Finished; // 声明 最终清理调用次数，后续初始化。
            /// <summary>
            /// 输入为负时抛异常，模拟模板校验失败。
            /// </summary>
            /// <param name="context">当前操作上下文。</param>
            protected override void Validate(int context) // 输入为负时抛异常，模拟模板校验失败。
            { // 开始方法。
                if (context < 0) // 当当前操作上下文小于0时，执行下方分支。
                    throw new ArgumentException(); // 抛出 ArgumentException，拒绝无效参数或状态。
            } // 结束方法。

            /// <summary>
            /// 将测试输入乘二，作为模板核心操作。
            /// </summary>
            /// <param name="context">当前操作上下文。</param>
            /// <returns>int 类型的处理结果。</returns>
            protected override int ExecuteCore(int context) => context * 2; // 将测试输入乘二，作为模板核心操作。
            /// <summary>
            /// 记录最终清理次数，验证成功失败都调用。
            /// </summary>
            /// <param name="context">当前操作上下文。</param>
            protected override void Finish(int context) // 记录最终清理次数，验证成功失败都调用。
            { // 开始方法。
                Finished++; // 递增 最终清理调用次数。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 为测试暴露类型事件发送入口的门面。
        /// </summary>
        private sealed class Facade : FacadeBase // 为测试暴露类型事件发送入口的门面。
        { // 开始类型。
            /// <summary>
            /// 公开测试门面的 Send 包装器，转发整型事件。
            /// </summary>
            /// <param name="value">当前处理值。</param>
            public void Notify(int value) // 公开测试门面的 Send 包装器，转发整型事件。
            { // 开始方法。
                Send(value); // 向 EventBus 发布一个强类型请求。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 验证工厂每次创建独立产品。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void FactoryMethodCreatesFreshProducts() // 验证工厂每次创建独立产品。
        { // 开始方法。
            var factory = new DelegateFactory<Box>(() => new Box()); // 创建 DelegateFactory<Box>，保存为 创建产品或实体的工厂。
            Assert.That(factory.Create(), Is.Not.SameAs(factory.Create())); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证抽象工厂提供配套的两种产品。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void AbstractFactoryCreatesBothProductKinds() // 验证抽象工厂提供配套的两种产品。
        { // 开始方法。
            var factory = new ProductFamilyFactory<int, string>(new DelegateFactory<int>(() => 7), new DelegateFactory<string>(() => "ghost")); // 创建 ProductFamilyFactory<int, string>，保存为 创建产品或实体的工厂。
            Assert.That(factory.CreateA(), Is.EqualTo(7)); // 验证实际结果符合预期约束。
            Assert.That(factory.CreateB(), Is.EqualTo("ghost")); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证构建器可复用，后续配置不改写已经创建的产品。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void BuilderIsReusableAndDoesNotMutateOldProduct() // 验证构建器可复用，后续配置不改写已经创建的产品。
        { // 开始方法。
            var builder = new ObjectBuilder<Box>(new DelegateFactory<Box>(() => new Box())).With(x => x.Value = 9); // 把 new ObjectBuilder<Box>(new DelegateFactory<Box>(() => new Box())).With 的返回值 保存为 builder。
            var a = builder.Build(); // 把 builder.Build 的返回值 保存为 a。
            a.Value = 1; // 更新 a.Value，使用 1。
            Assert.That(builder.Build().Value, Is.EqualTo(9)); // 验证实际结果符合预期约束。
            Assert.That(a.Value, Is.EqualTo(1)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证原型复制函数生成独立的深复制数据。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void PrototypeUsesIndependentDeepCopies() // 验证原型复制函数生成独立的深复制数据。
        { // 开始方法。
            var source = new[] // 把 new[] {     4 } 保存为 原始来源。
            { // 开始成员/集合初始化。
                4 // 提供初始化/参数元素 4，交给上方的对象或集合使用。
            }; // 结束成员/集合初始化。
            var prototype = new Prototype<int[]>(source, x => (int[])x.Clone()); // 创建 Prototype<int[]>，保存为 prototype。
            var clone = prototype.Clone(); // 把 prototype.Clone 的返回值 保存为 深复制函数。
            clone[0] = 8; // 更新 clone[0]，使用 8。
            Assert.That(source[0], Is.EqualTo(4)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证适配器转换旧数据的百分比表示。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void AdapterConvertsLegacyPercentage() // 验证适配器转换旧数据的百分比表示。
        { // 开始方法。
            Assert.That(new DelegateAdapter<int, float>(x => x / 100f).Adapt(25), Is.EqualTo(0.25f)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证桥接的消息格式与输出后端可以独立组合。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void BridgeSeparatesFormatFromOutput() // 验证桥接的消息格式与输出后端可以独立组合。
        { // 开始方法。
            string result = null; // 把 null（清空引用） 保存为 本次操作结果。
            new TaggedMessageChannel("AI", new ActionMessageSink(x => result = x)).Write("heard"); // 在消息前添加 [tag]，再调用 Sink.Write。
            Assert.That(result, Is.EqualTo("[AI] heard")); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证组合树拒绝循环与多父节点，移除后可以重新挂接。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void CompositePreventsCyclesAndReparentsAfterDetach() // 验证组合树拒绝循环与多父节点，移除后可以重新挂接。
        { // 开始方法。
            var a = new CompositeNode<int>(1); // 创建 CompositeNode<int>，保存为 a。
            var b = new CompositeNode<int>(2); // 创建 CompositeNode<int>，保存为 b。
            a.Add(b); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            Assert.Throws<InvalidOperationException>(() => b.Add(a)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            Assert.Throws<InvalidOperationException>(() => a.Add(b)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            Assert.That(a.Remove(b), Is.True); // 验证实际结果符合预期约束。
            b.Add(a); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            Assert.That(a.Parent, Is.SameAs(b)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证访问者按树遍历顺序接收进入/离开回调。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void VisitorReceivesEnterAndLeaveInTreeOrder() // 验证访问者按树遍历顺序接收进入/离开回调。
        { // 开始方法。
            var root = new CompositeNode<string>("root"); // 创建 CompositeNode<string>，保存为 根节点或根路径。
            root.Add(new CompositeNode<string>("leaf")); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            var visitor = new Visitor(); // 创建 Visitor，保存为 树节点访问者。
            root.Accept(visitor); // 先 Enter 当前节点，再按子节点快照递归访问，最后 Leave 当前节点。
            CollectionAssert.AreEqual(new[] { "+root", "+leaf", "-leaf", "-root" }, visitor.Visits); // 逐项比较实际集合与预期集合，同时验证元素和顺序。
        } // 结束方法。

        /// <summary>
        /// 验证前置、内部处理、后置装饰的顺序。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void DecoratorPreservesOrder() // 验证前置、内部处理、后置装饰的顺序。
        { // 开始方法。
            var value = new TransformDecorator<int>(new DelegateProcessor<int>(x => x * 2), x => x + 1, x => x + 5).Process(3); // 把 new TransformDecorator<int>(new DelegateProcessor<int>(x => x * 2), x => x + 1, x => x + 5).Process 的返回值 保存为 当前处理值。
            Assert.That(value, Is.EqualTo(13)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证同键享元共享，缓存容量不超过上限。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void FlyweightsShareValuesAndBoundCache() // 验证同键享元共享，缓存容量不超过上限。
        { // 开始方法。
            int created = 0; // 把 0 保存为 已经创建的对象数。
            var cache = new FlyweightCache<int, Box>(_ => // 创建 FlyweightCache<int, Box>，保存为 缓存字典。
            { // 开始回调。
                created++; // 递增 已经创建的对象数。
                return new Box(); // 返回 新建的 Box 对象。
            }, 2); // 结束回调。
            var a = cache.Get(1); // 把 cache.Get 的返回值 保存为 a。
            Assert.That(cache.Get(1), Is.SameAs(a)); // 验证实际结果符合预期约束。
            cache.Get(2); // 命中时返回共享值；未命中时创建，达到容量则按入队顺序移除最旧键。
            cache.Get(3); // 命中时返回共享值；未命中时创建，达到容量则按入队顺序移除最旧键。
            Assert.That(cache.Count, Is.EqualTo(2)); // 验证实际结果符合预期约束。
            Assert.That(cache.Get(1), Is.Not.SameAs(a)); // 验证实际结果符合预期约束。
            Assert.That(created, Is.EqualTo(4)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证延迟代理只初始化一次且只释放一次。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void ProxyInitializesOnceAndDisposesOnce() // 验证延迟代理只初始化一次且只释放一次。
        { // 开始方法。
            int made = 0, disposed = 0; // 把 0 保存为 made；把 0 保存为 已释放标记。
            var proxy = new LazyProxy<Box>(() => // 创建 LazyProxy<Box>，保存为 proxy。
            { // 开始回调。
                made++; // 递增 made。
                return new Box(); // 返回 新建的 Box 对象。
            }, _ => disposed++); // 结束回调。
            Assert.That(made, Is.Zero); // 验证实际结果符合预期约束。
            Assert.That(proxy.Value, Is.SameAs(proxy.Value)); // 验证实际结果符合预期约束。
            proxy.Dispose(); // 幂等标记释放，已创建时调用 release 或 IDisposable.Dispose；未使用的代理不会为释放而创建对象。
            proxy.Dispose(); // 幂等标记释放，已创建时调用 release 或 IDisposable.Dispose；未使用的代理不会为释放而创建对象。
            Assert.That(made, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.That(disposed, Is.EqualTo(1)); // 验证实际结果符合预期约束。
            Assert.Throws<ObjectDisposedException>(() => // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            { // 开始回调。
                var unused = proxy.Value; // 把 proxy.Value 保存为 unused。
            }); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 验证未使用的代理在 Dispose 时不会创建对象。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void ProxyDoesNotCreateDuringUnusedDispose() // 验证未使用的代理在 Dispose 时不会创建对象。
        { // 开始方法。
            int made = 0; // 把 0 保存为 made。
            var proxy = new LazyProxy<Box>(() => // 创建 LazyProxy<Box>，保存为 proxy。
            { // 开始回调。
                made++; // 递增 made。
                return new Box(); // 返回 新建的 Box 对象。
            }); // 结束回调。
            proxy.Dispose(); // 幂等标记释放，已创建时调用 release 或 IDisposable.Dispose；未使用的代理不会为释放而创建对象。
            Assert.That(made, Is.Zero); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证职责链在第一条拒绝规则处停止。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void ChainStopsAtFirstRejection() // 验证职责链在第一条拒绝规则处停止。
        { // 开始方法。
            int count = 0; // 把 0 保存为 count。
            var chain = new RuleChain<Box>(new DelegateRule<Box>(_ => false), new DelegateRule<Box>(_ => // 创建 RuleChain<Box>，保存为 顺序处理链。
            { // 开始回调。
                count++; // 递增 count。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            })); // 结束回调。
            Assert.That(chain.Apply(new Box()), Is.False); // 验证实际结果符合预期约束。
            Assert.That(count, Is.Zero); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证命令撤销、重做，以及新提交清除旧重做分支。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void CommandsUndoRedoAndDiscardOldBranch() // 验证命令撤销、重做，以及新提交清除旧重做分支。
        { // 开始方法。
            int value = 0; // 把 0 保存为 当前处理值。
            var history = new CommandHistory(); // 创建 CommandHistory，保存为 history。
            history.Execute(new DelegateCommand(() => value += 2, () => value -= 2)); // 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
            history.Undo(); // 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
            history.Redo(); // 重新执行最后一个可重做命令；成功后移回撤销历史。
            Assert.That(value, Is.EqualTo(2)); // 验证实际结果符合预期约束。
            history.Undo(); // 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
            history.Execute(new DelegateCommand(() => value += 3, () => value -= 3)); // 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
            Assert.That(history.RedoCount, Is.Zero); // 验证实际结果符合预期约束。
            Assert.That(value, Is.EqualTo(3)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证失败命令不会破坏已有重做历史。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void FailedCommandKeepsRedoHistory() // 验证失败命令不会破坏已有重做历史。
        { // 开始方法。
            var history = new CommandHistory(); // 创建 CommandHistory，保存为 history。
            history.Execute(new DelegateCommand(() => // 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
            { // 开始回调。
            }, () => // 结束回调。
            { // 开始回调。
            })); // 结束回调。
            history.Undo(); // 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
            Assert.Throws<InvalidOperationException>(() => history.Execute(new DelegateCommand(() => throw new InvalidOperationException(), () => // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            { // 开始回调。
            }))); // 结束回调。
            Assert.That(history.RedoCount, Is.EqualTo(1)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证组合命令失败时回滚已完成子命令。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void CompositeCommandRollsBackCompletedChildren() // 验证组合命令失败时回滚已完成子命令。
        { // 开始方法。
            int value = 0; // 把 0 保存为 当前处理值。
            var command = new CompositeCommand(new DelegateCommand(() => value++, () => value--), new DelegateCommand(() => throw new InvalidOperationException(), () => // 创建 CompositeCommand，保存为 本次可撤销命令。
            { // 开始回调。
            })); // 结束回调。
            Assert.Throws<InvalidOperationException>(() => command.Execute()); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            Assert.That(value, Is.Zero); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证命令历史拒绝重入执行。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void RecursiveHistoryExecutionIsRejected() // 验证命令历史拒绝重入执行。
        { // 开始方法。
            var history = new CommandHistory(); // 创建 CommandHistory，保存为 history。
            Assert.Throws<InvalidOperationException>(() => history.Execute(new DelegateCommand(() => history.Undo(), () => // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            { // 开始回调。
            }))); // 结束回调。
            Assert.That(history.UndoCount, Is.Zero); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证解释器的逻辑优先级和短路求值。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void InterpreterHonorsPrecedenceAndShortCircuit() // 验证解释器的逻辑优先级和短路求值。
        { // 开始方法。
            var expression = ConditionInterpreter.Parse("clue && (!locked || key)"); // 把 ConditionInterpreter.Parse 的返回值 保存为 待解析的布尔表达式。
            Assert.That(expression.Evaluate(x => x == "clue" || x == "key"), Is.True); // 验证实际结果符合预期约束。
            Assert.That(ConditionInterpreter.Parse("true || missing").Evaluate(_ => throw new Exception()), Is.True); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证非法条件表达式被格式校验拒绝。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        [TestCase("")] // 注册一组参数化测试输入，由 Test Runner 分别执行。
        [TestCase("a &&")] // 注册一组参数化测试输入，由 Test Runner 分别执行。
        [TestCase("(a")] // 注册一组参数化测试输入，由 Test Runner 分别执行。
        [TestCase("a & b")] // 注册一组参数化测试输入，由 Test Runner 分别执行。
        [TestCase("a b")] // 注册一组参数化测试输入，由 Test Runner 分别执行。
        public void InterpreterRejectsMalformedInput(string value) // 验证非法条件表达式被格式校验拒绝。
        { // 开始方法。
            Assert.Throws<FormatException>(() => ConditionInterpreter.Parse(value)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
        } // 结束方法。

        /// <summary>
        /// 验证深度与广度优先遍历顺序。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void IteratorVisitsDepthAndBreadthInOrder() // 验证深度与广度优先遍历顺序。
        { // 开始方法。
            var a = new CompositeNode<int>(1); // 创建 CompositeNode<int>，保存为 a。
            var b = new CompositeNode<int>(2); // 创建 CompositeNode<int>，保存为 b。
            a.Add(b); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            a.Add(new CompositeNode<int>(3)); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            b.Add(new CompositeNode<int>(4)); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            CollectionAssert.AreEqual(new[] { 1, 2, 4, 3 }, Traversal.DepthFirst(a, x => x.Children).Select(x => x.Value)); // 逐项比较实际集合与预期集合，同时验证元素和顺序。
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, Traversal.BreadthFirst(a, x => x.Children).Select(x => x.Value)); // 逐项比较实际集合与预期集合，同时验证元素和顺序。
        } // 结束方法。

        /// <summary>
        /// 验证中介者校验响应类型，释放令牌后移除处理者。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void MediatorChecksResponseAndRemovesHandler() // 验证中介者校验响应类型，释放令牌后移除处理者。
        { // 开始方法。
            var mediator = new Mediator(); // 创建 Mediator，保存为 mediator。
            using (mediator.Register<int, string>(x => x.ToString())) // 进入 using 作用域；离开时自动 Dispose 释放拥有的对象。
            { // 开始自动释放作用域。
                Assert.That(mediator.Send<int, string>(42), Is.EqualTo("42")); // 验证实际结果符合预期约束。
                Assert.Throws<InvalidOperationException>(() => mediator.Send<int, int>(42)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
                Assert.Throws<ArgumentException>(() => mediator.Register<int, int>(x => x)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            } // 结束自动释放作用域。

            Assert.Throws<KeyNotFoundException>(() => mediator.Send<int, string>(0)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
        } // 结束方法。

        /// <summary>
        /// 验证备忘录捕获/恢复均复制数据，并拒绝其他 store 的快照。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void MementoCopiesOnCaptureAndRestoreAndRejectsForeignSnapshots() // 验证备忘录捕获/恢复均复制数据，并拒绝其他 store 的快照。
        { // 开始方法。
            var store = new MementoStore<int[]>(x => (int[])x.Clone()); // 创建 MementoStore<int[]>，保存为 store。
            var state = new[] // 把 new[] {     7 } 保存为 目标状态或保存的状态数据。
            { // 开始成员/集合初始化。
                7 // 提供初始化/参数元素 7，交给上方的对象或集合使用。
            }; // 结束成员/集合初始化。
            var snapshot = store.Capture(state); // 把 store.Capture 的返回值 保存为 所属 store 的状态快照。
            state[0] = 0; // 更新 state[0]，使用 0。
            var restored = store.Restore(snapshot); // 把 store.Restore 的返回值 保存为 restored。
            restored[0] = 1; // 更新 restored[0]，使用 1。
            Assert.That(store.Restore(snapshot)[0], Is.EqualTo(7)); // 验证实际结果符合预期约束。
            Assert.Throws<ArgumentException>(() => new MementoStore<int[]>(x => x).Restore(snapshot)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
        } // 结束方法。

        /// <summary>
        /// 验证策略通过明确的注册键选择。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void StrategiesAreSelectedExplicitly() // 验证策略通过明确的注册键选择。
        { // 开始方法。
            var selector = new StrategySelector<string, int, int>(); // 创建 StrategySelector<string, int, int>，保存为 selector。
            selector.Register("normal", new DelegateStrategy<int, int>(x => x)); // 为键注册一个非空策略；重复键会报错。
            selector.Register("hard", new DelegateStrategy<int, int>(x => x * 2)); // 为键注册一个非空策略；重复键会报错。
            Assert.That(selector.Evaluate("hard", 5), Is.EqualTo(10)); // 验证实际结果符合预期约束。
            Assert.Throws<KeyNotFoundException>(() => selector.Evaluate("missing", 1)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
        } // 结束方法。

        /// <summary>
        /// 验证模板方法无论成功失败均执行最终清理。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void TemplateAlwaysFinishesOnSuccessAndFailure() // 验证模板方法无论成功失败均执行最终清理。
        { // 开始方法。
            var operation = new Operation(); // 创建 Operation，保存为 Unity 异步操作。
            Assert.That(operation.Execute(3), Is.EqualTo(6)); // 验证实际结果符合预期约束。
            Assert.Throws<ArgumentException>(() => operation.Execute(-1)); // 断言回调抛出指定异常类型，验证非法操作被正确拒绝。
            Assert.That(operation.Finished, Is.EqualTo(2)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证门面转发强类型事件。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void FacadePublishesTypedRequest() // 验证门面转发强类型事件。
        { // 开始方法。
            int received = 0; // 把 0 保存为 received。
            using (EventBus.Subscribe<int>(x => received = x)) // 进入 using 作用域；离开时自动 Dispose 释放拥有的对象。
                new Facade().Notify(9); // 公开测试门面的 Send 包装器，转发整型事件。
            Assert.That(received, Is.EqualTo(9)); // 验证实际结果符合预期约束。
        } // 结束方法。

        /// <summary>
        /// 验证世界标记命令撤销后恢复原值。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void WorldCommandRestoresPreviousFlag() // 验证世界标记命令撤销后恢复原值。
        { // 开始方法。
            string key = "test.pattern.command"; // 把 "test.pattern.command" 保存为 查找键。
            WorldState.Remove(key); // 移除标记，移除成功时广播变化并返回 true。
            var history = new CommandHistory(); // 创建 CommandHistory，保存为 history。
            history.Execute(new SetWorldFlagCommand(key)); // 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
            Assert.That(WorldState.Has(key), Is.True); // 验证实际结果符合预期约束。
            history.Undo(); // 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
            Assert.That(WorldState.Has(key), Is.False); // 验证实际结果符合预期约束。
            history.Redo(); // 重新执行最后一个可重做命令；成功后移回撤销历史。
            Assert.That(WorldState.Has(key), Is.True); // 验证实际结果符合预期约束。
            WorldState.Remove(key); // 移除标记，移除成功时广播变化并返回 true。
        } // 结束方法。

        /// <summary>
        /// 验证缺少动画组件时工厂使用安全空对象。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void AnimationFactorySupportsMissingAnimators() // 验证缺少动画组件时工厂使用安全空对象。
        { // 开始方法。
            var driver = new AnimatorDriverFactory(null).Create(); // 把 new AnimatorDriverFactory(null).Create 的返回值 保存为 统一动画驱动。
            Assert.That(driver.Available, Is.False); // 验证实际结果符合预期约束。
            driver.Play("Idle"); // 播放指定动作状态。
            driver.Bool("Dead", false); // 设置布尔动画参数。
            driver.Float("Speed", 2); // 设置浮点动画参数。
            driver.Trigger("Attack"); // 触发动画参数。
        } // 结束方法。

        /// <summary>
        /// 验证四种玩法的默认规则彼此不同。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void GameModeDefaultsProvideDistinctRules() // 验证四种玩法的默认规则彼此不同。
        { // 开始方法。
            Assert.That(GameModeRules.Default(GameMode.Exploration).HostileGhosts, Is.False); // 验证实际结果符合预期约束。
            Assert.That(GameModeRules.Default(GameMode.Exploration).PlayerDamage, Is.Zero); // 验证实际结果符合预期约束。
            Assert.That(GameModeRules.Default(GameMode.Challenge).AllowSave, Is.False); // 验证实际结果符合预期约束。
            Assert.That(GameModeRules.Default(GameMode.Challenge).TimeLimit, Is.GreaterThan(0)); // 验证实际结果符合预期约束。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
