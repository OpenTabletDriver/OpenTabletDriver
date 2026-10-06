using System;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Output.Area;
using OpenTabletDriver.UX.Controls.Utilities;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class TabletAreaEditor : RotationAreaEditor
    {
        public TabletAreaEditor()
        {
            this.ToolTip = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";
        }

        private BooleanCommand? lockArCmd, areaClippingCmd, ignoreOutsideAreaCmd;
        private bool lockAspectRatio, areaClipping, ignoreOutsideArea;

        public event EventHandler<EventArgs>? LockAspectRatioChanged;
        public event EventHandler<EventArgs>? AreaClippingChanged;
        public event EventHandler<EventArgs>? IgnoreOutsideAreaChanged;

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
                MenuText = "Clamp input outside area"
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
