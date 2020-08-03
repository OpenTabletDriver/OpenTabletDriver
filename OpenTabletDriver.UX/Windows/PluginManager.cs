using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using TabletDriverLib;

namespace OpenTabletDriver.UX.Windows
{
    public class PluginManager : Form
    {
        public PluginManager()
        {
            Title = "Plugin Manager";
            ClientSize = new Size(960 - 100, 730 - 100);
            MinimumSize = new Size(960 - 100, 730 - 100);
            Icon = App.Logo.WithSize(App.Logo.Size);

            _pluginList.SelectedIndexChanged += (sender, e) =>
            {
                if (_pluginList.SelectedIndex >= 0 && _pluginList.SelectedIndex <= PluginFiles.Count)
                    SelectedFile = PluginFiles[_pluginList.SelectedIndex];
            };

            this.DragEnter += (sender, e) => e.Effects = e.Data.ContainsUris ? DragEffects.Move : DragEffects.None;
            this.DragDrop += async (sender, e) => 
            {
                if (e.Data.ContainsUris)
                {
                    var files = from uri in e.Data.Uris
                        where uri.IsFile
                        let file = new FileInfo(uri.AbsolutePath)
                        where file.Extension == ".dll"
                        select file;

                    await InstallPlugins(files);                    
                }
            };

            this.LoadComplete += async (sender, e) => 
            {
                var appInfo = await App.DriverDaemon.InvokeAsync(d => d.GetApplicationInfo());
                AddPlugins(Directory.GetFiles(appInfo.PluginDirectory));

                Content = new Splitter
                {
                    Orientation = Orientation.Horizontal,
                    Panel1MinimumSize = 200,
                    Panel1 = new Scrollable { Content = _pluginList },
                    Panel2 = new Scrollable
                    { 
                        Content = _fileControls,
                        Padding = new Padding(5)
                    },
                };

                var quitCommand = new Command { MenuText = "Exit", Shortcut = Application.Instance.CommonModifier | Keys.W };
                quitCommand.Executed += (sender, e) => this.Close();

                var addCommand = new Command { MenuText = "Install", ToolBarText = "Install plugin..." };
                addCommand.Executed += async (sender, e) => 
                {
                    var fd = new OpenFileDialog
                    {
                        Title = "Select a OpenTabletDriver plugin file",
                        MultiSelect = true,
                        Filters =
                        {
                            new FileFilter("OpenTabletDriver Plugin (*.dll)", "*.dll")
                        }
                    };
                    switch (fd.ShowDialog(this))
                    {
                        case DialogResult.Ok:
                        case DialogResult.Yes:
                            var files = from filePath in fd.Filenames
                                select new FileInfo(filePath);
                            await InstallPlugins(files);
                            break;
                    }
                };

                var deleteCommand = new Command { ToolBarText = "Uninstall plugin" };
                deleteCommand.Executed += (sender, e) => DeletePlugin(SelectedFile);

                Menu = new MenuBar
                {
                    Items = 
                    {
                        new ButtonMenuItem
                        {
                            Text = "&File",
                            Items = 
                            {
                                addCommand
                            }
                        }
                    },
                    QuitItem = quitCommand
                };

                ToolBar = new ToolBar
                {
                    Items = 
                    {
                        addCommand,
                        deleteCommand
                    }
                };
            };
        }

        private async Task InstallPlugins(IEnumerable<FileInfo> files)
        {
            var appInfo = await App.DriverDaemon.InvokeAsync(d => d.GetApplicationInfo());
            foreach (var file in files)
            {
                var newPath = Path.Join(appInfo.PluginDirectory, file.Name);
                file.MoveTo(newPath, false);
                AddPlugins(file.FullName);

                TypeManager.AddPlugin(file);
                await App.DriverDaemon.InvokeAsync(d => d.ImportPlugin(file.FullName));
            }
        }

        private void AddPlugins(params string[] filePaths)
        {
            foreach (var filePath in filePaths.OrderBy(f => f))
            {
                var file = new FileInfo(filePath);
                var name = file.Name.Replace(file.Extension, string.Empty);
                _pluginList.Items.Add(name);
                PluginFiles.Add(file);
            }
        }

        private void DeletePlugin(FileInfo file)
        {
            var name = file.Name.Replace(file.Extension, string.Empty);
            var item = _pluginList.Items.FirstOrDefault(f => f.Text == name);
            _pluginList.Items.Remove(item);
            
            PluginFiles.Remove(file);
            file.Delete();
            
            if (_pluginList.SelectedValue == item)
                _pluginList.SelectedIndex = PluginFiles.Count - 1;
        }

        private IEnumerable<Control> GenerateControls(FileInfo file)
        {
            yield return new GroupBox
            {
                Text = "Plugin Name",
                Padding = App.GroupBoxPadding,
                Content = file.Name.Replace(file.Extension, string.Empty)
            };

            var isEnabledBox = new CheckBox
            {
                Text = "Enabled"
            };
            isEnabledBox.CheckedBinding.Bind(
                () => file.Extension == ".dll",
                (enabled) => file.MoveTo(file.FullName.Replace(file.Extension, (enabled ?? false) ? ".dll" : ".disabled"))
            );
            yield return isEnabledBox;
        }

        private ListBox _pluginList = new ListBox();
        private StackLayout _fileControls = new StackLayout
        {
            Padding = new Padding(5),
            Spacing = 5
        };

        private List<FileInfo> PluginFiles { set; get; } = new List<FileInfo>();

        private FileInfo _selectedFile;
        private FileInfo SelectedFile
        {
            set
            {
                _selectedFile = value;
                // TODO: update settings
                _fileControls.Items.Clear();
                foreach (var control in GenerateControls(SelectedFile))
                    _fileControls.Items.Add(new StackLayoutItem(control, HorizontalAlignment.Stretch));
            }
            get => _selectedFile;
        }
    }
}