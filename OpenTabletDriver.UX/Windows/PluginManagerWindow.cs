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
            this.ClientSize = new Size(700, 280);
            this.AllowDrop = true;
            this.Menu = ConstructMenu();

            this.pluginList = PluginManager.GetLoadedPluginNames().OrderBy(p => p).ToList();

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
                },
                AllowDrop = true
            };

            this.split = new Splitter()
            {
                Orientation = Orientation.Vertical,
                Panel1MinimumSize = 240,
                Panel2MinimumSize = 40,
                FixedPanel = SplitterFixedPanel.Panel2
            };

            this.pluginListBox = new ListBox
            {
                DataStore = pluginList
            };

            this.dragInstruction = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem
                    {
                        Control = "Drag and drop plugin zips/dlls to install!   o(≧▽≦)o",
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackLayoutItem(null, true)
                }
            };

            split.Panel1 = pluginListBox;
            split.Panel2 = dragInstruction;

            this.panel.Content = split;
            this.Content = panel;

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
                            this.panel.Content = dropArea;
                            args.Effects = DragEffects.Copy;
                        }
                    }
                    else
                    {
                        dropArea.Items[1].Control = dragDropNotSupported;
                        this.panel.Content = dropArea;
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
            dialog.ShowDialog(this);
            foreach(var file in dialog.Filenames)
            {
                InstallFile(file);
            }
            if (dialog.Filenames.Any())
            {
                LoadNewPlugins().ConfigureAwait(false);
            }
        }

        private void InstallFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            Log.Write("Plugin", $"Installing plugin: '{fileInfo.Name}'");
            switch (fileInfo.Extension)
            {
                case ".zip":
                    ZipFile.ExtractToDirectory(filePath,
                        Path.Join(AppInfo.Current.PluginDirectory, fileInfo.Name.Replace(".zip", string.Empty)),
                        true);
                    break;
                case ".dll":
                    File.Copy(filePath, AppInfo.Current.PluginDirectory, true);
                    break;
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

        private List<string> pluginList;
        private readonly Panel panel;
        private readonly Splitter split;
        private readonly ListBox pluginListBox;
        private readonly StackLayout dragInstruction;
        private readonly StackLayout dropArea;
        private const string dragDropSupported = "Drop plugin zip/dll here... (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧";
        private const string dragDropNotSupported = "Oh no! Drag and drop not supported! ＼(º □ º l|l)/";
    }
}