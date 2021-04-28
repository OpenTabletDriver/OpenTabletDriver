using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public class Settings : ViewModel
    {
        internal const int PenButtonCount = 2;
        internal const int AuxButtonCount = 8;

        private float _dW, _dH, _dX, _dY, _tW, _tH, _tX, _tY, _r, _xS, _yS, _relRot, _tP, _eP;
        private TimeSpan _rT;
        private bool _lockar, _sizeChanging, _autoHook, _clipping, _areaLimiting, _lockUsableAreaDisplay, _lockUsableAreaTablet;
        private PluginSettingStore _outputMode, _tipButton, _eraserButton;

        private PluginSettingStoreCollection _filters = new PluginSettingStoreCollection(),
            _penButtons = new PluginSettingStoreCollection(),
            _auxButtons = new PluginSettingStoreCollection(),
            _tools = new PluginSettingStoreCollection(),
            _interpolators = new PluginSettingStoreCollection();

        #region General Settings

        [JsonProperty("OutputMode")]
        public PluginSettingStore OutputMode
        {
            set => RaiseAndSetIfChanged(ref _outputMode, value);
            get => _outputMode;
        }

        [JsonProperty("Filters")]
        public PluginSettingStoreCollection Filters
        {
            set => RaiseAndSetIfChanged(ref _filters, value);
            get => _filters;
        }

        [JsonProperty("AutoHook")]
        public bool AutoHook
        {
            set => RaiseAndSetIfChanged(ref _autoHook, value);
            get => _autoHook;
        }

        [JsonProperty("LockUsableAreaDisplay")]
        public bool LockUsableAreaDisplay
        {
            set => this.RaiseAndSetIfChanged(ref _lockUsableAreaDisplay, value);
            get => _lockUsableAreaDisplay;
        }

        [JsonProperty("LockUsableAreaTablet")]
        public bool LockUsableAreaTablet
        {
            set => this.RaiseAndSetIfChanged(ref _lockUsableAreaTablet, value);
            get => _lockUsableAreaTablet;
        }

        #endregion

        #region Absolute Mode Settings

        [JsonProperty("DisplayWidth")]
        public float DisplayWidth
        {
            set
            {
                RaiseAndSetIfChanged(ref _dW, value);
                if (LockAspectRatio)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _dW;
        }

        [JsonProperty("DisplayHeight")]
        public float DisplayHeight
        {
            set
            {
                RaiseAndSetIfChanged(ref _dH, value);
                if (LockAspectRatio)
                    TabletWidth = DisplayWidth / DisplayHeight * TabletHeight;
            }
            get => _dH;
        }

        [JsonProperty("DisplayXOffset")]
        public float DisplayX
        {
            set => RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        [JsonProperty("DisplayYOffset")]
        public float DisplayY
        {
            set => RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        [JsonProperty("TabletWidth")]
        public float TabletWidth
        {
            set
            {
                RaiseAndSetIfChanged(ref _tW, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletHeight = DisplayHeight / DisplayWidth * value;
                    _sizeChanging = false;
                }
            }
            get => _tW;
        }

        [JsonProperty("TabletHeight")]
        public float TabletHeight
        {
            set
            {
                RaiseAndSetIfChanged(ref _tH, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletWidth = DisplayWidth / DisplayHeight * value;
                    _sizeChanging = false;
                }
            }
            get => _tH;
        }

        [JsonProperty("TabletXOffset")]
        public float TabletX
        {
            set => RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        [JsonProperty("TabletYOffset")]
        public float TabletY
        {
            set => RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        [JsonProperty("TabletRotation")]
        public float TabletRotation
        {
            set => RaiseAndSetIfChanged(ref _r, value);
            get => _r;
        }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping
        {
            set => RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [JsonProperty("EnableAreaLimiting")]
        public bool EnableAreaLimiting
        {
            set => RaiseAndSetIfChanged(ref _areaLimiting, value);
            get => _areaLimiting;
        }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio
        {
            set
            {
                RaiseAndSetIfChanged(ref _lockar, value);
                if (value)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _lockar;
        }

        #endregion

        #region Relative Mode Settings

        [JsonProperty("XSensitivity")]
        public float XSensitivity
        {
            set => RaiseAndSetIfChanged(ref _xS, value);
            get => _xS;
        }

        [JsonProperty("YSensitivity")]
        public float YSensitivity
        {
            set => RaiseAndSetIfChanged(ref _yS, value);
            get => _yS;
        }

        [JsonProperty("RelativeRotation")]
        public float RelativeRotation
        {
            set => RaiseAndSetIfChanged(ref _relRot, value);
            get => _relRot;
        }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => RaiseAndSetIfChanged(ref _rT, value);
            get => _rT;
        }

        #endregion

        #region Bindings

        [JsonProperty("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => RaiseAndSetIfChanged(ref _tP, value);
            get => _tP;
        }

        [JsonProperty("TipButton")]
        public PluginSettingStore TipButton
        {
            set => RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        [JsonProperty("EraserActivationPressure")]
        public float EraserActivationPressure
        {
            set => this.RaiseAndSetIfChanged(ref _eP, value);
            get => _eP;
        }

        [JsonProperty("EraserButton")]
        public PluginSettingStore EraserButton
        {
            set => this.RaiseAndSetIfChanged(ref _eraserButton, value);
            get => _eraserButton;
        }

        [JsonProperty("PenButtons")]
        public PluginSettingStoreCollection PenButtons
        {
            set => RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        [JsonProperty("AuxButtons")]
        public PluginSettingStoreCollection AuxButtons
        {
            set => RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
        }

        [JsonProperty("Tools")]
        public PluginSettingStoreCollection Tools
        {
            set => RaiseAndSetIfChanged(ref _tools, value);
            get => _tools;
        }

        [JsonProperty("Interpolators")]
        public PluginSettingStoreCollection Interpolators
        {
            set => RaiseAndSetIfChanged(ref _interpolators, value);
            get => _interpolators;
        }

        #endregion

        #region Tools

        public void SetDisplayArea(Area area)
        {
            _sizeChanging = true;
            DisplayWidth = area.Width;

            _sizeChanging = true;
            DisplayHeight = area.Height;

            DisplayX = area.Position.X;
            DisplayY = area.Position.Y;

            _sizeChanging = false;

            // Refresh aspect ratio lock
            if (LockAspectRatio)
            {
                LockAspectRatio = false;
                LockAspectRatio = true;
            }
        }

        public Area GetDisplayArea()
        {
            return new Area(DisplayWidth, DisplayHeight, new Vector2(DisplayX, DisplayY), 0);
        }

        public void SetTabletArea(Area area)
        {
            _sizeChanging = true;
            TabletWidth = area.Width;

            _sizeChanging = true;
            TabletHeight = area.Height;

            TabletX = area.Position.X;
            TabletY = area.Position.Y;
            TabletRotation = area.Rotation;

            _sizeChanging = false;

            // Refresh aspect ratio lock
            if (LockAspectRatio)
            {
                LockAspectRatio = false;
                LockAspectRatio = true;
            }
        }

        public Area GetTabletArea()
        {
            return new Area(TabletWidth, TabletHeight, new Vector2(TabletX, TabletY), TabletRotation);
        }

        #endregion

        public static Settings Default
        {
            get
            {
                var virtualScreen = DesktopInterop.VirtualScreen;
                var tablet = Info.Driver.Tablet?.Properties?.Specifications?.Digitizer;

                return new Settings
                {
                    OutputMode = new PluginSettingStore(typeof(AbsoluteMode)),
                    DisplayWidth = virtualScreen.Width,
                    DisplayHeight = virtualScreen.Height,
                    DisplayX = virtualScreen.Width / 2,
                    DisplayY = virtualScreen.Height / 2,
                    TabletWidth = tablet?.Width ?? 0,
                    TabletHeight = tablet?.Height ?? 0,
                    TabletX = tablet?.Width / 2 ?? 0,
                    TabletY = tablet?.Height / 2 ?? 0,
                    AutoHook = true,
                    EnableClipping = true,
                    LockUsableAreaDisplay = true,
                    LockUsableAreaTablet = true,
                    TipButton = new PluginSettingStore(
                        new MouseBinding
                        {
                            Button = nameof(Plugin.Platform.Pointer.MouseButton.Left)
                        }
                    ),
                    TipActivationPressure = 1,
                    PenButtons = new PluginSettingStoreCollection(),
                    AuxButtons = new PluginSettingStoreCollection(),
                    XSensitivity = 10,
                    YSensitivity = 10,
                    RelativeRotation = 0,
                    ResetTime = TimeSpan.FromMilliseconds(100)
                };
            }
        }

        #region Custom Serialization

        static Settings()
        {
            serializer.Error += SerializationErrorHandler;
        }

        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        private static void SerializationErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            if (args.ErrorContext.Path is string path)
            {
                if (args.CurrentObject == null)
                    return;

                var property = args.CurrentObject.GetType().GetProperty(path);
                if (property != null && property.PropertyType == typeof(PluginSettingStore))
                {
                    var match = propertyValueRegex.Match(args.ErrorContext.Error.Message);
                    if (match.Success)
                    {
                        var objPath = SettingsMigrator.MigrateNamespace(match.Groups[1].Value);
                        var newValue = PluginSettingStore.FromPath(objPath);
                        if (newValue != null)
                        {
                            property.SetValue(args.CurrentObject, newValue);
                            Log.Write("Settings", $"Migrated {path} to {nameof(PluginSettingStore)}");
                            return;
                        }
                    }
                }
                Log.Write("Settings", $"Unable to migrate {path}", LogLevel.Error);
                return;
            }
            Log.Exception(args.ErrorContext.Error);
        }

        private static Regex propertyValueRegex = new Regex(PROPERTY_VALUE_REGEX, RegexOptions.Compiled);
        private const string PROPERTY_VALUE_REGEX = "\\\"(.+?)\\\"";

        public static Settings Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return serializer.Deserialize<Settings>(jr);
        }

        public static void Recover(FileInfo file, Settings settings)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                void propertyWatch(object _, PropertyChangedEventArgs p)
                {
                    var prop = settings.GetType().GetProperty(p.PropertyName).GetValue(settings);
                    Log.Write("Settings", $"Recovered '{p.PropertyName}'", LogLevel.Debug);
                }

                settings.PropertyChanged += propertyWatch;

                try
                {
                    serializer.Populate(jr, settings);
                }
                catch (JsonReaderException e)
                {
                    Log.Write("Settings", $"Recovery ended. Reason: {e.Message}", LogLevel.Debug);
                }

                settings.PropertyChanged -= propertyWatch;
            }
        }

        public void Serialize(FileInfo file)
        {
            if (file.Exists)
                file.Delete();

            using (var sw = file.CreateText())
            using (var jw = new JsonTextWriter(sw))
                serializer.Serialize(jw, this);
        }

        #endregion
    }
}
