using System; // 引入 System 命名空间，使用其中的类型与 API。
using Guisha.Core.Patterns; // 引入 Guisha.Core.Patterns 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 设置世界标记的可撤销命令。
    /// </summary>
    public sealed class SetWorldFlagCommand : ICommand // 设置世界标记的可撤销命令。
    { // 开始类型。
        /// <summary>
        /// 保存稳定标识。
        /// </summary>
        private readonly string id; // 声明 稳定标识，后续初始化。
        /// <summary>
        /// 保存修改前的旧值。
        /// </summary>
        private bool previous; // 声明 修改前的旧值，后续初始化。
        /// <summary>
        /// 构造 SetWorldFlagCommand，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        public SetWorldFlagCommand(string id) // 构造 SetWorldFlagCommand，初始化其依赖与状态。
        { // 开始构造函数。
            if (string.IsNullOrWhiteSpace(id)) // 当条件 string.IsNullOrWhiteSpace(id) 成立时，执行下方分支。
                throw new ArgumentException("Flag ID is empty."); // 抛出 ArgumentException，拒绝无效参数或状态；错误信息为 "Flag ID is empty."。
            this.id = id; // 更新 稳定标识，使用 稳定标识。
        } // 结束构造函数。

        /// <summary>
        /// 获取命令能否执行。
        /// </summary>
        public bool CanExecute => true; // 获取命令能否执行。

        /// <summary>
        /// 记住标记原状态，再设置标记。
        /// </summary>
        public void Execute() // 记住标记原状态，再设置标记。
        { // 开始方法。
            previous = WorldState.Has(id); // 更新 修改前的旧值，使用 WorldState.Has 的返回值。
            WorldState.Set(id); // 首次加入标记时广播 WorldStateChanged。
        } // 结束方法。

        /// <summary>
        /// 按 Execute 时记录的状态恢复标记。
        /// </summary>
        public void Undo() // 按 Execute 时记录的状态恢复标记。
        { // 开始方法。
            if (previous) // 当修改前的旧值为 true时，执行下方分支。
                WorldState.Set(id); // 首次加入标记时广播 WorldStateChanged。
            else // 前面的条件不成立时，执行此备用分支。
                WorldState.Remove(id); // 移除标记，移除成功时广播变化并返回 true。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 移动 Transform 的可撤销命令。
    /// </summary>
    public sealed class MoveTransformCommand : ICommand // 移动 Transform 的可撤销命令。
    { // 开始类型。
        /// <summary>
        /// 保存目标对象。
        /// </summary>
        private readonly Transform target; // 声明 目标对象，后续初始化。
        /// <summary>
        /// 保存目标位置。
        /// </summary>
        private readonly Vector3 destination; // 声明 目标位置，后续初始化。
        /// <summary>
        /// 保存修改前的旧值。
        /// </summary>
        private Vector3 previous; // 声明 修改前的旧值，后续初始化。
        /// <summary>
        /// 构造 MoveTransformCommand，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="target">目标对象。</param>
        /// <param name="destination">目标位置。</param>
        public MoveTransformCommand(Transform target, Vector3 destination) // 构造 MoveTransformCommand，初始化其依赖与状态。
        { // 开始构造函数。
            this.target = target; // 更新 目标对象，使用 目标对象。
            this.destination = destination; // 更新 目标位置，使用 目标位置。
        } // 结束构造函数。

        /// <summary>
        /// 获取命令能否执行。
        /// </summary>
        public bool CanExecute => target != null; // 获取命令能否执行。

        /// <summary>
        /// 检查目标仍存在，记住原位置，再移动到目标位置。
        /// </summary>
        public void Execute() // 检查目标仍存在，记住原位置，再移动到目标位置。
        { // 开始方法。
            if (!CanExecute) // 当条件 CanExecute 不成立时，执行下方分支。
                throw new InvalidOperationException("Target was destroyed."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Target was destroyed."。
            previous = target.position; // 更新 修改前的旧值，使用 target.position。
            target.position = destination; // 更新 target.position，使用 目标位置。
        } // 结束方法。

        /// <summary>
        /// 把目标移回记录的位置；目标已销毁时抛异常。
        /// </summary>
        public void Undo() // 把目标移回记录的位置；目标已销毁时抛异常。
        { // 开始方法。
            if (target == null) // 当目标对象为空时，执行下方分支。
                throw new InvalidOperationException("Target was destroyed."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Target was destroyed."。
            target.position = previous; // 更新 target.position，使用 修改前的旧值。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 复制运行时 ScriptableObject 配置；副本由调用方 Destroy。
    /// </summary>
    /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
    public sealed class ScriptablePrototype<T> : IPrototype<T> where T : ScriptableObject // 复制运行时 ScriptableObject 配置；副本由调用方 Destroy。
    { // 开始类型。
        /// <summary>
        /// 保存原型模板。
        /// </summary>
        private readonly T template; // 声明 原型模板，后续初始化。
        /// <summary>
        /// 构造 ScriptablePrototype，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="template">原型模板。</param>
        public ScriptablePrototype(T template) // 构造 ScriptablePrototype，初始化其依赖与状态。
        { // 开始构造函数。
            if (template == null) // 当原型模板为空时，执行下方分支。
                throw new ArgumentNullException(nameof(template)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            this.template = template; // 更新 原型模板，使用 原型模板。
        } // 结束构造函数。

        // 运行时副本由调用方拥有，使用结束后必须调用 Destroy。
        /// <summary>
        /// 使用 UnityEngine.Object.Instantiate 复制模板资产，调用方拥有返回的运行时对象。
        /// </summary>
        /// <returns>本次创建或复制的 T 对象。</returns>
        public T Clone() => UnityEngine.Object.Instantiate(template); // 使用 UnityEngine.Object.Instantiate 复制模板资产，调用方拥有返回的运行时对象。
    } // 结束类型。
} // 结束命名空间。
