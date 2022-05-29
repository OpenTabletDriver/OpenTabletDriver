using System;
using System.Threading.Tasks;

namespace OpenTabletDriver.UX.MacOS
{
    class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            await App.Run(Eto.Platforms.Mac64, args);
        }
    }
}
