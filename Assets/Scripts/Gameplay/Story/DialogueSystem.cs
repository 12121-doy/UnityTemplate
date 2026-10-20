using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class DialogueSystem : MonoBehaviour
    {
        private DialogueAsset current;
        private int index;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private void OnEnable()
        {
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => End()));
            subscriptions.Add(EventBus.Subscribe<DialogueRequested>(e => Begin(e.Dialogue)));
            subscriptions.Add(EventBus.Subscribe<DialogueChoiceRequested>(e => Next(e.Index)));
        }
        private void OnDisable() { End(); subscriptions.Dispose(); }
        private void Begin(DialogueAsset asset)
        {
            if (asset == null || asset.nodes.Length == 0) return;
            current = asset; index = 0;
            EventBus.Publish(new InputLockChanged(this, true)); EventBus.Publish(new PanelRequested("Dialogue", true)); Show();
        }
        private void Show()
        {
            if (current == null || index < 0 || index >= current.nodes.Length) { End(); return; }
            EventBus.Publish(new DialogueLineChanged(current.nodes[index]));
        }
        private void Next(int choice)
        {
            if (current == null) return;
            var node = current.nodes[index];
            if (node.choices != null && node.choices.Length > 0)
            { if (choice < 0 || choice >= node.choices.Length) return; index = node.choices[choice].next; }
            else index = node.next;
            Show();
        }
        private void End()
        {
            if (current == null) return;
            current = null; EventBus.Publish(new InputLockChanged(this, false));
            EventBus.Publish(new PanelRequested("Dialogue", false)); EventBus.Publish(new DialogueEnded());
        }
    }
}
