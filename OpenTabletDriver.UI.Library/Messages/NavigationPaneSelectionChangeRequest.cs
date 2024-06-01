using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OpenTabletDriver.UI.Messages;

public sealed class NavigationPaneSelectionChangeRequest : ValueChangedMessage<NavigationItemSelection>
{
    public NavigationPaneSelectionChangeRequest(NavigationItemSelection value) : base(value)
    {
    }
}

public enum NavigationItemSelection
{
    None,
    Daemon,
    Tablet,
    Tool,
    PluginManager,
    Diagnostics,
    Settings
}
