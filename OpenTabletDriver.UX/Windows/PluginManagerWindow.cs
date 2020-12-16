using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Windows
{
    public class PluginManagerWindow : Form
    {
        private readonly ObservableCollection<PluginInfo> pluginList = new ObservableCollection<PluginInfo>();
        private readonly Panel panel; // Windows can't receive drag n drop events from Form, so use Panel as proxy
        private readonly ListBox pluginListBox;
        private readonly StackLayout split;
        private readonly StackLayout dropArea;
        private readonly Label dragInstruction = new Label
        {
            Text = "Drag and drop plugin zips/dlls to install!   o(≧▽≦)o",
            VerticalAlignment = VerticalAlignment.Center,
            Size = new Size(-1, 30)
        };

        private const string dragDropSupported = "Drop plugin zip/dll here... (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧";
        private const string dragDropNotSupported = "Oh no! Drag and drop not supported! ＼(º □ º l|l)/";

        public PluginManagerWindow()
        {
            this.Title = "Plugin Manager";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.Size = new Size(700, 350);
            this.AllowDrop = true;
            this.Menu = ConstructMenu();

            UpdateList();

            this.panel = new Panel
            {
                AllowDrop = true
            };

            this.dropArea = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem
                    {
                        Control = dragDropSupported,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackLayoutItem(null, true)
                }
            };

            var showPluginFolderCmd = new Command();
            showPluginFolderCmd.Executed += (_, _) => ShowPluginFolder();

            var uninstallPluginCmd = new Command();
            uninstallPluginCmd.Executed += (_, _) => UninstallPlugin();

            var contextMenu = new ContextMenu();
            var showPluginFolderMenu = contextMenu.Items.Add(showPluginFolderCmd);
            var uninstallPluginMenu = contextMenu.Items.Add(uninstallPluginCmd);

            showPluginFolderMenu.Text = "Show in folder...";
            uninstallPluginMenu.Text = "Uninstall";

            this.pluginListBox = new ListBox
            {
                DataStore = pluginList,
                ItemTextBinding = Binding.Property<PluginInfo, string>(p => p.Name)
            };

            this.pluginListBox.SelectedIndexChanged += (_, _) =>
            {
                var index = this.pluginListBox.SelectedIndex;
                if (index >= 0 && index < pluginList.Count)
                    this.pluginListBox.ContextMenu = contextMenu;
                else
                    this.pluginListBox.ContextMenu = null;
            };

            this.split = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(pluginListBox, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(dragInstruction, HorizontalAlignment.Center, false)
                },
                Orientation = Orientation.Vertical
            };

            this.panel.Content = this.split;
            this.Content = this.panel;

            this.panel.DragEnter += ShowDrop;
            this.panel.DragLeave += LeaveDrop;
            this.panel.DragDrop += DragDropPluginInstall;
        }

        private void ShowDrop(object _, DragEventArgs args)
        {
            try
            {
                if (args.Data.ContainsUris)
                {
                    // Skip if running on bugged platform
                    // https://github.com/picoe/Eto/issues/1812
                    if (args.Data.Uris != null && args.Data.Uris.Length > 0)
                    {
                        var uriList = args.Data.Uris;
                        var supportedType = uriList.All(uri =>
                        {
                            if (uri.IsFile && File.Exists(uri.LocalPath))
                            {
                                var fileInfo = new FileInfo(uri.LocalPath);
                                if (fileInfo.Extension == ".zip" || fileInfo.Extension == ".dll")
                                {
                                    return true;
                                }
                            }
                            return false;
                        });
                        if (supportedType)
                        {
                            dropArea.Items[1].Control = dragDropSupported;
                            this.panel.Content = dropArea;
                            args.Effects = DragEffects.Copy;
                        }
                    }
                    else
                    {
                        dropArea.Items[1].Control = dragDropNotSupported;
                        this.panel.Content = dropArea;
                        args.Effects = DragEffects.None;
                    }
                }
            }
            catch {}
        }

        private void LeaveDrop(object _, DragEventArgs args)
        {
            this.panel.Content = split;
        }

        private void DragDropPluginInstall(object _, DragEventArgs args)
        {
            try
            {
                if (args.Data.ContainsUris && args.Data.Uris != null && args.Data.Uris.Length > 0)
                {
                    var uriList = args.Data.Uris;
                    bool updateQueued = false;
                    foreach (var uri in uriList)
                    {
                        if (uri.IsFile && File.Exists(uri.LocalPath))
                        {
                            var result = AppInfo.PluginManager.InstallPlugin(uri.LocalPath);
                            if (result == PluginStateResult.UpdateQueued)
                                updateQueued = true;
                        }
                    }
                    LoadNewPlugins().ConfigureAwait(false);
                    if (updateQueued)
                        MessageBox.Show(this, "Plugin updates will be applied after restarting OTD.", "Plugin Manager");
                }
            }
            catch {}
        }

        private void FileDialogPluginInstall()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Choose plugin to install...",
                Filters =
                {
                    new FileFilter("OTD Plugin (.zip .dll)", ".zip", ".dll")
                },
                MultiSelect = true
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                bool updateQueued = false;
                foreach(var file in dialog.Filenames)
                {
                    var result = AppInfo.PluginManager.InstallPlugin(file);
                    switch (result)
                    {
                        case PluginStateResult.UpdateQueued:
                            updateQueued = true;
                            break;
                        case PluginStateResult.AlreadyQueued:
                            MessageBox.Show(this, $"{Path.GetFileNameWithoutExtension(file)} already have an enqueued process. Please restart OTD first.",
                                            "Plugin Manager", MessageBoxType.Warning);
                            break;
                    }
                    if (result == PluginStateResult.UpdateQueued)
                        updateQueued = true;
                }

                if (updateQueued)
                {
                    MessageBox.Show(this, "Plugin updates will be applied after restarting OTD.", "Plugin Manager");
                }

                LoadNewPlugins().ConfigureAwait(false);
            }
        }

        private void UninstallPlugin()
        {
            var plugin = pluginList[pluginListBox.SelectedIndex];
            var result = MessageBox.Show(this, $"Uninstall '{plugin.Name}'?", "Plugin Manager", MessageBoxButtons.YesNo, MessageBoxType.Question);
            if (result == DialogResult.Ok || result == DialogResult.Yes)
            {
                switch (AppInfo.PluginManager.UninstallPlugin(plugin))
                {
                    case PluginStateResult.UninstallQueued:
                        MessageBox.Show(this, $"{plugin.Name} will be completely uninstalled after restarting OTD.", "Plugin Manager", MessageBoxType.Information);
                        break;
                    case PluginStateResult.AlreadyQueued:
                        MessageBox.Show(this, $"{plugin.Name} is already queued for uninstall.", "Plugin Manager", MessageBoxType.Warning);
                        break;
                    case PluginStateResult.Error:
                        MessageBox.Show(this, $"{plugin.Name} failed to uninstall", "Plugin Manager", MessageBoxType.Error);
                        break;
                }
            }
        }

        private async Task LoadNewPlugins()
        {
            await App.Driver.Instance.LoadPlugins();
            AppInfo.PluginManager.LoadPlugins(new DirectoryInfo(AppInfo.Current.PluginDirectory));
            await (Application.Instance.MainForm as MainForm).Refresh();
            UpdateList();
        }

        private MenuBar ConstructMenu()
        {
            var installPlugin = new Command { MenuText = "Install plugin..." };
            installPlugin.Executed += (_, _) => FileDialogPluginInstall();

            var pluginsRepository = new Command { MenuText = "Get more plugins..." };
            pluginsRepository.Executed += (_, _) => SystemInterop.Open(App.PluginRepositoryUrl);

            var loadPlugins = new Command { MenuText = "Manually load plugins..." };
            loadPlugins.Executed += (_, _) => ManualLoad();

            var quitCommand = new Command { MenuText = "Exit" };
            quitCommand.Executed += (_, _) => this.Close();

            return new MenuBar()
            {
                ApplicationItems =
                {
                    installPlugin,
                    loadPlugins,
                    pluginsRepository
                },
                QuitItem = quitCommand
            };
        }

        private void ManualLoad()
        {
            var result = MessageBox.Show(this, "Manually loading plugins are not recommended. Are you sure you want to continue?", "Plugin Manager",
                                          MessageBoxButtons.YesNo, MessageBoxType.Warning);
            if (result == DialogResult.Yes || result == DialogResult.Ok)
                LoadNewPlugins().ConfigureAwait(false);
        }

        private void ShowPluginFolder()
        {
            var plugin = pluginList[pluginListBox.SelectedIndex];
            if (Directory.Exists(plugin.Path))
                SystemInterop.Open(plugin.Path);
            else
                SystemInterop.Open(AppInfo.Current.PluginDirectory);
        }

        private void UpdateList()
        {
            pluginList.Clear();
            foreach (var name in AppInfo.PluginManager.GetLoadedPluginInfos().OrderBy(x => x))
                pluginList.Add(name);
        }
    }
}