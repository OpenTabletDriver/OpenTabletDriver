using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class Group : Panel
    {
        public Group()
        {
        }

        public Group(string text, Control content, Orientation orientation = DEFAULT_ORIENTATION, bool expand = true)
            : this()
        {
            this.Text = text;
            this.Content = content;
            this.Orientation = orientation;
            this.ExpandContent = expand;
        }

        private const Orientation DEFAULT_ORIENTATION = Orientation.Vertical;

        protected virtual Padding ContentPadding => SystemInterop.CurrentPlatform == PluginPlatform.Windows ? new Padding(5, 10, 5, 5) : new Padding(5);

        protected virtual Color HorizontalBackgroundColor => SystemColors.ControlBackground;
        protected virtual Color VerticalBackgroundColor => SystemColors.WindowBackground;

        private string text;
        public string Text
        {
            set
            {
                this.text = value;
                UpdateControlLayout();
            }
            get => text;
        }

        private Control content;
        public new Control Content
        {
            set
            {
                this.content = value;
                UpdateControlLayout();
            }
            get => content;
        }

        public Orientation Orientation { set; get; } = DEFAULT_ORIENTATION;
        public bool ExpandContent { set; get; } = true;
        public HorizontalAlignment TitleHorizontalAlignment { set; get; } = HorizontalAlignment.Left;        
        public VerticalAlignment TitleVerticalAlignment { set; get; } = VerticalAlignment.Center;

        protected void UpdateControlLayout()
        {
            if (!this.Loaded)
                return;

            switch (Orientation, SystemInterop.CurrentPlatform)
            {
                case (_, PluginPlatform.MacOS):
                {
                    base.Content = new GroupBox
                    {
                        Text = this.Text,
                        Padding = new Padding(0, 2, 0, 0),
                        Content = this.Content
                    };
                    break;
                }
                case (Orientation.Horizontal, _):
                {
                    StackLayout contentLayout;
                    base.Content = new GroupBox
                    {
                        BackgroundColor = HorizontalBackgroundColor,
                        Content = contentLayout = new StackLayout
                        {
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Padding = ContentPadding,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    VerticalAlignment = TitleVerticalAlignment,
                                    Control = new Label
                                    {
                                        Text = this.Text
                                    }
                                },
                                new StackLayoutItem(this.Content, ExpandContent)
                            }
                        }
                    };
                    if (!ExpandContent)
                        contentLayout.Items.Insert(1, new StackLayoutItem(null, true));
                    break;
                }
                case (Orientation.Vertical, _):
                {
                    base.Content = new StackLayout
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Spacing = 5,
                        Padding = ContentPadding,
                        Items =
                        {
                            new StackLayoutItem
                            {
                                HorizontalAlignment = TitleHorizontalAlignment,
                                Control = new Label
                                {
                                    Text = this.Text,
                                    Font = SystemFonts.Bold(9)
                                }
                            },
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new GroupBox
                                {
                                    BackgroundColor = VerticalBackgroundColor,
                                    Padding = ContentPadding,
                                    Content = this.Content
                                }
                            }
                        }
                    };
                    break;
                }
            }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            UpdateControlLayout();
        }
    }
}
