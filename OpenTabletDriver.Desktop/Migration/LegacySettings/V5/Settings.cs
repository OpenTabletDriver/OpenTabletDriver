using System;
using JetBrains.Annotations;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V5
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class Settings
    {
        public PluginSettings? OutputMode { get; set; }
        public PluginSettingsCollection? Filters { get; set; }

        public bool AutoHook { get; set; }
        public bool LockUsableAreaDisplay { get; set; }
        public bool LockUsableAreaTablet { get; set; }

        public float DisplayWidth { get; set; }
        public float DisplayHeight { get; set; }
        public float DisplayXOffset { get; set; }
        public float DisplayYOffset { get; set; }

        public float TabletWidth { get; set; }
        public float TabletHeight { get; set; }
        public float TabletXOffset { get; set; }
        public float TabletYOffset { get; set; }
        public float TabletRotation { get; set; }

        public bool EnableClipping { get; set; }
        public bool EnableAreaLimiting { get; set; }
        public bool LockAspectRatio { get; set; }

        public float XSensitivity { get; set; }
        public float YSensitivity { get; set; }
        public float RelativeRotation { get; set; }
        public TimeSpan ResetTime { get; set; }

        public float TipActivationPressure { get; set; }
        public PluginSettings? TipButton { get; set; }

        public float EraserActivationPressure { get; set; }
        public PluginSettings? EraserButton { get; set; }

        public PluginSettingsCollection? PenButtons { get; set; }
        public PluginSettingsCollection? AuxButtons { get; set; }

        public PluginSettingsCollection? Tools { get; set; }
        public PluginSettingsCollection? Interpolators { get; set; }

        public Area GetDisplayArea() => new Area
        {
            Width = DisplayWidth,
            Height = DisplayHeight,
            XPosition = DisplayXOffset,
            YPosition = DisplayYOffset
        };

        public AngledArea GetTabletArea() => new AngledArea
        {
            Width = TabletWidth,
            Height = TabletHeight,
            XPosition = TabletXOffset,
            YPosition = TabletYOffset,
            Rotation = TabletRotation
        };

        public bool IsValid() => OutputMode != null;
    }
}
