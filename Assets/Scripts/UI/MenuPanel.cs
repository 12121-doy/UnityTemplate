using Guisha.Core;
using Guisha.Framework;
namespace Guisha.UI
{
    public sealed class MenuPanel : UIPanelBase
    {
        public void Resume() { EventBus.Publish(new PanelRequested(Id, false)); EventBus.Publish(new PauseRequested(false)); }
        public void Save() { EventBus.Publish(new SaveRequested()); }
        public void Load() { EventBus.Publish(new LoadRequested()); }
        public void SetMasterVolume(float value) { UnityEngine.AudioListener.volume = UnityEngine.Mathf.Clamp01(value); }
    }
}
