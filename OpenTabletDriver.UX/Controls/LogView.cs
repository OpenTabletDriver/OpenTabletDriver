using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var toolbar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Padding = new Padding(0, 5, 0, 0),
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Items = 
                {
                    GenerateFilterControl(),
                    new Button((sender, e) => Copy(GetFilteredMessages()))
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
            
            this.Items.Add(new StackLayoutItem(messageList, HorizontalAlignment.Stretch, true));
            this.Items.Add(new StackLayoutItem(toolbar, HorizontalAlignment.Stretch));

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var currentMessages = from message in await App.Driver.Instance.GetCurrentLog()
                where message is LogMessage
                select message;

            foreach (var message in currentMessages)
                AddItem(message);

            App.Driver.Instance.Message += (sender, message) => AddItem(message);
        }

        private void Copy(IEnumerable<LogMessage> messages)
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

        private Control GenerateFilterControl()
        {
            var filter = new ComboBox();
            var filterItems = EnumTools.GetValues<LogLevel>();
            foreach (var item in filterItems)
                filter.Items.Add(item.GetName());
            filter.SelectedKey = CurrentFilter.GetName();
            filter.SelectedIndexChanged += (sender, e) => 
            {
                CurrentFilter = filterItems[filter.SelectedIndex];
            };

            return filter;
        }

        private GridView<LogMessage> messageList = new GridView<LogMessage>
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

        private List<LogMessage> Messages { set; get; } = new List<LogMessage>();

        private LogLevel _currentFilter = LogLevel.Info;
        public LogLevel CurrentFilter
        {
            set
            {
                _currentFilter = value;
                Refresh();
            }
            get => _currentFilter;
        }

        private IEnumerable<LogMessage> GetFilteredMessages()
        {
            return from message in Messages
                where message.Level >= CurrentFilter
                select message;
        }

        private void Update(int index)
        {
            messageList.DataStore = GetFilteredMessages();
            messageList.ReloadData(index);
        }

        private void Update(int startIndex, int endIndex)
        {
            messageList.DataStore = GetFilteredMessages();
            messageList.ReloadData(new Range<int>(startIndex, endIndex));
        }

        private void AddItem(LogMessage message)
        {
            Application.Instance.Invoke(() => 
            {
                Messages.Add(message);

                if (message.Level >= CurrentFilter)
                    Update(Messages.Count - 1);

                if (messageList.SelectedRow == -1)
                    messageList.ScrollToRow(GetFilteredMessages().Count() - 1);
            });
        }

        private void Refresh()
        {
            Update(0, Messages.Count - 1);
        }

        public void Post(LogMessage message)
        {
            Application.Instance.AsyncInvoke(() => AddItem(message));
        }
    }
}