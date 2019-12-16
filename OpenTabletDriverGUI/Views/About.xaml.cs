using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NativeLib;

namespace OpenTabletDriverGUI.Views
{
    public class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var repoBlock = this.Find<TextBlock>("RepoBlock");
            repoBlock.PointerPressed += (s, e) => OpenRepoUrl();
            var versionBlock = this.Find<TextBlock>("VersionBlock");
            versionBlock.PointerPressed += async (s, e) => await Copy(Version);
        }

        private void OpenRepoUrl()
        {
            NativeLib.Tools.OpenUrl(RepoURL);
        }

        private async Task Copy(string text)
        {
            await App.Current.Clipboard.SetTextAsync(text);
        }

        public string Author => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;
        public string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public string RepoURL => @"https://github.com/InfinityGhost/OpenTabletDriver";
    }
}