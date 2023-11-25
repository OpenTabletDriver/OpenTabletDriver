using OpenTabletDriver.Daemon.Contracts.Persistence;

namespace OpenTabletDriver.UI.Models;

public readonly struct ProfileBinding
{
    private readonly Func<Profile, PluginSetting> _getter;
    private readonly Action<Profile, PluginSetting> _setter;

    public ProfileBinding(Func<Profile, PluginSetting> getter, Action<Profile, PluginSetting> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    public T? GetValue<T>(Profile profile)
    {
        return _getter(profile).GetValue<T>();
    }

    public void SetValue<T>(Profile profile, T value)
    {
        _setter(profile, new PluginSetting(_getter(profile).Property, value));
    }
}
