using System.Reflection;
using Eto.Drawing;

namespace OpenTabletDriver.UX
{
    public static class Metadata
    {
        public const string WIKI_URL = "https://opentabletdriver.net/Wiki";

        public static string FullVersion { get; } =
            Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        public static Version Version { get; } = Assembly.GetEntryAssembly()!.GetName().Version!;

        public static Bitmap Logo => new Bitmap(typeof(Metadata).Assembly.GetManifestResourceStream("OpenTabletDriver.UX.Assets.otd.png"));
    }
}
