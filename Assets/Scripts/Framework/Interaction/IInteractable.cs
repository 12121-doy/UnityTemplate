using UnityEngine;
namespace Guisha.Framework
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract(GameObject actor);
        void Interact(GameObject actor);
    }
}
