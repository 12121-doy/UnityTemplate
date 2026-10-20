using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.UI
{
    public sealed class UIInputRouter : MonoBehaviour
    {
        public GameModePanel modes;
        private bool inventoryOpen;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private void OnEnable() { subscriptions.Add(EventBus.Subscribe<SaveLoaded>(_ => inventoryOpen = false)); }
        private void OnDisable() { subscriptions.Dispose(); inventoryOpen = false; }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M) && (GameSession.State == GameState.Playing || GameSession.State == GameState.Paused))
                EventBus.Publish(new PanelRequested("Modes", modes == null || !modes.IsOpen));
            if (Input.GetKeyDown(KeyCode.F5)) EventBus.Publish(new SaveRequested());
            if (Input.GetKeyDown(KeyCode.F9)) EventBus.Publish(new LoadRequested());
            if (Input.GetKeyDown(KeyCode.Escape) && (GameSession.State == GameState.Playing || GameSession.State == GameState.Paused))
            {
                if (modes != null && modes.IsOpen) { EventBus.Publish(new PanelRequested("Modes", false)); return; }
                if (inventoryOpen) { inventoryOpen = false; EventBus.Publish(new PanelRequested("Inventory", false)); return; }
                bool pause = GameSession.State != GameState.Paused;
                EventBus.Publish(new PauseRequested(pause)); EventBus.Publish(new PanelRequested("Pause", pause));
            }
            if (Input.GetKeyDown(KeyCode.Tab) && GameSession.State == GameState.Playing)
            { inventoryOpen = !inventoryOpen; EventBus.Publish(new PanelRequested("Inventory", inventoryOpen)); }
        }
    }
}
