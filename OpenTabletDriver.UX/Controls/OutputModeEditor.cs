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
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class OutputModeEditor : Panel
    {
        public OutputModeEditor()
        {
            UpdateOutputMode(App.Settings?.OutputMode);
            App.SettingsChanged += (settings) => UpdateOutputMode(settings?.OutputMode);

            outputModeSelector.SelectedValueChanged += (sender, args) =>
            {
                App.Settings.OutputMode = new PluginSettingStore(outputModeSelector.SelectedType);
                UpdateOutputMode(App.Settings.OutputMode);
            };

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
                    new StackLayoutItem(outputModeSelector, HorizontalAlignment.Left, false)
                }
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
                tabletAreaEditor.SetBackground(new RectangleF(0, 0, tablet.Digitizer.Width, tablet.Digitizer.Height));

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
                tabletAreaEditor.SetBackground(null);
            }
        }

        public void SetDisplaySize(IEnumerable<IDisplay> displays)
        {
            var bgs = from disp in displays
                where !(disp is IVirtualScreen)
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            absoluteModeEditor.displayAreaEditor.SetBackground(bgs);
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
            }

            public DisplayAreaEditor displayAreaEditor = new DisplayAreaEditor();
            public TabletAreaEditor tabletAreaEditor = new TabletAreaEditor();

            public class DisplayAreaEditor : AreaEditor
            {
                public DisplayAreaEditor()
                    : base("px", false)
                {
                    base.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";

                    lockToUsableArea = AppendCheckBoxMenuItem(
                        "Lock to usable area",
                        value =>
                        {
                            base.ChangeLockingState(value);
                            App.Settings.LockUsableAreaDisplay = value;
                        }
                    );

                    AppendMenuItemSeparator();

                    foreach (var display in SystemInterop.VirtualScreen.Displays)
                    {
                        AppendMenuItem(
                            $"Set to {display}",
                            () =>
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
                        );
                    }

                    App.SettingsChanged += Rebind;
                }

                private CheckCommand lockToUsableArea;

                public void Rebind(Settings settings)
                {
                    this.Bind(c => c.ViewModel.Width, settings, m => m.DisplayWidth);
                    this.Bind(c => c.ViewModel.Height, settings, m => m.DisplayHeight);
                    this.Bind(c => c.ViewModel.X, settings, m => m.DisplayX);
                    this.Bind(c => c.ViewModel.Y, settings, m => m.DisplayY);
                    lockToUsableArea.Checked = settings.LockUsableAreaDisplay;
                    base.ChangeLockingState(settings.LockUsableAreaDisplay);
                }
            }

            public class TabletAreaEditor : AreaEditor
            {
                public TabletAreaEditor()
                    : base("mm", true)
                {
                    this.AreaDisplay.InvalidSizeError = "No tablet detected.";
                    this.ToolTip = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";

                    lockToUsableArea = AppendCheckBoxMenuItem(
                        "Lock to usable area",
                        value =>
                        {
                            base.ChangeLockingState(value);
                            App.Settings.LockUsableAreaTablet = value;
                        }
                    );

                    AppendMenuItemSeparator();

                    lockAr = AppendCheckBoxMenuItem("Lock aspect ratio", (value) => App.Settings.LockAspectRatio = value);
                    areaClipping = AppendCheckBoxMenuItem("Area clipping", (value) => App.Settings.EnableClipping = value);
                    ignoreOutsideArea = AppendCheckBoxMenuItem("Ignore reports outside area", (value) => App.Settings.EnableAreaLimiting = value);

                    AppendMenuItemSeparator();

                    AppendMenuItem("Convert area...", async () => await ConvertAreaDialog());

                    App.SettingsChanged += Rebind;
                }

                private CheckCommand lockToUsableArea, lockAr, areaClipping, ignoreOutsideArea;

                public void Rebind(Settings settings)
                {
                    this.Bind(c => c.ViewModel.Width, settings, m => m.TabletWidth);
                    this.Bind(c => c.ViewModel.Height, settings, m => m.TabletHeight);
                    this.Bind(c => c.ViewModel.X, settings, m => m.TabletX);
                    this.Bind(c => c.ViewModel.Y, settings, m => m.TabletY);
                    this.Bind(c => c.ViewModel.Rotation, settings, m => m.TabletRotation);

                    lockToUsableArea.Checked = settings.LockUsableAreaTablet;
                    base.ChangeLockingState(settings.LockUsableAreaTablet);

                    lockAr.Checked = settings.LockAspectRatio;
                    areaClipping.Checked = settings.EnableClipping;
                    ignoreOutsideArea.Checked = settings.EnableAreaLimiting;
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