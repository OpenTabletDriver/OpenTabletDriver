using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenTabletDriver.UI.Models;

// Do not use MVVM source gen here, as it will cause STJ source gen to fail to
// generate a serializer for this class.
public partial class UISettings : ObservableObject
{
    private bool _firstLaunch = true;
    private bool _kaomoji = true;
    private bool _transparency = true;
    private float _randomStuff = 0;

    public bool FirstLaunch
    {
        get => _firstLaunch;
        set => SetProperty(ref _firstLaunch, value);
    }

    public bool Kaomoji
    {
        get => _kaomoji;
        set => SetProperty(ref _kaomoji, value);
    }

    public bool Transparency
    {
        get => _transparency;
        set => SetProperty(ref _transparency, value);
    }

    public float RandomStuff
    {
        get => _randomStuff;
        set => SetProperty(ref _randomStuff, value);
    }
}
