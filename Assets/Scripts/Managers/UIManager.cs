using System.Collections.Generic;
using Guisha.Core;
using Guisha.Framework;
namespace Guisha.Managers
{
    public sealed class UIManager : MonoSingleton<UIManager>
    {
        private readonly Dictionary<string, IPanel> panels = new Dictionary<string, IPanel>();
        private readonly List<IPanel> stack = new List<IPanel>();
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        protected override void Awake()
        {
            base.Awake(); if (Instance != this) return;
            subscriptions.Add(EventBus.Subscribe<PanelRegistered>(e =>
            {
                if (panels.ContainsKey(e.Panel.Id)) { Guisha.Core.Logger.Warn("Duplicate panel: " + e.Panel.Id); return; }
                panels.Add(e.Panel.Id, e.Panel); e.Panel.Close();
            }));
            subscriptions.Add(EventBus.Subscribe<PanelUnregistered>(e =>
            {
                if (panels.TryGetValue(e.Panel.Id, out var current) && ReferenceEquals(current, e.Panel)) panels.Remove(e.Panel.Id);
                stack.Remove(e.Panel); EventBus.Publish(new InputLockChanged(e.Panel, false)); Refresh();
            }));
            subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ =>
            { foreach (var panel in stack.ToArray()) Close(panel.Id); }));
            subscriptions.Add(EventBus.Subscribe<PanelRequested>(e => { if (e.Open) Open(e.Id); else Close(e.Id); }));
        }
        public void Open(string id)
        {
            if (!panels.TryGetValue(id, out var panel)) { Guisha.Core.Logger.Warn("Panel not registered: " + id); return; }
            stack.Remove(panel); stack.Add(panel); panel.Open();
            if (panel.Modal) EventBus.Publish(new InputLockChanged(panel, true));
            Refresh();
        }
        public void Close(string id)
        {
            if (!panels.TryGetValue(id, out var panel)) return;
            stack.Remove(panel); panel.Close(); EventBus.Publish(new InputLockChanged(panel, false)); Refresh();
        }
        public void CloseTop() { if (stack.Count > 0) Close(stack[stack.Count - 1].Id); }
        private void Refresh()
        {
            bool blocked = false;
            for (int i = stack.Count - 1; i >= 0; i--)
            { stack[i].SetInteractable(!blocked); if (stack[i].Modal) blocked = true; }
        }
        protected override void OnDestroy() { subscriptions.Dispose(); base.OnDestroy(); }
    }
}
