using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class Door : MonoBehaviour, IInteractable
    {
        public Transform hinge;
        public string unlockMechanismId;
        public string persistentId;
        public float openAngle = 90;
        public bool locked;
        private bool opened;
        private Quaternion closedRotation;
        private bool initiallyLocked;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        public string Prompt => locked ? "门被锁住了" : opened ? "关闭门" : "打开门";
        private void Awake() { if (hinge == null) hinge = transform; closedRotation = hinge.localRotation; initiallyLocked = locked; }
        private void OnEnable()
        {
            subscriptions.Add(EventBus.Subscribe<MechanismActivated>(e =>
            {
                if (string.IsNullOrEmpty(unlockMechanismId) || e.Id != unlockMechanismId) return;
                locked = false;
                if (!string.IsNullOrEmpty(persistentId)) WorldState.Set(persistentId + ".unlocked");
            }));
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => { opened = false; hinge.localRotation = closedRotation; Refresh(); }));
            subscriptions.Add(EventBus.Subscribe<WorldStateChanged>(_ => Refresh())); Refresh();
        }
        private void OnDisable() { subscriptions.Dispose(); }
        public void Refresh() { if (!string.IsNullOrEmpty(persistentId)) locked = !WorldState.Has(persistentId + ".unlocked") && (initiallyLocked || !string.IsNullOrEmpty(unlockMechanismId)); }
        public bool CanInteract(GameObject actor) => true;
        public void Interact(GameObject actor)
        {
            if (locked) { EventBus.Publish(new Notification("需要先解开机关")); return; }
            opened = !opened;
            hinge.localRotation = closedRotation * Quaternion.Euler(0, opened ? openAngle : 0, 0);
            EventBus.Publish(new NoiseEmitted(transform.position, 8, actor));
        }
    }
}
