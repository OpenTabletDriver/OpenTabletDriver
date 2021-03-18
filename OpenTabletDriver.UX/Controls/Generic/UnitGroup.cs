using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class UnitGroup : Group
    {
        public string Unit
        {
            set => unitLabel.Text = value;
            get => unitLabel.Text;
        }

        private Label unitLabel = new Label();

        private Control content;
        public new Control Content
        {
            set
            {
                this.content = value;
                base.Content = new StackLayout
                {
                    Spacing = 5,
                    Orientation = Orientation.Horizontal,
                    Items =
                    {
                        new StackLayoutItem(this.Content, true),
                        new StackLayoutItem
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Control = this.unitLabel
                        }
                    }
                };
            }
            get => this.content;
        }
    }
}
