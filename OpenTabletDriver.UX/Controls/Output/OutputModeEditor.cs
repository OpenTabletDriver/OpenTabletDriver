using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Output.Area;
using OpenTabletDriver.UX.Controls.Utilities;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class OutputModeEditor : Panel
    {
        public OutputModeEditor()
        {
            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(outputModeEditor, true),
                    outputModeSelector
                }
            };

            outputModeSelector.SelectedValueChanged += (sender, args) =>
            {
                if (outputModeSelector.SelectedType is TypeInfo type)
                    this.Store = new PluginSettingStore(type);
            };

            outputModeSelector.SelectedTypeBinding.Bind(
                StoreBinding.Convert<TypeInfo>(
                    c => c?.GetPluginReference().GetTypeReference(),
                    t => PluginSettingStore.FromPath(t.FullName)
                )
            );
        }

        private PluginSettingStore store;
        public PluginSettingStore Store
        {
            set
            {
                this.store = value;
                this.OnStoreChanged();
            }
            get => this.store;
        }

        public event EventHandler<EventArgs> StoreChanged;

        protected virtual void OnStoreChanged()
        {
            StoreChanged?.Invoke(this, new EventArgs());
            UpdateOutputMode(this.Store);
        }

        public BindableBinding<OutputModeEditor, PluginSettingStore> StoreBinding
        {
            get
            {
                return new BindableBinding<OutputModeEditor, PluginSettingStore>(
                    this,
                    c => c.Store,
                    (c, v) => c.Store = v,
                    (c, h) => c.StoreChanged += h,
                    (c, h) => c.StoreChanged -= h
                );
            }
        }

        private Settings settings;
        public Settings Settings
        {
            set
            {
                this.settings = value;
                this.OnSettingsChanged();
            }
            get => this.settings;
        }

        public event EventHandler<EventArgs> SettingsChanged;

        protected virtual void OnSettingsChanged() => SettingsChanged?.Invoke(this, new EventArgs());

        public BindableBinding<OutputModeEditor, Settings> SettingsBinding
        {
            get
            {
                return new BindableBinding<OutputModeEditor, Settings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        private Panel outputModeEditor = new Panel();
        private AbsoluteModeEditor absoluteModeEditor = new AbsoluteModeEditor();
        private RelativeModeEditor relativeModeEditor = new RelativeModeEditor();
        private TypeDropDown<IOutputMode> outputModeSelector = new TypeDropDown<IOutputMode> { Width = 300 };

        public void SetTabletSize(TabletState tablet)
        {
            var tabletAreaEditor = absoluteModeEditor.tabletAreaEditor;
            if (tablet.Properties?.Specifications?.Digitizer is DigitizerSpecifications digitizer)
            {
                tabletAreaEditor.AreaBounds = new RectangleF[]
                {
                    new RectangleF(0, 0, digitizer.Width, digitizer.Height)
                };

                var settings = App.Current.Settings;
                if (settings != null && settings.TabletWidth == 0 && settings.TabletHeight == 0)
                {
                    settings.TabletWidth = digitizer.Width;
                    settings.TabletHeight = digitizer.Height;
                    settings.TabletX = digitizer.Width / 2;
                    settings.TabletY = digitizer.Height / 2;
                }
            }
            else
            {
                tabletAreaEditor.AreaBounds = null;
            }
        }

        public void SetDisplaySize(IEnumerable<IDisplay> displays)
        {
            var bgs = from disp in displays
                where !(disp is IVirtualScreen)
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            absoluteModeEditor.displayAreaEditor.AreaBounds = bgs;
        }

        private void UpdateOutputMode(PluginSettingStore store)
        {
            bool showAbsolute = false;
            bool showRelative = false;
            if (store != null)
            {
                var outputMode = store.GetPluginReference().GetTypeReference<IOutputMode>();
                showAbsolute = outputMode.IsSubclassOf(typeof(AbsoluteOutputMode));
                showRelative = outputMode.IsSubclassOf(typeof(RelativeOutputMode));
            }

            if (showAbsolute)
                outputModeEditor.Content = absoluteModeEditor;
            else if (showRelative)
                outputModeEditor.Content = relativeModeEditor;
        }

        private class AbsoluteModeEditor : Panel
        {
            public AbsoluteModeEditor()
            {
                this.Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new StackLayoutItem
                        {
                            Expand = true,
                            Control = new Group
                            {
                                Text = "Display",
                                Content = displayAreaEditor = new DisplayAreaEditor
                                {
                                    Unit = "px"
                                }
                            }
                        },
                        new StackLayoutItem
                        {
                            Expand = true,
                            Control = new Group
                            {
                                Text = "Tablet",
                                Content = tabletAreaEditor = new TabletAreaEditor
                                {
                                    InvalidBackgroundError = "No tablet detected.",
                                    Unit = "mm"
                                }
                            }
                        }
                    }
                };

                displayAreaEditor.AreaWidthBinding.BindDataContext<App>(c => c.Settings.DisplayWidth);
                displayAreaEditor.AreaHeightBinding.BindDataContext<App>(c => c.Settings.DisplayHeight);
                displayAreaEditor.AreaXOffsetBinding.BindDataContext<App>(c => c.Settings.DisplayX);
                displayAreaEditor.AreaYOffsetBinding.BindDataContext<App>(c => c.Settings.DisplayY);
                displayAreaEditor.LockToUsableAreaBinding.BindDataContext<App>(c => c.Settings.LockUsableAreaDisplay);

                tabletAreaEditor.AreaWidthBinding.BindDataContext<App>(c => c.Settings.TabletWidth);
                tabletAreaEditor.AreaHeightBinding.BindDataContext<App>(c => c.Settings.TabletHeight);
                tabletAreaEditor.AreaXOffsetBinding.BindDataContext<App>(c => c.Settings.TabletX);
                tabletAreaEditor.AreaYOffsetBinding.BindDataContext<App>(c => c.Settings.TabletY);
                tabletAreaEditor.AreaRotationBinding.BindDataContext<App>(c => c.Settings.TabletRotation);
                tabletAreaEditor.LockToUsableAreaBinding.BindDataContext<App>(c => c.Settings.LockUsableAreaTablet);
                tabletAreaEditor.LockAspectRatioBinding.BindDataContext<App>(c => c.Settings.LockAspectRatio);
                tabletAreaEditor.AreaClippingBinding.BindDataContext<App>(c => c.Settings.EnableClipping);
                tabletAreaEditor.IgnoreOutsideAreaBinding.BindDataContext<App>(c => c.Settings.EnableAreaLimiting);
            }

            internal DisplayAreaEditor displayAreaEditor;
            internal TabletAreaEditor tabletAreaEditor;

            public class DisplayAreaEditor : AreaEditor
            {
                public DisplayAreaEditor()
                    : base()
                {
                    this.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";
                }

                protected override void CreateMenu()
                {
                    base.CreateMenu();

                    var subMenu = base.ContextMenu.Items.GetSubmenu("Set to display");
                    foreach (var display in DesktopInterop.VirtualScreen.Displays)
                    {
                        subMenu.Items.Add(
                            new ActionCommand
                            {
                                MenuText = display.ToString(),
                                Action = () =>
                                {
                                    this.AreaWidth = display.Width;
                                    this.AreaHeight = display.Height;
                                    if (display is IVirtualScreen virtualScreen)
                                    {
                                        this.AreaXOffset = virtualScreen.Width / 2;
                                        this.AreaYOffset = virtualScreen.Height / 2;
                                    }
                                    else
                                    {
                                        virtualScreen = DesktopInterop.VirtualScreen;
                                        this.AreaXOffset = display.Position.X + virtualScreen.Position.X + (display.Width / 2);
                                        this.AreaYOffset = display.Position.Y + virtualScreen.Position.Y + (display.Height / 2);
                                    }
                                }
                            }
                        );
                    }
                }
            }

            public class TabletAreaEditor : RotationAreaEditor
            {
                public TabletAreaEditor()
                    : base()
                {
                    this.ToolTip = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";
                }

                private BooleanCommand lockArCmd, areaClippingCmd, ignoreOutsideAreaCmd;
                private bool lockAspectRatio, areaClipping, ignoreOutsideArea;

                public event EventHandler<EventArgs> LockAspectRatioChanged;
                public event EventHandler<EventArgs> AreaClippingChanged;
                public event EventHandler<EventArgs> IgnoreOutsideAreaChanged;

                protected virtual void OnLockAspectRatioChanged() => LockAspectRatioChanged?.Invoke(this, new EventArgs());
                protected virtual void OnAreaClippingChanged() => AreaClippingChanged?.Invoke(this, new EventArgs());
                protected virtual void OnIgnoreOutsideAreaChanged() => IgnoreOutsideAreaChanged?.Invoke(this, new EventArgs());

                public bool LockAspectRatio
                {
                    set
                    {
                        this.lockAspectRatio = value;
                        this.OnLockAspectRatioChanged();
                    }
                    get => this.lockAspectRatio;
                }

                public bool AreaClipping
                {
                    set
                    {
                        this.areaClipping = value;
                        this.OnAreaClippingChanged();
                    }
                    get => this.areaClipping;
                }

                public bool IgnoreOutsideArea
                {
                    set
                    {
                        this.ignoreOutsideArea = value;
                        this.OnIgnoreOutsideAreaChanged();
                    }
                    get => this.ignoreOutsideArea;
                }

                public BindableBinding<TabletAreaEditor, bool> LockAspectRatioBinding
                {
                    get
                    {
                        return new BindableBinding<TabletAreaEditor, bool>(
                            this,
                            c => c.LockAspectRatio,
                            (c, v) => c.LockAspectRatio = v,
                            (c, h) => c.LockAspectRatioChanged += h,
                            (c, h) => c.LockAspectRatioChanged -= h
                        );
                    }
                }

                public BindableBinding<TabletAreaEditor, bool> AreaClippingBinding
                {
                    get
                    {
                        return new BindableBinding<TabletAreaEditor, bool>(
                            this,
                            c => c.AreaClipping,
                            (c, v) => c.AreaClipping = v,
                            (c, h) => c.AreaClippingChanged += h,
                            (c, h) => c.AreaClippingChanged -= h
                        );
                    }
                }

                public BindableBinding<TabletAreaEditor, bool> IgnoreOutsideAreaBinding
                {
                    get
                    {
                        return new BindableBinding<TabletAreaEditor, bool>(
                            this,
                            c => c.IgnoreOutsideArea,
                            (c, v) => c.IgnoreOutsideArea = v,
                            (c, h) => c.IgnoreOutsideAreaChanged += h,
                            (c, h) => c.IgnoreOutsideAreaChanged -= h
                        );
                    }
                }

                protected override void CreateMenu()
                {
                    base.CreateMenu();

                    base.ContextMenu.Items.AddSeparator();

                    lockArCmd = new BooleanCommand
                    {
                        MenuText = "Lock aspect ratio"
                    };

                    areaClippingCmd = new BooleanCommand
                    {
                        MenuText = "Area clipping"
                    };

                    ignoreOutsideAreaCmd = new BooleanCommand
                    {
                        MenuText = "Ignore input outside area"
                    };

                    base.ContextMenu.Items.AddRange(
                        new Command[]
                        {
                            lockArCmd,
                            areaClippingCmd,
                            ignoreOutsideAreaCmd
                        }
                    );

                    base.ContextMenu.Items.AddSeparator();

                    base.ContextMenu.Items.Add(
                        new ActionCommand
                        {
                            MenuText = "Convert area...",
                            Action = async () => await ConvertAreaDialog()
                        }
                    );

                    lockArCmd.CheckedBinding.Cast<bool>().Bind(LockAspectRatioBinding);
                    areaClippingCmd.CheckedBinding.Cast<bool>().Bind(AreaClippingBinding);
                    ignoreOutsideAreaCmd.CheckedBinding.Cast<bool>().Bind(IgnoreOutsideAreaBinding);
                }

                private async Task ConvertAreaDialog()
                {
                    var converter = new AreaConverterDialog();
                    await converter.ShowModalAsync(Application.Instance.MainForm);
                }
            }
        }

        private class RelativeModeEditor : Panel
        {
            public RelativeModeEditor()
            {
                this.Content = new SensitivityEditor();
            }

            public class SensitivityEditor : StackView
            {
                public SensitivityEditor()
                {
                    base.Orientation = Orientation.Horizontal;
                    base.VerticalContentAlignment = VerticalAlignment.Top;

                    UpdateBindings();
                    // App.SettingsChanged += (s) => UpdateBindings();
                }

                public void UpdateBindings()
                {
                    this.Items.Clear();

                    var xSensBox = new SensitivityEditorBox(
                        "X Sensitivity",
                        (s) => App.Current.Settings.XSensitivity = float.TryParse(s, out var val) ? val : 0f,
                        () => App.Current.Settings?.XSensitivity.ToString(),
                        "px/mm"
                    );
                    AddControl(xSensBox, true);

                    var ySensBox = new SensitivityEditorBox(
                        "Y Sensitivity",
                        (s) => App.Current.Settings.YSensitivity = float.TryParse(s, out var val) ? val : 0f,
                        () => App.Current.Settings?.YSensitivity.ToString(),
                        "px/mm"
                    );
                    AddControl(ySensBox, true);

                    var rotationBox = new SensitivityEditorBox(
                        "Rotation",
                        (s) => App.Current.Settings.RelativeRotation = float.TryParse(s, out var val) ? val : 0f,
                        () => App.Current.Settings?.RelativeRotation.ToString(),
                        "°"
                    );
                    AddControl(rotationBox, true);

                    var resetTimeBox = new SensitivityEditorBox(
                        "Reset Time",
                        (s) => App.Current.Settings.ResetTime = TimeSpan.TryParse(s, out var val) ? val : TimeSpan.FromMilliseconds(100),
                        () => App.Current.Settings?.ResetTime.ToString()
                    );
                    AddControl(resetTimeBox, true);
                }

                private class SensitivityEditorBox : Group
                {
                    public SensitivityEditorBox(
                        string header,
                        Action<string> setValue,
                        Func<string> getValue,
                        string unit = null
                    )
                    {
                        this.Text = header;
                        this.setValue = setValue;
                        this.getValue = getValue;

                        var layout = new StackView
                        {
                            Orientation = Orientation.Horizontal,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        layout.AddControl(textBox, true);

                        if (unit != null)
                        {
                            var unitControl = new Label
                            {
                                Text = unit,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            layout.AddControl(unitControl);
                        }

                        UpdateBindings();
                        // App.Current.SettingsChanged += (Settings) => UpdateBindings();
                        this.Content = layout;
                    }

                    private Action<string> setValue;
                    private Func<string> getValue;

                    private TextBox textBox = new TextBox();

                    private void UpdateBindings()
                    {
                        textBox.TextBinding.Bind(getValue, setValue);
                    }
                }
            }
        }
    }
}
