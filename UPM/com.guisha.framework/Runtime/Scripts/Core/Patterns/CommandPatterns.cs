using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。

namespace Guisha.Core.Patterns // 将本文件类型归入 Guisha.Core.Patterns，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 支持可执行检查、执行和撤销的命令契约。
    /// </summary>
    public interface ICommand // 支持可执行检查、执行和撤销的命令契约。
    { // 开始接口。
        /// <summary>
        /// 获取命令能否执行。
        /// </summary>
        bool CanExecute { get; } // 获取命令能否执行。

        /// <summary>
        /// 执行命令定义的正向操作。
        /// </summary>
        void Execute(); // 执行命令定义的正向操作。
        /// <summary>
        /// 执行命令定义的逆向操作。
        /// </summary>
        void Undo(); // 执行命令定义的逆向操作。
    } // 结束接口。

    /// <summary>
    /// 用执行、撤销和检查委托构造命令。
    /// </summary>
    public sealed class DelegateCommand : ICommand // 用执行、撤销和检查委托构造命令。
    { // 开始类型。
        /// <summary>
        /// 保存正向执行委托、撤销操作委托。
        /// </summary>
        private readonly Action execute, undo; // 声明 正向执行委托，后续初始化；声明 撤销操作委托，后续初始化。
        /// <summary>
        /// 保存可执行条件委托。
        /// </summary>
        private readonly Func<bool> canExecute; // 声明 可执行条件委托，后续初始化。
        /// <summary>
        /// 构造 DelegateCommand，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="execute">正向执行委托；由调用方注入具体行为。</param>
        /// <param name="undo">撤销已经成功执行的正向操作所需的回调。</param>
        /// <param name="canExecute">可执行条件委托；由调用方注入具体行为。</param>
        public DelegateCommand(Action execute, Action undo, Func<bool> canExecute = null) // 构造 DelegateCommand，初始化其依赖与状态。
        { // 开始构造函数。
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute)); // 更新 正向执行委托，使用 execute ?? throw new ArgumentNullException(nameof(execute))。
            this.undo = undo ?? throw new ArgumentNullException(nameof(undo)); // 更新 撤销操作委托，使用 undo ?? throw new ArgumentNullException(nameof(undo))。
            this.canExecute = canExecute; // 更新 可执行条件委托，使用 可执行条件委托。
        } // 结束构造函数。

        /// <summary>
        /// 获取命令能否执行。
        /// </summary>
        public bool CanExecute => canExecute?.Invoke() ?? true; // 获取命令能否执行。

        /// <summary>
        /// 直接执行注入的命令动作；CanExecute 检查由 CommandHistory 或调用方负责。
        /// </summary>
        public void Execute() => execute(); // 直接执行注入的命令动作；CanExecute 检查由 CommandHistory 或调用方负责。
        /// <summary>
        /// 执行注入的撤销动作。
        /// </summary>
        public void Undo() => undo(); // 执行注入的撤销动作。
    } // 结束类型。

    /// <summary>
    /// 组合多个命令；执行失败时反向撤销已完成项。
    /// </summary>
    public sealed class CompositeCommand : ICommand // 组合多个命令；执行失败时反向撤销已完成项。
    { // 开始类型。
        /// <summary>
        /// 保存子命令数组。
        /// </summary>
        private readonly ICommand[] commands; // 声明 子命令数组，后续初始化。
        /// <summary>
        /// 构造 CompositeCommand，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="commands">子命令数组。</param>
        public CompositeCommand(params ICommand[] commands) // 构造 CompositeCommand，初始化其依赖与状态。
        { // 开始构造函数。
            this.commands = (ICommand[])(commands ?? throw new ArgumentNullException(nameof(commands))).Clone(); // 更新 子命令数组，使用 (ICommand[])(commands ?? throw new ArgumentNullException(nameof(commands))).Clone()。
            foreach (var command in this.commands) // 遍历 子命令数组，每次把一个元素保存为 command。
                if (command == null) // 当本次可撤销命令为空时，执行下方分支。
                    throw new ArgumentException("Null command."); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Null command."。
        } // 结束构造函数。

        /// <summary>
        /// 获取命令能否执行。
        /// </summary>
        public bool CanExecute // 获取命令能否执行。
        { // 开始属性访问器列表。
            get // 读取属性时执行下方取值与状态校验。
            { // 开始属性读取。
                foreach (var command in commands) // 遍历 子命令数组，每次把一个元素保存为 command。
                    if (!command.CanExecute) // 当条件 command.CanExecute 不成立时，执行下方分支。
                        return false; // 返回 false，将当前分支的布尔结果交给调用方。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            } // 结束属性读取。
        } // 结束属性访问器列表。

        /// <summary>
        /// 检查全部子命令后按顺序执行，失败时反向撤销已完成项；回滚也失败时聚合异常。
        /// </summary>
        public void Execute() // 检查全部子命令后按顺序执行，失败时反向撤销已完成项；回滚也失败时聚合异常。
        { // 开始方法。
            if (!CanExecute) // 当条件 CanExecute 不成立时，执行下方分支。
                throw new InvalidOperationException("Composite command cannot execute."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Composite command cannot execute."。
            int completed = 0; // 把 0 保存为 结果回调。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                for (; completed < commands.Length; completed++) // 按初始化与递增步骤循环；继续条件为 completed < commands.Length。
                    commands[completed].Execute(); // 执行命令定义的正向操作。
            } // 结束受异常保护的操作。
            catch (Exception original) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                var errors = new List<Exception> // 创建 List<Exception>，保存为 已观察到的异常次数。
                { // 开始成员/集合初始化。
                    original // 提供初始化/参数元素 original，交给上方的对象或集合使用。
                }; // 结束成员/集合初始化。
                for (int i = completed - 1; i >= 0; i--) // 按初始化与递增步骤循环；继续条件为 i >= 0。
                    try // 进入异常保护范围；失败时由 catch/finally 配对清理。
                    { // 开始受异常保护的操作。
                        commands[i].Undo(); // 执行命令定义的逆向操作。
                    } // 结束受异常保护的操作。
                    catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
                    { // 开始异常处理。
                        errors.Add(error); // 向 已观察到的异常次数 加入 error。
                    } // 结束异常处理。

                if (errors.Count > 1) // 当errors.Count大于1时，执行下方分支。
                    throw new AggregateException(errors); // 抛出 AggregateException，拒绝无效参数或状态。
                throw; // 重新抛出当前异常，让上层感知失败并保留原始堆栈。
            } // 结束异常处理。
        } // 结束方法。

        /// <summary>
        /// 按逆序撤销全部子命令。
        /// </summary>
        public void Undo() // 按逆序撤销全部子命令。
        { // 开始方法。
            for (int i = commands.Length - 1; i >= 0; i--) // 按初始化与递增步骤循环；继续条件为 i >= 0。
                commands[i].Undo(); // 执行命令定义的逆向操作。
        } // 结束方法。
    } // 结束类型。

    // 命令抛异常时须自行保证未提交副作用；历史仅在操作成功后记录。
    /// <summary>
    /// 有容量上限的撤销/重做历史，拒绝重入。
    /// </summary>
    public sealed class CommandHistory // 有容量上限的撤销/重做历史，拒绝重入。
    { // 开始类型。
        /// <summary>
        /// 保存撤销历史栈、重做历史栈。
        /// </summary>
        private readonly List<ICommand> undo = new List<ICommand>(), redo = new List<ICommand>(); // 创建 List<ICommand>，保存为 撤销历史栈；创建 List<ICommand>，保存为 重做历史栈。
        /// <summary>
        /// 保存容器容量上限。
        /// </summary>
        private readonly int capacity; // 声明 容器容量上限，后续初始化。
        /// <summary>
        /// 保存历史操作执行锁。
        /// </summary>
        private bool executing; // 声明 历史操作执行锁，后续初始化。
        /// <summary>
        /// 获取可撤销操作数。
        /// </summary>
        public int UndoCount => undo.Count; // 获取可撤销操作数。
        /// <summary>
        /// 获取可重做操作数。
        /// </summary>
        public int RedoCount => redo.Count; // 获取可重做操作数。

        /// <summary>
        /// 构造 CommandHistory，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="capacity">容器容量上限。</param>
        public CommandHistory(int capacity = 64) // 构造 CommandHistory，初始化其依赖与状态。
        { // 开始构造函数。
            if (capacity < 1) // 当容器容量上限小于1时，执行下方分支。
                throw new ArgumentOutOfRangeException(nameof(capacity)); // 抛出 ArgumentOutOfRangeException，拒绝无效参数或状态。
            this.capacity = capacity; // 更新 容器容量上限，使用 容器容量上限。
        } // 结束构造函数。

        /// <summary>
        /// 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
        /// </summary>
        /// <param name="command">本次可撤销命令。</param>
        /// <returns>当前操作或条件的布尔结果；具体判定见方法说明。</returns>
        public bool Execute(ICommand command) // 防重入，检查 CanExecute，成功后写入撤销历史并清空重做分支。
        { // 开始方法。
            if (command == null) // 当本次可撤销命令为空时，执行下方分支。
                throw new ArgumentNullException(nameof(command)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            Begin(); // 内部设置执行锁，拒绝递归进入历史操作。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                if (!command.CanExecute) // 当条件 command.CanExecute 不成立时，执行下方分支。
                    return false; // 返回 false，将当前分支的布尔结果交给调用方。
                command.Execute(); // 执行命令定义的正向操作。
                undo.Add(command); // 向 撤销历史栈 加入 command。
                if (undo.Count > capacity) // 当undo.Count大于容器容量上限时，执行下方分支。
                    undo.RemoveAt(0); // 从 撤销历史栈 移除 0。
                redo.Clear(); // 清空 重做历史栈 中保存的记录。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                executing = false; // 更新 历史操作执行锁，使用 false（关闭/不满足）。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
        /// </summary>
        /// <returns>true 表示有历史且已成功撤销。</returns>
        public bool Undo() // 撤销最后一次已提交命令；成功后移入重做历史，空历史返回 false。
        { // 开始方法。
            Begin(); // 内部设置执行锁，拒绝递归进入历史操作。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                if (undo.Count == 0) // 当undo.Count等于0时，执行下方分支。
                    return false; // 返回 false，将当前分支的布尔结果交给调用方。
                var command = undo[undo.Count - 1]; // 把 undo[undo.Count - 1] 保存为 本次可撤销命令。
                command.Undo(); // 执行命令定义的逆向操作。
                undo.RemoveAt(undo.Count - 1); // 从 撤销历史栈 移除 undo.Count - 1。
                redo.Add(command); // 向 重做历史栈 加入 command。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                executing = false; // 更新 历史操作执行锁，使用 false（关闭/不满足）。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 重新执行最后一个可重做命令；成功后移回撤销历史。
        /// </summary>
        /// <returns>true 表示有重做历史且已成功重做。</returns>
        public bool Redo() // 重新执行最后一个可重做命令；成功后移回撤销历史。
        { // 开始方法。
            Begin(); // 内部设置执行锁，拒绝递归进入历史操作。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                if (redo.Count == 0) // 当redo.Count等于0时，执行下方分支。
                    return false; // 返回 false，将当前分支的布尔结果交给调用方。
                var command = redo[redo.Count - 1]; // 把 redo[redo.Count - 1] 保存为 本次可撤销命令。
                if (!command.CanExecute) // 当条件 command.CanExecute 不成立时，执行下方分支。
                    return false; // 返回 false，将当前分支的布尔结果交给调用方。
                command.Execute(); // 执行命令定义的正向操作。
                redo.RemoveAt(redo.Count - 1); // 从 重做历史栈 移除 redo.Count - 1。
                undo.Add(command); // 向 撤销历史栈 加入 command。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                executing = false; // 更新 历史操作执行锁，使用 false（关闭/不满足）。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 在没有操作执行时清空撤销与重做记录。
        /// </summary>
        public void Clear() // 在没有操作执行时清空撤销与重做记录。
        { // 开始方法。
            if (executing) // 当历史操作执行锁为 true时，执行下方分支。
                throw new InvalidOperationException("History is executing."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "History is executing."。
            undo.Clear(); // 清空 撤销历史栈 中保存的记录。
            redo.Clear(); // 清空 重做历史栈 中保存的记录。
        } // 结束方法。

        /// <summary>
        /// 内部设置执行锁，拒绝递归进入历史操作。
        /// </summary>
        private void Begin() // 内部设置执行锁，拒绝递归进入历史操作。
        { // 开始方法。
            if (executing) // 当历史操作执行锁为 true时，执行下方分支。
                throw new InvalidOperationException("Recursive command execution."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Recursive command execution."。
            executing = true; // 更新 历史操作执行锁，使用 true（启用/满足）。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
