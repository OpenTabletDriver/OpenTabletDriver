using System;
using System.Threading.Tasks;
using Eto.Forms;

namespace OpenTabletDriverUX.Wpf
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			App.ThemeSetup(typeof(Eto.Wpf.Drawing.SystemColorsHandler));
			new Application(Eto.Platforms.Wpf).Run(new MainForm());
		}
	}
}
