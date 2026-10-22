using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Gameplay // 将本文件类型归入 Guisha.Gameplay，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 按线索和布尔表达式检验条件后激活机关。
    /// </summary>
    public sealed class Mechanism : MonoBehaviour, IInteractable // 按线索和布尔表达式检验条件后激活机关。
    { // 开始类型。
        /// <summary>
        /// 保存机关持久标识。
        /// </summary>
        public string mechanismId; // 声明 机关持久标识，后续初始化。
        /// <summary>
        /// 保存旧式必需线索标识。
        /// </summary>
        public string requiredClueId; // 声明 旧式必需线索标识，后续初始化。
        /// <summary>
        /// 保存机关布尔条件表达式。
        /// </summary>
        [TextArea] // 让该文本字段在 Inspector 中显示为多行编辑区域。
        public string conditionExpression; // 声明 机关布尔条件表达式，后续初始化。
        /// <summary>
        /// 保存上次已解析的条件文本。
        /// </summary>
        private string parsedText; // 声明 上次已解析的条件文本，后续初始化。
        /// <summary>
        /// 保存已解析的机关条件树。
        /// </summary>
        private Guisha.Core.Patterns.ICondition condition; // 声明 已解析的机关条件树，后续初始化。
        /// <summary>
        /// 获取交互提示文本。
        /// </summary>
        public string Prompt => "启动机关"; // 获取交互提示文本。

        /// <summary>
        /// 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        /// <returns>true 表示允许当前发起者交互。</returns>
        public bool CanInteract(GameObject actor) => true; // 检查当前交互对象是否接受此次交互；返回 true 表示允许。
        /// <summary>
        /// 解析/检查条件表达式和旧线索条件，通过后设置标记，广播 MechanismActivated 并提示。
        /// </summary>
        /// <param name="actor">交互或使用物品的发起对象。</param>
        public void Interact(GameObject actor) // 解析/检查条件表达式和旧线索条件，通过后设置标记，广播 MechanismActivated 并提示。
        { // 开始方法。
            if (!string.IsNullOrWhiteSpace(conditionExpression)) // 当条件 string.IsNullOrWhiteSpace(conditionExpression) 不成立时，执行下方分支。
            { // 开始条件分支。
                try // 进入异常保护范围；失败时由 catch/finally 配对清理。
                { // 开始受异常保护的操作。
                    if (parsedText != conditionExpression) // 当上次已解析的条件文本不等于机关布尔条件表达式时，执行下方分支。
                    { // 开始条件分支。
                        condition = Guisha.Core.Patterns.ConditionInterpreter.Parse(conditionExpression); // 更新 已解析的机关条件树，使用 Guisha.Core.Patterns.ConditionInterpreter.Parse 的返回值。
                        parsedText = conditionExpression; // 更新 上次已解析的条件文本，使用 机关布尔条件表达式。
                    } // 结束条件分支。

                    if (!condition.Evaluate(WorldState.Has)) // 当条件 condition.Evaluate(WorldState.Has) 不成立时，执行下方分支。
                    { // 开始条件分支。
                        EventBus.Publish(new Notification("机关条件尚未满足")); // 广播 Notification 事件，让 HUD 显示短通知。
                        return; // 提前结束当前方法/回调，跳过后续处理。
                    } // 结束条件分支。
                } // 结束受异常保护的操作。
                catch (System.FormatException error) // 捕获前面操作抛出的异常，执行失败处理。
                { // 开始异常处理。
                    Guisha.Core.Logger.Warn(error.Message); // 按 WARN 级别输出警告，并按开关写文件。
                    EventBus.Publish(new Notification("机关条件配置有误")); // 广播 Notification 事件，让 HUD 显示短通知。
                    return; // 提前结束当前方法/回调，跳过后续处理。
                } // 结束异常处理。
            } // 结束条件分支。

            if (!string.IsNullOrEmpty(requiredClueId) && !WorldState.Has(requiredClueId)) // 当（条件 string.IsNullOrEmpty(requiredClueId) 不成立；且条件 WorldState.Has(requiredClueId) 不成立）时，执行下方分支。
            { // 开始条件分支。
                EventBus.Publish(new Notification("还缺少线索")); // 广播 Notification 事件，让 HUD 显示短通知。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            WorldState.Set(mechanismId); // 首次加入标记时广播 WorldStateChanged。
            EventBus.Publish(new MechanismActivated(mechanismId)); // 广播 MechanismActivated 事件，通知开锁与挑战目标系统。
            EventBus.Publish(new Notification("机关已启动")); // 广播 Notification 事件，让 HUD 显示短通知。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
