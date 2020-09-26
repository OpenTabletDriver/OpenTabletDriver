using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using HidSharp;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

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
                QuitItem = quitCommand
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
            var appinfo = await App.Driver.Instance.GetApplicationInfo();
            var configDir = new DirectoryInfo(appinfo.ConfigurationDirectory);
            var sortedConfigs = from config in ReadConfigurations(configDir)
                orderby config.Name
                select config;
            Configurations = new List<TabletProperties>(sortedConfigs);
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
                if (!file.Directory.Exists)
                    file.Directory.Create();
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

        private IEnumerable<Control> GeneratePropertyControls() => new Control[]
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
                        (o) => SelectedConfiguration.ActiveReportID = DetectionRange.Parse(o)
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
                        placeholder: typeof(TabletReportParser).FullName
                    ),
                    GetControl("Feature Initialization Report",
                        () => SelectedConfiguration.DigitizerIdentifier.FeatureInitReport is byte[] report ? ToHexString(report) : string.Empty,
                        (o) => SelectedConfiguration.DigitizerIdentifier.FeatureInitReport = ToByteArray(o)
                    ),
                    GetControl("Output Initialization Report", 
                        () => SelectedConfiguration.DigitizerIdentifier.OutputInitReport is byte[] report ? ToHexString(report) : string.Empty,
                        (o) => SelectedConfiguration.DigitizerIdentifier.OutputInitReport = ToByteArray(o)
                    )
                ),
                GetExpander("Alternate Tablet Identifiers", isExpanded: false,
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
                        placeholder: typeof(TabletReportParser).FullName
                    ),
                    GetControl("Feature Initialization Report",
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.FeatureInitReport is byte[] report ? ToHexString(report) : string.Empty,
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.FeatureInitReport = ToByteArray(o)
                    ),
                    GetControl("Output Initialization Report", 
                        () => SelectedConfiguration.AlternateDigitizerIdentifier.OutputInitReport is byte[] report ? ToHexString(report) : string.Empty,
                        (o) => SelectedConfiguration.AlternateDigitizerIdentifier.OutputInitReport = ToByteArray(o)
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
                        placeholder: typeof(AuxReportParser).FullName
                    ),
                    GetControl("Feature Initialization Report",
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.FeatureInitReport is byte[] report ? ToHexString(report) : string.Empty,
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.FeatureInitReport = ToByteArray(o)
                    ),
                    GetControl("Output Initialization Report", 
                        () => SelectedConfiguration.AuxilaryDeviceIdentifier.OutputInitReport is byte[] report ? ToHexString(report) : string.Empty,
                        (o) => SelectedConfiguration.AuxilaryDeviceIdentifier.OutputInitReport = ToByteArray(o)
                    )
                ),
                GetExpander("Advanced", isExpanded: false,
                    GetDictionaryControl("Attributes",
                        () => SelectedConfiguration.Attributes,
                        (o) => SelectedConfiguration.Attributes = o
                    ),
                    GetDictionaryControl("Device Strings",
                        () =>
                        {
                            var dictionaryBuffer = new Dictionary<string, string>();
                            foreach (var pair in SelectedConfiguration.DeviceStrings)
                                dictionaryBuffer.Add($"{pair.Key}", pair.Value);
                            return dictionaryBuffer;
                        },
                        (o) =>
                        {
                            SelectedConfiguration.DeviceStrings.Clear();
                            foreach (KeyValuePair<string, string> pair in o)
                                if (byte.TryParse(pair.Key, out var keyByte))
                                    SelectedConfiguration.DeviceStrings.Add(keyByte, pair.Value);
                        }
                    ),
                    GetListControl("Initialization String Indexes",
                        () =>
                        {
                            var listBuffer = new List<string>();
                            foreach (var value in SelectedConfiguration.InitializationStrings)
                                listBuffer.Add($"{value}");
                            return listBuffer;
                        },
                        (o) =>
                        {
                            SelectedConfiguration.InitializationStrings.Clear();
                            foreach (string value in o)
                                if (byte.TryParse(value, out var byteValue))
                                    SelectedConfiguration.InitializationStrings.Add(byteValue);
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

        private Control GetListControl(
            string groupName,
            Func<IEnumerable<string>> getValue,
            Action<IEnumerable<string>> setValue
        )
        {
            var textArea = new TextArea();
            textArea.TextBinding.Bind(
                () => 
                {
                    StringBuilder buffer = new StringBuilder();
                    foreach (string value in getValue())
                        buffer.AppendLine(value);
                    return buffer.ToString();
                },
                (o) => 
                {
                    setValue(o.Split(Environment.NewLine));
                }
            );
            return new GroupBox
            {
                Text = groupName,
                Padding = App.GroupBoxPadding,
                Content = textArea
            };
        }

        private GroupBox GetDictionaryControl(
            string groupName,
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue
        )
        {
            var entries = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            
            foreach (var pair in getValue())
                entries.Items.Add(GetDictionaryEntryControl(getValue, setValue, pair.Key, pair.Value));

            var actions = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items = 
                {
                    new Button((sender, e) => entries.Items.Add(GetDictionaryEntryControl(getValue, setValue)))
                    {
                        Text = "+"
                    }
                }
            };

            var layout = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow
                    {
                        ScaleHeight = true,
                        Cells = 
                        {
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = entries
                            }
                        }
                    },
                    new TableRow
                    {
                        ScaleHeight = false,
                        Cells =
                        {
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = actions
                            }
                        }
                    }
                }
            };

            return new GroupBox
            {
                Text = groupName,
                Padding = App.GroupBoxPadding,
                Content = layout
            };
        }

        StackLayoutItem GetDictionaryEntryControl(
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue,
            string startKey = null,
            string startValue = null
        )
        {
            var keyBox = new TextBox
            {
                PlaceholderText = "Key",
                ToolTip = 
                    "The dictionary entry's key. This is what is indexed to find a value." + Environment.NewLine +
                    "If left empty, the entry will be removed on save or apply."
            };

            var valueBox = new TextBox
            {
                PlaceholderText = "Value",
                ToolTip = "The dictionary entry's value. This is what is retrieved when indexing with the specified key."
            };

            string oldKey = startKey;
            keyBox.TextBinding.Bind(
                () => startKey,
                (key) =>
                {
                    var dict = getValue();
                    var value = valueBox.Text;

                    if (string.IsNullOrWhiteSpace(key))
                        dict.Remove(key);
                    else if (!dict.TryAdd(key, value))
                        dict[key] = value;
                    
                    if (oldKey != null)
                        dict.Remove(oldKey);
                    oldKey = key;

                    setValue(dict);
                }
            );

            valueBox.TextBinding.Bind(
                () => startValue,
                (value) =>
                {
                    var dict = getValue();
                    var key = keyBox.Text;

                    if (!dict.TryAdd(key, value))
                        dict[key] = value;

                    setValue(dict);
                }
            );

            var stackLayout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = keyBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = valueBox,
                        Expand = true
                    }
                }
            };
            
            return new StackLayoutItem(stackLayout, true);
        }

        private static bool TryGetHexValue(string str, out byte value) => byte.TryParse(str.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out value);
        private static string ToHexString(byte[] value) => "0x" + BitConverter.ToString(value).Replace("-", " 0x") ?? string.Empty;
        
        private static byte[] ToByteArray(string hex)
        {
            var raw = hex.Split(' ');
            byte[] buffer = new byte[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                if (TryGetHexValue(raw[i], out var val))
                    buffer[i] = val;
                else
                    return null;
            }
            return buffer;
        }
    }
}