using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class ClueNote : MonoBehaviour, IInteractable
    {
        public string clueId;
        [TextArea] public string text;
        public string Prompt => "阅读纸条";
        public bool CanInteract(GameObject actor) => true;
        public void Interact(GameObject actor) { WorldState.Set(clueId); EventBus.Publish(new ClueRead(clueId, text)); EventBus.Publish(new Notification(text)); }
    }
}
