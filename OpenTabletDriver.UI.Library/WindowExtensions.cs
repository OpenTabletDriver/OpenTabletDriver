using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI;

public static class WindowExtensions
{
    private const double WINDOW_BG_SHOW = 1.0;
    private const double WINDOW_BG_HIDE = 0.0;

    public static IDisposable BootstrapTransparency(this Window window, WindowViewModelBase vm, IDispatcher dispatcher)
    {
        window.TransparencyLevelHint = new[]
        {
            WindowTransparencyLevel.Mica,
            WindowTransparencyLevel.AcrylicBlur
        };

        var windowBg = window.GetControl<Rectangle>("VIEW_WindowBg");
        var acrylicBorder = window.GetControl<ExperimentalAcrylicBorder>("VIEW_AcrylicBorder");

        bool windowTransparency = false; // this variable will be captured by the delegates below

        window.Activated += (_, _) => { if (windowTransparency) windowBg.Opacity = WINDOW_BG_HIDE; };
        window.Deactivated += (_, _) => { if (windowTransparency) windowBg.Opacity = WINDOW_BG_SHOW; };

        return vm.HandleProperty(
            nameof(vm.TransparencyEnabled),
            vm => vm.TransparencyEnabled,
            (vm, transparencyEnabled) => dispatcher.ProbablySynchronousPost(() =>
            {
                if (transparencyEnabled)
                {
                    enableWindowTransparency();
                }
                else
                {
                    disableWindowTransparency();
                }
            })
        );

        void enableWindowTransparency()
        {
            if (window.ActualTransparencyLevel != WindowTransparencyLevel.None)
            {
                windowTransparency = true;
                acrylicBorder.IsVisible = true;
                windowBg.Opacity = WINDOW_BG_HIDE;
            }
            else
            {
                // If transparency is not supported, disable it
                disableWindowTransparency();
            }
        }

        void disableWindowTransparency()
        {
            windowTransparency = false;
            windowBg.Opacity = WINDOW_BG_SHOW;
        }
    }
}
