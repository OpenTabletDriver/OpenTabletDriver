using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Plugins
{
    public class PluginMetadataList : ListBox<PluginMetadata>
    {
        public PluginMetadataList()
        {
            ItemTextBinding = Binding.Property<PluginMetadata, string>(m => m.Name);

            Refresh();
            AppInfo.PluginManager.AssembliesChanged += (sender, e) => Refresh();
        }

        public static PluginMetadataCollection Repository { private set; get; }

        private static readonly TimeSpan DOWNLOAD_TIMEOUT = TimeSpan.FromSeconds(5);
        private static readonly Version AppVersion = Assembly.GetEntryAssembly().GetName().Version;

        public void Refresh() => Application.Instance.AsyncInvoke(async () =>
        {
            SetRepository(await DownloadMetadataAsync());
        });

        public void SetRepository(PluginMetadataCollection repository) => Application.Instance.AsyncInvoke(() =>
        {
            Repository = repository;

            var selectedIndex = this.SelectedIndex;

            var installed = from plugin in AppInfo.PluginManager.GetLoadedPlugins()
                orderby plugin.FriendlyName
                select plugin;

            var installedMeta = from ctx in installed
                let meta = ctx.GetMetadata()
                where meta != null
                select meta;

            var fetched = from meta in Repository
                where meta.SupportedDriverVersion <= AppVersion
                where !installedMeta.Any(m => PluginMetadata.Match(m, meta))
                select meta;

            var versions = from meta in installedMeta.Concat(fetched)
                orderby meta.PluginVersion descending
                group meta by (meta.Name, meta.Owner, meta.RepositoryUrl);

            this.DataStore = from grp in versions
                let meta = grp.FirstOrDefault()
                orderby meta.Name
                orderby installedMeta.Any(m => PluginMetadata.Match(m, meta)) descending
                select meta;

            this.SelectedIndex = selectedIndex;
        });

        public void SelectFirstOrDefault(Func<PluginMetadata, bool> predicate)
        {
            var list = this.DataStore as IList<PluginMetadata>;

            if (list?.FirstOrDefault(m => predicate(m)) is PluginMetadata existingMeta)
            {
                this.SelectedValue = existingMeta;
            }
        }

        protected async Task<PluginMetadataCollection> DownloadMetadataAsync()
        {
            var repoFetch = PluginMetadataCollection.DownloadAsync();
            var timeoutTask = Task.Delay(DOWNLOAD_TIMEOUT);

            try
            {
                var completedTask = await Task.WhenAny(repoFetch, timeoutTask);
                if (completedTask == timeoutTask)
                    MessageBox.Show("Fetching plugin metadata timed-out. Only local plugins will be shown.", MessageBoxType.Warning);
                else
                    return await repoFetch;
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show(
                    "An error occurred when retrieving metadata. Only local plugins will be shown." + Environment.NewLine +
                    $"(Status code {httpEx.StatusCode})",
                    MessageBoxType.Warning
                );
            }
            catch (Exception e)
            {
                e.ShowMessageBox();
                Log.Exception(e);
            }
            return PluginMetadataCollection.Empty;
        }
    }
}