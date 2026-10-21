using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections; // 引入 System.Collections 命名空间，使用其中的类型与 API。
using System.Threading; // 引入 System.Threading 命名空间，使用其中的类型与 API。
using System.Threading.Tasks; // 引入 System.Threading.Tasks 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 协程、延时和 Unity 异步操作的辅助入口。
    /// </summary>
    public static class AsyncHelper // 协程、延时和 Unity 异步操作的辅助入口。
    { // 开始类型。
        /// <summary>
        /// 在指定 MonoBehaviour 上启动延时协程，返回 Coroutine；unscaled 控制是否受 Time.timeScale 影响。
        /// </summary>
        /// <param name="owner">本次操作的拥有者。</param>
        /// <param name="seconds">延时秒数。</param>
        /// <param name="callback">完成时执行的回调；由调用方注入具体行为。</param>
        /// <param name="unscaled">是否使用不受时间缩放影响的计时。</param>
        /// <returns>UnityEngine.Coroutine 类型的处理结果。</returns>
        public static Coroutine Delay(MonoBehaviour owner, float seconds, Action callback, bool unscaled = false) => owner.StartCoroutine(DelayRoutine(Mathf.Max(0, seconds), callback, unscaled)); // 在指定 MonoBehaviour 上启动延时协程，返回 Coroutine；unscaled 控制是否受 Time.timeScale 影响。
        /// <summary>
        /// 等待指定的缩放/非缩放秒数，随后执行 callback。
        /// </summary>
        /// <param name="seconds">延时秒数。</param>
        /// <param name="callback">完成时执行的回调；由调用方注入具体行为。</param>
        /// <param name="unscaled">是否使用不受时间缩放影响的计时。</param>
        /// <returns>协程枚举器，由 Unity 协程系统或 Test Runner 逐步推进。</returns>
        private static IEnumerator DelayRoutine(float seconds, Action callback, bool unscaled) // 等待指定的缩放/非缩放秒数，随后执行 callback。
        { // 开始方法。
            if (unscaled) // 当是否使用不受时间缩放影响的计时为 true时，执行下方分支。
                yield return new WaitForSecondsRealtime(seconds); // 暂停协程，等待指定帧/秒数或条件后继续。
            else // 前面的条件不成立时，执行此备用分支。
                yield return new WaitForSeconds(seconds); // 暂停协程，等待指定帧/秒数或条件后继续。
            callback?.Invoke(); // 仅在 完成时执行的回调 非空时，执行 完成时执行的回调 回调；仅在回调非空时调用。
        } // 结束方法。

        /// <summary>
        /// 把 Unity AsyncOperation 的 completed 事件包装成 Task；已完成时直接返回。
        /// </summary>
        /// <param name="operation">Unity 异步操作。</param>
        /// <returns>异步完成任务；应 await 或通过 AsyncHelper.Forget 观察异常。</returns>
        public static async Task Await(AsyncOperation operation) // 把 Unity AsyncOperation 的 completed 事件包装成 Task；已完成时直接返回。
        { // 开始方法。
            if (operation == null) // 当Unity 异步操作为空时，执行下方分支。
                throw new ArgumentNullException(nameof(operation)); // 抛出 ArgumentNullException，拒绝无效参数或状态。
            if (operation.isDone) // 当operation.isDone为 true时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var completion = new TaskCompletionSource<bool>(); // 创建 TaskCompletionSource<bool>，保存为 等待操作完成的 Task 完成源。
            operation.completed += _ => completion.TrySetResult(true); // 为 operation.completed 注册事件回调；事件发生时执行右侧处理逻辑。
            await completion.Task; // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
        } // 结束方法。

        /// <summary>
        /// 按 Unity 实时时钟等待，并在轮询和结束时检查 CancellationToken。
        /// </summary>
        /// <param name="seconds">延时秒数。</param>
        /// <param name="token">取消令牌；异步操作在可取消检查点观察取消。</param>
        /// <returns>异步完成任务；应 await 或通过 AsyncHelper.Forget 观察异常。</returns>
        public static async Task DelayAsync(float seconds, CancellationToken token = default) // 按 Unity 实时时钟等待，并在轮询和结束时检查 CancellationToken。
        { // 开始方法。
            double end = Time.realtimeSinceStartupAsDouble + Math.Max(0, seconds); // 把 Time.realtimeSinceStartupAsDouble + Math.Max(0, seconds) 保存为 终点。
            while (Time.realtimeSinceStartupAsDouble < end) // 只要Time.realtimeSinceStartupAsDouble小于终点，就继续循环。
            { // 开始条件循环。
                token.ThrowIfCancellationRequested(); // 观察取消状态，已取消时抛出取消异常。
                await Task.Yield(); // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
            } // 结束条件循环。

            token.ThrowIfCancellationRequested(); // 观察取消状态，已取消时抛出取消异常。
        } // 结束方法。

        // 在 Unity 的 void 回调边界使用，确保异步异常都被观察和记录。
        /// <summary>
        /// 用于 Unity void 边界观察 Task 异常；忽略取消，其他异常交给 Debug.LogException。
        /// </summary>
        /// <param name="task">待观察的异步任务。</param>
        public static async void Forget(Task task) // 用于 Unity void 边界观察 Task 异常；忽略取消，其他异常交给 Debug.LogException。
        { // 开始方法。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                await task; // 等待异步操作完成后再继续；取消与异常沿 async 调用链传播。
            } // 结束受异常保护的操作。
            catch (OperationCanceledException) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
            } // 结束异常处理。
            catch (Exception error) // 捕获前面操作抛出的异常，执行失败处理。
            { // 开始异常处理。
                Debug.LogException(error); // 将异常堆栈交给 Unity Console。
            } // 结束异常处理。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
