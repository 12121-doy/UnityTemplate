using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public sealed class StoryTrigger : MonoBehaviour
    {
        public bool once = true;
        public string persistentId;
        public DialogueAsset dialogue;
        public EntityDefinition spawn;
        public Transform spawnPoint;
        public CutscenePlayer cutscene;
        private bool used;
        private void Reset() { GetComponent<Collider>().isTrigger = true; }
        private void OnTriggerEnter(Collider other)
        {
            bool alreadyUsed = string.IsNullOrEmpty(persistentId) ? used : WorldState.Has(persistentId);
            if ((once && alreadyUsed) || other.GetComponentInParent<PlayerEntity>() == null) return;
            used = true; WorldState.Set(persistentId);
            if (dialogue != null) EventBus.Publish(new DialogueRequested(dialogue));
            if (spawn != null)
            { var point = spawnPoint != null ? spawnPoint : transform; EventBus.Publish(new SpawnEntityRequested(spawn, point.position, point.rotation, transform)); }
            if (cutscene != null) cutscene.Play();
        }
    }
}
