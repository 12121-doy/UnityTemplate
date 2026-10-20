using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class ScenePortal : MonoBehaviour, IInteractable
    {
        public string scenePath;
        public int dungeonId;
        public string Prompt => "进入血门";
        public bool CanInteract(GameObject actor) => GameSession.State == GameState.Playing;
        public void Interact(GameObject actor) { if (CanInteract(actor)) EventBus.Publish(new SceneTransitionRequested(scenePath, dungeonId)); }
    }
}
