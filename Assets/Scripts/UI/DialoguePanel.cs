using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.UI; // 引入 UnityEngine.UI 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 显示对白角色、正文与分支，处理继续和选择输入。
    /// </summary>
    public sealed class DialoguePanel : UIPanelBase // 显示对白角色、正文与分支，处理继续和选择输入。
    { // 开始类型。
        /// <summary>
        /// 保存角色名称文本组件、对白正文文本组件、选项文本组件。
        /// </summary>
        public Text speaker, body, choiceText; // 声明 角色名称文本组件，后续初始化；声明 对白正文文本组件，后续初始化；声明 选项文本组件，后续初始化。
        /// <summary>
        /// 保存当前对白选项数量。
        /// </summary>
        private int choices; // 声明 当前对白选项数量，后续初始化。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 执行面板基类注册，订阅对白台词并更新角色、正文和选项。
        /// </summary>
        protected override void OnEnable() // 执行面板基类注册，订阅对白台词并更新角色、正文和选项。
        { // 开始方法。
            base.OnEnable(); // 先执行基类启用注册流程。
            subscriptions.Add(EventBus.Subscribe<DialogueLineChanged>(e => // 订阅 DialogueLineChanged 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (speaker != null) // 当角色名称文本组件不为空时，执行下方分支。
                    speaker.text = e.Node.speaker; // 更新 speaker.text，使用 e.Node.speaker。
                if (body != null) // 当对白正文文本组件不为空时，执行下方分支。
                    body.text = e.Node.text; // 更新 body.text，使用 e.Node.text。
                choices = e.Node.choices?.Length ?? 0; // 更新 当前对白选项数量，使用 e.Node.choices?.Length ?? 0。
                if (choiceText == null) // 当选项文本组件为空时，执行下方分支。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                choiceText.text = choices == 0 ? "[空格] 继续" : ""; // 更新 choiceText.text，使用 按条件 choices == 0 选择的值。
                for (int i = 0; i < choices; i++) // 按初始化与递增步骤循环；继续条件为 i < choices。
                    choiceText.text += $"[{i + 1}] {e.Node.choices[i].text}\n"; // 累加 choiceText.text，使用 $"[{i + 1}] {e.Node.choices[i].text}\n"。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 面板打开且处于 Playing 时处理空格继续与数字选项。
        /// </summary>
        private void Update() // 面板打开且处于 Playing 时处理空格继续与数字选项。
        { // 开始方法。
            if (!IsOpen || GameSession.State != GameState.Playing) // 当（条件 IsOpen 不成立；或GameSession.State不等于GameState.Playing）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            if (choices == 0 && Input.GetKeyDown(KeyCode.Space)) // 当（当前对白选项数量等于0；且条件 Input.GetKeyDown(KeyCode.Space) 成立）时，执行下方分支。
                Choose(0); // 广播选项索引给对白系统。
            for (int i = 0; i < Mathf.Min(choices, 9); i++) // 按初始化与递增步骤循环；继续条件为 i < Mathf.Min(choices, 9)。
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) // 当条件 Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)) 成立时，执行下方分支。
                    Choose(i); // 广播选项索引给对白系统。
        } // 结束方法。

        /// <summary>
        /// 广播选项索引给对白系统。
        /// </summary>
        /// <param name="index">当前索引。</param>
        public void Choose(int index) // 广播选项索引给对白系统。
        { // 开始方法。
            EventBus.Publish(new DialogueChoiceRequested(index)); // 广播 DialogueChoiceRequested 事件，选择对白分支或继续。
        } // 结束方法。

        /// <summary>
        /// 释放对白订阅，再执行面板基类注销。
        /// </summary>
        protected override void OnDisable() // 释放对白订阅，再执行面板基类注销。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            base.OnDisable(); // 执行基类禁用与退订流程。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
