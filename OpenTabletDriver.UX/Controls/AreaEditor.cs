using System;
using System.ComponentModel;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public class AreaEditor : Panel, IViewModelRoot<AreaViewModel>
    {
        public AreaEditor(string unit, bool enableRotation = false)
        {
            this.DataContext = new AreaViewModel();
            this.ContextMenu = new ContextMenu();
            
            AreaDisplay = new AreaDisplay(unit)
            {
                Padding = new Padding(5)
            };
            AreaDisplay.Bind(c => c.ViewModel.Width, ViewModel, m => m.Width);
            AreaDisplay.Bind(c => c.ViewModel.Height, ViewModel, m => m.Height);
            AreaDisplay.Bind(c => c.ViewModel.X, ViewModel, m => m.X);
            AreaDisplay.Bind(c => c.ViewModel.Y, ViewModel, m => m.Y);
            AreaDisplay.Bind(c => c.ViewModel.Rotation, ViewModel, m => m.Rotation);
            AreaDisplay.Bind(c => c.ViewModel.Background, ViewModel, m => m.Background);

            float parseFloat(string s)
            {
                return !string.IsNullOrWhiteSpace(s) ? (float.TryParse(s, out var v) ? v : 1) : 0;
            }

            widthBox = new TextBox();
            widthBox.TextBinding.Convert(
                s => parseFloat(s),
                v => $"{v}"
            ).BindDataContext(Eto.Forms.Binding.Property((AreaViewModel d) =>  d.Width));
            
            heightBox = new TextBox();
            heightBox.TextBinding.Convert(
                s => parseFloat(s),
                v => $"{v}"
            ).BindDataContext(Eto.Forms.Binding.Property((AreaViewModel d) =>  d.Height));
            
            xOffsetBox = new TextBox();
            xOffsetBox.TextBinding.Convert(
                s => parseFloat(s),
                v => $"{v}"
            ).BindDataContext(Eto.Forms.Binding.Property((AreaViewModel d) =>  d.X));
            
            yOffsetBox = new TextBox();
            yOffsetBox.TextBinding.Convert(
                s => parseFloat(s),
                v => $"{v}"
            ).BindDataContext(Eto.Forms.Binding.Property((AreaViewModel d) =>  d.Y));

            rotationBox = new TextBox();
            rotationBox.TextBinding.Convert(
                s => parseFloat(s),
                v => $"{v}"
            ).BindDataContext(Eto.Forms.Binding.Property((AreaViewModel d) =>  d.Rotation));

            var stackLayout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items = 
                {
                    new GroupBox
                    {
                        Text = "Width",
                        Content = AppendUnit(widthBox, unit),
                        ToolTip = $"Width of the area"
                    },
                    new GroupBox
                    {
                        Text = "Height",
                        Content = AppendUnit(heightBox, unit),
                        ToolTip = $"Height of the area"
                    },
                    new GroupBox
                    {
                        Text = "X Offset",
                        Content = AppendUnit(xOffsetBox, unit),
                        ToolTip = $"Center X coordinate of the area"
                    },
                    new GroupBox
                    {
                        Text = "Y Offset",
                        Content = AppendUnit(yOffsetBox, unit),
                        ToolTip = $"Center Y coordinate of the area"
                    }
                }
            };

            if (enableRotation)
            {
                stackLayout.Items.Add(
                    new GroupBox
                    {
                        Text = "Rotation", 
                        Content = AppendUnit(rotationBox, "°"),
                        ToolTip = $"Rotation of the area about the center"
                    }
                );
            }

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

            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem(scrollview, VerticalAlignment.Center),
                    new StackLayoutItem(AreaDisplay, VerticalAlignment.Stretch, true)
                }
            };

            this.ContextMenu.Items.GetSubmenu("Align").Items.AddRange(
                new MenuItem[]
                {
                    CreateMenuItem("Left", () => ViewModel.X = ViewModel.Width / 2),
                    CreateMenuItem("Right", () => ViewModel.X = ViewModel.Background.Max(d => d.Width) - (ViewModel.Width / 2)),
                    CreateMenuItem("Top", () => ViewModel.Y = ViewModel.Height / 2),
                    CreateMenuItem("Bottom", ()  => ViewModel.Y = ViewModel.Background.Max(d => d.Height) - (ViewModel.Height / 2)),
                    CreateMenuItem("Center", 
                        () => 
                        {
                            ViewModel.X = ViewModel.Background.Max(d => d.Width) / 2;
                            ViewModel.Y = ViewModel.Background.Max(d => d.Height) / 2;
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
                            ViewModel.Width = ViewModel.Background.Max(d => d.Width);
                            ViewModel.Height = ViewModel.Background.Max(d => d.Height);
                            ViewModel.X = ViewModel.Background.Max(d => d.Width) / 2;
                            ViewModel.Y = ViewModel.Background.Max(d => d.Height) / 2;
                        }
                    ),
                    CreateMenuItem(
                        "Quarter area",
                        () => 
                        {
                            ViewModel.Height = ViewModel.Background.Max(d => d.Height) / 2;
                            ViewModel.Width = ViewModel.Background.Max(d => d.Width) / 2;
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
                case nameof(ViewModel.Background):
                    if (ViewModel.Background == null || ViewModel.FullBackground == null || ViewModel.FullBackground.Width == 0 || ViewModel.FullBackground.Height == 0)
                        break;

                    if (ViewModel.Y + (ViewModel.Height / 2) > ViewModel.FullBackground.Height)
                        AreaDisplay.ViewModel.Y = ViewModel.FullBackground.Height - (ViewModel.Height / 2);
                    else if (ViewModel.Y - (ViewModel.Height / 2) < 0)
                        AreaDisplay.ViewModel.Y = ViewModel.Height / 2;
                    if (ViewModel.X + (ViewModel.Width / 2) > ViewModel.FullBackground.Width)
                        AreaDisplay.ViewModel.X = ViewModel.FullBackground.Width - (ViewModel.Width / 2);
                    else if (ViewModel.X - (ViewModel.Width / 2) < 0)
                        AreaDisplay.ViewModel.X = ViewModel.Width / 2;
                    break;
            }
        }

        public AreaDisplay AreaDisplay { protected set; get; }

        private TextBox widthBox, heightBox, xOffsetBox, yOffsetBox, rotationBox;
    }
}