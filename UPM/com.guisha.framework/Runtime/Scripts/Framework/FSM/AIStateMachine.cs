using System; // 引入 System 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    // 这里是纯 C# FSM；Gameplay/GhostBrain 将其与 Unity 组件组合。
    /// <summary>
    /// 管理当前状态并防止进入/退出期间递归切换。
    /// </summary>
    public sealed class AIStateMachine // 管理当前状态并防止进入/退出期间递归切换。
    { // 开始类型。
        /// <summary>
        /// 获取当前 AI 状态。
        /// </summary>
        public AIBaseState Current { get; private set; } // 获取当前 AI 状态。

        /// <summary>
        /// 保存状态切换重入锁。
        /// </summary>
        private bool changing; // 声明 状态切换重入锁，后续初始化。
        /// <summary>
        /// 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
        /// </summary>
        /// <param name="next">下一个 AI 状态。</param>
        public void Change(AIBaseState next) // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
        { // 开始方法。
            if (ReferenceEquals(Current, next)) // 当条件 ReferenceEquals(Current, next) 成立时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (changing) // 当状态切换重入锁为 true时，执行下方分支。
                throw new InvalidOperationException("Do not change state inside Enter/Exit."); // 抛出 InvalidOperationException，拒绝无效参数或状态；错误信息为 "Do not change state inside Enter/Exit."。
            changing = true; // 更新 状态切换重入锁，使用 true（启用/满足）。
            try // 进入异常保护范围；失败时由 catch/finally 配对清理。
            { // 开始受异常保护的操作。
                Current?.Exit(); // 仅在 当前 AI 状态 非空时，退出状态时执行收尾；默认基类钩子可为空。
                Current = next; // 更新 当前 AI 状态，使用 下一个 AI 状态。
                Current?.Enter(); // 仅在 当前 AI 状态 非空时，进入状态时初始化上下文；默认基类钩子可为空。
            } // 结束受异常保护的操作。
            finally // 无论成功、失败或提前返回，都执行此清理分支。
            { // 开始最终清理。
                changing = false; // 更新 状态切换重入锁，使用 false（关闭/不满足）。
            } // 结束最终清理。
        } // 结束方法。

        /// <summary>
        /// 调用当前状态的 Tick。
        /// </summary>
        /// <param name="deltaTime">当前帧秒数。</param>
        public void Tick(float deltaTime) // 调用当前状态的 Tick。
        { // 开始方法。
            Current?.Tick(deltaTime); // 仅在 当前 AI 状态 非空时，按本状态的条件推进行为，并在合适时切换状态。
        } // 结束方法。

        /// <summary>
        /// 切换为 null，退出当前状态。
        /// </summary>
        public void Stop() // 切换为 null，退出当前状态。
        { // 开始方法。
            Change(null); // 同一状态不重复进入；退出旧状态并进入新状态，进入/退出过程中禁止递归切换。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
