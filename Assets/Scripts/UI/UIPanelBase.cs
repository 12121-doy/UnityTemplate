using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelBase : MonoBehaviour, IPanel
    {
        public string panelId = "Panel";
        public bool modal = true;
        private CanvasGroup group;
        public string Id => panelId;
        public bool Modal => modal;
        public bool IsOpen { get; private set; }
        protected virtual void Awake() { group = GetComponent<CanvasGroup>(); }
        protected virtual void OnEnable() { EventBus.Publish(new PanelRegistered(this)); }
        protected virtual void OnDisable() { EventBus.Publish(new PanelUnregistered(this)); }
        public virtual void Open() { IsOpen = true; group.alpha = 1; transform.SetAsLastSibling(); SetInteractable(true); }
        public virtual void Close() { IsOpen = false; group.alpha = 0; SetInteractable(false); }
        public void SetInteractable(bool value) { group.interactable = value && IsOpen; group.blocksRaycasts = value && IsOpen; }
        // Keep the GameObject active: hidden panels continue receiving bus messages.
    }
}
