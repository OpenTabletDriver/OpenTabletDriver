using Eto.Forms;

namespace OpenTabletDriver.UX.Dialogs
{
    public class OfflineAboutDialog : AboutDialog
    {
        public OfflineAboutDialog()
        {
            Title = "OpenTabletDriver";
            ProgramName = "OpenTabletDriver";
            ProgramDescription = "Open source, cross-platform tablet configurator";
            WebsiteLabel = "OpenTabletDriver";
            Website = new Uri(@"https://opentabletdriver.github.io");
            Version = $"v{Metadata.FullVersion}";
            Developers = new[] { "InfinityGhost" };
            Designers = new[] { "InfinityGhost" };
            Documenters = new[] { "InfinityGhost" };
            License = "LGPLv3";
            Copyright = string.Empty;
            Logo = Metadata.Logo.WithSize(256, 256);
        }
    }
}
