using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eto.Forms;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTabletDriver.UX.Tools;
using TabletDriverLib.Contracts;
using TabletDriverPlugin;
using TabletDriverPlugin.Logging;

namespace OpenTabletDriver.UX.Controls
{
    public class LogView : StackLayout, ILogServer
    {
        public LogView()
        {
            this.Orientation = Orientation.Vertical;

            var toolbar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Items = 
                {
                    GenerateFilterControl()
                }
            };
            
            this.Items.Add(new StackLayoutItem(logList, HorizontalAlignment.Stretch, true));
            this.Items.Add(new StackLayoutItem(toolbar, HorizontalAlignment.Stretch));

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var currentMessages = from message in await App.DriverDaemon.InvokeAsync(d => d.GetCurrentLog())
                where message is LogMessage
                select message;

            foreach (var message in currentMessages)
                AddItem(message);
            
            var exitHandle = new CancellationTokenSource();

            var serverGuid = await App.DriverDaemon.InvokeAsync(d => d.SetLogOutput(true));
            var host = CreateHostBuilder(serverGuid).Build();
            
            this.ParentWindow.Closing += async (sender, e) =>
            {
                await App.DriverDaemon.InvokeAsync(d => d.SetLogOutput(false));
                await host.StopAsync();
                host.Dispose();
            };

            await host.StartAsync(exitHandle.Token);
        }

        private IHostBuilder CreateHostBuilder(Guid guid) => 
            Host.CreateDefaultBuilder()
                .ConfigureServices(services => 
                {
                    services.AddSingleton<ILogServer, LogView>((s) => this);
                })
                .ConfigureIpcHost(builder => 
                {
                    builder.AddNamedPipeEndpoint<ILogServer>(guid.ToString());
                });

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

        private ListBox logList = new ListBox();
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

        private void AddItem(LogMessage message)
        {
            Messages.Add(message);
            if (message.Level >= CurrentFilter)
            {
                var str = Log.GetStringFormat(message);
                logList.Items.Add(str);
            }
        }

        private void Refresh()
        {
            logList.Items.Clear();

            var currentMessages = from message in Messages
                where message.Level >= CurrentFilter
                select new ListItem
                {
                    Text = Log.GetStringFormat(message)
                };

            logList.Items.AddRange(currentMessages);
        }

        public async void Post(LogMessage message)
        {
            await Application.Instance.InvokeAsync(() => AddItem(message));
        }
    }
}