using System;
using System.Numerics;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Interop;
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

            displayAreaEditor.AreaBinding.Bind(SettingsBinding.Child(c => c.Display));
            displayAreaEditor.LockToUsableAreaBinding.Bind(App.Current, c => c.Settings.LockUsableAreaDisplay);

            tabletAreaEditor.AreaBinding.Bind(SettingsBinding.Child(c => c.Tablet));
            tabletAreaEditor.LockToUsableAreaBinding.Bind(App.Current, c => c.Settings.LockUsableAreaTablet);

            tabletAreaEditor.LockAspectRatioBinding.Bind(SettingsBinding.Child(c => c.LockAspectRatio));
            tabletAreaEditor.AreaClippingBinding.Bind(SettingsBinding.Child(c => c.EnableClipping));
            tabletAreaEditor.IgnoreOutsideAreaBinding.Bind(SettingsBinding.Child(c => c.EnableAreaLimiting));

            displayWidth = SettingsBinding.Child(c => c.Display.Width);
            displayHeight = SettingsBinding.Child(c => c.Display.Height);
            tabletWidth = SettingsBinding.Child(c => c.Tablet.Width);
            tabletHeight = SettingsBinding.Child(c => c.Tablet.Height);
            tabletWidth.DataValueChanged += HandleTabletAreaConstraint;
            tabletHeight.DataValueChanged += HandleTabletAreaConstraint;

            tabletAreaEditor.LockAspectRatioChanged += HookAspectRatioLock;
            HookAspectRatioLock(tabletAreaEditor, EventArgs.Empty);
        }

        internal DisplayAreaEditor displayAreaEditor;
        internal TabletAreaEditor tabletAreaEditor;

        private bool handlingArLock;
        private bool handlingForcedArConstraint;
        private float? prevDisplayWidth;
        private float? prevDisplayHeight;
        private DirectBinding<float> displayWidth;
        private DirectBinding<float> displayHeight;
        private DirectBinding<float> tabletWidth;
        private DirectBinding<float> tabletHeight;

        private AbsoluteModeSettings settings;
        public AbsoluteModeSettings Settings
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

        public BindableBinding<AbsoluteModeEditor, AbsoluteModeSettings> SettingsBinding
        {
            get
            {
                return new BindableBinding<AbsoluteModeEditor, AbsoluteModeSettings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        private void HookAspectRatioLock(object sender, EventArgs args)
        {
            if (Settings?.LockAspectRatio ?? false)
            {
                lock (this)
                {
                    HandleAspectRatioLock(tabletAreaEditor, EventArgs.Empty);

                    displayWidth.DataValueChanged += HandleAspectRatioLock;
                    displayHeight.DataValueChanged += HandleAspectRatioLock;
                    tabletWidth.DataValueChanged += HandleAspectRatioLock;
                    tabletHeight.DataValueChanged += HandleAspectRatioLock;
                }
            }
            else
            {
                lock (this)
                {
                    displayWidth.DataValueChanged -= HandleAspectRatioLock;
                    displayHeight.DataValueChanged -= HandleAspectRatioLock;
                    tabletWidth.DataValueChanged -= HandleAspectRatioLock;
                    tabletHeight.DataValueChanged -= HandleAspectRatioLock;
                }
            }
        }

        private void HookAreaConstraint(object sender, EventArgs args)
        {
            var areaEditor = (AreaEditor)sender;
            if (areaEditor.LockToUsableArea)
            {
                lock (this)
                {
                    if (sender == tabletAreaEditor)
                    {
                        tabletWidth.DataValueChanged += HandleTabletAreaConstraint;
                        tabletHeight.DataValueChanged += HandleTabletAreaConstraint;
                    }
                    else if (sender == displayAreaEditor)
                    {
                        displayWidth.DataValueChanged += HandleDisplayAreaConstraint;
                        displayHeight.DataValueChanged += HandleDisplayAreaConstraint;
                    }
                }
            }
            else
            {
                lock (this)
                {
                    if (sender == tabletAreaEditor)
                    {
                        tabletWidth.DataValueChanged -= HandleTabletAreaConstraint;
                        tabletHeight.DataValueChanged -= HandleTabletAreaConstraint;
                    }
                    else if (sender == displayAreaEditor)
                    {
                        displayWidth.DataValueChanged -= HandleDisplayAreaConstraint;
                        displayHeight.DataValueChanged -= HandleDisplayAreaConstraint;
                    }
                }
            }
        }

        private void HandleAspectRatioLock(object sender, EventArgs e)
        {
            if (!handlingArLock)
            {
                // Avoids looping
                handlingArLock = true;

                if (sender == tabletWidth || sender == tabletAreaEditor)
                    tabletHeight.DataValue = displayHeight.DataValue / displayWidth.DataValue * tabletWidth.DataValue;
                else if (sender == tabletHeight)
                    tabletWidth.DataValue = displayWidth.DataValue / displayHeight.DataValue * tabletHeight.DataValue;
                else if ((sender == displayWidth) && prevDisplayWidth is float prevWidth)
                    tabletWidth.DataValue *= displayWidth.DataValue / prevWidth;
                else if ((sender == displayHeight) && prevDisplayHeight is float prevHeight)
                    tabletHeight.DataValue *= displayHeight.DataValue / prevHeight;

                prevDisplayWidth = displayWidth.DataValue;
                prevDisplayHeight = displayHeight.DataValue;

                handlingArLock = false;
            }
        }

        private void HandleTabletAreaConstraint(object sender, EventArgs args)
        {
            ForceAreaConstraint(tabletAreaEditor.Display, args);
        }

        private void HandleDisplayAreaConstraint(object sender, EventArgs args)
        {
            ForceAreaConstraint(displayAreaEditor.Display, args);
        }

        private void ForceAreaConstraint(object sender, EventArgs args)
        {
            var display = (AreaDisplay)sender;
            if (!handlingForcedArConstraint && display.LockToUsableArea && display.Area != null)
            {
                handlingForcedArConstraint = true;
                var fullBounds = display.FullAreaBounds;

                if (fullBounds.Width != 0 && fullBounds.Height != 0)
                {
                    if (display.Area.Width > fullBounds.Width)
                        display.Area.Width = fullBounds.Width;
                    if (display.Area.Height > fullBounds.Height)
                        display.Area.Height = fullBounds.Height;

                    var correction = GetOutOfBoundsAmount(display, display.Area.X, display.Area.Y);
                    display.Area.X -= correction.X;
                    display.Area.Y -= correction.Y;
                }

                handlingForcedArConstraint = false;
            }
        }

        private static Vector2 GetOutOfBoundsAmount(AreaDisplay display, float X, float Y)
        {
            var bounds = display.FullAreaBounds;
            bounds.X = 0;
            bounds.Y = 0;

            var area = display.Area;
            var rect = RectangleF.FromCenter(PointF.Empty, new SizeF(area.Width, area.Height));

            var corners = new PointF[]
            {
                    PointF.Rotate(rect.TopLeft, area.Rotation),
                    PointF.Rotate(rect.TopRight, area.Rotation),
                    PointF.Rotate(rect.BottomRight, area.Rotation),
                    PointF.Rotate(rect.BottomLeft, area.Rotation)
            };

            var pseudoArea = new RectangleF(
                PointF.Min(corners[0], PointF.Min(corners[1], PointF.Min(corners[2], corners[3]))),
                PointF.Max(corners[0], PointF.Max(corners[1], PointF.Max(corners[2], corners[3])))
            );

            pseudoArea.Center += new PointF(X, Y);

            return new Vector2
            {
                X = Math.Max(pseudoArea.Right - bounds.Right - 1, 0) + Math.Min(pseudoArea.Left - bounds.Left, 0),
                Y = Math.Max(pseudoArea.Bottom - bounds.Bottom - 1, 0) + Math.Min(pseudoArea.Top - bounds.Top, 0)
            };
        }

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

                var serviceProvider = AppInfo.PluginManager.BuildServiceProvider();
                var virtualScreen = serviceProvider.GetService<IVirtualScreen>();

                var subMenu = base.ContextMenu.Items.GetSubmenu("Set to display");
                foreach (var display in virtualScreen.Displays)
                {
                    subMenu.Items.Add(
                        new ActionCommand
                        {
                            MenuText = display.ToString(),
                            Action = () =>
                            {
                                this.Area.Width = display.Width;
                                this.Area.Height = display.Height;
                                if (display is IVirtualScreen root)
                                {
                                    this.Area.X = root.Width / 2;
                                    this.Area.Y = root.Height / 2;
                                }
                                else
                                {
                                    this.Area.X = display.Position.X + virtualScreen.Position.X + (display.Width / 2);
                                    this.Area.Y = display.Position.Y + virtualScreen.Position.Y + (display.Height / 2);
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
                var converter = new AreaConverterDialog
                {
                    DataContext = base.Area
                };
                await converter.ShowModalAsync(base.ParentWindow);
            }
        }
    }
}
