using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Area;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Utilities;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class OutputModeEditor : Panel
    {
        public OutputModeEditor()
        {
            this.Content = outputPanel = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(absoluteModeEditor, true),
                    new StackLayoutItem(relativeModeEditor, true),
                    new StackLayoutItem(noModeEditor, true),
                    new StackLayout(outputModeSelector)
                    {
                        Size = new Size(300, -1)
                    }
                }
            };

            UpdateOutputMode(App.Settings?.OutputMode);
            App.SettingsChanged += (settings) => UpdateOutputMode(settings?.OutputMode);

            outputModeSelector.SelectedValueChanged += (sender, args) =>
            {
                App.Settings.OutputMode = new PluginSettingStore(outputModeSelector.SelectedType);
                UpdateOutputMode(App.Settings.OutputMode);
            };
        }

        public void Refresh()
        {
            outputModeSelector.Refresh();
        }

        private StackLayout outputPanel;

        private Control noModeEditor = new Panel();
        private AbsoluteModeEditor absoluteModeEditor = new AbsoluteModeEditor();
        private RelativeModeEditor relativeModeEditor = new RelativeModeEditor();
        private OutputModeSelector outputModeSelector = new OutputModeSelector { Width = 300 };

        public void SetTabletSize(TabletState tablet)
        {
            var tabletAreaEditor = absoluteModeEditor.tabletAreaEditor;
            if (tablet != null && tablet.Digitizer != null)
            {
                tabletAreaEditor.ViewModel.Background = new RectangleF[]
                {
                    new RectangleF(0, 0, tablet.Digitizer.Width, tablet.Digitizer.Height)
                };

                var settings = App.Settings;
                if (settings != null && settings.TabletWidth == 0 && settings.TabletHeight == 0)
                {
                    settings.TabletWidth = tablet.Digitizer.Width;
                    settings.TabletHeight = tablet.Digitizer.Height;
                    settings.TabletX = tablet.Digitizer.Width / 2;
                    settings.TabletY = tablet.Digitizer.Height / 2;
                }
            }
            else
            {
                tabletAreaEditor.ViewModel.Background = null;
            }
        }

        public void SetDisplaySize(IEnumerable<IDisplay> displays)
        {
            var bgs = from disp in displays
                where !(disp is IVirtualScreen)
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            absoluteModeEditor.displayAreaEditor.ViewModel.Background = bgs;
        }

        private void UpdateOutputMode(PluginSettingStore store)
        {
            bool showNull = true;
            bool showAbsolute = false;
            bool showRelative = false;
            if (store != null)
            {
                App.Settings.OutputMode = store;
                var outputMode = store.GetPluginReference().GetTypeReference<IOutputMode>();
                showAbsolute = outputMode.IsSubclassOf(typeof(AbsoluteOutputMode));
                showRelative = outputMode.IsSubclassOf(typeof(RelativeOutputMode));
                showNull = !(showAbsolute | showRelative);
            }
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Linux:
                    noModeEditor.Visible = showNull;
                    absoluteModeEditor.Visible = showAbsolute;
                    relativeModeEditor.Visible = showRelative;
                    break;
                default:
                    SetVisibilityWorkaround(absoluteModeEditor, showAbsolute, 0);
                    SetVisibilityWorkaround(relativeModeEditor, showRelative, 1);
                    SetVisibilityWorkaround(noModeEditor, showNull, 2);
                    break;
            }
        }

        private void SetVisibilityWorkaround(
            Control control,
            bool visibility,
            int index
        )
        {
            if (control == null || outputPanel == null)
                return;
            var isContained = outputPanel.Items.Any(d => d.Control == control);
            if (!isContained & visibility)
            {
                if (outputPanel.Items.Count - index - 1 < 0)
                    index = 0;
                outputPanel.Items.Insert(index, new StackLayoutItem(control, HorizontalAlignment.Stretch, true));
            }
            else if (isContained & !visibility)
            {
                var item = outputPanel.Items.FirstOrDefault(d => d.Control == control);
                outputPanel.Items.Remove(item);
            }
        }

        private class OutputModeSelector : TypeDropDown<IOutputMode>
        {
            public OutputModeSelector()
            {
                UpdateSelectedMode(App.Settings?.OutputMode);
                App.SettingsChanged += (settings) => UpdateSelectedMode(settings?.OutputMode);
            }

            public void UpdateSelectedMode(PluginSettingStore store)
            {
                var typeReference = store.GetPluginReference().GetTypeReference();
                this.SelectedValue = typeReference;
            }
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
                        new StackLayoutItem(new Group("Display", displayAreaEditor), true),
                        new StackLayoutItem(new Group("Tablet", tabletAreaEditor), true)
                    }
                };

                var settings = App.Settings;
                displayAreaEditor.Rebind(settings);
                tabletAreaEditor.Rebind(settings);
            }

            public DisplayAreaEditor displayAreaEditor = new DisplayAreaEditor
            {
                ViewModel = new AreaViewModel
                {
                    Unit = "px",
                    EnableRotation = false
                }
            };

            public TabletAreaEditor tabletAreaEditor = new TabletAreaEditor
            {
                ViewModel = new AreaViewModel
                {
                    InvalidSizeError = "No tablet detected.",
                    Unit = "mm",
                    EnableRotation = true
                }
            };

            public class DisplayAreaEditor : AreaEditor
            {
                public DisplayAreaEditor()
                    : base()
                {
                    this.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";

                    Rebind(App.Settings);
                    App.SettingsChanged += Rebind;
                }

                public void Rebind(Settings settings)
                {
                    this.Bind(c => c.ViewModel.Width, settings, m => m.DisplayWidth);
                    this.Bind(c => c.ViewModel.Height, settings, m => m.DisplayHeight);
                    this.Bind(c => c.ViewModel.X, settings, m => m.DisplayX);
                    this.Bind(c => c.ViewModel.Y, settings, m => m.DisplayY);
                    this.Bind(c => c.ViewModel.LockToUsableArea, settings, m => m.LockUsableAreaDisplay);
                }

                protected override void OnLoadComplete(EventArgs e)
                {
                    base.OnLoadComplete(e);

                    var subMenu = base.ContextMenu.Items.GetSubmenu("Set to display");
                    foreach (var display in SystemInterop.VirtualScreen.Displays)
                    {
                        subMenu.Items.Add(
                            new ActionCommand
                            {
                                MenuText = display.ToString(),
                                Action = () =>
                                {
                                    this.ViewModel.Width = display.Width;
                                    this.ViewModel.Height = display.Height;
                                    if (display is IVirtualScreen virtualScreen)
                                    {
                                        this.ViewModel.X = virtualScreen.Width / 2;
                                        this.ViewModel.Y = virtualScreen.Height / 2;
                                    }
                                    else
                                    {
                                        virtualScreen = SystemInterop.VirtualScreen;
                                        this.ViewModel.X = display.Position.X + virtualScreen.Position.X + (display.Width / 2);
                                        this.ViewModel.Y = display.Position.Y + virtualScreen.Position.Y + (display.Height / 2);
                                    }
                                }
                            }
                        );
                    }
                }
            }

            public class TabletAreaEditor : AreaEditor
            {
                public TabletAreaEditor()
                    : base()
                {
                    this.ToolTip = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";
                }

                private BooleanCommand lockAr, areaClipping, ignoreOutsideArea;

                public void Rebind(Settings settings)
                {
                    this.Bind(c => c.ViewModel.Width, settings, m => m.TabletWidth);
                    this.Bind(c => c.ViewModel.Height, settings, m => m.TabletHeight);
                    this.Bind(c => c.ViewModel.X, settings, m => m.TabletX);
                    this.Bind(c => c.ViewModel.Y, settings, m => m.TabletY);
                    this.Bind(c => c.ViewModel.Rotation, settings, m => m.TabletRotation);
                    this.Bind(c => c.ViewModel.LockToUsableArea, settings, m => m.LockUsableAreaTablet);
                    lockAr?.CheckedBinding.BindDataContext<Settings>(m => m.LockAspectRatio);
                    areaClipping?.CheckedBinding.BindDataContext<Settings>(m => m.EnableClipping);
                    ignoreOutsideArea?.CheckedBinding.BindDataContext<Settings>(m => m.EnableAreaLimiting);
                }

                protected override void OnLoadComplete(EventArgs e)
                {
                    base.OnLoadComplete(e);

                    base.ContextMenu.Items.AddSeparator();

                    lockAr = new BooleanCommand
                    {
                        MenuText = "Lock aspect ratio",
                        DataContext = App.Settings
                    };

                    areaClipping = new BooleanCommand
                    {
                        MenuText = "Area clipping",
                        DataContext = App.Settings
                    };

                    ignoreOutsideArea = new BooleanCommand
                    {
                        MenuText = "Ignore input outside area",
                        DataContext = App.Settings
                    };

                    base.ContextMenu.Items.AddRange(
                        new Command[]
                        {
                            lockAr,
                            areaClipping,
                            ignoreOutsideArea
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

                    Rebind(App.Settings);
                    App.SettingsChanged += Rebind;
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
                    App.SettingsChanged += (s) => UpdateBindings();
                }

                public void UpdateBindings()
                {
                    this.Items.Clear();

                    var xSensBox = new SensitivityEditorBox(
                        "X Sensitivity",
                        (s) => App.Settings.XSensitivity = float.TryParse(s, out var val) ? val : 0f,
                        () => App.Settings?.XSensitivity.ToString(),
                        "px/mm"
                    );
                    AddControl(xSensBox, true);

                    var ySensBox = new SensitivityEditorBox(
                        "Y Sensitivity",
                        (s) => App.Settings.YSensitivity = float.TryParse(s, out var val) ? val : 0f,
                        () => App.Settings?.YSensitivity.ToString(),
                        "px/mm"
                    );
                    AddControl(ySensBox, true);

                    var rotationBox = new SensitivityEditorBox(
                        "Rotation",
                        (s) => App.Settings.RelativeRotation = float.TryParse(s, out var val) ? val : 0f,
                        () => App.Settings?.RelativeRotation.ToString(),
                        "°"
                    );
                    AddControl(rotationBox, true);

                    var resetTimeBox = new SensitivityEditorBox(
                        "Reset Time",
                        (s) => App.Settings.ResetTime = TimeSpan.TryParse(s, out var val) ? val : TimeSpan.FromMilliseconds(100),
                        () => App.Settings?.ResetTime.ToString()
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
                        App.SettingsChanged += (Settings) => UpdateBindings();
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
