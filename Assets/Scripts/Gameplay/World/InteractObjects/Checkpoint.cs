using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class Checkpoint : MonoBehaviour, IInteractable
    {
        public string checkpointId;
        public Transform respawnPoint;
        public string Prompt => "保存检查点";
        public bool CanInteract(GameObject actor) => true;
        public void Interact(GameObject actor)
        {
            var point = respawnPoint != null ? respawnPoint : actor.transform;
            EventBus.Publish(new CheckpointReached(checkpointId, point.position, point.rotation));
        }
    }
}
