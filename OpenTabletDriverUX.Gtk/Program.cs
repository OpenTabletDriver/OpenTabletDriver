using System;
using System.Threading.Tasks;
using Eto.Forms;
using TabletDriverLib.Contracts;

namespace OpenTabletDriverUX.Gtk
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(Eto.Platforms.Gtk).Run(new MainForm());
		}
	}
}
