using Eto.Forms;
using TabletDriverPlugin;

namespace OpenTabletDriverUX.Controls
{
    public class AreaEditor : Panel
    {
        public AreaEditor(Area area) : this()
        {
        }

        public AreaEditor()
        {
            var layout = new DynamicLayout();
            layout.BeginVertical();

            layout.EndVertical();

        }
    }
}