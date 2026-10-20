using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class Mechanism : MonoBehaviour, IInteractable
    {
        public string mechanismId;
        public string requiredClueId;
        [TextArea] public string conditionExpression;
        private string parsedText;
        private Guisha.Core.Patterns.ICondition condition;
        public string Prompt => "启动机关";
        public bool CanInteract(GameObject actor) => true;
        public void Interact(GameObject actor)
        {
            if (!string.IsNullOrWhiteSpace(conditionExpression))
            {
                try
                {
                    if (parsedText != conditionExpression) { condition = Guisha.Core.Patterns.ConditionInterpreter.Parse(conditionExpression); parsedText = conditionExpression; }
                    if (!condition.Evaluate(WorldState.Has)) { EventBus.Publish(new Notification("机关条件尚未满足")); return; }
                }
                catch (System.FormatException error) { Guisha.Core.Logger.Warn(error.Message); EventBus.Publish(new Notification("机关条件配置有误")); return; }
            }
            if (!string.IsNullOrEmpty(requiredClueId) && !WorldState.Has(requiredClueId))
            { EventBus.Publish(new Notification("还缺少线索")); return; }
            WorldState.Set(mechanismId); EventBus.Publish(new MechanismActivated(mechanismId));
            EventBus.Publish(new Notification("机关已启动"));
        }
    }
}
