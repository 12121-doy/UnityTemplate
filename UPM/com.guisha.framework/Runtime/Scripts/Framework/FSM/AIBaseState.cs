namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 有限状态机状态基类，提供 Enter/Tick/Exit 扩展点。
    /// </summary>
    public abstract class AIBaseState // 有限状态机状态基类，提供 Enter/Tick/Exit 扩展点。
    { // 开始类型。
        /// <summary>
        /// 进入状态时初始化上下文；默认基类钩子可为空。
        /// </summary>
        public virtual void Enter() // 进入状态时初始化上下文；默认基类钩子可为空。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 按本状态的条件推进行为，并在合适时切换状态。
        /// </summary>
        /// <param name="deltaTime">当前帧秒数。</param>
        public abstract void Tick(float deltaTime); // 按本状态的条件推进行为，并在合适时切换状态。
        /// <summary>
        /// 退出状态时执行收尾；默认基类钩子可为空。
        /// </summary>
        public virtual void Exit() // 退出状态时执行收尾；默认基类钩子可为空。
        { // 开始方法。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
