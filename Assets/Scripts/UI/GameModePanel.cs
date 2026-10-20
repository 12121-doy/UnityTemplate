using Guisha.Core;
using Guisha.Framework;
namespace Guisha.UI
{
    public sealed class GameModePanel : UIPanelBase
    {
        public void Select(int mode)
        { EventBus.Publish(new GameModeRequested((GameMode)mode)); EventBus.Publish(new PanelRequested(Id, false)); }
        public void Back() { EventBus.Publish(new PanelRequested(Id, false)); }
    }
}
