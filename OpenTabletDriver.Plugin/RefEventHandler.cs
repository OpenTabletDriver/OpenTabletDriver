namespace OpenTabletDriver.Plugin
{

    public delegate void RefEventHandler<TEventArgs>(object? sender, ref TEventArgs e) where TEventArgs : allows ref struct;
}
