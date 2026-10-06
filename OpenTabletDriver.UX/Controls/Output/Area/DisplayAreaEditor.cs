using System;
using System.Linq;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.UX.Controls.Output.Area;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class DisplayAreaEditor : AreaEditor
    {
        public bool handlingDisplayAreaResize;
        private Action triggerAspectRatioLock;
        public DisplayAreaEditor(Action triggerAspectRatioLock)
        {
            this.ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";
            this.triggerAspectRatioLock = triggerAspectRatioLock;
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

            var displays = DesktopInterop.VirtualScreen?.Displays.ToArray()
                ?? throw new InvalidOperationException("Could not get VirtualScreen");

            // account for monitor layouts with negative offsets (e.g. Wayland supports this)
            // skip IVirtualScreen's as these tend to be normalized to 0,0, which may confuse these methods
            float xOffset = displays.Where(d => d is not IVirtualScreen).MinBy(d => d.Position.X)?.Position.X
                ?? throw new InvalidOperationException("Unable to look up X offset");
            float yOffset = displays.Where(d => d is not IVirtualScreen).MinBy(d => d.Position.Y)?.Position.Y
                ?? throw new InvalidOperationException("Unable to look up Y offset");

            foreach (var display in displays)
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
                            if (display is IVirtualScreen virtualScreen)
                            {
                                this.Area.X = virtualScreen.Width / 2;
                                this.Area.Y = virtualScreen.Height / 2;
                            }
                            else
                            {
                                virtualScreen = DesktopInterop.VirtualScreen;
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
    }
}
