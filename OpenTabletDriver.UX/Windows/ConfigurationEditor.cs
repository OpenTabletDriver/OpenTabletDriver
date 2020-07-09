using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using HidSharp;
using TabletDriverLib.Tablet;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.UX.Windows
{
    public class ConfigurationEditor : Form
    {
        public ConfigurationEditor()
        {
            Title = "Configuration Editor";
            ClientSize = new Size(960 - 50, 730 - 50);
            MinimumSize = new Size(960 - 50, 730 - 50);
            Icon = App.Logo.WithSize(App.Logo.Size);

            // Main Controls
            _configList.SelectedIndexChanged += (sender, e) => 
            {
                if (_configList.SelectedIndex >= 0)
                    SelectedConfiguration = Configurations[_configList.SelectedIndex];
            };
            
            Content = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Panel1MinimumSize = 200,
                Panel1 = _configList,
                Panel2 = new Scrollable
                { 
                    Content = _configControls,
                    Padding = new Padding(5)
                },
            };

            // MenuBar Commands
            var quitCommand = new Command { MenuText = "Close", Shortcut = Application.Instance.CommonModifier | Keys.W };
            quitCommand.Executed += (sender, e) => Close();

            var aboutCommand = new Command { MenuText = "About...", Shortcut = Keys.F1 };
            aboutCommand.Executed += (sender, e) => App.AboutDialog.ShowDialog(this);

            var loadDirectory = new Command { MenuText = "Load configurations...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            loadDirectory.Executed += (sender, e) => LoadConfigurationsDialog();

            var saveDirectory = new Command { MenuText = "Save configurations...", Shortcut = Application.Instance.CommonModifier | Application.Instance.AlternateModifier | Keys.S };
            saveDirectory.Executed += (sender, e) => SaveConfigurationsDialog();

            var newConfiguration = new Command { ToolBarText = "New configuration", Shortcut = Application.Instance.CommonModifier | Keys.N };
            newConfiguration.Executed += (sender, e) => CreateNewConfiguration();

            var deleteConfiguration = new Command { ToolBarText = "Delete configuration" };
            deleteConfiguration.Executed += (sender, e) => DeleteConfiguration(SelectedConfiguration);

            var generateConfiguration = new Command { ToolBarText = "Generate configuration..." };
            generateConfiguration.Executed += async (sender, e) => await GenerateConfiguration();

            // Menu
            Menu = new MenuBar
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
                            saveDirectory
                        }
                    }
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            ToolBar = new ToolBar
            {
                Items =
                {
                    newConfiguration,
                    deleteConfiguration,
                    generateConfiguration
                }
            };

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var appinfo = await App.DriverDaemon.InvokeAsync(d => d.GetApplicationInfo());
            var configDir = new DirectoryInfo(appinfo.ConfigurationDirectory);
            Configurations = ReadConfigurations(configDir);
            _configList.SelectedIndex = 0;
        }

        private List<TabletProperties> _configs;
        private List<TabletProperties> Configurations
        {
            set
            {
                _configs = value;
                _configList.Items.Clear();
                foreach (var config in Configurations)
                    _configList.Items.Add(config.Name);
            }
            get => _configs;
        }
        
        private TabletProperties _selected;
        private TabletProperties SelectedConfiguration
        {
            set
            {
                _selected = value;
                _configControls.Items.Clear();
                foreach (var control in GeneratePropertyControls())
                {
                    var item = new StackLayoutItem(control, HorizontalAlignment.Stretch);
                    _configControls.Items.Add(item);
                }
            }
            get => _selected;
        }

        private ListBox _configList = new ListBox();
        private StackLayout _configControls = new StackLayout
        {
            Spacing = 5,
        };

        private List<TabletProperties> ReadConfigurations(DirectoryInfo dir)
        {
            var configs = from file in dir.GetFiles("*.json", SearchOption.AllDirectories)
                select TabletProperties.Read(file);
            return new List<TabletProperties>(configs);
        }

        private void WriteConfigurations(IEnumerable<TabletProperties> configs, DirectoryInfo dir)
        {
            var regex = new Regex("(?<Manufacturer>.+?) (?<TabletName>.+?)$");
            foreach (var config in configs)
            {
                var match = regex.Match(config.Name);
                var manufacturer = match.Groups["Manufacturer"].Value;
                var tabletName = match.Groups["TabletName"].Value;

                var path = Path.Join(dir.FullName, manufacturer, string.Format("{0}.json", tabletName));
                var file = new FileInfo(path);
                config.Write(file);
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
            var newTablet = new TabletProperties
            {
                Name = "New Tablet"
            };
            Configurations = Configurations.Append(newTablet).ToList();
            _configList.SelectedIndex = Configurations.IndexOf(newTablet);
        }

        private void DeleteConfiguration(TabletProperties config)
        {
            Configurations = Configurations.Where(c => c != config).ToList();
            if (SelectedConfiguration == config)
                _configList.SelectedIndex = Configurations.Count - 1;
        }

        private async Task GenerateConfiguration()
        {
            var dialog = new DeviceListDialog();
            if (await dialog.ShowModalAsync() is HidDevice device)
            {
                try
                {
                    var generatedConfig = new TabletProperties
                    {
                        Name = device.GetManufacturer() + " " + device.GetProductName(),
                        DigitizerIdentifier = new DeviceIdentifier
                        {
                            VendorID = device.VendorID,
                            ProductID = device.ProductID,
                            InputReportLength = (uint)device.GetMaxInputReportLength(),
                            OutputReportLength = (uint)device.GetMaxOutputReportLength()
                        }
                    };
                    Configurations = Configurations.Append(generatedConfig).ToList();
                    _configList.SelectedIndex = Configurations.IndexOf(generatedConfig);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        private IEnumerable<Control> GeneratePropertyControls() => new List<Control>
            {
                GetControl("Name",
                    () => SelectedConfiguration.Name,
                    (o) => SelectedConfiguration.Name = o
                ),
                GetExpander("Tablet Specifications", isExpanded: true,
                    GetControl("Width (mm)",
                        () => SelectedConfiguration.Width.ToString(),
                        (o) => SelectedConfiguration.Width = float.TryParse(o, out var val) ? val : 0f
                    ),
                    GetControl("Height (mm)",
                        () => SelectedConfiguration.Height.ToString(),
                        (o) => SelectedConfiguration.Height = float.TryParse(o, out var val) ? val : 0f
                    ),
                    GetControl("Max X (px)",
                        () => SelectedConfiguration.MaxX.ToString(),
                        (o) => SelectedConfiguration.MaxX = float.TryParse(o, out var val) ? val : 0f
                    ),
                    GetControl("Max Y (px)",
                        () => SelectedConfiguration.MaxY.ToString(),
                        (o) => SelectedConfiguration.MaxY = float.TryParse(o, out var val) ? val : 0f
                    ),
                    GetControl("Max Pressure",
                        () => SelectedConfiguration.MaxPressure.ToString(),
                        (o) => SelectedConfiguration.MaxPressure = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Active Report ID",
                        () => SelectedConfiguration.ActiveReportID.ToString(),
                        (o) => SelectedConfiguration.ActiveReportID = uint.TryParse(o, out var val) ? val : 0
                    )
                ),
                GetExpander("Tablet Identifiers", isExpanded: false,
                    GetControl("Vendor ID",
                        () => SelectedConfiguration.DigitizerIdentifier.VendorID.ToString(),
                        (o) => SelectedConfiguration.DigitizerIdentifier.VendorID = int.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Product ID",
                        () => SelectedConfiguration.DigitizerIdentifier.ProductID.ToString(),
                        (o) => SelectedConfiguration.DigitizerIdentifier.ProductID = int.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Input Report Length",
                        () => SelectedConfiguration.DigitizerIdentifier.InputReportLength.ToString(),
                        (o) => SelectedConfiguration.DigitizerIdentifier.InputReportLength = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Output Report Length",
                        () => SelectedConfiguration.DigitizerIdentifier.OutputReportLength.ToString(),
                        (o) => SelectedConfiguration.DigitizerIdentifier.OutputReportLength = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Report Parser",
                        () => SelectedConfiguration.DigitizerIdentifier.ReportParser,
                        (o) => SelectedConfiguration.DigitizerIdentifier.ReportParser = o,
                        typeof(TabletReportParser).FullName
                    ),
                    GetControl("Feature Initialization Report",
                        () => SelectedConfiguration.DigitizerIdentifier.FeatureInitReport is byte[] report ? ToHexValue(report) : string.Empty,
                        (o) =>
                        {
                            var raw = o.Split(' ');
                            byte[] buffer = new byte[raw.Length];
                            for (int i = 0; i < raw.Length; i++)
                            {
                                if (TryGetHexValue(raw[i], out var val))
                                    buffer[i] = val;
                                else
                                {
                                    SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = null;
                                    return;
                                }
                            }
                            SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = buffer;
                        }
                    ),
                    GetControl("Output Initialization Report", 
                        () => SelectedConfiguration.DigitizerIdentifier.OutputInitReport is byte[] report ? ToHexValue(report) : string.Empty,
                        (o) => 
                        {
                            var raw = o.Split(' ');
                            byte[] buffer = new byte[raw.Length];
                            for (int i = 0; i < raw.Length; i++)
                            {
                                if (TryGetHexValue(raw[i], out var val))
                                    buffer[i] = val;
                                else
                                {
                                    SelectedConfiguration.DigitizerIdentifier.OutputInitReport = null;
                                    return;
                                }
                            }
                            SelectedConfiguration.DigitizerIdentifier.OutputInitReport = buffer;
                        }
                    )
                ),
                GetExpander("Alternate Tablet Identifiers", false,
                    GetControl("Vendor ID",
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.VendorID.ToString(),
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.VendorID = int.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Product ID",
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.ProductID.ToString(),
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.ProductID = int.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Input Report Length",
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.InputReportLength.ToString(),
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.InputReportLength = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Output Report Length",
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.OutputReportLength.ToString(),
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.OutputReportLength = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Report Parser",
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.ReportParser,
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.ReportParser = o,
                        typeof(TabletReportParser).FullName
                    ),
                    GetControl("Feature Initialization Report",
                        () => SelectedConfiguration.DigitizerIdentifier.FeatureInitReport is byte[] report ? ToHexValue(report) : string.Empty,
                        (o) =>
                        {
                            var raw = o.Split(' ');
                            byte[] buffer = new byte[raw.Length];
                            for (int i = 0; i < raw.Length; i++)
                            {
                                if (TryGetHexValue(raw[i], out var val))
                                    buffer[i] = val;
                                else
                                {
                                    SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = null;
                                    return;
                                }
                            }
                            SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = buffer;
                        }
                    ),
                    GetControl("Output Initialization Report", 
                        () => SelectedConfiguration.DigitizerIdentifier.OutputInitReport is byte[] report ? ToHexValue(report) : string.Empty,
                        (o) => 
                        {
                            var raw = o.Split(' ');
                            byte[] buffer = new byte[raw.Length];
                            for (int i = 0; i < raw.Length; i++)
                            {
                                if (TryGetHexValue(raw[i], out var val))
                                    buffer[i] = val;
                                else
                                {
                                    SelectedConfiguration.DigitizerIdentifier.OutputInitReport = null;
                                    return;
                                }
                            }
                            SelectedConfiguration.DigitizerIdentifier.OutputInitReport = buffer;
                        }
                    )
                ),
                GetExpander("Auxiliary Device Identifiers", isExpanded: false,
                    GetControl("Vendor ID",
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.VendorID.ToString(),
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.VendorID = int.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Product ID",
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.ProductID.ToString(),
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.ProductID = int.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Input Report Length",
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.InputReportLength.ToString(),
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.InputReportLength = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Output Report Length",
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.OutputReportLength.ToString(),
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.OutputReportLength = uint.TryParse(o, out var val) ? val : 0
                    ),
                    GetControl("Report Parser",
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.ReportParser,
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.ReportParser = o,
                        typeof(AuxReportParser).FullName
                    ),
                    GetControl("Feature Initialization Report",
                        () => SelectedConfiguration.DigitizerIdentifier.FeatureInitReport is byte[] report ? ToHexValue(report) : string.Empty,
                        (o) =>
                        {
                            var raw = o.Split(' ');
                            byte[] buffer = new byte[raw.Length];
                            for (int i = 0; i < raw.Length; i++)
                            {
                                if (TryGetHexValue(raw[i], out var val))
                                    buffer[i] = val;
                                else
                                {
                                    SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = null;
                                    return;
                                }
                            }
                            SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = buffer;
                        }
                    ),
                    GetControl("Output Initialization Report", 
                        () => SelectedConfiguration.DigitizerIdentifier.OutputInitReport is byte[] report ? ToHexValue(report) : string.Empty,
                        (o) => 
                        {
                            var raw = o.Split(' ');
                            byte[] buffer = new byte[raw.Length];
                            for (int i = 0; i < raw.Length; i++)
                            {
                                if (TryGetHexValue(raw[i], out var val))
                                    buffer[i] = val;
                                else
                                {
                                    SelectedConfiguration.DigitizerIdentifier.OutputInitReport = null;
                                    return;
                                }
                            }
                            SelectedConfiguration.DigitizerIdentifier.OutputInitReport = buffer;
                        }
                    )
                )
            };

        private GroupBox GetControl(string groupName, Func<string> getValue, Action<string> setValue, string placeholder = null)
        {
            var textBox = new TextBox
            {
                PlaceholderText = placeholder
            };
            textBox.TextBinding.Bind(getValue, setValue);
            return new GroupBox
            {
                Text = groupName,
                Padding = App.GroupBoxPadding,
                Content = textBox
            };
        }

        private Expander GetExpander(string groupName, bool isExpanded, params Control[] controls)
        {
            var stack = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };

            var expander = new Expander
            {
                Header = groupName,
                Content = stack,
                Expanded = isExpanded,
                Padding = new Padding(0, 5, 0, 0)
            };
            
            foreach (var ctrl in controls)
            {
                var stackitem = new StackLayoutItem
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Control = ctrl
                };
                stack.Items.Add(ctrl);
            }

            return expander;
        }

        private static bool TryGetHexValue(string str, out byte value) => byte.TryParse(str.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out value);
        private static string ToHexValue(byte[] value) => "0x" + BitConverter.ToString(value).Replace("-", " 0x") ?? string.Empty;
    }
}