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
            if (!Debugger.Launch())
                Console.WriteLine("failed to launch debugger");
            new Application(Eto.Platforms.Mac64).Run(new MainForm());
        }
    }
}