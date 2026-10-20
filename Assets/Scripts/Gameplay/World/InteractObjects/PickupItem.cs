using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class PickupItem : MonoBehaviour, IInteractable
    {
        public string itemId;
        public string persistentId;
        private bool collected;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        public string Prompt => "拾取鬼器";
        private void OnEnable() { subscriptions.Add(EventBus.Subscribe<WorldStateChanged>(_ => Refresh())); Refresh(); }
        private void OnDisable() { subscriptions.Dispose(); }
        public void Refresh()
        {
            if (!string.IsNullOrEmpty(persistentId)) collected = WorldState.Has(persistentId);
            foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = !collected;
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = !collected;
        }
        public bool CanInteract(GameObject actor) => !collected;
        public void Interact(GameObject actor)
        {
            if (collected) return;
            EventBus.Publish(new ItemPickupRequested(actor, itemId, success =>
            { if (!success) return; collected = true; WorldState.Set(persistentId); Refresh(); }));
        }
    }
}
