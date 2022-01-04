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

            this.Items.Add(new StackLayoutItem(messageList, HorizontalAlignment.Stretch, true));
            this.Items.Add(new StackLayoutItem(toolbar, HorizontalAlignment.Stretch));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var currentMessages = await App.Driver.Instance.GetCurrentLog();
            messageList.DataStore = messageStore = new LogDataStore(currentMessages);

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

            App.Driver.Message += (sender, message) => AddMessage(message);
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
                        Binding = Binding.Property<LogMessage, string>(m => m.Time.ToLongTimeString())
                    }
                },
                new GridColumn
                {
                    HeaderText = "Level",
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<LogMessage, string>(m => m.Level.GetName())
                    }
                },
                new GridColumn
                {
                    HeaderText = "Group",
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<LogMessage, string>(m => m.Group)
                    }
                },
                new GridColumn
                {
                    HeaderText = "Message",
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<LogMessage, string>(m => m.Message)
                    }
                }
            }
        };

        private LogDataStore messageStore;

        private void AddMessage(LogMessage message)
        {
            Application.Instance.AsyncInvoke(() => this.messageStore.Add(message));

            if (message.Notification)
            {
                var notify = new Notification
                {
                    Title = "OpenTabletDriver - " + message.Level.ToString(),
                    Message = message.Message,
                    ContentImage = App.Logo,
                    ID = "log-message-notification"
                };
                notify.Show();
            }
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

        private class FilterDropDown : EnumDropDown<LogLevel>
        {
            public FilterDropDown(LogLevel activeFilter = LogLevel.Info)
            {
                SelectedValue = activeFilter;
            }
        }
    }
}
