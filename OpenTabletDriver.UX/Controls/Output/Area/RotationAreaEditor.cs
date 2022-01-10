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

            var rotationBinding = AreaBinding.Child(c => c.Rotation);
            rotation.ValueBinding.Bind(rotationBinding);
        }

        private MaskedTextBox<float> rotation;

        protected override void CreateMenu()
        {
            base.CreateMenu();

            this.ContextMenu.Items.GetSubmenu("Flip").Items.Add(
                new ActionCommand
                {
                    MenuText = "Handedness",
                    Action = () =>
                    {
                        Area.Rotation += 180;
                        Area.Rotation %= 360;
                        Area.X = FullAreaBounds.Width - Area.X;
                        Area.Y = FullAreaBounds.Height - Area.Y;
                    }
                }
            );
        }
    }
}
