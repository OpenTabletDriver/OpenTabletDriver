using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.UI.ViewModels;

public partial class AreaDisplayViewModel : ObservableObject
{
    private bool _restricting;
    private bool _processRestrictions = true;
    private readonly Rect _maximumMappableBounds;

    [ObservableProperty]
    private bool _restrictToMaximumBounds;

    public ReadOnlyCollection<Bounds> Bounds { get; }
    public Bounds MaximumBounds { get; }
    public Mapping Mapping { get; }

    public AreaDisplayViewModel(IEnumerable<Bounds> bounds, Mapping? mapping, string? maxBoundsName = null)
    {
        var canvas = new Canvas();
        Bounds = new(bounds.ToArray());

        var minX = Bounds.Min(b => b.X);
        var minY = Bounds.Min(b => b.Y);
        var offsetX = Math.Abs(Math.Min(minX, 0));
        var offsetY = Math.Abs(Math.Min(minY, 0));
        var maxWidth = Bounds.Max(b => b.X + b.Width + offsetX);
        var maxHeight = Bounds.Max(b => b.Y + b.Height + offsetY);

        MaximumBounds = new Bounds(
            minX,
            minY,
            maxWidth,
            maxHeight,
            0,
            maxBoundsName ?? "Full Area"
        );
        _maximumMappableBounds = new Rect(
            MaximumBounds.X,
            MaximumBounds.Y,
            MaximumBounds.Width,
            MaximumBounds.Height
        );

        Mapping = mapping ?? new Mapping(
            MaximumBounds.X,
            MaximumBounds.Y,
            MaximumBounds.Width,
            MaximumBounds.Height,
            0
        );

        Mapping.PropertyChanged += ProcessRestrictionsWrapper;
        ProcessRestrictionsWrapper(Mapping, new PropertyChangedEventArgs(null));
    }

    [RelayCommand]
    private void ToggleRestrictToMaximumBounds()
    {
        RestrictToMaximumBounds = !RestrictToMaximumBounds;
    }

    partial void OnRestrictToMaximumBoundsChanged(bool value)
    {
        if (value)
        {
            ProcessRestrictionsWrapper(Mapping, new PropertyChangedEventArgs(null));
        }
    }

    internal void SetProcessRestrictions(bool restrict)
    {
        var oldValue = _processRestrictions;
        _processRestrictions = restrict;
        if (!oldValue && restrict)
            ProcessRestrictionsWrapper(Mapping, new PropertyChangedEventArgs(null));
    }

    protected void ProcessRestrictionsWrapper(object? sender, PropertyChangedEventArgs args)
    {
        if (!_restricting && _processRestrictions)
        {
            _restricting = true;
            ProcessRestrictions(sender, args);
            _restricting = false;
        }
    }

    protected virtual void ProcessRestrictions(object? sender, PropertyChangedEventArgs args)
    {
        OnMappingPropertyChanged(sender, args);
    }

    private void OnMappingPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        var mapping = Mapping;
        var maximumBounds = RestrictToMaximumBounds
            ? _maximumMappableBounds
            : _maximumMappableBounds.Inflate(new Thickness(mapping.Width / 2.0, mapping.Height / 2.0));

        RestrictToBounds(mapping, maximumBounds);
    }

    private void RestrictToBounds(Mapping mapping, Rect bounds)
    {
        var mappingRect = ToRect(mapping);

        if (mapping.Rotation != 0)
        {
            var theta = mapping.Rotation * Math.PI / 180.0;
            var centerPoint = mappingRect.Center;

            var translationMatrix =
                Matrix.CreateTranslation(-centerPoint.X, -centerPoint.Y) *
                Matrix.CreateRotation(theta) *
                Matrix.CreateTranslation(centerPoint.X, centerPoint.Y);

            mappingRect = mappingRect.TransformToAABB(translationMatrix);
        }

        if (mappingRect.Width > bounds.Width || mappingRect.Height > bounds.Height)
        {
            var scaledWidth = bounds.Height / mappingRect.Height * mappingRect.Width;
            var scaledHeight = bounds.Width / mappingRect.Width * mappingRect.Height;

            if (scaledWidth >= mappingRect.Width)
            {
                var reductionScale = bounds.Width / mappingRect.Width;
                var mapRatio = mapping.Width / mapping.Height;
                var xDiff = (mappingRect.Width - bounds.Width) * reductionScale;
                var width = mapping.Width - xDiff;
                mapping.UntranslatedX = Math.Round(mapping.UntranslatedX + xDiff / 2.0, 2);
                mapping.Width = Math.Round(width, 2);
                mapping.Height = Math.Round(width / mapRatio, 2);
            }
            else
            {
                var reductionScale = bounds.Height / mappingRect.Height;
                var mapRatio = mapping.Height / mapping.Width;
                var yDiff = (mappingRect.Height - bounds.Height) * reductionScale;
                var height = mapping.Height - yDiff;
                mapping.UntranslatedY = Math.Round(mapping.UntranslatedY + yDiff / 2.0, 2);
                mapping.Height = Math.Round(height, 2);
                mapping.Width = Math.Round(height / mapRatio, 2);
            }
        }

        if (mappingRect.Left < bounds.Left)
        {
            mapping.UntranslatedX = Math.Round(mapping.UntranslatedX + (bounds.Left - mappingRect.Left), 2);
        }
        else if (mappingRect.Right > bounds.Right)
        {
            mapping.UntranslatedX = Math.Round(mapping.UntranslatedX + (bounds.Right - mappingRect.Right), 2);
        }

        if (mappingRect.Top < bounds.Top)
        {
            mapping.UntranslatedY = Math.Round(mapping.UntranslatedY + (bounds.Top - mappingRect.Top), 2);
        }
        else if (mappingRect.Bottom > bounds.Bottom)
        {
            mapping.UntranslatedY = Math.Round(mapping.UntranslatedY + (bounds.Bottom - mappingRect.Bottom), 2);
        }
    }

    private static Rect ToRect(Mapping mapping)
    {
        return new Rect(mapping.UntranslatedX, mapping.UntranslatedY, mapping.Width, mapping.Height);
    }
}

