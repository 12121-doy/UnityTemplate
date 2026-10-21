using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    // 组件读取共享流程状态；流程写入集中由 GameManager 负责。
    /// <summary>
    /// 集中读取游戏状态和输入锁，流程写入由 GameManager 负责。
    /// </summary>
    public static class GameSession // 集中读取游戏状态和输入锁，流程写入由 GameManager 负责。
    { // 开始类型。
        /// <summary>
        /// 保存输入锁拥有者集合。
        /// </summary>
        private static readonly HashSet<object> Locks = new HashSet<object>(); // 创建 HashSet<object>，保存为 输入锁拥有者集合。
        /// <summary>
        /// 获取当前状态。
        /// </summary>
        public static GameState State { get; private set; } = GameState.Playing; // 获取当前状态。
        /// <summary>
        /// 仅 Playing 状态且没有拥有者输入锁时允许操作。
        /// </summary>
        public static bool InputEnabled => State == GameState.Playing && Locks.Count == 0; // 仅 Playing 状态且没有拥有者输入锁时允许操作。
        /// <summary>
        /// 仅 Playing 状态允许推进游戏模拟。
        /// </summary>
        public static bool SimulationEnabled => State == GameState.Playing; // 仅 Playing 状态允许推进游戏模拟。

        /// <summary>
        /// 更新共享流程状态并发布 GameStateChanged。
        /// </summary>
        /// <param name="state">目标状态或保存的状态数据。</param>
        public static void SetState(GameState state) // 更新共享流程状态并发布 GameStateChanged。
        { // 开始方法。
            State = state; // 更新 当前状态，使用 目标状态或保存的状态数据。
            EventBus.Publish(new GameStateChanged(state)); // 广播 GameStateChanged 事件，通知模块刷新流程状态。
        } // 结束方法。

        /// <summary>
        /// 按 owner 添加或移除输入锁；多个 owner 的锁独立持有。
        /// </summary>
        /// <param name="owner">本次操作的拥有者。</param>
        /// <param name="locked">输入是否锁定。</param>
        public static void SetLock(object owner, bool locked) // 按 owner 添加或移除输入锁；多个 owner 的锁独立持有。
        { // 开始方法。
            if (owner == null) // 当本次操作的拥有者为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (locked) // 当输入是否锁定为 true时，执行下方分支。
                Locks.Add(owner); // 向 输入锁拥有者集合 加入 owner。
            else // 前面的条件不成立时，执行此备用分支。
                Locks.Remove(owner); // 从 输入锁拥有者集合 移除 owner。
        } // 结束方法。

        /// <summary>
        /// SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 指定由 Unity 在配置的运行时初始化阶段自动调用。
        private static void Reset() // SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        { // 开始方法。
            Locks.Clear(); // 清空 输入锁拥有者集合 中保存的记录。
            State = GameState.Playing; // 更新 当前状态，使用 GameState.Playing。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
