using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 从相机中央发射射线，更新提示并调用 IInteractable。
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour // 从相机中央发射射线，更新提示并调用 IInteractable。
    { // 开始类型。
        /// <summary>
        /// 保存子节点只读视图。
        /// </summary>
        public Camera view; // 声明 子节点只读视图，后续初始化。
        /// <summary>
        /// 保存交互射线层掩码。
        /// </summary>
        public LayerMask interactionMask = ~0; // 把 ~0 保存为 交互射线层掩码。
        /// <summary>
        /// 保存作用或检测范围。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float range = 5; // 把 5 保存为 作用或检测范围。
        /// <summary>
        /// 保存当前项。
        /// </summary>
        private IInteractable current; // 声明 当前项，后续初始化。
        /// <summary>
        /// 保存交互提示文本。
        /// </summary>
        private string prompt; // 声明 交互提示文本，后续初始化。
        /// <summary>
        /// 射线按距离排序并过滤玩家自身，更新提示，E 键执行交互并广播交互动画。
        /// </summary>
        private void Update() // 射线按距离排序并过滤玩家自身，更新提示，E 键执行交互并广播交互动画。
        { // 开始方法。
            current = null; // 清空 当前项 引用，表示当前不再持有该对象。
            if (view == null) // 当子节点只读视图为空时，执行下方分支。
                view = Camera.main; // 更新 子节点只读视图，使用 主相机。
            if (view != null && GameSession.InputEnabled) // 当（子节点只读视图不为空；且当前允许玩家输入）时，执行下方分支。
            { // 开始条件分支。
                var ray = view.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // 把 view.ViewportPointToRay 的返回值 保存为 ray。
                var hits = Physics.RaycastAll(ray, range, interactionMask, QueryTriggerInteraction.Ignore); // 把 Physics.RaycastAll 的返回值 保存为 hits。
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance)); // 按命中距离排序射线结果，使近处实体先参与遮挡/交互判断。
                foreach (var hit in hits) // 遍历 hits，每次把一个元素保存为 hit。
                { // 开始遍历循环。
                    if (hit.transform.IsChildOf(transform)) // 当条件 hit.transform.IsChildOf(transform) 成立时，执行下方分支。
                        continue; // 跳过本次循环剩余步骤，继续处理下一项。
                    current = hit.collider.GetComponentInParent<IInteractable>(); // 更新 当前项，使用 hit.collider.GetComponentInParent<IInteractable> 的返回值。
                    if (current != null && !current.CanInteract(gameObject)) // 当（当前项不为空；且条件 current.CanInteract(gameObject) 不成立）时，执行下方分支。
                        current = null; // 清空 当前项 引用，表示当前不再持有该对象。
                    break; // Walls block interaction with objects behind them.；结束当前循环或 switch 分支。
                } // 结束遍历循环。
            } // 结束条件分支。

            string next = current != null ? "[E] " + current.Prompt : ""; // 把 按条件 current != null 选择的值 保存为 下一项。
            if (prompt != next) // 当交互提示文本不等于下一项时，执行下方分支。
            { // 开始条件分支。
                prompt = next; // 更新 交互提示文本，使用 下一项。
                EventBus.Publish(new InteractionPromptChanged(prompt)); // 广播 InteractionPromptChanged 事件，更新 HUD 的交互提示。
            } // 结束条件分支。

            if (current != null && Input.GetKeyDown(KeyCode.E)) // 当（当前项不为空；且条件 Input.GetKeyDown(KeyCode.E) 成立）时，执行下方分支。
            { // 开始条件分支。
                current.Interact(gameObject); // 执行交互契约。
                EventBus.Publish(new EntityAnimationRequested(GetComponent<BaseEntity>(), EntityAnimationState.Interact)); // 广播 EntityAnimationRequested 事件，让动画组件播放指定实体动作。
            } // 结束条件分支。
        } // 结束方法。

        /// <summary>
        /// 清空 HUD 交互提示，避免对象停用后保留旧目标文本。
        /// </summary>
        private void OnDisable() // 清空 HUD 交互提示，避免对象停用后保留旧目标文本。
        { // 开始方法。
            EventBus.Publish(new InteractionPromptChanged("")); // 广播 InteractionPromptChanged 事件，更新 HUD 的交互提示。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
