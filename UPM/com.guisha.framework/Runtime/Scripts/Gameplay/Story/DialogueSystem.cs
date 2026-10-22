using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 运行对白树、分支和输入锁。
    /// </summary>
    public sealed class DialogueSystem : MonoBehaviour // 运行对白树、分支和输入锁。
    { // 开始类型。
        /// <summary>
        /// 保存当前项。
        /// </summary>
        private DialogueAsset current; // 声明 当前项，后续初始化。
        /// <summary>
        /// 保存当前对白节点索引。
        /// </summary>
        private int index; // 声明 当前对白节点索引，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 订阅对白请求、选项和读档；读档会结束当前对白。
        /// </summary>
        private void OnEnable() // 订阅对白请求、选项和读档；读档会结束当前对白。
        { // 开始方法。
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => End())); // 订阅 SaveLoaded 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<DialogueRequested>(e => Begin(e.Dialogue))); // 订阅 DialogueRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<DialogueChoiceRequested>(e => Next(e.Index))); // 订阅 DialogueChoiceRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
        } // 结束方法。

        /// <summary>
        /// 结束当前对白并释放全部订阅及自身输入锁。
        /// </summary>
        private void OnDisable() // 结束当前对白并释放全部订阅及自身输入锁。
        { // 开始方法。
            End(); // 清空对白，关闭面板、释放锁并广播 DialogueEnded。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
        } // 结束方法。

        /// <summary>
        /// 启动有效对白，从第 0 节点开始并打开面板/输入锁。
        /// </summary>
        /// <param name="asset">已经加载的 Unity 资产。</param>
        private void Begin(DialogueAsset asset) // 启动有效对白，从第 0 节点开始并打开面板/输入锁。
        { // 开始方法。
            if (asset == null || asset.nodes.Length == 0) // 当（已经加载的 Unity 资产为空；或asset.nodes.Length等于0）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            current = asset; // 更新 当前项，使用 已经加载的 Unity 资产。
            index = 0; // 更新 当前对白节点索引，使用 0。
            EventBus.Publish(new InputLockChanged(this, true)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
            EventBus.Publish(new PanelRequested("Dialogue", true)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            Show(); // 节点有效时广播 DialogueLineChanged，越界或结束时转 End。
        } // 结束方法。

        /// <summary>
        /// 节点有效时广播 DialogueLineChanged，越界或结束时转 End。
        /// </summary>
        private void Show() // 节点有效时广播 DialogueLineChanged，越界或结束时转 End。
        { // 开始方法。
            if (current == null || index < 0 || index >= current.nodes.Length) // 当（当前项为空；或当前对白节点索引小于0；或当前对白节点索引不小于current.nodes.Length）时，执行下方分支。
            { // 开始条件分支。
                End(); // 清空对白，关闭面板、释放锁并广播 DialogueEnded。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            EventBus.Publish(new DialogueLineChanged(current.nodes[index])); // 广播 DialogueLineChanged 事件，刷新对白面板。
        } // 结束方法。

        /// <summary>
        /// 有选项时按索引跳转，否则使用 next；随后显示新节点。
        /// </summary>
        /// <param name="choice">选项索引。</param>
        private void Next(int choice) // 有选项时按索引跳转，否则使用 next；随后显示新节点。
        { // 开始方法。
            if (current == null) // 当当前项为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var node = current.nodes[index]; // 把 current.nodes[index] 保存为 当前树节点或对白节点。
            if (node.choices != null && node.choices.Length > 0) // 当（node.choices不为空；且node.choices.Length大于0）时，执行下方分支。
            { // 开始条件分支。
                if (choice < 0 || choice >= node.choices.Length) // 当（选项索引小于0；或选项索引不小于node.choices.Length）时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                index = node.choices[choice].next; // 更新 当前对白节点索引，使用 node.choices[choice].next。
            } // 结束条件分支。
            else // 前面的条件不成立时，执行此备用分支。
                index = node.next; // 更新 当前对白节点索引，使用 node.next。
            Show(); // 节点有效时广播 DialogueLineChanged，越界或结束时转 End。
        } // 结束方法。

        /// <summary>
        /// 清空对白，关闭面板、释放锁并广播 DialogueEnded。
        /// </summary>
        private void End() // 清空对白，关闭面板、释放锁并广播 DialogueEnded。
        { // 开始方法。
            if (current == null) // 当当前项为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            current = null; // 清空 当前项 引用，表示当前不再持有该对象。
            EventBus.Publish(new InputLockChanged(this, false)); // 广播 InputLockChanged 事件，按拥有者增删输入锁。
            EventBus.Publish(new PanelRequested("Dialogue", false)); // 广播 PanelRequested 事件，交给 UIManager 维护面板栈和输入锁。
            EventBus.Publish(new DialogueEnded()); // 广播 DialogueEnded 事件，通知对白结束。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
