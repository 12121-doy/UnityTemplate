using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class HumanNPC : BaseEntity, IInteractable
    {
        [SerializeField] private DialogueAsset dialogue;
        public override EntityType Type => EntityType.Human;
        public void SetDialogue(DialogueAsset asset) { dialogue = asset; }
        public string Prompt => "交谈";
        public bool CanInteract(GameObject actor) => IsAlive && dialogue != null;
        public void Interact(GameObject actor) { if (CanInteract(actor)) EventBus.Publish(new DialogueRequested(dialogue)); }
    }
}
