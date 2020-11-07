using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows
{
    public class PluginManagerWindow : Form
    {
        public PluginManagerWindow()
        {
            this.Title = "Plugin Manager";
            this.Icon = App.Logo.WithSize(App.Logo.Size);

            this.pluginNameList = PluginManager.GetLoadedPluginNames().ToList();

            this.AllowDrop = true;

            this.panel = new Panel()
            {
                MinimumSize = new Size(700, 280)
            };

            this.dropArea = new StackLayout()
            {
                Items =
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem()
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

            this.pluginList = new ListBox()
            {
                DataStore = pluginNameList
            };

            this.dragInstruction = new StackLayout()
            {
                Items =
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem()
                    {
                        Control = "Drag and drop plugin zips/dlls to install!   o(≧▽≦)o",
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackLayoutItem(null, true)
                }
            };

            split.Panel1 = pluginList;
            split.Panel2 = dragInstruction;

            this.panel.Content = split;
            this.Content = panel;

            this.DragEnter += ShowDrop;
            this.DragLeave += LeaveDrop;
            this.dropArea.DragDrop += PluginInstall;
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
                                if (fileInfo.Extension == "zip" || fileInfo.Extension == "dll")
                                {
                                    return true;
                                }
                            }
                            return false;
                        });
                        if (supportedType)
                        {
                            dropArea.Items[1].Control = dragDropSupported;
                            var size = split.Panel2.Size;
                            this.panel.Content = dropArea;
                            split.Panel2.Size = size;
                            args.Effects = DragEffects.Copy;
                        }
                    }
                    else
                    {
                        dropArea.Items[1].Control = dragDropNotSupported;
                        var size = split.Panel2.Size;
                        this.panel.Content = dropArea;
                        split.Panel2.Size = size;
                    }
                }
            }
            catch {}
        }

        private void LeaveDrop(object _, DragEventArgs args)
        {
            var size = split.Panel2.Size;
            this.panel.Content = split;
            split.Panel2.Size = size;
        }

        private void PluginInstall(object _, DragEventArgs args)
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
                            var fileInfo = new FileInfo(uri.LocalPath);
                            switch (fileInfo.Extension)
                            {
                                case "zip":
                                    ZipFile.ExtractToDirectory(uri.LocalPath, Path.Join(AppInfo.Current.PluginDirectory, fileInfo.Name), true);
                                    PluginManager.LoadPlugins();
                                    pluginList.DataStore = pluginNameList = PluginManager.GetLoadedPluginNames().ToList();
                                    break;
                                case "dll":
                                    File.Copy(uri.LocalPath, AppInfo.Current.PluginDirectory, true);
                                    PluginManager.LoadPlugins();
                                    pluginList.DataStore = pluginNameList = PluginManager.GetLoadedPluginNames().ToList();
                                    break;
                            }
                        }
                    }
                }
            }
            catch {}
        }

        private List<string> pluginNameList;
        private readonly Panel panel;
        private readonly Splitter split;
        private readonly ListBox pluginList;
        private readonly StackLayout dragInstruction;
        private readonly StackLayout dropArea;
        private const string dragDropSupported = "Drop plugin zip here... (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧";
        private const string dragDropNotSupported = "Oh no! Drag and drop not supported! ＼(º □ º l|l)/";
    }
}