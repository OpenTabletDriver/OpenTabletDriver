using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Numeric;
using OpenTabletDriver.UX.Dialogs;
using OpenTabletDriver.UX.Utilities;

namespace OpenTabletDriver.UX.Windows
{
    public sealed class ConfigurationEditor : DesktopForm
    {
        private readonly IDriverDaemon _driverDaemon;
        private readonly App _app;
        private readonly ListBox<TabletConfiguration> _configsList;

        public ConfigurationEditor(IDriverDaemon driverDaemon, App app)
        {
            _driverDaemon = driverDaemon;
            _app = app;

            Title = "Configuration Editor";

            Width = 1000;
            Height = 700;

            var placeholder = new Placeholder("No configuration selected.");

            var splitter = new Splitter
            {
                Panel1MinimumSize = 250,
                Panel1 = _configsList = new ListBox<TabletConfiguration>(),
                Panel2 = placeholder
            };

            _configsList.ItemTextBinding = Binding.Property<TabletConfiguration, string>(c => c.Name);
            Refresh().Run();

            Content = splitter;

            var removeCommand = new AppCommand("Remove", RemoveConfiguration);
            _configsList.SelectedIndexChanged += (_, _) => removeCommand.Enabled = _configsList.SelectedIndex >= 0;

            ToolBar = new ToolBar
            {
                Items =
                {
                    new AppCommand("Add", AddConfiguration),
                    removeCommand,
                    new AppCommand("Generate", GenerateConfiguration)
                }
            };

            var digitizerSpecifications = SpecificationEditorsFor(c => c.Specifications.Digitizer);
            var penSpecifications = SpecificationEditorsFor(c => c.Specifications.Pen);
            var auxiliarySpecifications = SpecificationEditorsFor(c => c.Specifications.AuxiliaryButtons);
            var mouseSpecifications = SpecificationEditorsFor(c => c.Specifications.MouseButtons);
            var touchSpecifications = SpecificationEditorsFor(c => c.Specifications.Touch);

            var digitizerIdentifiers = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5
            };

            var auxiliaryIdentifiers = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5
            };

