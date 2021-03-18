using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
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
            this.Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(outputModeEditor, true),
                    new StackLayout(outputModeSelector)
                    {
                        Size = new Size(300, -1)
                    }
                }
            };

            outputModeSelector.SelectedValueChanged += (sender, args) =>
            {
                App.Settings.OutputMode = new PluginSettingStore(outputModeSelector.SelectedType);
            };
        }

        public void Refresh()
        {
            outputModeSelector.Refresh();
        }

        private UnifiedOutputModeEditor outputModeEditor = new UnifiedOutputModeEditor();
        private OutputModeSelector outputModeSelector = new OutputModeSelector { Width = 300 };

        public void SetTabletSize(TabletState tablet)
        {
            var tabletAreaEditor = outputModeEditor.tabletAreaEditor;
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
            outputModeEditor.displayAreaEditor.ViewModel.Background = bgs;
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
                var typeReference = store?.GetPluginReference().GetTypeReference();
                this.SelectedValue = typeReference;
            }
        }

        private class UnifiedOutputModeEditor : Panel
        {
            public UnifiedOutputModeEditor()
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

            public DisplayAreaEditor displayAreaEditor = new DisplayAreaEditor(new AreaViewModel
            {
                Unit = "px",
                EnableRotation = false
            });

            public TabletAreaEditor tabletAreaEditor = new TabletAreaEditor(new AreaViewModel
            {
                InvalidBackgroundError = "No tablet detected.",
                Unit = "mm",
                EnableRotation = true
            });

            public class DisplayAreaEditor : AreaEditor
            {
                public DisplayAreaEditor(AreaViewModel viewModel)
                    : base(viewModel)
                {
                    this.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";

                    Rebind(App.Settings);
                    App.SettingsChanged += Rebind;

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

                public void Rebind(Settings settings)
                {
                    this.Bind(c => c.ViewModel.Width, settings, m => m.DisplayWidth);
                    this.Bind(c => c.ViewModel.Height, settings, m => m.DisplayHeight);
                    this.Bind(c => c.ViewModel.X, settings, m => m.DisplayX);
                    this.Bind(c => c.ViewModel.Y, settings, m => m.DisplayY);
                    this.Bind(c => c.ViewModel.LockToUsableArea, settings, m => m.LockUsableAreaDisplay);
                }
            }

            public class TabletAreaEditor : AreaEditor
            {
                public TabletAreaEditor(AreaViewModel viewModel)
                    : base(viewModel)
                {
                    this.ToolTip = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";

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

                private async Task ConvertAreaDialog()
                {
                    var converter = new AreaConverterDialog();
                    await converter.ShowModalAsync(Application.Instance.MainForm);
                }
            }
        }
    }
}
