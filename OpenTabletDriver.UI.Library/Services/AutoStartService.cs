namespace OpenTabletDriver.UI.Services;

public interface IAutoStartService
{
    bool AutoStartSupported { get; }
    bool AutoStart { get; }
    string? BackendName { get; }
    bool TrySetAutoStart(bool autoStart);
}

public class NullAutoStartService : IAutoStartService
{
    public bool AutoStartSupported => false;
    public bool AutoStart => false;
    public string? BackendName => null;
    public bool TrySetAutoStart(bool autoStart) => false;
}
