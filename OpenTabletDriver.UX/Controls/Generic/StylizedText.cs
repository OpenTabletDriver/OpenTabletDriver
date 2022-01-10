using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class StylizedText : Panel
    {
        public StylizedText()
            : base()
        {
            Content = new Label();
        }

        public StylizedText(string text, Font font)
            : this()
        {
            Text = text;
            Font = font;
        }

        public StylizedText(string text, Font font, Padding padding)
            : this(text, font)
        {
            Padding = padding;
        }

        public string Text
        {
            get => ((Label)Content).Text;
            set => ((Label)Content).Text = value;
        }

        public Font Font
        {
            get => ((Label)Content).Font;
            set => ((Label)Content).Font = value;
        }
    }
}
