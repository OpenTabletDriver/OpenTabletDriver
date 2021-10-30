using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Controls
{
    public class LogView : StackLayout
    {
        public LogView()
        {
            this.Orientation = Orientation.Vertical;

            var filterSelector = new FilterDropDown();
            filterSelector.SelectedValueChanged += (sender, filter) => this.messageStore.Filter = filterSelector.SelectedValue;

            var toolbar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Padding = new Padding(0, 5, 0, 0),
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Items =
                {
                    filterSelector,
                    new Button((sender, e) => Copy(this.messageStore))
                    {
                        Text = "Copy All"
                    }
                }
            };

            var copyCommand = new Command((sender, e) => Copy(messageList.SelectedItems))
            {
                MenuText = "Copy"
            };

            messageList.ContextMenu = new ContextMenu
            {
                Items =
                {
                    copyCommand
                }
            };

            messageList.KeyDown += (sender, e) =>
            {
                switch (e.Modifiers, e.Key)
                {
                    case (Keys.Control, Keys.C):
                    {
                        copyCommand.Execute();
                        break;
                    }
                }
            };

            messageList.DataStore = this.messageStore;
            this.messageStore.CollectionChanged += (sender, e) =>
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    if (this.messageStore.Count > 0)
                    {
                        if (messageList.SelectedRow == -1)
                            messageList.ScrollToRow(this.messageStore.Count - 1);
                        else
                            messageList.ScrollToRow(messageList.SelectedRow);
                    }
                });
            };

            //Console coloring causes non-accelerated drawing on MacOS, cannot be done without performance hit
            if (Interop.SystemInterop.CurrentPlatform != PluginPlatform.MacOS)
            {
                messageList.CellFormatting += (_, entry) =>
                {
                    var entryLogMessage = entry.Item as LogMessage;

                    switch (entryLogMessage.Level)
                    {
                        case LogLevel.Debug:
                            entry.ForegroundColor = Colors.Black;
                            entry.BackgroundColor = Colors.LightBlue;
                            break;
                        case LogLevel.Warning:
                            entry.ForegroundColor = Colors.Black;
                            entry.BackgroundColor = Colors.Yellow;
                            break;
                        case LogLevel.Error:
                            entry.ForegroundColor = Colors.Black;
                            entry.BackgroundColor = Colors.Pink;
                            break;
                        case LogLevel.Fatal:
                            entry.ForegroundColor = Colors.White;
                            entry.BackgroundColor = Colors.DarkRed;
                            break;
                        default:
                            entry.ForegroundColor = SystemColors.ControlText;
                            entry.BackgroundColor = SystemColors.ControlBackground;
                            break;
                    }
                };
            }

            this.Items.Add(new StackLayoutItem(messageList, HorizontalAlignment.Stretch, true));
            this.Items.Add(new StackLayoutItem(toolbar, HorizontalAlignment.Stretch));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var currentMessages = from message in await App.Driver.Instance.GetCurrentLog()
                where message is LogMessage
                select message;

            foreach (var message in currentMessages)
                AddMessage(message);

            App.Driver.AddConnectionHook(i => i.Message += (sender, message) => AddMessage(message));
        }

        private readonly GridView<LogMessage> messageList = new GridView<LogMessage>
        {
            AllowMultipleSelection = true,
            Columns =
            {
                new GridColumn
                {
                    HeaderText = "Time",
                    DataCell = new TextBoxCell
                    {
                        Binding = Eto.Forms.Binding.Property<LogMessage, string>(m => m.Time.ToLongTimeString())
                    }
                },
                new GridColumn
                {
                    HeaderText = "Level",
                    DataCell = new TextBoxCell
                    {
                        Binding = Eto.Forms.Binding.Property<LogMessage, string>(m => m.Level.GetName())
                    }
                },
                new GridColumn
                {
                    HeaderText = "Group",
                    DataCell = new TextBoxCell
                    {
                        Binding = Eto.Forms.Binding.Property<LogMessage, string>(m => m.Group)
                    }
                },
                new GridColumn
                {
                    HeaderText = "Message",
                    DataCell = new TextBoxCell
                    {
                        Binding = Eto.Forms.Binding.Property<LogMessage, string>(m => m.Message)
                    }
                }
            }
        };

        private readonly LogDataStore messageStore = new LogDataStore();

        private void AddMessage(LogMessage message)
        {
            Application.Instance.AsyncInvoke(() => this.messageStore.Add(message));
        }

        private static void Copy(IEnumerable<LogMessage> messages)
        {
            if (messages.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (var message in messages)
                {
                    var line = Log.GetStringFormat(message);
                    sb.AppendLine(line);
                }
                Clipboard.Instance.Clear();
                Clipboard.Instance.Text = sb.ToString();
            }
        }

        private class FilterDropDown  : EnumDropDown<LogLevel>
        {
            public FilterDropDown(LogLevel activeFilter = LogLevel.Info)
            {
                base.SelectedValue = activeFilter;
            }
        }
    }
}