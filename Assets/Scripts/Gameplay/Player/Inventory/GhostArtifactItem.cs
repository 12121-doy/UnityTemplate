using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 恢复玩家 San 的演示鬼器。
    /// </summary>
    [CreateAssetMenu(menuName = "Guisha/Items/Ghost Artifact")] // 在 Unity Create 菜单中提供该 ScriptableObject 配置的创建入口。
    public sealed class GhostArtifactItem : Item // 恢复玩家 San 的演示鬼器。
    { // 开始类型。
        /// <summary>
        /// 保存鬼器恢复的理智量。
        /// </summary>
        [Min(0)] // 约束 Inspector 中可编辑的数值范围。
        public float sanRestore = 25; // 把 25 保存为 鬼器恢复的理智量。
        /// <summary>
        /// 找到玩家状态组件后恢复 San 并发提示；缺少组件返回 false。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示使用成功；背包仅在成功且为消耗品时移除物品。</returns>
        public override bool Use(GameObject actor) // 找到玩家状态组件后恢复 San 并发提示；缺少组件返回 false。
        { // 开始方法。
            var status = actor.GetComponent<PlayerStatus>(); // 把 actor.GetComponent<PlayerStatus> 的返回值 保存为 玩家状态组件。
            if (status == null) // 当玩家状态组件为空时，执行下方分支。
                return false; // 返回 false，将当前分支的布尔结果交给调用方。
            status.ChangeSan(sanRestore); // 按 delta 改变 San，钳制到有效范围，数值变化时广播。
            EventBus.Publish(new Notification("使用了 " + displayName)); // 广播 Notification 事件，让 HUD 显示短通知。
            return true; // 返回 true，将当前分支的布尔结果交给调用方。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
