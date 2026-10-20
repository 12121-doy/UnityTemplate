using System;
using System.Collections.Generic;
using System.Linq;
using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    public sealed class Inventory : MonoBehaviour
    {
        public Item[] catalog = Array.Empty<Item>();
        [SerializeField, Min(1)] private int capacity = 12;
        private readonly List<Item> items = new List<Item>();
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        public IReadOnlyList<Item> Items => items;
        private void OnEnable()
        {
            AsyncHelper.Delay(this, 0, Publish);
            subscriptions.Add(EventBus.Subscribe<ItemPickupRequested>(e =>
            { if (e.Actor == gameObject) e.Completed?.Invoke(TryAdd(e.ItemId)); }));
            subscriptions.Add(EventBus.Subscribe<UseItemRequested>(e => Use(e.Slot)));
            subscriptions.Add(EventBus.Subscribe<SaveCapture>(e => e.Data.inventory = Snapshot()));
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(e =>
            {
                items.Clear(); foreach (var id in e.Data.inventory ?? Array.Empty<string>())
                { var item = Array.Find(catalog, x => x != null && x.id == id); if (item != null && items.Count < capacity) items.Add(item); }
                Publish();
            }));
        }
        private void Start() { Publish(); }
        private void OnDisable() { subscriptions.Dispose(); }
        private void Update() { if (GameSession.InputEnabled && Input.GetKeyDown(KeyCode.Alpha1)) Use(0); }
        public bool TryAdd(string id)
        {
            var item = Array.Find(catalog, x => x != null && x.id == id);
            if (item == null || items.Count >= capacity) { EventBus.Publish(new Notification("背包已满或物品未配置")); return false; }
            items.Add(item); Publish(); return true;
        }
        public void Use(int slot)
        {
            if (!GameSession.SimulationEnabled || slot < 0 || slot >= items.Count) return;
            var item = items[slot]; if (!item.Use(gameObject)) return;
            if (item.consumable) items.RemoveAt(slot); Publish();
        }
        private string[] Snapshot() => items.Select(x => x.id).ToArray();
        private void Publish() { EventBus.Publish(new InventoryChanged(Snapshot())); }
    }
}
