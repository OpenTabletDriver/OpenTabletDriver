using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows
{
    public class PluginManagerWindow : Form
    {
        public PluginManagerWindow()
        {
            this.Title = "Plugin Manager";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.Size = new Size(700, 550);
            this.AllowDrop = true;

            this.Menu = ConstructMenu();
            this.Content = dropPanel;

            dropPanel.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Splitter
                        {
                            Panel1MinimumSize = 150,
                            Panel1 = pluginList,
                            Panel2 = metadataViewer
                        }
                    },
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Control = new Panel
                        {
                            Content = dragInstruction,
                            Padding = 5
                        }
                    }
                }
            };

            dropPanel.RequestPluginInstall += async (path) => await Install(path);
            pluginList.RequestPluginUninstall += async (ctx) => await Uninstall(ctx);
            pluginList.SelectedMetadataChanged += (meta) => metadataViewer.Update(meta);
            metadataViewer.RequestPluginUninstall += async (meta) => await Uninstall(pluginList.SelectedPlugin);
            metadataViewer.RequestPluginInstall += async (meta) => await DownloadAndInstall(meta);

            _ = Refresh();
        }

        private readonly PluginDropPanel dropPanel = new PluginDropPanel();
        private readonly PluginListBox pluginList = new PluginListBox();
        private readonly MetadataViewer metadataViewer = new MetadataViewer();
        private readonly Label dragInstruction = new Label
        {
            Text = "Drag and drop plugins to install.",
            VerticalAlignment = VerticalAlignment.Center
        };

        public async Task Refresh()
        {
            await App.Driver.Instance.LoadPlugins();
            AppInfo.PluginManager.Load();

            (Application.Instance.MainForm as MainForm).Refresh();
            pluginList.Refresh();
        }

        protected async Task DownloadAndInstall(PluginMetadata metadata)
         {
            if (await App.Driver.Instance.DownloadPlugin(metadata))
            {
                await Refresh();
            }
        }

        protected async Task Install(string path)
        {
            if (await App.Driver.Instance.InstallPlugin(path))
            {
                AppInfo.PluginManager.Load();
                await Refresh();
            }
            else
            {
                MessageBox.Show(this, $"Failed to install plugin from '{path}'", "Plugin Manager", MessageBoxType.Error);
            }
        }

        protected async Task Uninstall(DesktopPluginContext context)
        {
            if (await App.Driver.Instance.UninstallPlugin(context.FriendlyName))
            {
                AppInfo.PluginManager.UnloadPlugin(context);
                await Refresh();
            }
            else
            {
                MessageBox.Show(this, $"'{context.FriendlyName}' failed to uninstall", "Plugin Manager", MessageBoxType.Error);
            }
        }

        private MenuBar ConstructMenu()
        {
            var quitCommand = new Command { MenuText = "Exit", Shortcut = Keys.Escape  };
            quitCommand.Executed += (_, _) => this.Close();

            var install = new Command { MenuText = "Install plugin...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            install.Executed += PromptInstallPlugin;

            var refresh = new Command { MenuText = "Refresh", Shortcut = Application.Instance.CommonModifier | Keys.R };
            refresh.Executed += async (_, _) => await Refresh();

            var openRepository = new Command { MenuText = "Get more plugins..." };
            openRepository.Executed += (_, _) => SystemInterop.Open(App.PluginRepositoryUrl);

            return new MenuBar()
            {
                QuitItem = quitCommand,
                ApplicationItems =
                {
                    install,
                    refresh,
                    openRepository
                }
            };
        }

        private async void PromptInstallPlugin(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Choose a plugin to install...",
                MultiSelect = true,
                Filters =
                {
                    new FileFilter("Plugin (.zip .dll)", ".zip", ".dll")
                }
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                foreach(var file in dialog.Filenames)
                {
                    await Install(file);
                }

                await Refresh();
            }
        }

        private class MetadataViewer : Panel
        {
            public event Action<PluginMetadata> RequestPluginUninstall;
            public event Action<PluginMetadata> RequestPluginInstall;

            protected WeakReference<PluginMetadata> MetadataReference { set; get; } = new WeakReference<PluginMetadata>(null);

            private EmptyMetadataControl emptyMetadataControl = new EmptyMetadataControl();

            public void Update(PluginMetadata metadata)
            {
                if (metadata != null)
                {
                    MetadataReference.SetTarget(metadata);
                    Refresh();
                }
                else
                {
                    base.Content = emptyMetadataControl;
                }
            }

            public async void Refresh()
            {
                if (MetadataReference.TryGetTarget(out var metadata))
                {
                    var contexts = AppInfo.PluginManager.GetLoadedPlugins();
                    var repository = await PluginMetadataCollection.DownloadAsync();
                    
                    bool isInstalled = contexts.Any(t => PluginMetadata.Match(t.GetMetadata(), metadata));
                    bool canUpdate = repository.Any(t => t.PluginVersion > metadata.PluginVersion);

                    var updatableFromRepository = from meta in repository
                        where meta.PluginVersion > metadata.PluginVersion
                        orderby meta.PluginVersion
                        select meta;

                    var actions = new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new Button((sender, e) => RequestPluginUninstall?.Invoke(metadata))
                                {
                                    Text = "Uninstall",
                                    Enabled = isInstalled
                                }
                            },
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new Button((sender, e) => RequestPluginInstall?.Invoke(canUpdate ? updatableFromRepository.First() : metadata))
                                {
                                    Text = canUpdate ? "Update" : "Install",
                                    Enabled = canUpdate || !isInstalled
                                },
                            }
                        }
                    };

                    base.Content = new Scrollable
                    {
                        Content = new StackLayout
                        {
                            Padding = 5,
                            Spacing = 5,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Items =
                            {
                                new AlignedGroup("Name", metadata.Name),
                                new AlignedGroup("Owner", metadata.Owner),
                                new AlignedGroup("Description", metadata.Description),
                                new AlignedGroup("Driver Version", metadata.SupportedDriverVersion.ToString()),
                                new AlignedGroup("Plugin Version", metadata.PluginVersion.ToString()),
                                new LinkButtonGroup("Source Code Repository", metadata.RepositoryUrl, "Show source code"),
                                new LinkButtonGroup("Wiki", metadata.WikiUrl, "Show plugin wiki"),
                                new AlignedGroup("License", metadata.LicenseIdentifier),
                                new StackLayoutItem(null, true),
                                actions
                            }
                        }
                    };
                }
            }

            private class AlignedGroup : Group
            {
                public AlignedGroup(string text, Control content)
                {
                    this.Text = text;
                    this.Content = new StackLayout
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                        Padding = 5,
                        Items =
                        {
                            content
                        }
                    };
                    this.Orientation = Orientation.Horizontal;
                }
            }

            private class LinkButtonGroup : Group
            {
                public LinkButtonGroup(string header, string link, string text = null)
                {
                    var linkButton = new Button
                    {
                        Text = text ?? header,
                        Width = 175
                    };
                    linkButton.Click += (sender, e) => SystemInterop.Open(link);

                    this.Text = header;
                    this.Content = new StackLayout
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                        Items =
                        {
                            linkButton
                        }
                    };
                    this.Orientation = Orientation.Horizontal;
                }
            }

            private class EmptyMetadataControl : Panel
            {
                public EmptyMetadataControl()
                {
                    base.Content = new StackLayout
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new StackLayoutItem("No plugin selected."),
                            new StackLayoutItem(null, true)
                        }
                    };
                }
            }
        }

        private class PluginListBox : ListBox
        {
            public PluginListBox()
            {
                this.DataStore = DisplayedPlugins;
                this.ItemTextBinding = Binding.Property<PluginMetadata, string>(p => p.Name);
            }

            public event Action<DesktopPluginContext> RequestPluginUninstall;
            public event Action<DesktopPluginContext> SelectedPluginChanged;
            public event Action<PluginMetadata> SelectedMetadataChanged;

            public DesktopPluginContext SelectedPlugin { protected set; get; }

            private static readonly Version AppVersion = Assembly.GetEntryAssembly().GetName().Version;

            private readonly ObservableCollection<PluginMetadata> DisplayedPlugins = new ObservableCollection<PluginMetadata>();
            private List<DesktopPluginContext> InstalledPlugins = new List<DesktopPluginContext>();

            public async void Refresh()
            {
                var index = SelectedIndex;

                var installed = from plugin in AppInfo.PluginManager.GetLoadedPlugins()
                    orderby plugin.FriendlyName
                    select plugin;

                var installedMeta = from ctx in installed
                    let meta = ctx.GetMetadata()
                    where meta != null
                    select meta;

                var fetched = from meta in await PluginMetadataCollection.DownloadAsync()
                    where meta.SupportedDriverVersion >= AppVersion
                    where !installedMeta.Any(m => PluginMetadata.Match(m, meta))
                    select meta;

                var metaQuery = from meta in installedMeta.Concat(fetched)
                    orderby meta.Name
                    select meta;

                this.InstalledPlugins = installed.ToList();

                this.DisplayedPlugins.Clear();
                foreach (var meta in metaQuery)
                    this.DisplayedPlugins.Add(meta);

                SelectedIndex = index;
            }

            private void ShowPluginFolder(object sender, EventArgs e)
            {
                SystemInterop.Open(SelectedPlugin.Directory.FullName);
            }

            private void UninstallPlugin(object sender, EventArgs e)
            {
                var result = MessageBox.Show(this, $"Uninstall '{SelectedPlugin.FriendlyName}'?", "Plugin Manager", MessageBoxButtons.YesNo, MessageBoxType.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                    case DialogResult.Ok:
                    {
                        RequestPluginUninstall?.Invoke(SelectedPlugin);
                        break;
                    }
                }
            }

            protected override void OnSelectedValueChanged(EventArgs e)
            {
                base.OnSelectedValueChanged(e);

                var metadata = SelectedValue as PluginMetadata;
                SelectedMetadataChanged?.Invoke(metadata);

                this.SelectedPlugin = SelectedValue is PluginMetadata selected ? InstalledPlugins.FirstOrDefault(p => PluginMetadata.Match(selected, p.GetMetadata())) : null;
                SelectedPluginChanged?.Invoke(this.SelectedPlugin);
            }

            protected override void OnLoadComplete(EventArgs e)
            {
                base.OnLoadComplete(e);
                Refresh();
            }
        }

        private class PluginDropPanel : Panel
        {
            public PluginDropPanel()
            {
                AllowDrop = true;

                DropContent = new StackLayout
                {
                    Items =
                    {
                        new StackLayoutItem(null, true),
                        new StackLayoutItem
                        {
                            Control = dropTextLabel,
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new StackLayoutItem(null, true)
                    }
                };
            }

            private const string DRAG_DROP_SUPPORTED = "Drop plugin here...";
            private const string DRAG_DROP_UNSUPPORTED = "Drag and drop is not supported on this platform.";

            public event Action<string> RequestPluginInstall;

            private Control content;
            public new Control Content
            {
                set
                {
                    this.content = value;
                    base.Content = this.Content;
                }
                get => this.content;
            }

            private readonly Label dropTextLabel = new Label
            {
                Text = DRAG_DROP_SUPPORTED
            };

            protected StackLayout DropContent { get; }

            protected override void OnLoadComplete(EventArgs e)
            {
                base.OnLoadComplete(e);
                base.Content = this.Content;
            }

            protected override void OnDragEnter(DragEventArgs args)
            {
                base.OnDragEnter(args);
                try
                {
                    if (args.Data.ContainsUris)
                    {
                        // Skip if running on bugged platform
                        // https://github.com/picoe/Eto/issues/1812
                        if (args.Data.Uris != null && args.Data.Uris?.Length > 0)
                        {
                            var uriList = args.Data.Uris;
                            var supportedType = uriList.All(uri =>
                            {
                                if (uri.IsFile && File.Exists(uri.LocalPath))
                                {
                                    var fileInfo = new FileInfo(uri.LocalPath);
                                    return fileInfo.Extension switch
                                    {
                                        ".zip" => true,
                                        ".dll" => true,
                                        _ => false
                                    };
                                }
                                return false;
                            });
                            if (supportedType)
                            {
                                dropTextLabel.Text = DRAG_DROP_SUPPORTED;
                                args.Effects = DragEffects.Copy;
                            }
                        }
                        else
                        {
                            dropTextLabel.Text = DRAG_DROP_UNSUPPORTED;
                            args.Effects = DragEffects.None;
                        }
                        base.Content = DropContent;
                    }
                }
                catch (Exception ex)
                {
                    ShowException(ex);
                }
            }

            protected override void OnDragLeave(DragEventArgs args)
            {
                base.OnDragLeave(args);
                base.Content = this.Content;
            }

            protected override void OnDragDrop(DragEventArgs args)
            {
                base.OnDragDrop(args);
                try
                {
                    if (args.Data.ContainsUris && args.Data.Uris != null && args.Data.Uris.Length > 0)
                    {
                        var uriList = args.Data.Uris;
                        foreach (var uri in uriList)
                        {
                            if (uri.IsFile && File.Exists(uri.LocalPath))
                            {
                                RequestPluginInstall?.Invoke(uri.LocalPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowException(ex);
                }
            }

            protected static void ShowException(Exception exception)
            {
                MessageBox.Show(
                    exception.ToString(),
                    $"Error: {exception.GetType().Name}",
                    MessageBoxButtons.OK,
                    MessageBoxType.Error
                );
            }
        }
    }
}