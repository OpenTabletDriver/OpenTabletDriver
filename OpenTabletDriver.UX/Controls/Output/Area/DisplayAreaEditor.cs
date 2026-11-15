using System;
using System.ComponentModel;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class DisplayAreaEditor : AreaEditor
    {
        public bool handlingDisplayAreaResize;
        private Action triggerAspectRatioLock;
        public DisplayAreaEditor(Action triggerAspectRatioLock)
        {
            base.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";
            this.triggerAspectRatioLock = triggerAspectRatioLock;
            SetDisplaySize();
            App.Current.PropertyChanged += OnGlobalViewmodelOnPropertyChanged;
        }

        private void OnGlobalViewmodelOnPropertyChanged(object? o, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(App.Current.Displays))
            {
                Application.Instance.AsyncInvoke(SetDisplaySize);
                Application.Instance.AsyncInvoke(CreateMenu);
            }
        }

        private void SetDisplaySize()
        {
            var bgs = from disp in App.Current.Displays
                      where !disp.IsVirtualScreen
                      select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            AreaBounds = bgs;
        }

        protected override void CreateMenu()
        {
            base.CreateMenu();

            this.ContextMenu.Items.GetSubmenu("Resize").Items.AddRange([
                new ActionCommand
                {
                    MenuText = "Full area",
                    Action = () =>
                    {
                        handlingDisplayAreaResize = true;

                        Area!.Y = FullAreaBounds!.Value.Center.Y;
                        Area!.X = FullAreaBounds!.Value.Center.X;
                        Area!.Width = FullAreaBounds!.Value.Width;
                        Area!.Height = FullAreaBounds!.Value.Height;

                        handlingDisplayAreaResize = false;
                        triggerAspectRatioLock();
                    }
                },
                new ActionCommand
                {
                    MenuText = "Quarter area",
                    Action = () =>
                    {
                        handlingDisplayAreaResize = true;

                        Area!.Y = FullAreaBounds!.Value.Center.Y;
                        Area!.X = FullAreaBounds!.Value.Center.X;
                        Area!.Height = FullAreaBounds!.Value.Height / 2;
                        Area!.Width = FullAreaBounds!.Value.Width / 2;

                        handlingDisplayAreaResize = false;
                        triggerAspectRatioLock();
                    }
                }
            ]);

            base.ContextMenu.Items.AddSeparator();

            var subMenu = base.ContextMenu.Items.GetSubmenu("Set to display");

            // account for monitor layouts with negative offsets (e.g. Wayland supports this)
            // skip IVirtualScreen's as these tend to be normalized to 0,0, which may confuse these methods
            var displaysWithoutVirtualScreens = App.Current.Displays.Where(d => !d.IsVirtualScreen).ToArray();
            float xOffset = displaysWithoutVirtualScreens.MinBy(d => d.Position.X)?.Position.X
                ?? throw new InvalidOperationException("Unable to look up X offset");
            float yOffset = displaysWithoutVirtualScreens.MinBy(d => d.Position.Y)?.Position.Y
                ?? throw new InvalidOperationException("Unable to look up Y offset");

            foreach (var display in App.Current.Displays)
            {
                subMenu.Items.Add(
                    new ActionCommand
                    {
                        MenuText = display.ToString(),
                        Action = () =>
                        {
                            if (this.Area == null)
                                throw new InvalidOperationException("Area null, somehow?");

                            handlingDisplayAreaResize = true;

                            this.Area.Width = display.Width;
                            this.Area.Height = display.Height;
                            if (display.IsVirtualScreen)
                            {
                                this.Area.X = display.Width / 2;
                                this.Area.Y = display.Height / 2;
                            }
                            else
                            {
                                this.Area.X = display.Position.X - xOffset + (display.Width / 2);
                                this.Area.Y = display.Position.Y - yOffset + (display.Height / 2);
                            }

                            handlingDisplayAreaResize = false;
                            triggerAspectRatioLock();
                        }
                    }
                );
            }
        }

        protected override void Dispose(bool disposing)
        {
            App.Current.PropertyChanged -= OnGlobalViewmodelOnPropertyChanged;
            base.Dispose(disposing);
        }
    }
}
