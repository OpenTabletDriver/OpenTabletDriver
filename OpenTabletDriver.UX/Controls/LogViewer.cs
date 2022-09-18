using System.Text;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Logging;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class LogViewer : DesktopPanel
    {
        private readonly IDriverDaemon _daemon;
        private readonly GridView<LogMessage> _list;

        private LogDataStore? _logMessages;

        public LogViewer(RpcClient<IDriverDaemon> rpc)
        {
            _daemon = rpc.Instance!;

            _list = new GridView<LogMessage>
            {
                AllowMultipleSelection = true,
                Columns =
                {
                    new GridColumn
                    {
                        HeaderText = "Time",
                        DataCell = new TextBoxCell
                        {
                            Binding = Binding.Property<LogMessage, string>(m => m.Time.ToLongTimeString())
                        }
                    },
                    new GridColumn
                    {
                        HeaderText = "Level",
                        DataCell = new TextBoxCell
                        {
                            Binding = Binding.Property<LogMessage, string>(m =>
                                Enum.GetName(m.Level) ?? m.Level.ToString())
                        }
                    },
                    new GridColumn
                    {
                        HeaderText = "Group",
                        DataCell = new TextBoxCell
                        {
                            Binding = Binding.Property<LogMessage, string>(m => m.Group ?? string.Empty)
                        }
                    },
                    new GridColumn
                    {
                        HeaderText = "Message",
                        DataCell = new TextBoxCell
                        {
                            Binding = Binding.Property<LogMessage, string>(m => m.Message ?? string.Empty)
                        }
                    }
                }
            };

            var filter = new EnumDropDown<LogLevel>
            {
                SelectedValue = LogLevel.Info
            };
            filter.SelectedValueChanged += (_, _) => _logMessages!.Filter = filter.SelectedValue;

            var toolbar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Padding = new Padding(0, 5, 0, 0),
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Items =
                {
                    filter,
                    new Button((_, _) => Copy(_logMessages))
                    {
                        Text = "Copy All"
                    }
                }
            };

            var modifier = Application.Instance.CommonModifier;
            var copy = new AppCommand("Copy", () => Copy(_list.SelectedItems), modifier | Keys.C);
            _list.ContextMenu = new ContextMenu
            {
                Items =
                {
                    copy
                }
            };

            _list.KeyDown += (_, e) =>
            {
                if ((e.KeyData & copy.Shortcut) == copy.Shortcut)
                    copy.Execute();
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(_list, true),
                    new StackLayoutItem(toolbar)
                }
            };

            InitializeAsync().Run();
        }

        private async Task InitializeAsync()
        {
            var messages = await _daemon.GetCurrentLog();
            _list.DataStore = _logMessages = new LogDataStore(messages);
            _logMessages.CollectionChanged += (_, _) =>
            {
                if (_logMessages.Count > 0)
                {
                    var row = _list.SelectedRow == -1 ? _logMessages.Count - 1 : _list.SelectedRow;
                    _list.ScrollToRow(row);
                }
            };

            _daemon.Message += (_, m) => Application.Instance.AsyncInvoke(() =>
            {
                _logMessages.Add(m);
                if (m.Level >= LogLevel.Info)
                    _list.SelectedRow = _logMessages.Count - 2;

                if (m.Notification)
                    MessageBox.Show(m.Group, m.Message);
            });
        }

        /// <summary>
        /// Copies log messages to the clipboard.
        /// </summary>
        /// <param name="messages">The messages to copy.</param>
        private static void Copy(IEnumerable<LogMessage>? messages)
        {
            if (messages == null)
                return;

            var sb = new StringBuilder();
            foreach (var message in messages)
            {
                var line = Log.GetStringFormat(message);
                sb.AppendLine(line);
            }

            if (sb.Length > 0)
            {
                Clipboard.Instance.Clear();
                Clipboard.Instance.Text = sb.ToString();
            }
        }
    }
}
