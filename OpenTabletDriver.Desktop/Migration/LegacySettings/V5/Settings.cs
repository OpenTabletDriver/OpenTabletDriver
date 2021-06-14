using System;
using System.Numerics;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V5
{
    internal class Settings
    {
        public PluginSettingStore OutputMode { get; set; }
        public PluginSettingStoreCollection Filters { get; set; }

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
        public PluginSettingStore TipButton { get; set; }

        public float EraserActivationPressure { get; set; }
        public PluginSettingStore EraserButton { get; set; }

        public PluginSettingStoreCollection PenButtons { get; set; }
        public PluginSettingStoreCollection AuxButtons { get; set; }

        public PluginSettingStoreCollection Tools { get; set; }
        public PluginSettingStoreCollection Interpolators { get; set; }

        public Area GetDisplayArea() => new Area
        {
            Width = DisplayWidth,
            Height = DisplayHeight,
            Position = new Vector2(DisplayXOffset, DisplayYOffset)
        };

        public Area GetTabletArea() => new Area
        {
            Width = TabletWidth,
            Height = TabletHeight,
            Position = new Vector2(TabletXOffset, TabletYOffset),
            Rotation = TabletRotation
        };

        public bool IsValid() => OutputMode != null;
    }
}