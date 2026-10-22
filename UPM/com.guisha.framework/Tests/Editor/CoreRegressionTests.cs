using System; // 引入 System 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using NUnit.Framework; // 引入 NUnit.Framework 命名空间，使用其中的类型与 API。

namespace Guisha.Tests // 将本文件类型归入 Guisha.Tests，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 事件、对象池与 FSM 的 EditMode 回归测试。
    /// </summary>
    public sealed class CoreRegressionTests // 事件、对象池与 FSM 的 EditMode 回归测试。
    { // 开始类型。
        /// <summary>
        /// 每个测试前清空事件总线并恢复异常处理器。
        /// </summary>
        [SetUp] // 把下方方法注册为每个测试开始前的初始化。
        public void Reset() // 每个测试前清空事件总线并恢复异常处理器。
        { // 开始方法。
            EventBus.Clear(); // 清空事件频道并使旧订阅失效。
            EventBus.ExceptionHandler = UnityEngine.Debug.LogException; // 更新 EventBus.ExceptionHandler，使用 UnityEngine.Debug.LogException。
        } // 结束方法。

        /// <summary>
        /// 每个测试后清理事件订阅并恢复异常处理器。
        /// </summary>
        [TearDown] // 把下方方法注册为每个测试结束后的清理。
        public void Clean() // 每个测试后清理事件订阅并恢复异常处理器。
        { // 开始方法。
            EventBus.Clear(); // 清空事件频道并使旧订阅失效。
            EventBus.ExceptionHandler = UnityEngine.Debug.LogException; // 更新 EventBus.ExceptionHandler，使用 UnityEngine.Debug.LogException。
        } // 结束方法。

        /// <summary>
        /// 条件不满足时抛异常，使当前回归用例失败。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        /// <param name="message">消息或事件载荷。</param>
        static void Assert(bool value, string message = "assertion") // 条件不满足时抛异常，使当前回归用例失败。
        { // 开始方法。
            if (!value) // 当条件 value 不成立时，执行下方分支。
                throw new Exception(message); // 抛出 Exception，拒绝无效参数或状态。
        } // 结束方法。

        /// <summary>
        /// 验证委托确实抛出指定异常类型；未抛出时报告测试失败。
        /// </summary>
        /// <typeparam name="T">预期抛出的异常类型。</typeparam>
        /// <param name="action">操作委托；由调用方注入具体行为。</param>
        static void Throws<T>(Action action) // 验证委托确实抛出指定异常类型；未抛出时报告测试失败。
            where T : Exception // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                action(); // 执行注入的操作委托，使用参数 空参数列表。
            } // 结束受异常保护的操作。
            catch (T) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束异常处理。

            throw new Exception("Expected " + typeof(T).Name); // 抛出 Exception，拒绝无效参数或状态；错误信息为 "Expected " + typeof(T).Name。
        } // 结束方法。

        /// <summary>
        /// 故意覆盖相等性和哈希的测试对象。
        /// </summary>
        sealed class EqualItem // 故意覆盖相等性和哈希的测试对象。
        { // 开始类型。
            /// <summary>
            /// 测试用相等比较：不同实例也视为相等，用于验证引用身份对象池。
            /// </summary>
            /// <param name="other">待比较对象。</param>
            /// <returns>当前相等性比较的结果。</returns>
            public override bool Equals(object other) => other is EqualItem; // 测试用相等比较：不同实例也视为相等，用于验证引用身份对象池。
            /// <summary>
            /// 测试故意返回固定哈希，检验对象池不依赖对象自定义相等性。
            /// </summary>
            /// <returns>int 类型的处理结果。</returns>
            public override int GetHashCode() => 1; // 测试故意返回固定哈希，检验对象池不依赖对象自定义相等性。
        } // 结束类型。

        /// <summary>
        /// 记录 FSM Enter/Tick/Exit 次数的测试状态。
        /// </summary>
        sealed class State : AIBaseState // 记录 FSM Enter/Tick/Exit 次数的测试状态。
        { // 开始类型。
            /// <summary>
            /// 保存进入状态的测试计数、退出状态的测试计数、状态更新的测试计数。
            /// </summary>
            public int enters, exits, ticks; // 声明 进入状态的测试计数，后续初始化；声明 退出状态的测试计数，后续初始化；声明 状态更新的测试计数，后续初始化。
            /// <summary>
            /// 保存进入状态时执行的测试委托。
            /// </summary>
            public Action enter; // 声明 进入状态时执行的测试委托，后续初始化。
            /// <summary>
            /// 记录测试中的状态进入或节点进入回调。
            /// </summary>
            public override void Enter() // 记录测试中的状态进入或节点进入回调。
            { // 开始方法。
                enters++; // 递增 进入状态的测试计数。
                enter?.Invoke(); // 仅在 进入状态时执行的测试委托 非空时，执行 进入状态时执行的测试委托 回调；仅在回调非空时调用。
            } // 结束方法。

            /// <summary>
            /// 增加状态更新测试计数。
            /// </summary>
            /// <param name="dt">当前帧秒数。</param>
            public override void Tick(float dt) // 增加状态更新测试计数。
            { // 开始方法。
                ticks++; // 递增 状态更新的测试计数。
            } // 结束方法。

            /// <summary>
            /// 增加状态退出测试计数。
            /// </summary>
            public override void Exit() // 增加状态退出测试计数。
            { // 开始方法。
                exits++; // 递增 退出状态的测试计数。
            } // 结束方法。
        } // 结束类型。

        // 验证重复订阅令牌彼此独立，并支持重复释放。
        /// <summary>
        /// 验证重复订阅令牌彼此独立，并支持重复释放。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case01() // 验证重复订阅令牌彼此独立，并支持重复释放。
        { // 开始方法。
            int n = 0; // 把 0 保存为 本次测试的计数。
            Action<int> fn = _ => n++; // 把 _ => n++ 保存为 fn。
            var a = EventBus.Subscribe(fn); // 把 EventBus.Subscribe 的返回值 保存为 a。
            var b = EventBus.Subscribe(fn); // 把 EventBus.Subscribe 的返回值 保存为 b。
            a.Dispose(); // 释放 a 持有的资源或注册；具体拥有权由该对象负责。
            a.Dispose(); // 释放 a 持有的资源或注册；具体拥有权由该对象负责。
            EventBus.Publish(1); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 1); // 检查断言条件，不满足时让当前测试失败。
            b.Dispose(); // 释放 b 持有的资源或注册；具体拥有权由该对象负责。
            EventBus.Publish(1); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 1); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证发布期间退订的监听器不会继续收到本次事件。
        /// <summary>
        /// 验证发布期间退订的监听器不会继续收到本次事件。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case02() // 验证发布期间退订的监听器不会继续收到本次事件。
        { // 开始方法。
            int n = 0; // 把 0 保存为 本次测试的计数。
            IDisposable b = null; // 把 null（清空引用） 保存为 b。
            EventBus.Subscribe<int>(_ => b.Dispose()); // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            b = EventBus.Subscribe<int>(_ => n++); // 保存订阅令牌；订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            EventBus.Publish(1); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 0); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证发布期间新订阅的监听器从下次事件开始接收。
        /// <summary>
        /// 验证发布期间新订阅的监听器从下次事件开始接收。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case03() // 验证发布期间新订阅的监听器从下次事件开始接收。
        { // 开始方法。
            int n = 0; // 把 0 保存为 本次测试的计数。
            EventBus.Subscribe<int>(_ => EventBus.Subscribe<int>(__ => n++)); // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            EventBus.Publish(1); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 0); // 检查断言条件，不满足时让当前测试失败。
            EventBus.Publish(1); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 1); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证嵌套事件发布保持频道与次数正确。
        /// <summary>
        /// 验证嵌套事件发布保持频道与次数正确。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case04() // 验证嵌套事件发布保持频道与次数正确。
        { // 开始方法。
            int n = 0; // 把 0 保存为 本次测试的计数。
            EventBus.Subscribe<int>(v => // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            { // 开始回调。
                n++; // 递增 本次测试的计数。
                if (v == 0) // 当当前输入值等于0时，执行下方分支。
                    EventBus.Publish(1); // 同步广播当前类型载荷，通知频道中的有效监听器。
            }); // 结束回调。
            EventBus.Publish(0); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 2); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证单个监听器异常不会阻止其他监听器执行。
        /// <summary>
        /// 验证单个监听器异常不会阻止其他监听器执行。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case05() // 验证单个监听器异常不会阻止其他监听器执行。
        { // 开始方法。
            int errors = 0, n = 0; // 把 0 保存为 已观察到的异常次数；把 0 保存为 本次测试的计数。
            EventBus.ExceptionHandler = _ => errors++; // 为 EventBus.ExceptionHandler 保存回调；此处仅注册，回调被调用时才执行内部代码。
            EventBus.Subscribe<int>(_ => throw new Exception()); // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            EventBus.Subscribe<int>(_ => n++); // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            EventBus.Publish(0); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(errors == 1 && n == 1); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证 Clear 使正在派发的旧快照记录失效。
        /// <summary>
        /// 验证 Clear 使正在派发的旧快照记录失效。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case06() // 验证 Clear 使正在派发的旧快照记录失效。
        { // 开始方法。
            int n = 0; // 把 0 保存为 本次测试的计数。
            EventBus.Subscribe<int>(_ => EventBus.Clear()); // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            EventBus.Subscribe<int>(_ => n++); // 订阅 int 事件，返回可释放的退订令牌；回调在发布事件时执行。
            EventBus.Publish(0); // 同步广播当前类型载荷，通知频道中的有效监听器。
            Assert(n == 0); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证对象池复用对象并按借还次数执行回调。
        /// <summary>
        /// 验证对象池复用对象并按借还次数执行回调。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case07() // 验证对象池复用对象并按借还次数执行回调。
        { // 开始方法。
            int created = 0, gets = 0, returns = 0; // 把 0 保存为 已经创建的对象数；把 0 保存为 借出回调次数；把 0 保存为 归还回调次数。
            var p = new ObjectPool<object>(() => // 创建 ObjectPool<object>，保存为 p。
            { // 开始回调。
                created++; // 递增 已经创建的对象数。
                return new object (); // 返回 新建的 object 对象。
            }, _ => gets++, _ => returns++); // 结束回调。
            var a = p.Get(); // 把 p.Get 的返回值 保存为 a。
            p.Return(a); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            var b = p.Get(); // 把 p.Get 的返回值 保存为 b。
            Assert(ReferenceEquals(a, b) && created == 1 && gets == 2 && returns == 1); // 检查断言条件，不满足时让当前测试失败。
            p.Return(b); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            p.Dispose(); // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
        } // 结束方法。

        // 验证对象池拒绝外来对象与重复归还。
        /// <summary>
        /// 验证对象池拒绝外来对象与重复归还。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case08() // 验证对象池拒绝外来对象与重复归还。
        { // 开始方法。
            var p = new ObjectPool<object>(() => new object ()); // 创建 ObjectPool<object>，保存为 p。
            var a = p.Get(); // 把 p.Get 的返回值 保存为 a。
            Throws<InvalidOperationException>(() => p.Return(new object ())); // 验证委托确实抛出指定异常类型；未抛出时报告测试失败。
            p.Return(a); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            Throws<InvalidOperationException>(() => p.Return(a)); // 验证委托确实抛出指定异常类型；未抛出时报告测试失败。
            p.Dispose(); // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
        } // 结束方法。

        // 验证对象池按引用身份记录借出项，不受自定义 Equals 干扰。
        /// <summary>
        /// 验证对象池按引用身份记录借出项，不受自定义 Equals 干扰。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case09() // 验证对象池按引用身份记录借出项，不受自定义 Equals 干扰。
        { // 开始方法。
            var p = new ObjectPool<EqualItem>(() => new EqualItem()); // 创建 ObjectPool<EqualItem>，保存为 p。
            var a = p.Get(); // 把 p.Get 的返回值 保存为 a。
            var b = p.Get(); // 把 p.Get 的返回值 保存为 b。
            Assert(p.ActiveCount == 2); // 检查断言条件，不满足时让当前测试失败。
            p.Return(a); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            p.Return(b); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            p.Dispose(); // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
        } // 结束方法。

        // 验证空闲容量溢出时销毁，以及 Dispose 后归还借出对象的清理。
        /// <summary>
        /// 验证空闲容量溢出时销毁，以及 Dispose 后归还借出对象的清理。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case10() // 验证空闲容量溢出时销毁，以及 Dispose 后归还借出对象的清理。
        { // 开始方法。
            int destroyed = 0; // 把 0 保存为 已经销毁的对象数。
            var p = new ObjectPool<object>(() => new object (), onDestroy: _ => destroyed++, capacity: 1); // 创建 ObjectPool<object>，保存为 p。
            var a = p.Get(); // 把 p.Get 的返回值 保存为 a。
            var b = p.Get(); // 把 p.Get 的返回值 保存为 b。
            p.Return(a); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            p.Return(b); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            Assert(destroyed == 1); // 检查断言条件，不满足时让当前测试失败。
            var c = p.Get(); // 把 p.Get 的返回值 保存为 c。
            p.Dispose(); // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
            Assert(destroyed == 1); // 检查断言条件，不满足时让当前测试失败。
            p.Return(c); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
            Assert(destroyed == 2); // 检查断言条件，不满足时让当前测试失败。
            Throws<ObjectDisposedException>(() => p.Get()); // 验证委托确实抛出指定异常类型；未抛出时报告测试失败。
        } // 结束方法。

        // 验证状态进入、更新、退出顺序和同状态切换的空操作。
        /// <summary>
        /// 验证状态进入、更新、退出顺序和同状态切换的空操作。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case11() // 验证状态进入、更新、退出顺序和同状态切换的空操作。
        { // 开始方法。
            var f = new AIStateMachine(); // 创建 AIStateMachine，保存为 f。
            var a = new State(); // 创建 State，保存为 a。
            var b = new State(); // 创建 State，保存为 b。
            f.Change(a); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
            f.Change(a); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
            f.Tick(1); // 调用当前状态的 Tick。
            f.Change(b); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
            f.Stop(); // 切换为 null，退出当前状态。
            Assert(a.enters == 1 && a.ticks == 1 && a.exits == 1 && b.enters == 1 && b.exits == 1 && f.Current == null); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。

        // 验证 Enter 期间递归切换状态会被重入保护拒绝。
        /// <summary>
        /// 验证 Enter 期间递归切换状态会被重入保护拒绝。
        /// </summary>
        [Test] // 把下方方法注册为 Unity/NUnit 测试用例。
        public void Case12() // 验证 Enter 期间递归切换状态会被重入保护拒绝。
        { // 开始方法。
            var f = new AIStateMachine(); // 创建 AIStateMachine，保存为 f。
            var b = new State(); // 创建 State，保存为 b。
            var a = new State // 创建 State，保存为 a。
            { // 开始成员/集合初始化。
                enter = () => f.Change(b) // 为 进入状态时执行的测试委托 保存回调；此处仅注册，回调被调用时才执行内部代码。
            }; // 结束成员/集合初始化。
            Throws<InvalidOperationException>(() => f.Change(a)); // 验证委托确实抛出指定异常类型；未抛出时报告测试失败。
            f.Change(b); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
            Assert(f.Current == b); // 检查断言条件，不满足时让当前测试失败。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
