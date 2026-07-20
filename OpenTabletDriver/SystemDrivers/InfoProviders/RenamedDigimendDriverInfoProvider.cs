namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    /// <summary>
    /// Detects <c>hid_digimend_uclogic</c>
    /// <para/>
    /// The Digimend driver has had <c>hid_digimend_</c> prefixed to its modules on some distributions,
    /// e.g. see Debian's <c>digimend-dkms 13-3</c> changelog:
    /// <a href="https://tracker.debian.org/media/packages/d/digimend-dkms/changelog-13-4">
    /// digimend-dkms (13-4) changelog
    /// </a>
    /// </summary>
    internal class RenamedDigimendDriverInfoProvider : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "Digimend";

        protected override string LinuxFriendlyName => "Digimend UC Logic";

        protected override string LinuxModuleName => "hid_digimend_uclogic";

        protected override string[] WinProcessNames => [];

        protected override string[] Heuristics { get; } = [];
    }
}
