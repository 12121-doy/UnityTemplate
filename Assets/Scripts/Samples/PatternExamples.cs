using System;
using System.Collections.Generic;
using Guisha.Core.Patterns;
namespace Guisha.Samples
{
    public static class PatternExamples
    {
        private sealed class Stats { public int Health; }
        private sealed class CountVisitor : INodeVisitor<string>
        {
            public int Count;
            public void Enter(CompositeNode<string> node) { Count++; }
            public void Leave(CompositeNode<string> node) { }
        }
        private sealed class DoubleOperation : OperationTemplate<int, int>
        { protected override int ExecuteCore(int context) => context * 2; }
        public static string Run()
        {
            var healthFactory = new DelegateFactory<Stats>(() => new Stats { Health = 100 });
            var family = new ProductFamilyFactory<Stats, string>(healthFactory, new DelegateFactory<string>(() => "RedGhost"));
            var built = new ObjectBuilder<Stats>(healthFactory).With(s => s.Health = 150).Build();
            var clone = new Prototype<Stats>(built, s => new Stats { Health = s.Health }).Clone();
            float adapted = new DelegateAdapter<int, float>(value => value / 100f).Adapt(clone.Health);
            var messages = new List<string>();
            new TaggedMessageChannel("AI", new ActionMessageSink(messages.Add)).Write(family.CreateB());
            var tree = new CompositeNode<string>("Dungeon"); tree.Add(new CompositeNode<string>("Clue")); tree.Add(new CompositeNode<string>("Door"));
            var visitor = new CountVisitor(); tree.Accept(visitor);
            int traversed = 0; foreach (var node in Traversal.DepthFirst(tree, n => n.Children)) traversed++;
            var decorated = new TransformDecorator<int>(new DelegateProcessor<int>(value => value * 2), before: value => value + 1).Process(3);
            var weights = new FlyweightCache<string, string>(key => key.ToUpperInvariant());
            using (var proxy = new LazyProxy<Stats>(healthFactory.Create)) { proxy.Value.Health = 90; }
            int score = 0; var history = new CommandHistory(); history.Execute(new DelegateCommand(() => score += 10, () => score -= 10)); history.Undo(); history.Redo();
            var chain = new RuleChain<Stats>(new DelegateRule<Stats>(s => s.Health > 0), new DelegateRule<Stats>(s => { s.Health -= 5; return true; })); chain.Apply(clone);
            bool condition = ConditionInterpreter.Parse("clue && !locked").Evaluate(key => key == "clue");
            var mediator = new Mediator(); int answer;
            using (mediator.Register<int, int>(value => value + 1)) answer = mediator.Send<int, int>(41);
            var memories = new MementoStore<int[]>(values => (int[])values.Clone()); var state = new[] { 7 }; var snapshot = memories.Capture(state); state[0] = 0; state = memories.Restore(snapshot);
            var strategies = new StrategySelector<string, int, int>(); strategies.Register("double", new DelegateStrategy<int, int>(value => value * 2));
            return $"Patterns OK: HP={clone.Health}, adapted={adapted}, nodes={visitor.Count}/{traversed}, decorated={decorated}, flyweight={weights.Get("ghost")}, score={score}, condition={condition}, query={answer}, restored={state[0]}, strategy={strategies.Evaluate("double", 3)}, template={new DoubleOperation().Execute(4)}, bridge={messages[0]}";
        }
    }
}
