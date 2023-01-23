using Eto;

namespace OpenTabletDriver.UX.Wpf
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new App(Platforms.Wpf).Start();
        }
    }
}
