using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Area
{
    public class AreaEditor : Panel, IViewModelRoot<AreaViewModel>
    {
        public AreaViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (AreaViewModel)this.DataContext;
        }

        private MaskedTextBox<float> width, height, x, y, rotation;
        private BooleanCommand lockToUsableArea;

        protected AreaDisplay Display { set; get; }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            StackLayout settingsPanel;

            this.Content = new StackLayout
            {
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Control = new Panel
                        {
                            Padding = new Padding(5),
                            Content = this.Display ??= new AreaDisplay
                            {
                                ViewModel = this.ViewModel
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Control = settingsPanel = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Control = new UnitGroup
                                    {
                                        Text = "Width",
                                        Unit = ViewModel.Unit,
                                        ToolTip = $"Area width in {ViewModel.Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = width = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = new UnitGroup
                                    {
                                        Text = "Height",
                                        Unit = ViewModel.Unit,
                                        ToolTip = $"Area height in {ViewModel.Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = height = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = new UnitGroup
                                    {
                                        Text = "X",
                                        Unit = ViewModel.Unit,
                                        ToolTip = $"Area center X offset in {ViewModel.Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = x = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = new UnitGroup
                                    {
                                        Text = "Y",
                                        Unit = ViewModel.Unit,
                                        ToolTip = $"Area center Y offset in {ViewModel.Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = y = new FloatNumberBox()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            if (ViewModel.EnableRotation)
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
            }

            this.ContextMenu = new ContextMenu();

            this.ContextMenu.Items.GetSubmenu("Align").Items.AddRange(
                new MenuItem[]
                {
                    new ActionCommand
                    {
                        MenuText = "Left",
                        Action = () => ViewModel.X = Display.GetAreaCenterOffset().X
                    },
                    new ActionCommand
                    {
                        MenuText = "Right",
                        Action = () => ViewModel.X = ViewModel.FullBackground.Width - Display.GetAreaCenterOffset().X
                    },
                    new ActionCommand
                    {
                        MenuText = "Top",
                        Action = () => ViewModel.Y = Display.GetAreaCenterOffset().Y
                    },
                    new ActionCommand
                    {
                        MenuText = "Bottom",
                        Action = () => ViewModel.Y = ViewModel.FullBackground.Height - Display.GetAreaCenterOffset().Y
                    },
                    new ActionCommand
                    {
                        MenuText = "Center",
                        Action = () =>
                        {
                            ViewModel.X = ViewModel.FullBackground.Center.X;
                            ViewModel.Y = ViewModel.FullBackground.Center.Y;
                        }
                    }
                }
            );

            this.ContextMenu.Items.GetSubmenu("Resize").Items.AddRange(
                new MenuItem[]
                {
                    new ActionCommand
                    {
                        MenuText = "Full area",
                        Action = () =>
                        {
                            ViewModel.Height = ViewModel.FullBackground.Height;
                            ViewModel.Width = ViewModel.FullBackground.Width;
                            ViewModel.Y = ViewModel.FullBackground.Center.Y;
                            ViewModel.X = ViewModel.FullBackground.Center.X;
                        }
                    },
                    new ActionCommand
                    {
                        MenuText = "Quarter area",
                        Action = () =>
                        {
                            ViewModel.Height = ViewModel.FullBackground.Height / 2;
                            ViewModel.Width = ViewModel.FullBackground.Width / 2;
                        }
                    }
                }
            );

            this.ContextMenu.Items.GetSubmenu("Flip").Items.AddRange(
                new MenuItem[]
                {
                    new ActionCommand
                    {
                        MenuText = "Horizontal",
                        Action = () => ViewModel.X = ViewModel.FullBackground.Width - ViewModel.X
                    },
                    new ActionCommand
                    {
                        MenuText = "Vertical",
                        Action = () => ViewModel.Y = ViewModel.FullBackground.Height - ViewModel.Y
                    }
                }
            );

            if (ViewModel.EnableRotation)
            {
                this.ContextMenu.Items.GetSubmenu("Flip").Items.Add(
                    new ActionCommand
                    {
                        MenuText = "Handedness",
                        Action = () =>
                        {
                            ViewModel.Rotation += 180;
                            ViewModel.Rotation %= 360;
                            ViewModel.X = ViewModel.FullBackground.Width - ViewModel.X;
                            ViewModel.Y = ViewModel.FullBackground.Height - ViewModel.Y;
                        }
                    }
                );
            }

            this.ContextMenu.Items.AddSeparator();

            lockToUsableArea = new BooleanCommand
            {
                MenuText = "Lock to usable area",
                DataContext = this.DataContext
            };
            this.ContextMenu.Items.Add(lockToUsableArea);

            BindAllToDataContext();
        }

        public void BindAllToDataContext()
        {
            width?.ValueBinding.BindDataContext<AreaViewModel>(m => m.Width);
            height?.ValueBinding.BindDataContext<AreaViewModel>(m => m.Height);
            x?.ValueBinding.BindDataContext<AreaViewModel>(m => m.X);
            y?.ValueBinding.BindDataContext<AreaViewModel>(m => m.Y);
            rotation?.ValueBinding.BindDataContext<AreaViewModel>(m => m.Rotation);
            lockToUsableArea?.CheckedBinding.BindDataContext<AreaViewModel>(m => m.LockToUsableArea);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Buttons)
            {
                case MouseButtons.Alternate:
                {
                    this.ContextMenu.Show(this);
                    break;
                }
            }
        }

        private class UnitGroup : Group
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
}
