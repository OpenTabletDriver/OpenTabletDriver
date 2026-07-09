using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class RotationAreaEditor : AreaEditor
    {
        public RotationAreaEditor()
        {
            settingsPanel.Items.Add(
                new StackLayoutItem
                {
                    Control = new UnitGroup
                    {
                        Text = Strings.Rotation,
                        Unit = "°",
                        ToolTip = Strings.Angleofrotationaboutthecenterofthearea,
                        Orientation = Orientation.Horizontal,
                        Content = rotation = new FloatNumberBox()
                    }
                }
            );

            var rotationBinding = AreaBinding.Child(c => c!.Rotation);
            rotation.ValueBinding.Bind(rotationBinding);
            rotation.ValueChanged += (_, _) => Display.Invalidate();
        }

        private MaskedTextBox<float> rotation;

        protected override void CreateMenu()
        {
            base.CreateMenu();

            this.ContextMenu.Items.GetSubmenu(Strings.Flip).Items.Add(
                new ActionCommand
                {
                    MenuText = Strings.Handedness,
                    Action = () =>
                    {
                        Area!.Rotation += 180;
                        Area!.Rotation %= 360;
                        Area!.X = FullAreaBounds!.Value.Width - Area.X;
                        Area!.Y = FullAreaBounds!.Value.Height - Area.Y;
                    }
                }
            );
        }
    }
}
