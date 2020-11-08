using System;
using System.Diagnostics;
using Eto.Forms;

namespace OpenTabletDriver.UX.MacOS
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Mac64).Run(new MainForm());
        }
    }
}