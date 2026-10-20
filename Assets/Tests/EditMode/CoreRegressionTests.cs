using System;
using Guisha.Core;
using Guisha.Framework;
using NUnit.Framework;

namespace Guisha.Tests
{
public sealed class CoreRegressionTests
{
    [SetUp] public void Reset() { EventBus.Clear(); EventBus.ExceptionHandler = UnityEngine.Debug.LogException; }
    [TearDown] public void Clean() { EventBus.Clear(); EventBus.ExceptionHandler = UnityEngine.Debug.LogException; }
    static void Assert(bool value, string message="assertion") {if (!value) throw new Exception(message);}
    static void Throws<T>(Action action) where T:Exception
    {try { action(); } catch(T) { return; } throw new Exception("Expected " + typeof(T).Name);}
    sealed class EqualItem {public override bool Equals(object other)=>other is EqualItem; public override int GetHashCode()=>1;}
    sealed class State:AIBaseState
    {
        public int enters, exits, ticks;
        public Action enter;
        public override void Enter(){enters++; enter?.Invoke();}
        public override void Tick(float dt){ticks++;}
        public override void Exit(){exits++;}
    }
    // events: independent duplicate subscription tokens
    [Test] public void Case01()
    {
            int n=0; Action<int> fn=_=>n++; var a=EventBus.Subscribe(fn); var b=EventBus.Subscribe(fn);
            a.Dispose(); a.Dispose(); EventBus.Publish(1); Assert(n==1); b.Dispose(); EventBus.Publish(1); Assert(n==1);
    }
    // events: unsubscribe during publish
    [Test] public void Case02()
    {
            int n=0; IDisposable b=null; EventBus.Subscribe<int>(_=>b.Dispose()); b=EventBus.Subscribe<int>(_=>n++);
            EventBus.Publish(1); Assert(n==0);
    }
    // events: subscriptions added during publish start next delivery
    [Test] public void Case03()
    {
            int n=0; EventBus.Subscribe<int>(_=>EventBus.Subscribe<int>(__=>n++)); EventBus.Publish(1); Assert(n==0); EventBus.Publish(1); Assert(n==1);
    }
    // events: nested delivery preserves channels
    [Test] public void Case04()
    {
            int n=0; EventBus.Subscribe<int>(v=>{n++; if(v==0) EventBus.Publish(1);}); EventBus.Publish(0); Assert(n==2);
    }
    // events: one failing listener does not suppress others
    [Test] public void Case05()
    {
            int errors=0,n=0; EventBus.ExceptionHandler=_=>errors++; EventBus.Subscribe<int>(_=>throw new Exception()); EventBus.Subscribe<int>(_=>n++);
            EventBus.Publish(0); Assert(errors==1&&n==1);
    }
    // events: clear invalidates in-flight snapshot
    [Test] public void Case06()
    {
            int n=0; EventBus.Subscribe<int>(_=>EventBus.Clear()); EventBus.Subscribe<int>(_=>n++); EventBus.Publish(0); Assert(n==0);
    }
    // pool: reuse and callbacks
    [Test] public void Case07()
    {
            int created=0,gets=0,returns=0; var p=new ObjectPool<object>(()=>{created++;return new object();},_=>gets++,_=>returns++);
            var a=p.Get(); p.Return(a); var b=p.Get(); Assert(ReferenceEquals(a,b)&&created==1&&gets==2&&returns==1); p.Return(b); p.Dispose();
    }
    // pool: rejects foreign and duplicate returns
    [Test] public void Case08()
    {
            var p=new ObjectPool<object>(()=>new object()); var a=p.Get(); Throws<InvalidOperationException>(()=>p.Return(new object()));
            p.Return(a); Throws<InvalidOperationException>(()=>p.Return(a)); p.Dispose();
    }
    // pool: identity tracking works with custom Equals
    [Test] public void Case09()
    {
            var p=new ObjectPool<EqualItem>(()=>new EqualItem()); var a=p.Get(); var b=p.Get(); Assert(p.ActiveCount==2); p.Return(a); p.Return(b); p.Dispose();
    }
    // pool: overflow and active rental after dispose
    [Test] public void Case10()
    {
            int destroyed=0; var p=new ObjectPool<object>(()=>new object(),onDestroy:_=>destroyed++,capacity:1);
            var a=p.Get(); var b=p.Get(); p.Return(a); p.Return(b); Assert(destroyed==1); var c=p.Get(); p.Dispose(); Assert(destroyed==1);
            p.Return(c); Assert(destroyed==2); Throws<ObjectDisposedException>(()=>p.Get());
    }
    // FSM: enter tick exit and same-state no-op
    [Test] public void Case11()
    {
            var f=new AIStateMachine(); var a=new State(); var b=new State(); f.Change(a); f.Change(a); f.Tick(1); f.Change(b); f.Stop();
            Assert(a.enters==1&&a.ticks==1&&a.exits==1&&b.enters==1&&b.exits==1&&f.Current==null);
    }
    // FSM: recursive enter transition guarded
    [Test] public void Case12()
    {
            var f=new AIStateMachine(); var b=new State(); var a=new State {enter=()=>f.Change(b)}; Throws<InvalidOperationException>(()=>f.Change(a)); f.Change(b); Assert(f.Current==b);
    }
}
}
