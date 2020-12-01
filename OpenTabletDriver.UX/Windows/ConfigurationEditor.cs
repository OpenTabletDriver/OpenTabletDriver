using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using HidSharp;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Windows
{
    using static ParseTools;

    public class ConfigurationEditor : Form
    {
        public ConfigurationEditor()
        {
            base.Title = "Configuration Editor";
            base.ClientSize = new Size(960 - 50, 730 - 50);
            base.MinimumSize = new Size(960 - 50, 730 - 50);
            base.Icon = App.Logo.WithSize(App.Logo.Size);

            // Main Controls
            this.configList.SelectedIndexChanged += (sender, e) =>
            {
                if (this.configList.SelectedIndex >= 0)
                    SelectedConfiguration = Configurations[this.configList.SelectedIndex];
            };

            base.Content = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Panel1MinimumSize = 200,
                Panel1 = this.configList,
                Panel2 = new Scrollable
                {
                    Content = this.configControls,
                    Padding = new Padding(5)
                },
            };

            // MenuBar Commands
            var quitCommand = new Command { MenuText = "Close", Shortcut = Application.Instance.CommonModifier | Keys.W };
            quitCommand.Executed += (sender, e) => Close();

            var loadDirectory = new Command { MenuText = "Load configurations...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            loadDirectory.Executed += (sender, e) => LoadConfigurationsDialog();

            var saveDirectory = new Command { MenuText = "Save configurations", Shortcut = Application.Instance.CommonModifier | Keys.S };
            saveDirectory.Executed += (sender, e) => WriteConfigurations(Configurations, new DirectoryInfo(AppInfo.Current.ConfigurationDirectory));

            var saveToDirectory = new Command { MenuText = "Save configurations to...", Shortcut = Application.Instance.CommonModifier | Application.Instance.AlternateModifier | Keys.S };
            saveToDirectory.Executed += (sender, e) => SaveConfigurationsDialog();

            var newConfiguration = new Command { ToolBarText = "New configuration", Shortcut = Application.Instance.CommonModifier | Keys.N };
            newConfiguration.Executed += (sender, e) => CreateNewConfiguration();

            var deleteConfiguration = new Command { ToolBarText = "Delete configuration" };
            deleteConfiguration.Executed += (sender, e) => DeleteConfiguration(SelectedConfiguration);

            var generateConfiguration = new Command { ToolBarText = "Generate configuration..." };
            generateConfiguration.Executed += async (sender, e) => await GenerateConfiguration();

            // Menu
            base.Menu = new MenuBar
            {
                Items =
                {
                    // File submenu
                    new ButtonMenuItem
                    {
                        Text = "&File",
                        Items =
                        {
                            loadDirectory,
                            saveDirectory,
                            saveToDirectory
                        }
                    }
                },
                QuitItem = quitCommand
            };

            base.ToolBar = new ToolBar
            {
                Items =
                {
                    newConfiguration,
                    deleteConfiguration,
                    generateConfiguration
                }
            };

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var appinfo = await App.Driver.Instance.GetApplicationInfo();
            var configDir = new DirectoryInfo(appinfo.ConfigurationDirectory);
            var sortedConfigs = from config in ReadConfigurations(configDir)
                                orderby config.Name
                                select config;
            Configurations = new List<TabletConfiguration>(sortedConfigs);
            this.configList.SelectedIndex = 0;
        }

        private List<TabletConfiguration> configs;
        private List<TabletConfiguration> Configurations
        {
            set
            {
                this.configs = value;
                this.configList.Items.Clear();
                foreach (var config in Configurations)
                    this.configList.Items.Add(config.Name);
            }
            get => this.configs;
        }

        private TabletConfiguration selected;
        private TabletConfiguration SelectedConfiguration
        {
            set
            {
                this.selected = value;
                Refresh();
            }
            get => this.selected;
        }

        private ListBox configList = new ListBox();
        private StackView configControls = new StackView();

        private static readonly Regex NameRegex = new Regex("(?<Manufacturer>.+?) (?<TabletName>.+?)$");

        private List<TabletConfiguration> ReadConfigurations(DirectoryInfo dir)
        {
            var configs = from file in dir.GetFiles("*.json", SearchOption.AllDirectories)
                select Serialization.Deserialize<TabletConfiguration>(file);
            return new List<TabletConfiguration>(configs);
        }

        private void WriteConfigurations(IEnumerable<TabletConfiguration> configs, DirectoryInfo dir)
        {
            foreach (var config in configs)
            {
                var match = NameRegex.Match(config.Name);
                var manufacturer = match.Groups["Manufacturer"].Value;
                var tabletName = match.Groups["TabletName"].Value;

                var path = Path.Join(dir.FullName, manufacturer, string.Format("{0}.json", tabletName));
                var file = new FileInfo(path);
                if (!file.Directory.Exists)
                    file.Directory.Create();
                Serialization.Serialize(file, config);
            }
        }

        private void LoadConfigurationsDialog()
        {
            var folderDialog = new SelectFolderDialog
            {
                Title = "Open configuration folder..."
            };
            switch (folderDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var dir = new DirectoryInfo(folderDialog.Directory);
                    Configurations = ReadConfigurations(dir);
                    break;
            }
        }

        private void SaveConfigurationsDialog()
        {
            var folderDialog = new SelectFolderDialog
            {
                Title = "Save configurations to..."
            };
            switch (folderDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var dir = new DirectoryInfo(folderDialog.Directory);
                    WriteConfigurations(Configurations, dir);
                    break;
            }
        }

        private void CreateNewConfiguration()
        {
            var newTablet = new TabletConfiguration
            {
                Name = "New Tablet"
            };
            Configurations = Configurations.Append(newTablet).ToList();
            this.configList.SelectedIndex = Configurations.IndexOf(newTablet);
        }

        private void DeleteConfiguration(TabletConfiguration config)
        {
            Configurations = Configurations.Where(c => c != config).ToList();
            if (SelectedConfiguration == config)
                this.configList.SelectedIndex = Configurations.Count - 1;
        }

        private async Task GenerateConfiguration()
        {
            var dialog = new DeviceListDialog();
            if (await dialog.ShowModalAsync() is HidDevice device)
            {
                try
                {
                    var generatedConfig = new TabletConfiguration
                    {
                        Name = device.GetManufacturer() + " " + device.GetProductName(),
                        DigitizerIdentifiers =
                        {
                            new DigitizerIdentifier
                            {
                                VendorID = device.VendorID,
                                ProductID = device.ProductID,
                                InputReportLength = (uint)device.GetMaxInputReportLength(),
                                OutputReportLength = (uint)device.GetMaxOutputReportLength()
                            }
                        }
                    };
                    Configurations = Configurations.Append(generatedConfig).ToList();
                    this.configList.SelectedIndex = Configurations.IndexOf(generatedConfig);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        private void Refresh()
        {
            this.configControls.Items.Clear();
            this.configControls.AddControls(GetPropertyControls());
        }

        private IEnumerable<Control> GetPropertyControls()
        {
            yield return new InputBox("Name",
                () => SelectedConfiguration.Name,
                (o) => SelectedConfiguration.Name = o
            );

            yield return new DigitizerIdentifierEditor(
                "Digitizer Identifiers",
                () => SelectedConfiguration.DigitizerIdentifiers,
                (o) => SelectedConfiguration.DigitizerIdentifiers = o
            );

            yield return new AuxiliaryIdentifierEditor(
                "Auxiliary Device Identifiers",
                () => SelectedConfiguration.AuxilaryDeviceIdentifiers,
                (o) => SelectedConfiguration.AuxilaryDeviceIdentifiers = o
            );

            yield return new DictionaryEditor("Attributes",
                () => SelectedConfiguration.Attributes,
                (o) => SelectedConfiguration.Attributes = o
            );
        }
    }
}
