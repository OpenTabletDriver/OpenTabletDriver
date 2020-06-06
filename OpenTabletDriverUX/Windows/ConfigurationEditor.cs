using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using TabletDriverLib;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverUX.Windows
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
            Configurations = ReadConfigurations(AppInfo.ConfigurationDirectory);
            SelectedConfiguration = Configurations[0];
            _configList.SelectedIndexChanged += (sender, e) => SelectedConfiguration = Configurations[_configList.SelectedIndex];
            
            this.Content = new Splitter
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
        }

        private List<TabletProperties> _configs;
        private List<TabletProperties> Configurations
        {
            set
            {
                _configs = value;
                _configList.Items.Clear();
                foreach (var config in Configurations)
                    _configList.Items.Add(config.TabletName);
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
        private StackLayout _configControls = new StackLayout();

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
                var match = regex.Match(config.TabletName);
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

        private IEnumerable<Control> GeneratePropertyControls() => new List<Control>
            {
                GetControl("Name",
                    () => SelectedConfiguration.TabletName,
                    (o) => SelectedConfiguration.TabletName = o),
                GetControl("Vendor ID",
                    () => SelectedConfiguration.VendorID.ToString(),
                    (o) => SelectedConfiguration.VendorID = int.TryParse(o, out var val) ? val : 0
                ),
                GetControl("Product ID",
                    () => SelectedConfiguration.ProductID.ToString(),
                    (o) => SelectedConfiguration.ProductID = int.TryParse(o, out var val) ? val : 0
                ),
                GetControl("Input Report Length",
                    () => SelectedConfiguration.InputReportLength.ToString(),
                    (o) => SelectedConfiguration.InputReportLength = uint.TryParse(o, out var val) ? val : 0
                ),
                GetControl("Output Report Length",
                    () => SelectedConfiguration.OutputReportLength.ToString(),
                    (o) => SelectedConfiguration.OutputReportLength = uint.TryParse(o, out var val) ? val : 0
                ),
                GetControl("Report Parser",
                    () => SelectedConfiguration.ReportParserName,
                    (o) => SelectedConfiguration.ReportParserName = o),
                GetControl("Custom Input Report Length",
                    () => SelectedConfiguration.CustomInputReportLength.ToString(),
                    (o) => SelectedConfiguration.CustomInputReportLength = uint.TryParse(o, out var val) ? val : 0
                ),
                GetControl("Custom Report Parser",
                    () => SelectedConfiguration.CustomReportParserName,
                    (o) => SelectedConfiguration.CustomReportParserName = o
                ),
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
                ),
                GetControl("Auxiliary Input Report Length",
                    () => SelectedConfiguration.AuxReportLength.ToString(),
                    (o) => SelectedConfiguration.AuxReportLength = uint.TryParse(o, out var val) ? val : 0
                ),
                GetControl("Auxiliary Report Parser",
                    () => SelectedConfiguration.AuxReportParserName,
                    (o) => SelectedConfiguration.AuxReportParserName = o
                ),
                GetControl("Feature Initialization Report",
                    () => SelectedConfiguration.FeatureInitReport != null ? ToHexValue(SelectedConfiguration.FeatureInitReport) : string.Empty,
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
                                SelectedConfiguration.FeatureInitReport = null;
                                return;
                            }
                        }
                        SelectedConfiguration.FeatureInitReport = buffer;
                    }
                ),
            };

        private GroupBox GetControl(string groupName, Func<string> getValue, Action<string> setValue)
        {
            var textBox = new TextBox();
            textBox.TextBinding.Bind(getValue, setValue);
            return new GroupBox
            {
                Text = groupName,
                Padding = App.GroupBoxPadding,
                Content = textBox
            };
        }

        private static bool TryGetHexValue(string str, out byte value) => byte.TryParse(str.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out value);
        private static string ToHexValue(byte[] value) => "0x" + BitConverter.ToString(value).Replace("-", " 0x") ?? string.Empty;
    }
}