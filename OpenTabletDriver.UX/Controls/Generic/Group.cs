using System;
using Eto.Drawing;
using Eto.Forms;

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

        private const int TITLE_FONT_SIZE = 9;
        private const Orientation DEFAULT_ORIENTATION = Orientation.Vertical;

        protected virtual Padding ContentPadding => new Padding(5);

        protected virtual Color HorizontalBackgroundColor => SystemColors.ControlBackground;
        protected virtual Color VerticalBackgroundColor => SystemColors.WindowBackground;
        
        private Label titleLabel = new Label
        {
            Font = Fonts.Sans(TITLE_FONT_SIZE, FontStyle.Bold)
        };

        public string Text
        {
            set => titleLabel.Text = value;
            get => titleLabel.Text;
        }

        public new Control Content { set; get; }

        public Orientation Orientation { set; get; } = DEFAULT_ORIENTATION;
        public bool ExpandContent { set; get; } = true;
        public HorizontalAlignment TitleHorizontalAlignment { set; get; } = HorizontalAlignment.Left;        
        public VerticalAlignment TitleVerticalAlignment { set; get; } = VerticalAlignment.Center;

        protected void UpdateControlLayout()
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:
                {
                    base.Content = new GroupBox
                    {
                        BackgroundColor = HorizontalBackgroundColor,
                        Content = new StackLayout
                        {
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Padding = ContentPadding,
                            Items =
                            {
                                new StackLayoutItem(titleLabel, TitleVerticalAlignment),
                                ExpandContent ? new StackLayoutItem(null, true) : null,
                                new StackLayoutItem(this.Content, ExpandContent)
                            }
                        }
                    };
                    break;
                }
                case Orientation.Vertical:
                {
                    base.Content = new StackLayout
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = 5,
                        Padding = ContentPadding,
                        Items =
                        {
                            new StackLayoutItem(titleLabel, TitleHorizontalAlignment),
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new GroupBox
                                {
                                    Padding = App.GroupBoxPadding,
                                    BackgroundColor = VerticalBackgroundColor,
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