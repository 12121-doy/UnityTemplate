using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 人类 NPC 实体与对白交互对象。
    /// </summary>
    public sealed class HumanNPC : BaseEntity, IInteractable // 人类 NPC 实体与对白交互对象。
    { // 开始类型。
        /// <summary>
        /// 保存对白资产或当前对白。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private DialogueAsset dialogue; // 声明 对白资产或当前对白，后续初始化。
        /// <summary>
        /// 获取实体类别。
        /// </summary>
        public override EntityType Type => EntityType.Human; // 获取实体类别。

        /// <summary>
        /// 绑定对白资产。
        /// </summary>
        /// <param name="asset">已经加载的 Unity 资产。</param>
        public void SetDialogue(DialogueAsset asset) // 绑定对白资产。
        { // 开始方法。
            dialogue = asset; // 更新 对白资产或当前对白，使用 已经加载的 Unity 资产。
        } // 结束方法。

        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => "交谈"; // 获取交互提示文本。

        /// <summary>
        /// 存活且绑定对白时允许交谈。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => IsAlive && dialogue != null; // 存活且绑定对白时允许交谈。
        /// <summary>
        /// 检查交互条件后发布 DialogueRequested。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 检查交互条件后发布 DialogueRequested。
        { // 开始方法。
            if (CanInteract(actor)) // 当条件 CanInteract(actor) 成立时，执行下方分支。
                EventBus.Publish(new DialogueRequested(dialogue)); // 广播 DialogueRequested 事件，启动指定对白树。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
