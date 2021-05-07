using System;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Output.Area;
using OpenTabletDriver.UX.Controls.Utilities;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class AbsoluteModeEditor : Panel
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
        }

        internal DisplayAreaEditor displayAreaEditor;
        internal TabletAreaEditor tabletAreaEditor;

        public class DisplayAreaEditor : AreaEditor
        {
            public DisplayAreaEditor()
                : base()
            {
                this.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";

                AreaWidthBinding.Bind(App.Current.ProfileBinding.Child(p => p.DisplayWidth));
                AreaHeightBinding.Bind(App.Current.ProfileBinding.Child(p => p.DisplayHeight));
                AreaXOffsetBinding.Bind(App.Current.ProfileBinding.Child(p => p.DisplayX));
                AreaYOffsetBinding.Bind(App.Current.ProfileBinding.Child(p => p.DisplayY));
                LockToUsableAreaBinding.Bind(App.Current.ProfileBinding.Child(p => p.LockUsableAreaDisplay));
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

                AreaWidthBinding.Bind(App.Current.ProfileBinding.Child(p => p.TabletWidth));
                AreaHeightBinding.Bind(App.Current.ProfileBinding.Child(p => p.TabletHeight));
                AreaXOffsetBinding.Bind(App.Current.ProfileBinding.Child(p => p.TabletX));
                AreaYOffsetBinding.Bind(App.Current.ProfileBinding.Child(p => p.TabletY));
                AreaRotationBinding.Bind(App.Current.ProfileBinding.Child(p => p.TabletRotation));
                LockToUsableAreaBinding.Bind(App.Current.ProfileBinding.Child(p => p.LockUsableAreaTablet));
                LockAspectRatioBinding.Bind(App.Current.ProfileBinding.Child(p => p.LockAspectRatio));
                AreaClippingBinding.Bind(App.Current.ProfileBinding.Child(p => p.EnableClipping));
                IgnoreOutsideAreaBinding.Bind(App.Current.ProfileBinding.Child(p => p.EnableAreaLimiting));
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
}
