using System;
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
        public PluginManagerWindow()
        {
            this.Title = "Plugin Manager";
            this.Icon = App.Logo.WithSize(App.Logo.Size);
            this.Size = new Size(700, 350);
            this.AllowDrop = true;

            this.Menu = ConstructMenu();
            this.Content = dropPanel;

            dropPanel.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(pluginList, true),
                    new StackLayoutItem
                    {
                        Control = new Panel
                        {
                            Content = dragInstruction,
                            Padding = 5
                        },
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                },
                Orientation = Orientation.Vertical
            };

            dropPanel.RequestPluginInstall += async (path) => await InstallPlugin(path);
            pluginList.RequestPluginUninstall += async (ctx) => await UninstallPlugin(ctx);

            _ = Refresh();
        }

        private readonly PluginDropPanel dropPanel = new PluginDropPanel();
        private readonly PluginListBox pluginList = new PluginListBox();
        private readonly Label dragInstruction = new Label
        {
            Text = "Drag and drop plugins to install!   o(≧▽≦)o",
            VerticalAlignment = VerticalAlignment.Center
        };

        public async Task Refresh()
        {
            await App.Driver.Instance.LoadPlugins();
            AppInfo.PluginManager.Load();

            (Application.Instance.MainForm as MainForm).Refresh();
            pluginList.Refresh();
        }

        protected async Task InstallPlugin(string path)
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

        protected async Task UninstallPlugin(DesktopPluginContext context)
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
                    await InstallPlugin(file);
                }

                await Refresh();
            }
        }

        private class PluginListBox : ListBox
        {
            public PluginListBox()
            {
                this.DataStore = DisplayedPlugins;
                this.ItemTextBinding = Binding.Property<DesktopPluginContext, string>(p => p.FriendlyName);

                this.ItemContextMenu = new ContextMenu
                {
                    Items =
                    {
                        new Command(ShowPluginFolder)
                        {
                            MenuText = "Show in folder..."
                        },
                        new Command(UninstallPlugin)
                        {
                            MenuText = "Uninstall"
                        }
                    }
                };
            }

            public event Action<DesktopPluginContext> RequestPluginUninstall;

            private readonly ObservableCollection<DesktopPluginContext> DisplayedPlugins = new ObservableCollection<DesktopPluginContext>();

            private ContextMenu ItemContextMenu { get; }

            public DesktopPluginContext SelectedPlugin { protected set; get; }

            public void Refresh()
            {
                var plugins = from plugin in AppInfo.PluginManager.GetLoadedPlugins()
                    orderby plugin.Name
                    select plugin;

                this.DisplayedPlugins.Clear();
                foreach (var ctx in plugins)
                    this.DisplayedPlugins.Add(ctx);

                OnSelectedIndexChanged(new EventArgs());
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

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);

                var index = base.SelectedIndex;
                if (index >= 0 && index < DisplayedPlugins.Count)
                {
                    this.SelectedPlugin = DisplayedPlugins[index];
                    base.ContextMenu = ItemContextMenu;
                }
                else
                {
                    this.SelectedPlugin = null;
                    base.ContextMenu = null;
                }
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

            private const string DRAG_DROP_SUPPORTED = "Drop plugin zip/dll here... (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧";
            private const string DRAG_DROP_UNSUPPORTED = "Oh no! Drag and drop not supported! ＼(º □ º l|l)/";

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