using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OpenTabletDriver.UI.Messages;

public class UILayoutChangedMessage : ValueChangedMessage<UILayoutChange>
{
    public UILayoutChangedMessage(UILayoutChange value) : base(value)
    {
    }
}

public enum UILayoutChange
{
    None,
    SidebarOpen,
    SidebarHidden
}
