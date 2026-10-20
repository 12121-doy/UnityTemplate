using System;
using System.Collections.Generic;
using System.Linq;
using Guisha.Core;
using Guisha.Core.Patterns;
using Guisha.Framework;
using NUnit.Framework;
namespace Guisha.Tests
{
    public sealed class PatternRegressionTests
    {
        private sealed class Box { public int Value; }
        private sealed class Visitor : INodeVisitor<string>
        {
            public readonly List<string> Visits = new List<string>();
            public void Enter(CompositeNode<string> node) { Visits.Add("+" + node.Value); }
            public void Leave(CompositeNode<string> node) { Visits.Add("-" + node.Value); }
        }
        private sealed class Operation : OperationTemplate<int, int>
        {
            public int Finished;
            protected override void Validate(int context) { if (context < 0) throw new ArgumentException(); }
            protected override int ExecuteCore(int context) => context * 2;
            protected override void Finish(int context) { Finished++; }
        }
        private sealed class Facade : FacadeBase { public void Notify(int value) { Send(value); } }
        [Test] public void FactoryMethodCreatesFreshProducts()
        { var factory = new DelegateFactory<Box>(() => new Box()); Assert.That(factory.Create(), Is.Not.SameAs(factory.Create())); }
        [Test] public void AbstractFactoryCreatesBothProductKinds()
        { var factory = new ProductFamilyFactory<int, string>(new DelegateFactory<int>(() => 7), new DelegateFactory<string>(() => "ghost")); Assert.That(factory.CreateA(), Is.EqualTo(7)); Assert.That(factory.CreateB(), Is.EqualTo("ghost")); }
        [Test] public void BuilderIsReusableAndDoesNotMutateOldProduct()
        { var builder = new ObjectBuilder<Box>(new DelegateFactory<Box>(() => new Box())).With(x => x.Value = 9); var a = builder.Build(); a.Value = 1; Assert.That(builder.Build().Value, Is.EqualTo(9)); Assert.That(a.Value, Is.EqualTo(1)); }
        [Test] public void PrototypeUsesIndependentDeepCopies()
        { var source = new[] { 4 }; var prototype = new Prototype<int[]>(source, x => (int[])x.Clone()); var clone = prototype.Clone(); clone[0] = 8; Assert.That(source[0], Is.EqualTo(4)); }
        [Test] public void AdapterConvertsLegacyPercentage()
        { Assert.That(new DelegateAdapter<int, float>(x => x / 100f).Adapt(25), Is.EqualTo(0.25f)); }
        [Test] public void BridgeSeparatesFormatFromOutput()
        { string result = null; new TaggedMessageChannel("AI", new ActionMessageSink(x => result = x)).Write("heard"); Assert.That(result, Is.EqualTo("[AI] heard")); }
        [Test] public void CompositePreventsCyclesAndReparentsAfterDetach()
        { var a = new CompositeNode<int>(1); var b = new CompositeNode<int>(2); a.Add(b); Assert.Throws<InvalidOperationException>(() => b.Add(a)); Assert.Throws<InvalidOperationException>(() => a.Add(b)); Assert.That(a.Remove(b), Is.True); b.Add(a); Assert.That(a.Parent, Is.SameAs(b)); }
        [Test] public void VisitorReceivesEnterAndLeaveInTreeOrder()
        { var root = new CompositeNode<string>("root"); root.Add(new CompositeNode<string>("leaf")); var visitor = new Visitor(); root.Accept(visitor); CollectionAssert.AreEqual(new[] { "+root", "+leaf", "-leaf", "-root" }, visitor.Visits); }
        [Test] public void DecoratorPreservesOrder()
        { var value = new TransformDecorator<int>(new DelegateProcessor<int>(x => x * 2), x => x + 1, x => x + 5).Process(3); Assert.That(value, Is.EqualTo(13)); }
        [Test] public void FlyweightsShareValuesAndBoundCache()
        { int created = 0; var cache = new FlyweightCache<int, Box>(_ => { created++; return new Box(); }, 2); var a = cache.Get(1); Assert.That(cache.Get(1), Is.SameAs(a)); cache.Get(2); cache.Get(3); Assert.That(cache.Count, Is.EqualTo(2)); Assert.That(cache.Get(1), Is.Not.SameAs(a)); Assert.That(created, Is.EqualTo(4)); }
        [Test] public void ProxyInitializesOnceAndDisposesOnce()
        { int made = 0, disposed = 0; var proxy = new LazyProxy<Box>(() => { made++; return new Box(); }, _ => disposed++); Assert.That(made, Is.Zero); Assert.That(proxy.Value, Is.SameAs(proxy.Value)); proxy.Dispose(); proxy.Dispose(); Assert.That(made, Is.EqualTo(1)); Assert.That(disposed, Is.EqualTo(1)); Assert.Throws<ObjectDisposedException>(() => { var unused = proxy.Value; }); }
        [Test] public void ProxyDoesNotCreateDuringUnusedDispose()
        { int made = 0; var proxy = new LazyProxy<Box>(() => { made++; return new Box(); }); proxy.Dispose(); Assert.That(made, Is.Zero); }
        [Test] public void ChainStopsAtFirstRejection()
        { int count = 0; var chain = new RuleChain<Box>(new DelegateRule<Box>(_ => false), new DelegateRule<Box>(_ => { count++; return true; })); Assert.That(chain.Apply(new Box()), Is.False); Assert.That(count, Is.Zero); }
        [Test] public void CommandsUndoRedoAndDiscardOldBranch()
        { int value = 0; var history = new CommandHistory(); history.Execute(new DelegateCommand(() => value += 2, () => value -= 2)); history.Undo(); history.Redo(); Assert.That(value, Is.EqualTo(2)); history.Undo(); history.Execute(new DelegateCommand(() => value += 3, () => value -= 3)); Assert.That(history.RedoCount, Is.Zero); Assert.That(value, Is.EqualTo(3)); }
        [Test] public void FailedCommandKeepsRedoHistory()
        { var history = new CommandHistory(); history.Execute(new DelegateCommand(() => { }, () => { })); history.Undo(); Assert.Throws<InvalidOperationException>(() => history.Execute(new DelegateCommand(() => throw new InvalidOperationException(), () => { }))); Assert.That(history.RedoCount, Is.EqualTo(1)); }
        [Test] public void CompositeCommandRollsBackCompletedChildren()
        { int value = 0; var command = new CompositeCommand(new DelegateCommand(() => value++, () => value--), new DelegateCommand(() => throw new InvalidOperationException(), () => { })); Assert.Throws<InvalidOperationException>(() => command.Execute()); Assert.That(value, Is.Zero); }
        [Test] public void RecursiveHistoryExecutionIsRejected()
        { var history = new CommandHistory(); Assert.Throws<InvalidOperationException>(() => history.Execute(new DelegateCommand(() => history.Undo(), () => { }))); Assert.That(history.UndoCount, Is.Zero); }
        [Test] public void InterpreterHonorsPrecedenceAndShortCircuit()
        { var expression = ConditionInterpreter.Parse("clue && (!locked || key)"); Assert.That(expression.Evaluate(x => x == "clue" || x == "key"), Is.True); Assert.That(ConditionInterpreter.Parse("true || missing").Evaluate(_ => throw new Exception()), Is.True); }
        [TestCase("")] [TestCase("a &&")] [TestCase("(a")] [TestCase("a & b")] [TestCase("a b")]
        public void InterpreterRejectsMalformedInput(string value) { Assert.Throws<FormatException>(() => ConditionInterpreter.Parse(value)); }
        [Test] public void IteratorVisitsDepthAndBreadthInOrder()
        { var a = new CompositeNode<int>(1); var b = new CompositeNode<int>(2); a.Add(b); a.Add(new CompositeNode<int>(3)); b.Add(new CompositeNode<int>(4)); CollectionAssert.AreEqual(new[] {1,2,4,3}, Traversal.DepthFirst(a, x => x.Children).Select(x => x.Value)); CollectionAssert.AreEqual(new[] {1,2,3,4}, Traversal.BreadthFirst(a, x => x.Children).Select(x => x.Value)); }
        [Test] public void MediatorChecksResponseAndRemovesHandler()
        { var mediator = new Mediator(); using (mediator.Register<int, string>(x => x.ToString())) { Assert.That(mediator.Send<int, string>(42), Is.EqualTo("42")); Assert.Throws<InvalidOperationException>(() => mediator.Send<int, int>(42)); Assert.Throws<ArgumentException>(() => mediator.Register<int, int>(x => x)); } Assert.Throws<KeyNotFoundException>(() => mediator.Send<int, string>(0)); }
        [Test] public void MementoCopiesOnCaptureAndRestoreAndRejectsForeignSnapshots()
        { var store = new MementoStore<int[]>(x => (int[])x.Clone()); var state = new[] {7}; var snapshot = store.Capture(state); state[0] = 0; var restored = store.Restore(snapshot); restored[0] = 1; Assert.That(store.Restore(snapshot)[0], Is.EqualTo(7)); Assert.Throws<ArgumentException>(() => new MementoStore<int[]>(x => x).Restore(snapshot)); }
        [Test] public void StrategiesAreSelectedExplicitly()
        { var selector = new StrategySelector<string,int,int>(); selector.Register("normal",new DelegateStrategy<int,int>(x => x)); selector.Register("hard",new DelegateStrategy<int,int>(x => x*2)); Assert.That(selector.Evaluate("hard",5),Is.EqualTo(10)); Assert.Throws<KeyNotFoundException>(()=>selector.Evaluate("missing",1)); }
        [Test] public void TemplateAlwaysFinishesOnSuccessAndFailure()
        { var operation = new Operation(); Assert.That(operation.Execute(3),Is.EqualTo(6)); Assert.Throws<ArgumentException>(()=>operation.Execute(-1)); Assert.That(operation.Finished,Is.EqualTo(2)); }
        [Test] public void FacadePublishesTypedRequest()
        { int received=0; using(EventBus.Subscribe<int>(x=>received=x)) new Facade().Notify(9); Assert.That(received,Is.EqualTo(9)); }
        [Test] public void WorldCommandRestoresPreviousFlag()
        { string key="test.pattern.command"; WorldState.Remove(key); var history=new CommandHistory(); history.Execute(new SetWorldFlagCommand(key)); Assert.That(WorldState.Has(key),Is.True); history.Undo(); Assert.That(WorldState.Has(key),Is.False); history.Redo(); Assert.That(WorldState.Has(key),Is.True); WorldState.Remove(key); }
        [Test] public void AnimationFactorySupportsMissingAnimators()
        { var driver=new AnimatorDriverFactory(null).Create(); Assert.That(driver.Available,Is.False); driver.Play("Idle"); driver.Bool("Dead",false); driver.Float("Speed",2); driver.Trigger("Attack"); }
        [Test] public void GameModeDefaultsProvideDistinctRules()
        { Assert.That(GameModeRules.Default(GameMode.Exploration).HostileGhosts,Is.False); Assert.That(GameModeRules.Default(GameMode.Exploration).PlayerDamage,Is.Zero); Assert.That(GameModeRules.Default(GameMode.Challenge).AllowSave,Is.False); Assert.That(GameModeRules.Default(GameMode.Challenge).TimeLimit,Is.GreaterThan(0)); }
    }
}
