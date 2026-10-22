using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct WorldStateChanged // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
    } // 结束数据结构。

    /// <summary>
    /// 用稳定字符串 ID 保存可变世界标记。
    /// </summary>
    public static class WorldState // 用稳定字符串 ID 保存可变世界标记。
    { // 开始类型。
        /// <summary>
        /// 保存世界标记集合。
        /// </summary>
        private static readonly HashSet<string> Flags = new HashSet<string>(); // 创建 HashSet<string>，保存为 世界标记集合。
        /// <summary>
        /// 检查非空 ID 是否已存在。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <returns>true 表示指定世界标记存在。</returns>
        public static bool Has(string id) => !string.IsNullOrEmpty(id) && Flags.Contains(id); // 检查非空 ID 是否已存在。
        /// <summary>
        /// 首次加入标记时广播 WorldStateChanged。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        public static void Set(string id) // 首次加入标记时广播 WorldStateChanged。
        { // 开始方法。
            if (!string.IsNullOrEmpty(id) && Flags.Add(id)) // 当（条件 string.IsNullOrEmpty(id) 不成立；且条件 Flags.Add(id) 成立）时，执行下方分支。
                EventBus.Publish(new WorldStateChanged()); // 广播 WorldStateChanged 事件，刷新门锁、拾取物等世界交互。
        } // 结束方法。

        /// <summary>
        /// 移除标记，移除成功时广播变化并返回 true。
        /// </summary>
        /// <param name="id">稳定标识。</param>
        /// <returns>true 表示目标元素已成功移除。</returns>
        public static bool Remove(string id) // 移除标记，移除成功时广播变化并返回 true。
        { // 开始方法。
            bool removed = !string.IsNullOrEmpty(id) && Flags.Remove(id); // 把 !string.IsNullOrEmpty(id) && Flags.Remove(id) 保存为 removed。
            if (removed) // 当removed为 true时，执行下方分支。
                EventBus.Publish(new WorldStateChanged()); // 广播 WorldStateChanged 事件，刷新门锁、拾取物等世界交互。
            return removed; // 返回 removed。
        } // 结束方法。

        /// <summary>
        /// 复制标记为字符串数组。
        /// </summary>
        /// <returns>独立复制的当前状态快照。</returns>
        public static string[] Snapshot() // 复制标记为字符串数组。
        { // 开始方法。
            var result = new string[Flags.Count]; // 把 new string[Flags.Count] 保存为 本次操作结果。
            Flags.CopyTo(result); // 把当前集合元素复制到独立数组，作为快照返回。
            return result; // 返回 本次操作结果。
        } // 结束方法。

        /// <summary>
        /// 替换全部标记，忽略空值，随后广播变化。
        /// </summary>
        /// <param name="values">候选数据集合。</param>
        public static void Restore(string[] values) // 替换全部标记，忽略空值，随后广播变化。
        { // 开始方法。
            Flags.Clear(); // 清空 世界标记集合 中保存的记录。
            foreach (var value in values ?? Array.Empty<string>()) // 遍历 values ?? Array.Empty<string>()，每次把一个元素保存为 value。
                if (!string.IsNullOrEmpty(value)) // 当条件 string.IsNullOrEmpty(value) 不成立时，执行下方分支。
                    Flags.Add(value); // 向 世界标记集合 加入 value。
            EventBus.Publish(new WorldStateChanged()); // 广播 WorldStateChanged 事件，刷新门锁、拾取物等世界交互。
        } // 结束方法。

        /// <summary>
        /// SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 指定由 Unity 在配置的运行时初始化阶段自动调用。
        private static void Reset() // SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        { // 开始方法。
            Flags.Clear(); // 清空 世界标记集合 中保存的记录。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
