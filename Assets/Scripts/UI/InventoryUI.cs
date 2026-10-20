using Guisha.Core;
using Guisha.Framework;
using UnityEngine.UI;
namespace Guisha.UI
{
    public sealed class InventoryUI : UIPanelBase
    {
        public Text itemsText;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        protected override void OnEnable()
        {
            base.OnEnable(); subscriptions.Add(EventBus.Subscribe<InventoryChanged>(e =>
            {
                if (itemsText == null) return;
                itemsText.text = "背包\n";
                for (int i = 0; i < e.ItemIds.Length; i++) itemsText.text += $"{i + 1}. {e.ItemIds[i]}\n";
            }));
        }
        public void Use(int slot) { EventBus.Publish(new UseItemRequested(slot)); }
        protected override void OnDisable() { subscriptions.Dispose(); base.OnDisable(); }
    }
}