public sealed partial class TabletAreaDisplayViewModel : AreaDisplayViewModel
{
    private readonly AreaDisplayViewModel _display;
    private double? _prevDisplayWidth;
    private double? _prevDisplayHeight;

    [ObservableProperty]
    private bool _lockAspectRatio;

    [ObservableProperty]
    private bool _clipInput;

    [ObservableProperty]
    private bool _dropInput;

    public TabletAreaDisplayViewModel(AreaDisplayViewModel display, IEnumerable<Bounds> bounds, Mapping? mapping, string? maxBoundsName = null)
        : base(bounds, mapping, maxBoundsName)
    {
        _display = display;
        _display.Mapping.PropertyChanged += ProcessRestrictionsWrapper;
    }

    protected override void ProcessRestrictions(object? sender, PropertyChangedEventArgs args)
    {
        // apply aspect ratio lock before other restrictions, eg. bounds
        ApplyAspectRatioLock(sender, args);
        base.ProcessRestrictions(sender, args);
    }

    partial void OnLockAspectRatioChanged(bool value)
    {
        if (value)
        {
            ProcessRestrictionsWrapper(Mapping, new PropertyChangedEventArgs(null));
        }
    }

    [RelayCommand]
    private void ToggleState(string? parameter)
    {
        switch (parameter)
        {
            case "LockAspectRatio":
                LockAspectRatio = !LockAspectRatio;
                break;
            case "ClipInput":
                ClipInput = !ClipInput;
                break;
            case "DropInput":
                DropInput = !DropInput;
                break;
        }
    }

    private void ApplyAspectRatioLock(object? sender, PropertyChangedEventArgs args)
    {
        if (!LockAspectRatio)
            return;

        if (sender == Mapping && args.PropertyName is nameof(Mapping.Width))
        {
            Mapping.Height = Math.Round(_display.Mapping.Height / _display.Mapping.Width * Mapping.Width, 2);
        }
        else if (sender == Mapping && args.PropertyName is nameof(Mapping.Height))
        {
            Mapping.Width = Math.Round(_display.Mapping.Width / _display.Mapping.Height * Mapping.Height, 2);
        }
        else if (sender == _display.Mapping && args.PropertyName is nameof(Mapping.Width) && _prevDisplayWidth is double prevDisplayWidth)
        {
            Mapping.Width = Math.Round(Mapping.Width * _display.Mapping.Width / prevDisplayWidth, 2);
        }
        else if (sender == _display.Mapping && args.PropertyName is nameof(Mapping.Height) && _prevDisplayHeight is double prevDisplayHeight)
        {
            Mapping.Height = Math.Round(Mapping.Height * _display.Mapping.Height / prevDisplayHeight, 2);
        }
        else
        {
            var scale = _display.Mapping.Width / _display.Mapping.Height;
            var scaledWidth = Mapping.Height * scale;
            var scaledHeight = Mapping.Width / scale;

            if (scaledWidth >= Mapping.Width)
            {
                Mapping.Height = Math.Round(scaledHeight, 2);
                Mapping.Width = Math.Round(scaledHeight * scale, 2);
            }
            else
            {
                Mapping.Width = Math.Round(scaledWidth, 2);
                Mapping.Height = Math.Round(scaledWidth / scale, 2);
            }
        }

        _prevDisplayWidth = _display.Mapping.Width;
        _prevDisplayHeight = _display.Mapping.Height;
    }
}

public partial class Mapping : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _width;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _rotation;

    public bool CenterOrigin { get; }

    public double UntranslatedX
    {
        get => CenterOrigin ? X - Width / 2.0 : X;
        set => X = Math.Round(CenterOrigin ? value + Width / 2.0 : value, 2);
    }
    public double UntranslatedY
    {
        get => CenterOrigin ? Y - Height / 2.0 : Y;
        set => Y = Math.Round(CenterOrigin ? value + Height / 2.0 : value, 2);
    }

    public Mapping(double x, double y, double width, double height, double rotation, bool centerOrigin = false)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _rotation = rotation;
        CenterOrigin = centerOrigin;
    }
}

public partial class Bounds : Mapping
{
    [ObservableProperty]
    private string? _name;

    public Bounds(double x, double y, double width, double height, double rotation, string? name = null, bool centerOrigin = false)
        : base(x, y, width, height, rotation, centerOrigin)
    {
        _name = name;
    }

    public static Bounds FromDto(DisplayDto displayDto)
    {
        return new(
            displayDto.X,
            displayDto.Y,
            displayDto.Width,
            displayDto.Height,
            0,
            $"Display {displayDto.Index}" // OTD does not support display names yet
        );
    }
}
