using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Native;
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

            this.pluginList = PluginManager.GetLoadedPluginNames().OrderBy(p => p).ToList();

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
            contextMenu.Opening += (_, _) =>
            {
                var index = pluginListBox.SelectedIndex;
                if (index >= 0 && index < pluginList.Count)
                    showPluginFolder.Visible = true;
                else
                    showPluginFolder.Visible = false;
            };

            this.pluginListBox = new ListBox
            {
                DataStore = pluginList,
                ContextMenu = contextMenu
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

            this.Content = split;

            this.DragEnter += ShowDrop;
            this.DragLeave += LeaveDrop;
            this.DragDrop += DragDropPluginInstall;
        }

        private void ShowDrop(object _, DragEventArgs args)
        {
            try
            {
                if (args.Data.ContainsUris)
                {
                    // Skip if running on bugged platform
                    // https://github.com/picoe/Eto/issues/1812
                    if (args.Data.Uris != null && args.Data.Uris.Count() > 0)
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
                            this.Content = dropArea;
                            args.Effects = DragEffects.Copy;
                        }
                    }
                    else
                    {
                        dropArea.Items[1].Control = dragDropNotSupported;
                        this.Content = dropArea;
                        args.Effects = DragEffects.None;
                    }
                }
            }
            catch {}
        }

        private void LeaveDrop(object _, DragEventArgs args)
        {
            this.Content = split;
        }

        private void DragDropPluginInstall(object _, DragEventArgs args)
        {
            try
            {
                if (args.Data.ContainsUris && args.Data.Uris != null && args.Data.Uris.Count() > 0)
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

        private void FileDialogPluginInstall(object _, EventArgs args)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Choose plugin to install...",
                Filters =
                {
                    new FileFilter("OTD Plugin (.zip)", ".zip"),
                    new FileFilter("Deprecated Plugin (.dll)", ".dll")
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

        private void InstallFile(string filePath)
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
                        File.Copy(filePath, AppInfo.Current.PluginDirectory, true);
                    break;
                }
            }
        }

        private async Task LoadNewPlugins()
        {
            await App.Driver.Instance.LoadPlugins();
            await PluginManager.LoadPluginsAsync();
            await MainForm.FormInstance.ReloadUX();
            pluginListBox.DataStore = pluginList = PluginManager.GetLoadedPluginNames().OrderBy(x => x).ToList();
        }

        private MenuBar ConstructMenu()
        {
            var installPlugin = new Command { MenuText = "Install plugin..." };
            installPlugin.Executed += FileDialogPluginInstall;

            var pluginsRepository = new Command { MenuText = "Get more plugins..." };
            pluginsRepository.Executed += (_, o) => SystemInfo.Open(App.PluginRepositoryUrl);

            var loadPlugins = new Command { MenuText = "Load plugins" };
            loadPlugins.Executed += (_, o) => LoadNewPlugins().ConfigureAwait(false);

            var quitCommand = new Command { MenuText = "Exit" };
            quitCommand.Executed += (_, o) => this.Close();

            return new MenuBar()
            {
                ApplicationItems =
                {
                    installPlugin,
                    pluginsRepository
                },
                QuitItem = quitCommand
            };
        }

        private void ShowPluginFolder()
        {
            var plugin = pluginList[pluginListBox.SelectedIndex];
            var path = Path.Join(AppInfo.Current.PluginDirectory, plugin);
            if (Directory.Exists(path))
                SystemInfo.Open(path);
            else
                SystemInfo.Open(AppInfo.Current.PluginDirectory);
        }

        private List<string> pluginList;
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