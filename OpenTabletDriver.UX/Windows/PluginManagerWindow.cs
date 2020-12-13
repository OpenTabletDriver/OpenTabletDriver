using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.Windows
{
    public class PluginManagerWindow : Form
    {
        public PluginManagerWindow()
        {
            this.Title = "Plugin Manager";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.MinimumSize = new Size(700, 350);
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

            var contextCommand = new Command();
            contextCommand.Executed += (_, _) => ShowPluginFolder();

            var contextMenu = new ContextMenu();
            var showPluginFolder = contextMenu.Items.Add(contextCommand);
            showPluginFolder.Text = "Show in folder...";

            this.pluginListBox = new ListBox
            {
                DataStore = pluginList
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
                    foreach (var uri in uriList)
                    {
                        if (uri.IsFile && File.Exists(uri.LocalPath))
                        {
                            InstallFile(uri.LocalPath);
                        }
                    }
                    LoadNewPlugins().ConfigureAwait(false);
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
                foreach(var file in dialog.Filenames)
                {
                    InstallFile(file);
                }
                LoadNewPlugins().ConfigureAwait(false);
            }
        }

        private static void InstallFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            switch (fileInfo.Extension)
            {
                case ".zip":
                {
                        var path = Path.Join(AppInfo.Current.PluginDirectory, fileInfo.Name.Replace(".zip", string.Empty));
                        Log.Write("Plugin", $"Installing plugin zip: '{fileInfo.Name}'");
                        if (Directory.Exists(path))
                        {
                            Log.Write("Plugin", updateNotSupported, LogLevel.Error);
                            MessageBox.Show(updateNotSupported, MessageBoxType.Error);
                        }
                        else
                            ZipFile.ExtractToDirectory(filePath, path, true);
                    break;
                }
                case ".dll":
                {
                    Log.Write("Plugin", $"Installing plugin dll: '{fileInfo.Name}'");
                    if (File.Exists(Path.Join(AppInfo.Current.PluginDirectory, filePath)))
                    {
                        Log.Write("Plugin", updateNotSupported, LogLevel.Error);
                        MessageBox.Show(updateNotSupported, MessageBoxType.Error);
                    }
                    else
                    {
                        var name = Path.GetFileName(filePath);
                        var dest = Path.Join(AppInfo.Current.PluginDirectory, name);
                        File.Copy(filePath, dest, true);
                    }
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
            var result = MessageBox.Show("Manually loading plugins are not recommended. Are you sure you want to continue?",
                                          MessageBoxButtons.YesNo, MessageBoxType.Warning);
            if (result == DialogResult.Yes || result == DialogResult.Ok)
                LoadNewPlugins().ConfigureAwait(false);
        }

        private void ShowPluginFolder()
        {
            var plugin = pluginList[pluginListBox.SelectedIndex];
            var path = Path.Join(AppInfo.Current.PluginDirectory, plugin);
            if (Directory.Exists(path))
                SystemInterop.Open(path);
            else
                SystemInterop.Open(AppInfo.Current.PluginDirectory);
        }

        private void UpdateList()
        {
            pluginList.Clear();
            foreach (var name in AppInfo.PluginManager.GetLoadedPluginNames().OrderBy(x => x))
            {
                pluginList.Add(name);
            }
        }

        private readonly ObservableCollection<string> pluginList = new ObservableCollection<string>();
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
        private const string updateNotSupported = "Updating plugins during runtime are not supported";
    }
}