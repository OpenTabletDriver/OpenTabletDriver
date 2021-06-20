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
            this.Enabled = false;

            SetRepository(await DownloadMetadataAsync());

            this.Enabled = true;
        });

        public void SetRepository(PluginMetadataCollection repository) => Application.Instance.AsyncInvoke(() =>
        {
            Repository = repository;

            var selected = this.SelectedItem;

            var local = from ctx in AppInfo.PluginManager.GetLoadedPlugins()
                orderby ctx.FriendlyName
                select ctx.GetMetadata();

            var remote = from meta in Repository
                where meta.SupportedDriverVersion <= AppVersion
                where !local.Any(m => PluginMetadata.Match(m, meta))
                select meta;

            var plugins = from meta in local.Concat(remote)
                orderby meta.PluginVersion descending
                group meta by (meta.Name, meta.Owner, meta.RepositoryUrl);

            this.DataStore = from plugin in plugins
                let meta = plugin.FirstOrDefault()
                orderby meta.Name
                orderby local.Any(m => PluginMetadata.Match(m, meta)) descending
                select meta;

            SelectFirstOrDefault(p => PluginMetadata.Match(p, selected));
        });

        public void SelectFirstOrDefault(Func<PluginMetadata, bool> predicate)
        {
            if ((this.DataStore as IEnumerable<PluginMetadata>)?.FirstOrDefault(m => predicate(m)) is PluginMetadata existingMeta)
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