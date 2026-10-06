using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Output.Area;

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
                                InvalidForegroundError = "Invalid display area.",
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
                                InvalidForegroundError = "Invalid tablet area.",
                                Unit = "mm"
                            }
                        }
                    }
                }
            };

            displayAreaEditor.AreaBinding.Bind(SettingsBinding.Child(c => c!.Display)!);
            displayAreaEditor.LockToUsableAreaBinding.Bind(App.Current, c => c.Settings.LockUsableAreaDisplay);

            tabletAreaEditor.AreaBinding.Bind(SettingsBinding.Child(c => c!.Tablet)!);
            tabletAreaEditor.LockToUsableAreaBinding.Bind(App.Current, c => c.Settings.LockUsableAreaTablet);

            tabletAreaEditor.LockAspectRatioBinding.Bind(SettingsBinding.Child(c => c!.LockAspectRatio));
            tabletAreaEditor.AreaClippingBinding.Bind(SettingsBinding.Child(c => c!.EnableClipping));
            tabletAreaEditor.IgnoreOutsideAreaBinding.Bind(SettingsBinding.Child(c => c!.EnableAreaLimiting));

            displayWidth = SettingsBinding.Child(c => c!.Display.Width);
            displayHeight = SettingsBinding.Child(c => c!.Display.Height);
            var displayX = SettingsBinding.Child(c => c!.Display.X);
            var displayY = SettingsBinding.Child(c => c!.Display.Y);
            displayWidth.DataValueChanged += HandleDisplayAreaConstraint;
            displayHeight.DataValueChanged += HandleDisplayAreaConstraint;
            displayX.DataValueChanged += HandleDisplayAreaConstraint;
            displayY.DataValueChanged += HandleDisplayAreaConstraint;

            tabletWidth = SettingsBinding.Child(c => c!.Tablet.Width);
            tabletHeight = SettingsBinding.Child(c => c!.Tablet.Height);
            var tabletX = SettingsBinding.Child(c => c!.Tablet.X);
            var tabletY = SettingsBinding.Child(c => c!.Tablet.Y);
            tabletWidth.DataValueChanged += HandleTabletAreaConstraint;
            tabletHeight.DataValueChanged += HandleTabletAreaConstraint;
            tabletX.DataValueChanged += HandleTabletAreaConstraint;
            tabletY.DataValueChanged += HandleTabletAreaConstraint;

            tabletAreaEditor.LockAspectRatioChanged += HookAspectRatioLock;

            tabletAreaEditor.LockToUsableAreaChanged += HandleTabletAreaConstraint;
            displayAreaEditor.LockToUsableAreaChanged += HandleDisplayAreaConstraint;

            HookAspectRatioLock(tabletAreaEditor, EventArgs.Empty);
        }

        internal DisplayAreaEditor displayAreaEditor;
        internal TabletAreaEditor tabletAreaEditor;

        private bool arLockHooked;
        private bool handlingArLock;
        private bool handlingForcedArConstraint;
        private bool handlingSettingsChanging;
        private float? prevDisplayWidth;
        private float? prevDisplayHeight;
        private DirectBinding<float> displayWidth;
        private DirectBinding<float> displayHeight;
        private DirectBinding<float> tabletWidth;
        private DirectBinding<float> tabletHeight;

        private AbsoluteModeSettings? settings;
        public AbsoluteModeSettings? Settings
        {
            set
            {
                this.settings = value;
                this.OnSettingsChanged();
            }
            get => this.settings;
        }

        public event EventHandler<EventArgs>? SettingsChanged;

        protected virtual void OnSettingsChanged()
        {
            handlingSettingsChanging = true;
            SettingsChanged?.Invoke(this, new EventArgs());
            handlingSettingsChanging = false;
        }

        public BindableBinding<AbsoluteModeEditor, AbsoluteModeSettings?> SettingsBinding
        {
            get
            {
                return new BindableBinding<AbsoluteModeEditor, AbsoluteModeSettings?>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        private void HookAspectRatioLock(object? sender, EventArgs args)
        {
            lock (this)
            {
                if (Settings?.LockAspectRatio ?? false)
                {
                    if (arLockHooked)
                        return;

                    HandleAspectRatioLock(tabletAreaEditor, EventArgs.Empty);

                    displayWidth.DataValueChanged += HandleAspectRatioLock;
                    displayHeight.DataValueChanged += HandleAspectRatioLock;
                    tabletWidth.DataValueChanged += HandleAspectRatioLock;
                    tabletHeight.DataValueChanged += HandleAspectRatioLock;
                    arLockHooked = true;
                }
                else
                {
                    if (!arLockHooked)
                        return;

                    displayWidth.DataValueChanged -= HandleAspectRatioLock;
                    displayHeight.DataValueChanged -= HandleAspectRatioLock;
                    tabletWidth.DataValueChanged -= HandleAspectRatioLock;
                    tabletHeight.DataValueChanged -= HandleAspectRatioLock;
                    arLockHooked = false;
                }
            }
        }

        private void HandleAspectRatioLock(object? sender, EventArgs e)
        {
            if (!handlingArLock && !handlingSettingsChanging)
            {
                // Avoids looping
                handlingArLock = true;

                if (sender == tabletWidth || sender == tabletAreaEditor)
                {
                    var fullHeight = tabletAreaEditor.FullAreaBounds!.Value.Height;
                    var scaledHeight = displayHeight.DataValue / displayWidth.DataValue * tabletWidth.DataValue;
                    if (tabletAreaEditor.FullAreaCommandExecuting && scaledHeight > fullHeight)
                    {
                        tabletHeight.DataValue = fullHeight;
                        tabletWidth.DataValue = displayWidth.DataValue / displayHeight.DataValue * fullHeight;
                    }
                    else
                    {
                        tabletHeight.DataValue = scaledHeight;
                    }
                }
                else if (sender == tabletHeight)
                {
                    tabletWidth.DataValue = displayWidth.DataValue / displayHeight.DataValue * tabletHeight.DataValue;
                }
                else if ((sender == displayWidth) && prevDisplayWidth is float prevWidth)
                {
                    tabletWidth.DataValue *= displayWidth.DataValue / prevWidth;
                }
                else if ((sender == displayHeight) && prevDisplayHeight is float prevHeight)
                {
                    tabletHeight.DataValue *= displayHeight.DataValue / prevHeight;
                }

                prevDisplayWidth = displayWidth.DataValue;
                prevDisplayHeight = displayHeight.DataValue;

                handlingArLock = false;
            }
        }

        private void HandleTabletAreaConstraint(object? sender, EventArgs args)
        {
            ForceAreaConstraint(tabletAreaEditor.Display, args);
        }

        private void HandleDisplayAreaConstraint(object? sender, EventArgs args)
        {
            ForceAreaConstraint(displayAreaEditor.Display, args);
        }

        private void ForceAreaConstraint(object? sender, EventArgs args)
        {
            if (sender is not AreaDisplay display) return;

            if (!handlingForcedArConstraint && !handlingSettingsChanging && display.LockToUsableArea && display.Area != null)
            {
                handlingForcedArConstraint = true;
                Debug.Assert(display.FullAreaBounds.HasValue);
                var fullBounds = display.FullAreaBounds.Value;

                if (fullBounds.Width != 0 && fullBounds.Height != 0)
                {
                    if (display.Area.Rotation % 180 == 0)
                    {
                        if (display.Area.Width > fullBounds.Width)
                            display.Area.Width = fullBounds.Width;
                        if (display.Area.Height > fullBounds.Height)
                            display.Area.Height = fullBounds.Height;
                    }

                    var correction = GetOutOfBoundsAmount(display, display.Area.X, display.Area.Y);
                    display.Area.X -= correction.X;
                    display.Area.Y -= correction.Y;
                }

                handlingForcedArConstraint = false;
            }
        }

        private static Vector2 GetOutOfBoundsAmount(AreaDisplay display, float X, float Y)
        {
            Debug.Assert(display.FullAreaBounds.HasValue);
            var bounds = display.FullAreaBounds.Value;
            bounds.X = 0;
            bounds.Y = 0;

            var area = display.Area!;
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
    }
}
