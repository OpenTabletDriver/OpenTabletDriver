using Eto.Forms;

namespace OpenTabletDriver.UX.Tools
{
    public class NumberBox : TextBox
    {
        public NumberBox()
        {
            TextChanging += UXTools.RestrictToNumber;
        }
    }
}