using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core.Patterns; // 引入 Guisha.Core.Patterns 命名空间，使用其中的类型与 API。

namespace Guisha.Samples // 将本文件类型归入 Guisha.Samples，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 组合运行新增设计模式的无场景示例。
    /// </summary>
    public static class PatternExamples // 组合运行新增设计模式的无场景示例。
    { // 开始类型。
        /// <summary>
        /// 设计模式示例用的可复制数据。
        /// </summary>
        private sealed class Stats // 设计模式示例用的可复制数据。
        { // 开始类型。
            /// <summary>
            /// 保存当前生命值。
            /// </summary>
            public int Health; // 声明 当前生命值，后续初始化。
        } // 结束类型。

        /// <summary>
        /// 演示访问者，统计节点数。
        /// </summary>
        private sealed class CountVisitor : INodeVisitor<string> // 演示访问者，统计节点数。
        { // 开始类型。
            /// <summary>
            /// 保存当前元素数量。
            /// </summary>
            public int Count; // 声明 当前元素数量，后续初始化。
            /// <summary>
            /// 进入节点时递增统计。
            /// </summary>
            /// <param name="node">当前树节点或对白节点。</param>
            public void Enter(CompositeNode<string> node) // 进入节点时递增统计。
            { // 开始方法。
                Count++; // 递增 当前元素数量。
            } // 结束方法。

            /// <summary>
            /// 访问者离开钩子的演示空实现。
            /// </summary>
            /// <param name="node">当前树节点或对白节点。</param>
            public void Leave(CompositeNode<string> node) // 访问者离开钩子的演示空实现。
            { // 开始方法。
            } // 结束方法。
        } // 结束类型。

        /// <summary>
        /// 演示模板方法，将输入乘二。
        /// </summary>
        private sealed class DoubleOperation : OperationTemplate<int, int> // 演示模板方法，将输入乘二。
        { // 开始类型。
            /// <summary>
            /// 模板方法示例核心：输入乘二。
            /// </summary>
            /// <param name="context">当前操作上下文。</param>
            /// <returns>int 类型的处理结果。</returns>
            protected override int ExecuteCore(int context) => context * 2; // 模板方法示例核心：输入乘二。
        } // 结束类型。

        /// <summary>
        /// 组合运行工厂、建造者、原型、结构型和行为型模式示例，返回结果文本。
        /// </summary>
        /// <returns>string 类型的处理结果。</returns>
        public static string Run() // 组合运行工厂、建造者、原型、结构型和行为型模式示例，返回结果文本。
        { // 开始方法。
            var healthFactory = new DelegateFactory<Stats>(() => new Stats { Health = 100 }); // 创建 DelegateFactory<Stats>，保存为 healthFactory。
            var family = new ProductFamilyFactory<Stats, string>(healthFactory, new DelegateFactory<string>(() => "RedGhost")); // 创建 ProductFamilyFactory<Stats, string>，保存为 family。
            var built = new ObjectBuilder<Stats>(healthFactory).With(s => s.Health = 150).Build(); // 把 new ObjectBuilder<Stats>(healthFactory).With(s => s.Health = 150).Build 的返回值 保存为 built。
            var clone = new Prototype<Stats>(built, s => new Stats { Health = s.Health }).Clone(); // 把 new Prototype<Stats>(built, s => new Stats { Health = s.Health }).Clone 的返回值 保存为 深复制函数。
            float adapted = new DelegateAdapter<int, float>(value => value / 100f).Adapt(clone.Health); // 把 new DelegateAdapter<int, float>(value => value / 100f).Adapt 的返回值 保存为 adapted。
            var messages = new List<string>(); // 创建 List<string>，保存为 messages。
            new TaggedMessageChannel("AI", new ActionMessageSink(messages.Add)).Write(family.CreateB()); // 在消息前添加 [tag]，再调用 Sink.Write。
            var tree = new CompositeNode<string>("Dungeon"); // 创建 CompositeNode<string>，保存为 tree。
            tree.Add(new CompositeNode<string>("Clue")); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            tree.Add(new CompositeNode<string>("Door")); // 检查 null、祖先循环和已有父节点，成功后挂入子节点并设置 Parent。
            var visitor = new CountVisitor(); // 创建 CountVisitor，保存为 树节点访问者。
            tree.Accept(visitor); // 先 Enter 当前节点，再按子节点快照递归访问，最后 Leave 当前节点。
            int traversed = 0; // 把 0 保存为 traversed。
            foreach (var node in Traversal.DepthFirst(tree, n => n.Children)) // 遍历 Traversal.DepthFirst(tree, n => n.Children)，每次把一个元素保存为 node。
                traversed++; // 递增 traversed。
            var decorated = new TransformDecorator<int>(new DelegateProcessor<int>(value => value * 2), before: value => value + 1).Process(3); // 把 new TransformDecorator<int>(new DelegateProcessor<int>(value => value * 2), before: value => value + 1).Process 的返回值 保存为 decorated。
            var weights = new FlyweightCache<string, string>(key => key.ToUpperInvariant()); // 创建 FlyweightCache<string, string>，保存为 weights。
            using (var proxy = new LazyProxy<Stats>(healthFactory.Create)) // 进入 using 作用域；离开时自动 Dispose 释放拥有的对象。
            { // 开始自动释放作用域。
                proxy.Value.Health = 90; // 更新 proxy.Value.Health，使用 90。
            } // 结束自动释放作用域。

            int score = 0; // 把 0 保存为 score。
            var history = new CommandHistory(); // 创建 CommandHistory，保存为 history。
            history.Execute(new DelegateCommand(() => score += 10, () => score -= 10)); // 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
            history.Undo(); // 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
            history.Redo(); // 重新执行最后一个可重做命令；成功后移回撤销历史。
            var chain = new RuleChain<Stats>(new DelegateRule<Stats>(s => s.Health > 0), new DelegateRule<Stats>(s => // 创建 RuleChain<Stats>，保存为 顺序处理链。
            { // 开始回调。
                s.Health -= 5; // 扣减 s.Health，使用 5。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            })); // 结束回调。
            chain.Apply(clone); // 按构造时复制的规则顺序执行，遇到 false 立即返回；全部通过则返回 true。
            bool condition = ConditionInterpreter.Parse("clue && !locked").Evaluate(key => key == "clue"); // 把 ConditionInterpreter.Parse("clue && !locked").Evaluate 的返回值 保存为 已解析的机关条件树。
            var mediator = new Mediator(); // 创建 Mediator，保存为 mediator。
            int answer; // 声明 answer，后续初始化。
            using (mediator.Register<int, int>(value => value + 1)) // 进入 using 作用域；离开时自动 Dispose 释放拥有的对象。
                answer = mediator.Send<int, int>(41); // 更新 answer，使用 mediator.Send<int, int> 的返回值。
            var memories = new MementoStore<int[]>(values => (int[])values.Clone()); // 创建 MementoStore<int[]>，保存为 memories。
            var state = new[] // 把 new[] {     7 } 保存为 目标状态或保存的状态数据。
            { // 开始成员/集合初始化。
                7 // 提供初始化/参数元素 7，交给上方的对象或集合使用。
            }; // 结束成员/集合初始化。
            var snapshot = memories.Capture(state); // 把 memories.Capture 的返回值 保存为 所属 store 的状态快照。
            state[0] = 0; // 更新 state[0]，使用 0。
            state = memories.Restore(snapshot); // 更新 目标状态或保存的状态数据，使用 memories.Restore 的返回值。
            var strategies = new StrategySelector<string, int, int>(); // 创建 StrategySelector<string, int, int>，保存为 策略注册表。
            strategies.Register("double", new DelegateStrategy<int, int>(value => value * 2)); // 为键注册一个非空策略；重复键会报错。
            return $"Patterns OK: HP={clone.Health}, adapted={adapted}, nodes={visitor.Count}/{traversed}, decorated={decorated}, flyweight={weights.Get("ghost")}, score={score}, condition={condition}, query={answer}, restored={state[0]}, strategy={strategies.Evaluate("double", 3)}, template={new DoubleOperation().Execute(4)}, bridge={messages[0]}"; // 返回 $"Patterns OK: HP={clone.Health}, adapted={adapted}, nodes={visitor.Count}/{traversed}, decorated={decorated}, flyweight={weights.Get("ghost")}, score={score}, condition={condition}, query={answer}, restored={state[0]}, strategy={strategies.Evaluate("double", 3)}, template={new DoubleOperation().Execute(4)}, bridge={messages[0]}"。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
