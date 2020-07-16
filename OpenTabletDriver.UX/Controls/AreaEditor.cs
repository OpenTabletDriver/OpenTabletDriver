using System;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using TabletDriverLib;

namespace OpenTabletDriver.UX.Controls
{
    public class AreaEditor : Panel, IViewModelRoot<AreaViewModel>
    {
        public AreaEditor(string unit, bool enableRotation = false)
        {
            this.DataContext = new AreaViewModel();
            this.ContextMenu = new ContextMenu();
            
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

            var scrollview = new Scrollable
            {
                Content = stackLayout,
                Border = BorderType.None
            };

            TableCell[] cells = 
            {
                new TableCell(scrollview),
                new TableCell(areaDisplay, true)
            };
            Content = TableLayout.Horizontal(5, cells);

            this.ContextMenu.Items.GetSubmenu("Align").Items.AddRange(
                new MenuItem[]
                {
                    CreateMenuItem("Left", () => ViewModel.X = ViewModel.Width / 2),
                    CreateMenuItem("Right", () => ViewModel.X = ViewModel.MaxWidth - (ViewModel.Width / 2)),
                    CreateMenuItem("Top", () => ViewModel.Y = ViewModel.Height / 2),
                    CreateMenuItem("Bottom", ()  => ViewModel.Y = ViewModel.MaxHeight - (ViewModel.Height / 2)),
                    CreateMenuItem("Center", 
                        () => 
                        {
                            ViewModel.X = ViewModel.MaxWidth / 2;
                            ViewModel.Y = ViewModel.MaxHeight / 2;
                        }
                    )
                }
            );

            this.ContextMenu.Items.GetSubmenu("Resize").Items.AddRange(
                new MenuItem[]
                {
                    CreateMenuItem(
                        "Full area",
                        () => 
                        {
                            ViewModel.Height = ViewModel.MaxHeight;
                            ViewModel.Width = ViewModel.MaxWidth;
                            ViewModel.Y = ViewModel.MaxHeight / 2;
                            ViewModel.X = ViewModel.MaxWidth / 2;
                        }
                    ),
                    CreateMenuItem(
                        "Quarter area",
                        () => 
                        {
                            ViewModel.Height = ViewModel.MaxHeight / 2;
                            ViewModel.Width = ViewModel.MaxWidth / 2;
                        }
                    )
                }
            );

            AppendMenuItemSeparator();
            
            AppendCheckBoxMenuItem(
                "Lock to usable area",
                lockToMax =>
                {
                    if (lockToMax)
                        ViewModel.PropertyChanged += LimitArea;
                    else
                        ViewModel.PropertyChanged -= LimitArea;
                },
                defaultValue: true
            );
            
            this.MouseDown += (sender, e) =>
            {
                if (e.Buttons.HasFlag(MouseButtons.Alternate))
                    this.ContextMenu.Show(this);
            };
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

        private Command CreateMenuItem(string menuText, Action handler)
        {
            var command = new Command { MenuText = menuText };
            command.Executed += (sender, e) => handler();
            return command;
        }

        public Command AppendMenuItem(string menuText, Action handler)
        {
            var item = CreateMenuItem(menuText, handler);   
            this.ContextMenu.Items.Add(item);
            return item;
        }

        public CheckCommand AppendCheckBoxMenuItem(string menuText, Action<bool> handler, bool defaultValue = false)
        {
            var command = new CheckCommand { MenuText = menuText };
            command.Executed += (sender, e) => handler(command.Checked);
            command.Checked = defaultValue;
            if (defaultValue)
                command.Execute();
            this.ContextMenu.Items.Add(command);
            return command;
        }

        public void AppendMenuItemSeparator()
        {
            this.ContextMenu.Items.AddSeparator();
        }

        private void LimitArea(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.X):
                case nameof(ViewModel.Y):
                case nameof(ViewModel.Width):
                case nameof(ViewModel.Height):
                case nameof(ViewModel.MaxWidth):
                case nameof(ViewModel.MaxHeight):
                    if (ViewModel.MaxWidth == 0 || ViewModel.MaxHeight == 0)
                        break;
                    
                    if (ViewModel.X + (ViewModel.Width / 2) > ViewModel.MaxWidth)
                        areaDisplay.ViewModel.X = ViewModel.MaxWidth - (ViewModel.Width / 2);
                    else if (ViewModel.X - (ViewModel.Width / 2) < 0)
                        areaDisplay.ViewModel.X = ViewModel.Width / 2;
                    if (ViewModel.Y + (ViewModel.Height / 2) > ViewModel.MaxHeight)
                        areaDisplay.ViewModel.Y = ViewModel.MaxHeight - (ViewModel.Height / 2);
                    else if (ViewModel.Y - (ViewModel.Height / 2) < 0)
                        areaDisplay.ViewModel.Y = ViewModel.Height / 2;
                    break;
            }
        }

        private AreaDisplay areaDisplay;
        private TextBox widthBox, heightBox, xOffsetBox, yOffsetBox, rotationBox;
    }
}