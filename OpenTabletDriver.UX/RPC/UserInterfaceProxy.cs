using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.RPC
{
    public class UserInterfaceProxy : IUserInterface
    {
        public async Task ShowClient()
        {
            await Application.Instance.InvokeAsync(() =>
            {
                if (Application.Instance?.MainForm is Form form)
                {
                    form.Show();
                    form.BringToFront();
                }
            });
        }
    }
}