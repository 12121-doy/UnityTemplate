using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace Guisha.UI
{
    public sealed class HUD : MonoBehaviour
    {
        public Slider sanBar, staminaBar;
        public Text modeText;
        public Text statsText, promptText, notificationText, quickSlotsText;
        private readonly SubscriptionBag subscriptions = new SubscriptionBag();
        private float hideNotificationAt;
        private void OnEnable()
        {
            RefreshMode();
            subscriptions.Add(EventBus.Subscribe<GameModeChanged>(_ => RefreshMode()));
            subscriptions.Add(EventBus.Subscribe<GameModeProgress>(_ => RefreshMode()));
            subscriptions.Add(EventBus.Subscribe<PlayerStatsChanged>(e =>
            {
                if (sanBar != null) sanBar.value = e.San / Mathf.Max(1, e.MaxSan);
                if (staminaBar != null) staminaBar.value = e.Stamina / Mathf.Max(1, e.MaxStamina);
                if (statsText != null) statsText.text = $"SAN {e.San:F0} / {e.MaxSan:F0}     体力 {e.Stamina:F0} / {e.MaxStamina:F0}";
            }));
            subscriptions.Add(EventBus.Subscribe<InteractionPromptChanged>(e => { if (promptText != null) promptText.text = e.Text; }));
            subscriptions.Add(EventBus.Subscribe<Notification>(e => { if (notificationText != null) notificationText.text = e.Text; hideNotificationAt = Time.unscaledTime + 5; }));
            subscriptions.Add(EventBus.Subscribe<InventoryChanged>(e => { if (quickSlotsText != null) quickSlotsText.text = "快捷栏 [1]  " + string.Join(" | ", e.ItemIds); }));
            subscriptions.Add(EventBus.Subscribe<EntityDied>(e => { if (e.Entity.Type == EntityType.Player) EventBus.Publish(new Notification("你已死亡，按 F9 读取检查点")); }));
            subscriptions.Add(EventBus.Subscribe<LowSanChanged>(e => { if (statsText != null) statsText.color = e.Active ? Color.red : Color.white; }));
        }
        private void RefreshMode()
        {
            if (modeText == null) return;
            var rules = GameModeRuntime.Rules;
            modeText.text = GameModeRuntime.Label(rules.Mode) + (rules.TimeLimit > 0 ? " · 剩余 " + Mathf.CeilToInt(GameModeRuntime.Remaining) + " 秒" : rules.Mode == GameMode.Endless ? " · 波次 " + GameModeRuntime.Wave : "") + "  [M]";
        }
        private void Update() { if (notificationText != null && Time.unscaledTime > hideNotificationAt) notificationText.text = ""; }
        private void OnDisable() { subscriptions.Dispose(); }
    }
}