            var attributes = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5
            };

            DataContextChanged += delegate
            {
                AssignIdentifierEditors(digitizerIdentifiers, c => c.DigitizerIdentifiers);
                AssignIdentifierEditors(auxiliaryIdentifiers, c => c.AuxiliaryDeviceIdentifiers);
                AssignDictionaryEditor(attributes, c => c.Attributes);
            };

            var editorPanel = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                Items =
                {
                    TextBoxFor((TabletConfiguration c) => c.Name),
                    new Expander
                    {
                        Header = LabelFor((TabletConfiguration c) => c.Specifications),
                        Content = new StackLayout
                        {
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Spacing = 5,
                            Padding = 5,
                            Items =
                            {
                                digitizerSpecifications,
                                penSpecifications,
                                auxiliarySpecifications,
                                mouseSpecifications,
                                touchSpecifications
                            }
                        }
                    },
                    new Expander
                    {
                        Header = LabelFor((TabletConfiguration c) => c.DigitizerIdentifiers),
                        Content = digitizerIdentifiers
                    },
                    new Expander
                    {
                        Header = LabelFor((TabletConfiguration c) => c.AuxiliaryDeviceIdentifiers),
                        Content = auxiliaryIdentifiers
                    },
                    new Expander
                    {
                        Header = LabelFor((TabletConfiguration c) => c.Attributes),
                        Content = attributes
                    }
                }
            };

            DataContextBinding.Bind(_configsList.SelectedValueBinding);
            DataContextChanged += (_, _) => splitter.Panel2 = DataContext is TabletConfiguration ? editorPanel : placeholder;

            var modifier = Application.Instance.CommonModifier;

            var fileMenu = new ButtonMenuItem
            {
                Text = "&File",
                Items =
                {
                    new AppCommand("Discard", Refresh),
                    new AppCommand("Load...", LoadDialog, modifier | Keys.O),
                    new AppCommand("Save...", SaveDialog, modifier | Keys.S)
                }
            };

            Menu = new MenuBar
            {
                Items =
                {
                    fileMenu
                },
                QuitItem = new AppCommand("Close", Close, Keys.Escape)
            };
        }

        private static BindableBinding<T, bool?> GetEnabledBinding<T>(T control) where T : Control
        {
            return new BindableBinding<T, bool?>(
                control,
                c => c.Enabled,
                (c, v) => c.Enabled = v.GetValueOrDefault(),
                (c, h) => c.EnabledChanged += h,
                (c, h) => c.EnabledChanged -= h
            );
        }

        /// <summary>
        /// Refreshes the configuration editor from the configuration directory.
        /// </summary>
        private async Task Refresh()
        {
            var appInfo = await _driverDaemon.GetApplicationInfo();
            LoadConfigurations(appInfo.ConfigurationDirectory);
        }

        /// <summary>
        /// Adds a new configuration to the list.
        /// </summary>
        private void AddConfiguration()
        {
            var configs = (IList<TabletConfiguration>) _configsList.DataStore;
            var newConfig = new TabletConfiguration
            {
                Name = "New Configuration"
            };

            configs.Add(newConfig);
            _configsList.SelectedIndex = configs.IndexOf(newConfig);
        }

        /// <summary>
        /// Removes the selected configuration from the list.
        /// </summary>
        private void RemoveConfiguration()
        {
            var configs = (IList<TabletConfiguration>) _configsList.DataStore;
            configs.Remove(_configsList.SelectedItem);
        }

        /// <summary>
        /// Generates a new configuration from a device selected in a dialog.
        /// </summary>
        private void GenerateConfiguration()
        {
            var dialog = _app.ShowDialog<DeviceDialog>(this);
            if (dialog.Result is not IDeviceEndpoint device)
                return;

            var newConfig = new TabletConfiguration
            {
                Name = $"{device.Manufacturer} {device.FriendlyName ?? device.ProductName}",
                DigitizerIdentifiers = new List<DeviceIdentifier>
                {
                    new DeviceIdentifier
                    {
                        VendorID = device.VendorID,
                        ProductID = device.ProductID,
                        InputReportLength = (uint) device.InputReportLength,
                        OutputReportLength = (uint) device.OutputReportLength
                    }
                }
            };

            var configs = (IList<TabletConfiguration>) _configsList.DataStore;
            configs.Add(newConfig);
            _configsList.SelectedIndex = configs.IndexOf(newConfig);
        }

        private Container SpecificationEditorsFor<T>(Expression<Func<TabletConfiguration, T?>> expression) where T : class, new()
        {
            var type = typeof(T);

            var editors = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };

            // this.((TabletConfiguration)DataContext).<expression>
            var thisExpr = Expression.Constant(this);
            var dataContextExpr = Expression.Convert(Expression.Property(thisExpr, nameof(DataContext)), typeof(TabletConfiguration));
            var baseExpr = new ExpressionMemberAccessor().AccessMember(dataContextExpr, expression);
            var getExpr = Expression.Lambda<Func<T?>>(baseExpr);
            var get = getExpr.Compile();

            foreach (var property in type.GetProperties())
            {
                if (!property.PropertyType.IsValueType)
                    continue;

                var defaultValue = Activator.CreateInstance(property.PropertyType);

                var binding = new DelegateBinding<string?>(
                    () =>
                    {
                        if (DataContext != null && get() is T t)
                            return property.GetValue(t)?.ToString();
                        return defaultValue?.ToString() ?? string.Empty;
                    },
                    v =>
                    {
                        if (get() is T t)
                        {
                            var value = string.IsNullOrWhiteSpace(v) ? defaultValue : v;
                            property.SetValue(t, Convert.ChangeType(value, property.PropertyType));
                        }
                    },
                    h => DataContextChanged += h,
                    h => DataContextChanged -= h
                );

                var textBox = new TextBox();
                textBox.TextBinding.Bind(binding);
                textBox.TextChanging += (s, e) =>
                {
                    try
                    {
                        _ = Convert.ChangeType(e.NewText, property.PropertyType);
                    }
                    catch
                    {
                        e.Cancel = true;
                    }
                };

                var name = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName ?? property.Name;
                var control = new LabeledGroup(name, textBox);

                editors.Items.Add(control);
            }

            return new Expander
            {
                Header = LabelFor(expression),
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 5,
                    Padding = 5,
                    Items =
                    {
                        ToggleFor(expression, editors),
                        editors
                    }
                }
            };
        }

        /// <summary>
        /// Updates all items in a <see cref="StackLayout"/> to target an expression with <see cref="DeviceIdentifier"/> editor controls.
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="expression"></param>
        private void AssignIdentifierEditors(StackLayout layout, Expression<Func<TabletConfiguration, IList<DeviceIdentifier>>> expression)
        {
            // Keeps items open whenever this function is re-invoked (remove button invocation)
            var openItemsQuery = from item in layout.Items
                let container = item.Control as Container
                where container is not null
                let expander = container.FindChild<Expander>()
                where expander.Expanded
                select ((Label) expander.Header).Text;

            var openItems = openItemsQuery.ToImmutableArray();

            layout.Items.Clear();

            if (DataContext is not TabletConfiguration configuration)
                return;

            var get = expression.Compile();
            for (var i = 0; i < get(configuration).Count; i++)
            {
                var control = IdentifierEditorForIndex(i, expression);
                var expander = control.FindChild<Expander>();
                if (expander.Header is Label label)
                    expander.Expanded = openItems.Any(t => t == label.Text);

                layout.Items.Add(control);
            }

            var addButton = new Button
            {
                Text = "+"
            };
            addButton.Click += delegate
            {
                var instance = get((TabletConfiguration) DataContext);
                var identifier = new DeviceIdentifier();
                instance.Add(identifier);

                var index = instance.IndexOf(identifier);
                var control = IdentifierEditorForIndex(index, expression);

                layout.Items.Insert(index, control);
            };
            layout.Items.Add(new StackLayoutItem(addButton, HorizontalAlignment.Right));
        }

        /// <summary>
        /// Creates an identifier editor for the target index
        /// </summary>
        /// <param name="i">The target index</param>
        /// <param name="expression">
        /// An expression pointing within the <see cref="BindableWidget.DataContext"/>
        /// to a list of <see cref="DeviceIdentifier"/>s.
        /// </param>
        private Container IdentifierEditorForIndex(int i, Expression<Func<TabletConfiguration, IList<DeviceIdentifier>>> expression)
        {
            var vendorID = HexEditorFor((DeviceIdentifier d) => d.VendorID);
            var productID = HexEditorFor((DeviceIdentifier d) => d.ProductID);
            var inputReportLength = TextBoxFor((DeviceIdentifier d) => d.InputReportLength);
            var outputReportLength = TextBoxFor((DeviceIdentifier d) => d.OutputReportLength);
            var reportParser = TextBoxFor((DeviceIdentifier d) => d.ReportParser);

            var expander = new Expander
            {
                Header = $"Identifier {i}",
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Padding = 5,
                    Spacing = 5,
                    Items =
                    {
                        vendorID,
                        productID,
                        inputReportLength,
                        outputReportLength,
                        reportParser
                    }
                }
            };

            var removeButton = new Button
            {
                Text = "-"
            };

            var control = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(expander, true),
                    new StackLayoutItem(removeButton, VerticalAlignment.Bottom)
                }
            };

            var get = expression.Compile();
            control.BindDataContext(c => c.DataContext, (TabletConfiguration c) => get(c)[i]);

            removeButton.Click += delegate
            {
                var parentLayout = control.FindParent<StackLayout>()!;
                parentLayout.Items.RemoveAt(i);
                get((TabletConfiguration)DataContext).RemoveAt(i);

                // Regenerate layout to fix bindings
                AssignIdentifierEditors(parentLayout, expression);
            };

            return control;
        }

        private void AssignDictionaryEditor(StackLayout layout, Expression<Func<TabletConfiguration, IDictionary<string, string>>> expression)
        {
            layout.Items.Clear();

            if (DataContext is not TabletConfiguration config)
                return;

            var get = expression.Compile();

            foreach (var pair in get(config))
            {
                var control = DictionaryItemForKey(pair.Key, expression);
                layout.Items.Add(control);
            }

            var addButton = new Button
            {
                Text = "+"
            };
            addButton.Click += delegate
            {
                var dict = get((TabletConfiguration) DataContext);
                if (dict.TryAdd(string.Empty, string.Empty))
                {
                    var control = DictionaryItemForKey(string.Empty, expression);
                    layout.Items.Insert(layout.Items.Count - 1, control);
                }
            };
            layout.Items.Add(new StackLayoutItem(addButton, HorizontalAlignment.Right));
        }

        private Container DictionaryItemForKey(string key, Expression<Func<TabletConfiguration, IDictionary<string, string>>> expression)
        {
            var get = expression.Compile();

            var keyBinding = new DelegateBinding<string>(
                () => key,
                newKey =>
                {
                    var dict = get((TabletConfiguration) DataContext);
                    var value = string.Empty;

                    if (dict.ContainsKey(key))
                    {
                        value = dict[key];
                        dict.Remove(key);
                    }

                    dict.Add(newKey, value);
                    key = newKey;
                }
            );

            var valueBinding = new DelegateBinding<string>(
                () => get((TabletConfiguration) DataContext)[key],
                v =>
                {
                    var dict = get((TabletConfiguration) DataContext);
                    if (dict.ContainsKey(key))
                        dict[key] = v;
                    else
                        dict.Add(key, v);
                }
            );

            var keyBox = new TextBox();
            keyBox.TextBinding.Bind(keyBinding);
            keyBox.TextChanging += (_, args) =>
            {
                var dict = get((TabletConfiguration) DataContext);
                if (dict.ContainsKey(args.NewText))
                    args.Cancel = true;
            };

            var valueBox = new TextBox();
            valueBox.TextBinding.Bind(valueBinding);

            var removeButton = new Button
            {
                Text = "-"
            };

            var control = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(keyBox, true),
                    new StackLayoutItem(valueBox, true),
                    removeButton
                }
            };

            removeButton.Click += delegate
            {
                var layout = control.FindParent<StackLayout>();
                var instance = get((TabletConfiguration) DataContext);
                instance.Remove(keyBinding.DataValue);
                layout.Items.Remove(control);

                // Regenerate layout to fix bindings
                AssignDictionaryEditor(layout, expression);
            };

            return control;
        }

        /// <summary>
        /// Creates a label for an expression, using friendly names where possible.
        /// </summary>
        /// <param name="expression">Expression pointing to the target member.</param>
        /// <typeparam name="T">The source type.</typeparam>
        /// <typeparam name="TValue">The target value type.</typeparam>
        private static Control LabelFor<T, TValue>(Expression<Func<T, TValue?>> expression)
        {
            return new Panel
            {
                Padding = new Padding(3, 0, 0, 0),
                Content = new Label
                {
                    Text = expression.GetFriendlyName()
                }
            };
        }

        /// <summary>
        /// Creates a toggle bound to an expression.
        /// </summary>
        /// <remarks>
        /// When the checked state is changed, it will update the binding with a new value.
        /// When unchecked, the bound value is set to null.
        /// When checked, the bound value is set to a new instance of <typeparamref name="T"/>
        /// </remarks>
        /// <param name="expression">The expression to bind to.</param>
        /// <param name="control">The control to enable when checked.</param>
        /// <typeparam name="T">The expected <see cref="BindableWidget.DataContext"/> type.</typeparam>
        /// <typeparam name="TValue">The target member type in the expression.</typeparam>
        private CheckBox ToggleFor<T, TValue>(Expression<Func<T, TValue?>> expression, Control control) where TValue : class, new()
        {
            var toggle = new CheckBox
            {
                Text = "Enable"
            };

            // this.((T)DataContext).<expression>
            var thisExpr = Expression.Constant(this);
            var dataContextExpr = Expression.Convert(Expression.Property(thisExpr, nameof(DataContext)), typeof(T));
            var baseExpr = new ExpressionMemberAccessor().AccessMember(dataContextExpr, expression);

            // v => this.((T)DataContext).<expression> = v
            var parameter = Expression.Parameter(typeof(TValue));
            var setExpr = Expression.Assign(baseExpr, parameter);

            var set = Expression.Lambda<Action<TValue?>>(setExpr, parameter).Compile();
            var get = Expression.Lambda<Func<TValue>>(baseExpr).Compile();

            toggle.CheckedBinding.Convert<TValue?>(
                b =>
                {
                    if (DataContext is null)
                        return default;
                    set(b.GetValueOrDefault() ? new TValue() : null);
                    return get();
                },
                v => v != null
            ).BindDataContext(expression);

            toggle.CheckedBinding.Bind(GetEnabledBinding(control));
            toggle.CheckedChanged += (_, _) => control.UpdateBindings();

            return toggle;
        }

        /// <summary>
        /// Creates a TextBox bound to an expression.
        /// </summary>
        /// <param name="expression">An expression pointing to a member.</param>
        /// <typeparam name="T">The <see cref="BindableWidget.DataContext"/> type.</typeparam>
        private Control TextBoxFor<T>(Expression<Func<T, object?>> expression)
        {
            var textBox = new TextBox();
            textBox.TextBinding.Convert(s => s, (object? o) => o?.ToString()).BindDataContext(expression);
            return new LabeledGroup(expression.GetFriendlyName(), textBox);
        }

        /// <summary>
        /// Creates a hexadecimal number editor bound to an expression.
        /// </summary>
        /// <param name="expression">An expression pointing to a member.</param>
        /// <typeparam name="T">The <see cref="BindableWidget.DataContext"/> type.</typeparam>
        private Control HexEditorFor<T>(Expression<Func<T, int>> expression)
        {
            var hex = new HexNumberBox();
            hex.ValueBinding.BindDataContext(expression);
            return new LabeledGroup(expression.GetFriendlyName(), hex);
        }

        /// <summary>
        /// Load dialog for loading tablet configurations.
        /// </summary>
        private void LoadDialog()
        {
            var dialog = new SelectFolderDialog
            {
                Title = "Load tablet configurations...",
                Directory = EtoEnvironment.GetFolderPath(EtoSpecialFolder.Documents)
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
                LoadConfigurations(dialog.Directory);
        }

        /// <summary>
        /// Save dialog for saving tablet configurations.
        /// </summary>
        private void SaveDialog()
        {
            var dialog = new SelectFolderDialog
            {
                Title = "Save tablet configurations...",
                Directory = EtoEnvironment.GetFolderPath(EtoSpecialFolder.Documents)
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
                SaveConfigurations(dialog.Directory);
        }

        /// <summary>
        /// Internal method for loading configurations into the <see cref="ListBox{T}"/> data store.
        /// </summary>
        /// <param name="path">The source directory.</param>
        private void LoadConfigurations(string path)
        {
            var oldValue = _configsList.SelectedValue;

            _configsList.Enabled = false;

            var configs = new ObservableCollection<TabletConfiguration>(EnumerateConfigurations(path));
            _configsList.DataStore = configs;

            _configsList.Enabled = true;
            if (oldValue is TabletConfiguration config)
                _configsList.SelectedIndex = configs.IndexOf(configs.First(c => c.Name == config.Name));
        }

        /// <summary>
        /// Internal method for saving configurations into a directory.
        /// </summary>
        /// <param name="path">The directory to save to.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SaveConfigurations(string path)
        {
            if (_configsList.DataStore is not IEnumerable<TabletConfiguration> configs)
                throw new InvalidOperationException();

            foreach (var config in configs)
            {
                var spaceIndex = config.Name.IndexOf(' ');
                var manufacturer = config.Name[..spaceIndex];
                var tabletName = config.Name[spaceIndex..].TrimStart();

                var configPath = Path.Join(path, manufacturer, $"{tabletName}.json");
                var file = new FileInfo(configPath);

                if (!file.Directory!.Exists)
                    file.Directory.Create();

                Serialization.Serialize(file, config);
            }
        }

        /// <summary>
        /// Enumerates configurations from a directory path.
        /// </summary>
        /// <param name="path">The directory to enumerate for <see cref="TabletConfiguration"/>s.</param>
        private static IEnumerable<TabletConfiguration> EnumerateConfigurations(string path)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                return Array.Empty<TabletConfiguration>();

            return from file in dir.EnumerateFiles("*.json", SearchOption.AllDirectories)
                let config = Serialization.Deserialize<TabletConfiguration>(file)
                orderby config.Name
                select config;
        }
    }
}
