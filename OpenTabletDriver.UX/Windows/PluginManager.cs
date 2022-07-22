using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls;

namespace OpenTabletDriver.UX.Windows
{
    public sealed class PluginManager : DesktopForm
    {
        private readonly IDriverDaemon _daemon;
        private readonly App _app;
        private readonly ListBox<PluginMetadata> _pluginsList;
        private ImmutableArray<PluginMetadata> _remotePlugins;
        private ImmutableArray<PluginMetadata> _installedPlugins;

        public PluginManager(IDriverDaemon daemon, App app)
        {
            _daemon = daemon;
            _app = app;

            Title = "Plugin Manager";

            var placeholder = new Placeholder
            {
                Text = "No plugin is selected."
            };

            var splitter = new Splitter
            {
                Panel1MinimumSize = 250,
                Panel1 = _pluginsList = new ListBox<PluginMetadata>(),
                Panel2 = placeholder
            };

            _pluginsList.ItemTextBinding = Binding.Property<PluginMetadata, string>(p => p.Name);
            Refresh().Run();

            Content = splitter;

            var installButton = new Button((_, _) => Install().Run())
            {
                Text = "Install"
            };

            var uninstallButton = new Button((_, _) => Uninstall().Run())
            {
                Text = "Uninstall"
            };

            var details = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    ControlFor(m => m.Name),
                    ControlFor(m => m.Owner),
                    ControlFor(m => m.Description),
                    ControlFor(m => m.PluginVersion),
                    ControlFor(m => m.RepositoryUrl),
                    ControlFor(m => m.SupportedDriverVersion),
                    ControlFor(m => m.WikiUrl),
                    ControlFor(m => m.LicenseIdentifier),
                    new StackLayoutItem(null, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalContentAlignment = VerticalAlignment.Bottom,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(uninstallButton, true),
                            new StackLayoutItem(installButton, true)
                        }
                    }
                }
            };

            DataContextBinding.Bind(_pluginsList.SelectedValueBinding);

            DataContextChanged += (_, _) =>
            {
                splitter.Panel2 = DataContext is PluginMetadata ? details : placeholder;
                if (DataContext is not PluginMetadata meta)
                    return;

                uninstallButton.Enabled = _installedPlugins.Any(meta.Match);

                if (uninstallButton.Enabled && _remotePlugins.FirstOrDefault(meta.Match) is PluginMetadata remoteMeta)
                    installButton.Enabled = remoteMeta.PluginVersion > meta.PluginVersion;
                else
                    installButton.Enabled = !uninstallButton.Enabled;

                installButton.Text = uninstallButton.Enabled ? "Update" : "Install";
            };

            var fileMenu = new ButtonMenuItem
            {
                Text = "&File",
                Items =
                {
                    new AppCommand("Install...", InstallDialog),
                    new AppCommand("Refresh", Refresh, Application.Instance.CommonModifier | Keys.R)
                }
            };

            Menu = new MenuBar
            {
                Items =
                {
                    fileMenu
                },
                QuitItem = new AppCommand("Close", Close, Keys.Escape)
            };
        }

        /// <summary>
        /// Refreshes the plugin list from the default remote source.
        /// </summary>
        private async Task Refresh()
        {
            _pluginsList.Enabled = false;
            var selectedItem = _pluginsList.SelectedValue as PluginMetadata;

            var appVersion = Assembly.GetEntryAssembly()!.GetName().Version!;

            var installedQuery = await _daemon.GetInstalledPlugins();
            _installedPlugins = installedQuery.ToImmutableArray();

            var remoteQuery = await _daemon.GetRemotePlugins();
            _remotePlugins = remoteQuery.ToImmutableArray();

            var remote = from meta in _remotePlugins
                where meta.IsSupportedBy(appVersion)
                where !_installedPlugins.Any(meta.Match)
                orderby meta.Name
                select meta;

            var plugins = from meta in _installedPlugins.Concat(remote)
                orderby meta.PluginVersion descending
                group meta by (meta.Name, meta.Owner, meta.RepositoryUrl);

            var query = from plugin in plugins
                let meta = plugin.FirstOrDefault()
                orderby meta.Name, _installedPlugins.Any(m => m.Match(meta)) descending, _installedPlugins.Any(meta.Match)
                select meta;

            var store = query.ToImmutableList();

            _pluginsList.DataStore = store;
            _pluginsList.Enabled = true;

            if (selectedItem != null)
                _pluginsList.SelectedIndex = store.FindIndex(m => m.Match(selectedItem));
        }

        /// <summary>
        /// Installs the currently selected plugin.
        /// </summary>
        private async Task Install()
        {
            Content.Enabled = false;

            var plugin = (PluginMetadata) DataContext;
            await _daemon.DownloadPlugin(plugin);

            Content.Enabled = true;
            await Refresh();
        }

        /// <summary>
        /// Uninstalls the currently selected plugin.
        /// </summary>
        private async Task Uninstall()
        {
            Content.Enabled = false;

            var plugin = (PluginMetadata) DataContext;
            await _daemon.UninstallPlugin(plugin);

            Content.Enabled = true;
            await Refresh();
        }

        /// <summary>
        /// Shows an install dialog for a plugin zip file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task InstallDialog()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Install OpenTabletDriver plugin...",
                CheckFileExists = true,
                MultiSelect = false,
                Filters =
                {
                    new FileFilter("Plugin", "*.zip")
                }
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                await _daemon.InstallPlugin(dialog.FileName);
            }
        }

        /// <summary>
        /// Creates a control from an expression.
        /// </summary>
        /// <param name="expression">The expression pointing to the target member.</param>
        private Control ControlFor(Expression<Func<PluginMetadata, object?>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var title = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;

            TextControl? control = null;

            if (property.GetCustomAttribute<UrlAttribute>() is not null)
            {
                var url = DataContext != null ? property.GetValue(DataContext)?.ToString() : null;
                var linkButton = new LinkButton();
                linkButton.Click += (_, _) =>
                {
                    if (url != null)
                        _app.Open(url);
                };

                control = linkButton;
            }

            control ??= new Label
            {
                Wrap = WrapMode.Word
            };
            control.TextBinding.Convert(t => t, (object? o) => o?.ToString()).BindDataContext(expression);
            return new LabeledGroup(title, null, control)
            {
                MinimumSize = new Size(0, 46)
            };
        }
    }
}
