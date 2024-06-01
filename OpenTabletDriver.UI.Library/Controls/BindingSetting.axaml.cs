using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.UI.ViewModels;
using OpenTabletDriver.UI.ViewModels.Plugin;
using OpenTabletDriver.UI.Views.Dialogs;
using PlatformMouseButton = OpenTabletDriver.Platform.Pointer.MouseButton;


namespace OpenTabletDriver.UI.Controls;

public partial class BindingSetting : UserControl
{
    private KeyModStruct _modifiers;
    private bool _capturing;

    public BindingSetting()
    {
        InitializeComponent();
        VIEW_BindingButton.AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel);
        VIEW_BindingButton.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    public void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Handled || _capturing) return;

        e.Handled = true;
        var currPointProp = e.GetCurrentPoint(this).Properties;
        switch (currPointProp.PointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonReleased:
                if (sender == VIEW_BindingButton)
                    StartCapture();
                break;
            case PointerUpdateKind.RightButtonReleased:
                ShowBindingMenu();
                break;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || _capturing) return;

        e.Handled = true;

        if (e.Key == Key.Back)
        {
            var vm = (BindingSettingViewModel)DataContext!;
            vm.SelectedBinding = null;
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (e.Handled) return;
        VIEW_BindingButton.Focus();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (e.Handled) return;

        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.FocusManager?.ClearFocus();
    }

    private void StartCapture()
    {
        if (_capturing)
            return;
        _capturing = true;

        VIEW_BindingButton.RemoveHandler(PointerReleasedEvent, OnReleased);
        VIEW_BindingButton.AddHandler(PointerPressedEvent, SilenceEvent, RoutingStrategies.Tunnel);
        VIEW_BindingButton.AddHandler(PointerReleasedEvent, CapturePointerKeyDownReleased, RoutingStrategies.Tunnel);
        VIEW_BindingButton.AddHandler(KeyDownEvent, CaptureKeyDown, RoutingStrategies.Tunnel);
        VIEW_BindingButton.AddHandler(KeyUpEvent, CaptureKeyUp, RoutingStrategies.Tunnel);

        var vm = (BindingSettingViewModel)DataContext!;
        vm.BindingDescription = "Press a key...";
    }

    private void SilenceEvent(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void CapturePointerKeyDownReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Handled = true;
        var key = e.GetCurrentPoint(sender as Visual).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonReleased => PlatformMouseButton.Left,
            PointerUpdateKind.RightButtonReleased => PlatformMouseButton.Right,
            PointerUpdateKind.MiddleButtonReleased => PlatformMouseButton.Middle,
            PointerUpdateKind.XButton1Released => PlatformMouseButton.Backward,
            PointerUpdateKind.XButton2Released => PlatformMouseButton.Forward,
            _ => PlatformMouseButton.None
        };

        var vm = (BindingSettingViewModel)DataContext!;
        vm.SetToCapturedMouseKey(key.ToString());
        StopCapture();
    }

    private void CaptureKeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
        if (!_modifiers.SetModifier(e.Key, true))
        {
            var keyList = _modifiers.ToList();
            keyList.Add(e.Key.ToBindableKey());

            var vm = (BindingSettingViewModel)DataContext!;
            var str = string.Join(" + ", keyList.Select(key => key.ToStringFast()));
            vm.SetToCapturedKey(str);
            StopCapture();
        }
    }

    private void CaptureKeyUp(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
        _modifiers.SetModifier(e.Key, false);
    }

    private void StopCapture()
    {
        VIEW_BindingButton.RemoveHandler(PointerPressedEvent, SilenceEvent);
        VIEW_BindingButton.RemoveHandler(PointerReleasedEvent, CapturePointerKeyDownReleased);
        VIEW_BindingButton.RemoveHandler(KeyDownEvent, CaptureKeyDown);
        VIEW_BindingButton.RemoveHandler(KeyUpEvent, CaptureKeyUp);
        VIEW_BindingButton.AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel);

        _capturing = false;
        _modifiers = default;
    }

    private async void ShowBindingMenu()
    {
        var dialogVm = Ioc.Default.GetRequiredService<BindingMenuViewModel>()!;
        dialogVm.Setting = (BindingSettingViewModel)DataContext!;
        var dialog = new BindingMenuDialog()
        {
            DataContext = dialogVm
        };

        var window = (Window)TopLevel.GetTopLevel(this)!;
        await dialog.ShowDialog(window);
    }

    private struct KeyModStruct
    {
        public BindableKey Alt;
        public BindableKey Control;
        public BindableKey Shift;
        public BindableKey Meta;

        public bool SetModifier(Key key, bool state)
        {
            var bindableKey = state ? key.ToBindableKey() : BindableKey.None;

            switch (bindableKey)
            {
                case BindableKey.LeftAlt or BindableKey.RightAlt:
                    Alt = bindableKey;
                    return true;
                case BindableKey.LeftControl or BindableKey.RightControl:
                    Control = bindableKey;
                    return true;
                case BindableKey.LeftShift or BindableKey.RightShift:
                    Shift = bindableKey;
                    return true;
                case BindableKey.LeftWindows or BindableKey.RightWindows:
                    Meta = bindableKey;
                    return true;
                default:
                    return false;
            }
        }

        public readonly List<BindableKey> ToList()
        {
            var list = new List<BindableKey>(5);

            if (Control != BindableKey.None)
                list.Add(Control);
            if (Alt != BindableKey.None)
                list.Add(Alt);
            if (Shift != BindableKey.None)
                list.Add(Shift);
            if (Meta != BindableKey.None)
                list.Add(Meta);

            return list;
        }
    }
}
