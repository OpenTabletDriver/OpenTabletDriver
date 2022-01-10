using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.UX.Attributes;

namespace OpenTabletDriver.UX.Windows
{
    public class StylizedPage : DocumentPage
    {
        public StylizedPage()
        {
            this.Closable = false;

            var type = this.GetType();
            this.Text = type.GetCustomAttribute<PageNameAttribute>()?.Name ?? type.Name;
        }
    }
}
