namespace Guisha.Framework
{
    public interface IPanel
    {
        string Id { get; }
        bool Modal { get; }
        bool IsOpen { get; }
        void Open();
        void Close();
        void SetInteractable(bool value);
    }
    public readonly struct PanelRegistered { public readonly IPanel Panel; public PanelRegistered(IPanel panel) { Panel = panel; } }
    public readonly struct PanelUnregistered { public readonly IPanel Panel; public PanelUnregistered(IPanel panel) { Panel = panel; } }
}
