using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using OpenTabletDriver.UI.Navigation;

namespace OpenTabletDriver.UI.Controls;

// TODO: Convert to templated control

/// <summary>
/// Displays <see cref="ContentControl.Content"/> with different possible
/// transitions for different navigation types.
/// </summary>
public partial class NavigationHost : UserControl
{
    private readonly INavigatorFactory _navigatorFactory;
    private INavigator? _navigator;

    public NavigationHost()
    {
        InitializeComponent();
        _navigatorFactory = Ioc.Default.GetRequiredService<INavigatorFactory>();
    }

    public static readonly StyledProperty<string?> NavigationHostNameProperty =
        AvaloniaProperty.Register<NavigationHost, string?>(nameof(NavigationHostName));

    public string? NavigationHostName
    {
        get => GetValue(NavigationHostNameProperty);
        set => SetValue(NavigationHostNameProperty, value);
    }

    public static readonly StyledProperty<IPageTransition?> NextTransitionProperty =
        AvaloniaProperty.Register<NavigationHost, IPageTransition?>(nameof(NextTransition),
            new CrossFade(TimeSpan.FromSeconds(0.125)));

    public IPageTransition? NextTransition
    {
        get => GetValue(NextTransitionProperty);
        set => SetValue(NextTransitionProperty, value);
    }

    public static readonly StyledProperty<IPageTransition?> BackTransitionProperty =
        AvaloniaProperty.Register<NavigationHost, IPageTransition?>(nameof(BackTransition),
            new CrossFade(TimeSpan.FromSeconds(0.125)));

    public IPageTransition? BackTransition
    {
        get => GetValue(BackTransitionProperty);
        set => SetValue(BackTransitionProperty, value);
    }

    public static readonly StyledProperty<IPageTransition?> NextAsRootTransitionProperty =
        AvaloniaProperty.Register<NavigationHost, IPageTransition?>(nameof(NextAsRootTransition));

    public IPageTransition? NextAsRootTransition
    {
        get => GetValue(NextAsRootTransitionProperty);
        set => SetValue(NextAsRootTransitionProperty, value);
    }

    public static readonly StyledProperty<IPageTransition?> BackToRootTransitionProperty =
        AvaloniaProperty.Register<NavigationHost, IPageTransition?>(nameof(BackToRootTransition));

    public IPageTransition? BackToRootTransition
    {
        get => GetValue(BackToRootTransitionProperty);
        set => SetValue(BackToRootTransitionProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == NavigationHostNameProperty)
        {
            if (_navigator is not null)
            {
                _navigator.Navigated -= HandleNavigated;
            }

            if (change.NewValue is string newNavHostName)
            {
                _navigator = _navigatorFactory.GetOrCreate(newNavHostName);
                _navigator.Navigated += HandleNavigated;
            }
        }
        base.OnPropertyChanged(change);
    }

    private void HandleNavigated(object? sender, NavigationEventData e)
    {
        VIEW_TransitioningContentControl.PageTransition = e.Kind switch
        {
            NavigationKind.Push => NextTransition,
            NavigationKind.Pop => BackTransition ?? NextTransition,
            NavigationKind.PushAsRoot => NextAsRootTransition ?? NextTransition,
            NavigationKind.PopToRoot => BackToRootTransition ?? BackTransition ?? NextTransition,
            _ => throw new InvalidOperationException("Invalid navigation kind.")
        };

        VIEW_TransitioningContentControl.Content = e.Current;
    }
}
