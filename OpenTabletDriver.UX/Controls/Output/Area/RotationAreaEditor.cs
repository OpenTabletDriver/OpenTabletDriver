using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class RotationAreaEditor : AreaEditor
    {
        public RotationAreaEditor()
            : base()
        {
            settingsPanel.Items.Add(
                new StackLayoutItem
                {
                    Control = new UnitGroup
                    {
                        Text = "Rotation",
                        Unit = "°",
                        ToolTip = "Angle of rotation about the center of the area.",
                        Orientation = Orientation.Horizontal,
                        Content = rotation = new FloatNumberBox()
                    }
                }
            );

            rotation.ValueBinding.Bind(AreaRotationBinding);

            display.AreaRotationBinding.Bind(AreaRotationBinding);
        }

        protected override void CreateMenu()
        {
            base.CreateMenu();

            this.ContextMenu.Items.GetSubmenu("Flip").Items.Add(
                new ActionCommand
                {
                    MenuText = "Handedness",
                    Action = () =>
                    {
                        AreaRotation += 180;
                        AreaRotation %= 360;
                        AreaXOffset = FullAreaBounds.Width - AreaXOffset;
                        AreaYOffset = FullAreaBounds.Height - AreaYOffset;
                    }
                }
            );
        }
    }
}