using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Controls;

public partial class AreaDisplay : UserControl
{
    private static readonly IImmutableSolidColorBrush _boundsBrush = new ImmutableSolidColorBrush(0x0AFFFFFF);
    private static readonly IImmutableSolidColorBrush _mappingBrush = new ImmutableSolidColorBrush(0x800078D7);
    private double _scale;
    private double _xOffset;
    private double _yOffset;
    private bool _captured;
    private Point _hitPoint;

    private IList? _menuItems = new AvaloniaList<object>();

    public static readonly DirectProperty<AreaDisplay, IList?> MenuItemsProperty =
        AvaloniaProperty.RegisterDirect<AreaDisplay, IList?>(
            nameof(MenuItems),
            o => o.MenuItems,
            (o, v) => o.MenuItems = v);

    public IList? MenuItems
    {
        get => _menuItems;
        set => SetAndRaise(MenuItemsProperty, ref _menuItems, value);
    }

    public AreaDisplay()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is AreaDisplayViewModel vm)
        {
            _xOffset = Math.Abs(Math.Min(vm.MaximumBounds.X, 0));
            _yOffset = Math.Abs(Math.Min(vm.MaximumBounds.Y, 0));

            CreateContextMenu(vm);

            VIEW_AreaCanvas.Children.Clear();
            foreach (var bounds in vm.Bounds)
            {
                var border = CreateFromBounds(bounds);
                VIEW_AreaCanvas.Children.Add(border);
            }

            var mappingBorder = CreateFromMap(vm.Mapping);
            VIEW_AreaCanvas.Children.Add(mappingBorder);
        }

        base.OnDataContextChanged(e);
    }

    private void CreateContextMenu(AreaDisplayViewModel vm)
    {
        var menuItems = new List<object>()
        {
            new MenuItem()
            {
                Header = "Map to...",
                ItemsSource = CreateQuickMapMenuItems(vm)
            },
            new MenuItem()
            {
                Header = "Restrict to mappable bounds",
                Icon = new CheckBox()
                {
                    BorderThickness = new Thickness(0),
                    IsHitTestVisible = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding()
                    {
                        Source = vm,
                        Path = nameof(vm.RestrictToMaximumBounds),
                        Mode = BindingMode.OneWay
                    },
                },
                Command = vm.ToggleRestrictToMaximumBoundsCommand
            }
        };

        if (MenuItems is not null)
        {
            menuItems.AddRange(MenuItems.Cast<object>());
        }

        ContextMenu = new ContextMenu
        {
            ItemsSource = menuItems
        };
    }

    private static MenuItem[] CreateQuickMapMenuItems(AreaDisplayViewModel vm)
    {
        static MenuItem createQuickMapMenuItem(AreaDisplayViewModel vm, Bounds bounds)
        {
            return new MenuItem
            {
                Header = bounds.Name,
                Command = new RelayCommand(() =>
                {
                    vm.SetProcessRestrictions(false);
                    vm.Mapping.UntranslatedX = bounds.X;
                    vm.Mapping.UntranslatedY = bounds.Y;
                    vm.Mapping.Width = bounds.Width;
                    vm.Mapping.Height = bounds.Height;
                    vm.Mapping.Rotation = bounds.Rotation;
                    vm.SetProcessRestrictions(true);
                })
            };
        }

        var fullVirtualDesktop = createQuickMapMenuItem(vm, vm.MaximumBounds);

        return vm.Bounds
            .Where(b => b.Name is not null)
            .Select(b => createQuickMapMenuItem(vm, b))
            .Prepend(fullVirtualDesktop)
            .ToArray();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (DataContext is AreaDisplayViewModel vm)
        {
            var padding = VIEW_AreaBorder.Padding;
            var maxCanvasSize = finalSize.Deflate(padding);

            var scaledWidth = maxCanvasSize.Height / vm.MaximumBounds.Height * vm.MaximumBounds.Width;
            var scaledHeight = maxCanvasSize.Width / vm.MaximumBounds.Width * vm.MaximumBounds.Height;

            if (scaledWidth > maxCanvasSize.Width)
            {
                VIEW_AreaCanvas.Width = maxCanvasSize.Width;
                VIEW_AreaCanvas.Height = scaledHeight;
            }
            else
            {
                VIEW_AreaCanvas.Width = scaledWidth;
                VIEW_AreaCanvas.Height = maxCanvasSize.Height;
            }

            _scale = VIEW_AreaCanvas.Bounds.Width / vm.MaximumBounds.Width;

            foreach (var border in VIEW_AreaCanvas.Children.OfType<Border>())
            {
                var mapping = (Mapping)border.Tag!;
                SetSize(border, mapping, _scale);
                SetPosition(border, mapping, _scale);
            }
        }

        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // layout-wise idk how this fixes bounds overflow but it does
        return base.MeasureOverride(availableSize.Deflate(VIEW_AreaBorder.Padding));
    }

    private static Border CreateFromBounds(Bounds bounds)
    {
        var border = new Border
        {
            Background = _boundsBrush,
            CornerRadius = new CornerRadius(6),
            Tag = bounds
        };

        return border;
    }

    private Border CreateFromMap(Mapping mapping)
    {
        var mapVisual = new Border
        {
            Background = _mappingBrush,
            CornerRadius = new CornerRadius(6),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            Tag = mapping
        };

        mapping.PropertyChanged += (s, e) =>
        {
            InvalidateArrange();
        };

        mapVisual.PointerPressed += (object? s, PointerPressedEventArgs e) =>
        {
            var point = e.GetCurrentPoint(mapVisual);
            if (point.Properties.IsLeftButtonPressed)
            {
                ToolTip.SetIsOpen(this, false); // hide any active tooltip
                _captured = true;
                _hitPoint = point.Position;
                e.Pointer.Capture(mapVisual);
            }
        };

        mapVisual.PointerReleased += (object? s, PointerReleasedEventArgs e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                _captured = false;
                e.Pointer.Capture(null);
            }
        };

        mapVisual.PointerMoved += (object? s, PointerEventArgs e) =>
        {
            if (_captured)
            {
                var point = e.GetPosition(VIEW_AreaCanvas) - _hitPoint;
                var mapping = (Mapping)mapVisual.Tag!;
                mapping.UntranslatedX = (point.X / _scale) - _xOffset;
                mapping.UntranslatedY = (point.Y / _scale) - _yOffset;
            }
        };

        return mapVisual;
    }

    private static void SetSize(Border border, Mapping mapping, double scale)
    {
        border.Width = (mapping.Width * scale) - 1;
        border.Height = (mapping.Height * scale) - 1;
    }

    private void SetPosition(Border border, Mapping mapping, double scale)
    {
        Canvas.SetLeft(border, (mapping.UntranslatedX + _xOffset) * scale);
        Canvas.SetTop(border, (mapping.UntranslatedY + _yOffset) * scale);
        border.RenderTransform = new RotateTransform(mapping.Rotation);
    }
}
