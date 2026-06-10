using Eto.Forms;

namespace OpenTabletDriver.UX.MacOS;

public class Platform : Eto.Mac.Platform
{
    public Platform() : base()
    {
        Add<Form.IHandler>(() => new FormHandler());
    }
}
