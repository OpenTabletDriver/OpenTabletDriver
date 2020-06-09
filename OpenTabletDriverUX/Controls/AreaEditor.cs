using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriverUX.Controls
{
    public class AreaEditor : Panel, IViewModelRoot<AreaViewModel>
    {
        public AreaEditor(string unit, bool enableRotation = false)
        {
            this.DataContext = new AreaViewModel();
            
            areaDisplay = new AreaDisplay(unit)
            {
                Padding = new Padding(5)
            };
            areaDisplay.Bind(c => c.ViewModel.Width, ViewModel, m => m.Width);
            areaDisplay.Bind(c => c.ViewModel.Height, ViewModel, m => m.Height);
            areaDisplay.Bind(c => c.ViewModel.X, ViewModel, m => m.X);
            areaDisplay.Bind(c => c.ViewModel.Y, ViewModel, m => m.Y);
            areaDisplay.Bind(c => c.ViewModel.Rotation, ViewModel, m => m.Rotation);
            areaDisplay.Bind(c => c.ViewModel.MaxWidth, ViewModel, m => m.MaxWidth);
            areaDisplay.Bind(c => c.ViewModel.MaxHeight, ViewModel, m => m.MaxHeight);

            widthBox = new TextBox();
            widthBox.TextBinding.Convert(
                s => float.TryParse(s, out var v) ? v : 0,
                f => f.ToString()).BindDataContext(
                    Binding.Property(
                        (AreaViewModel d) =>  d.Width));
            
            heightBox = new TextBox();
            heightBox.TextBinding.Convert(
                s => float.TryParse(s, out var v) ? v : 0,
                f => f.ToString()).BindDataContext(
                    Binding.Property(
                        (AreaViewModel d) =>  d.Height));
            
            xOffsetBox = new TextBox();
            xOffsetBox.TextBinding.Convert(
                s => float.TryParse(s, out var v) ? v : 0,
                f => f.ToString()).BindDataContext(
                    Binding.Property(
                        (AreaViewModel d) =>  d.X));
            
            yOffsetBox = new TextBox();
            yOffsetBox.TextBinding.Convert(
                s => float.TryParse(s, out var v) ? v : 0,
                f => f.ToString()).BindDataContext(
                    Binding.Property(
                        (AreaViewModel d) =>  d.Y));

            rotationBox = new TextBox();
            rotationBox.TextBinding.Convert(
                s => float.TryParse(s, out var v) ? v : 0,
                f => f.ToString()).BindDataContext(
                    Binding.Property(
                        (AreaViewModel d) =>  d.Rotation));

            var stackLayout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items = 
                {
                    new GroupBox
                    {
                        Text = "Width",
                        Content = AppendUnit(widthBox, unit)
                    },
                    new GroupBox
                    {
                        Text = "Height",
                        Content = AppendUnit(heightBox, unit)
                    },
                    new GroupBox
                    {
                        Text = "X Offset",
                        Content = AppendUnit(xOffsetBox, unit)
                    },
                    new GroupBox
                    {
                        Text = "Y Offset",
                        Content = AppendUnit(yOffsetBox, unit)
                    },
                    new GroupBox
                    {
                        Text = "Rotation", 
                        Content = AppendUnit(rotationBox, "°"),
                        Visible = enableRotation
                    }
                }
            };

            foreach (var item in stackLayout.Items)
            {
                if (item.Control is GroupBox groupBox)
                    groupBox.Padding = App.GroupBoxPadding;
            }

            TableCell[] cells = 
            {
                new TableCell(stackLayout),
                new TableCell(areaDisplay, true)
            };
            Content = TableLayout.Horizontal(5, cells);
        }

        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }

        private Control AppendUnit(Control control, string unit)
        {
            return TableLayout.Horizontal(
                5,
                new TableCell(control, true),
                new Label
                {
                    Text = unit,
                    VerticalAlignment = VerticalAlignment.Center,
                });
        }

        private AreaDisplay areaDisplay;
        private TextBox widthBox, heightBox, xOffsetBox, yOffsetBox, rotationBox;
    }
}