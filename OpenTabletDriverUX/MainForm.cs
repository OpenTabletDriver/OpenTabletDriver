using Eto.Forms;
using Eto.Drawing;
using OpenTabletDriverUX.Controls;
using TabletDriverLib.Contracts;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using TabletDriverPlugin.Tablet;
using System.Threading.Tasks;
using System;
using TabletDriverLib.Plugins;
using TabletDriverLib;

namespace OpenTabletDriverUX
{
    public partial class MainForm : Form
	{
		public MainForm()
		{
			// Register IPC Clients
			ServiceProvider serviceProvider = new ServiceCollection()
				.AddNamedPipeIpcClient<IDriverDaemon>("OpenTabletDriverUX", "OpenTabletDriver")
				.BuildServiceProvider();

			// Resolve IPC client factory
			IIpcClientFactory<IDriverDaemon> clientFactory = serviceProvider
				.GetRequiredService<IIpcClientFactory<IDriverDaemon>>();

			// Create client
			DriverDaemon = clientFactory.CreateClient("OpenTabletDriverUX");

			// Window
			Title = "OpenTabletDriver";
			ClientSize = new Size(960, 720);
			MinimumSize = new Size(960, 720);
			Icon = AppInfo.Logo.WithSize(256, 256);

			var mainTabLayout = new DynamicLayout();
			mainTabLayout.BeginVertical();

			var displayGroup = new GroupBox
			{
				Text = "Display Area",
				Padding = new Padding(5),
				Content = new AreaEditor()
			};
			mainTabLayout.AddRow(displayGroup);
			mainTabLayout.EndVertical();
			
			mainTabLayout.BeginVertical();
			var tabletGroup = new GroupBox
			{
				Text = "Tablet Area",
				Padding = new Padding(5),
				Content = new AreaEditor()
			};
			mainTabLayout.AddRow(tabletGroup);
			mainTabLayout.EndVertical();

			// Main Content
			Content = new TabControl
			{
				Pages = 
				{
					// Main Tab
					new TabPage
					{
						Text = "Output Configuration",
						Content = mainTabLayout
					},
					new TabPage
					{
						Text = "Bindings",
						Content = new StackLayout
						{
							
						}
					},
					new TabPage
					{
						Text = "Filters",
						Content = new StackLayout
						{
							
						}
					},
					new TabPage
					{
						Text = "Plugins",
						Content = new StackLayout
						{
							
						}
					},
					new TabPage
					{
						Text = "Console",
						Content = new TableLayout
						{

						}
					}
				}
			};

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => AppInfo.AboutDialog.ShowDialog(this);

			var resetSettings = new Command { MenuText = "Reset to defaults" };
			resetSettings.Executed += async (sender, e) => await ResetSettings();

			var loadSettings = new Command { MenuText = "Load settings..." };
			loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

			var saveSettingsAs = new Command { MenuText = "Save settings as..." };
			saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

			var saveSettings = new Command { MenuText = "Save settings" };
			saveSettings.Executed += async (sender, e) => await SaveSettings();

			var detectTablet = new Command { MenuText = "Detect tablet" };
			// TODO: Detect tablet
			detectTablet.Executed += async (sender, e) => await DetectAllTablets();

			var showTabletDebugger = new Command { MenuText = "Tablet debugger..." };
			// TODO: Show tablet debugger

			// create menu
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
							loadSettings,
							saveSettingsAs,
							resetSettings
						}
					},
					// Tablets submenu
					new ButtonMenuItem
					{
						Text = "Tablets",
						Items =
						{
							detectTablet,
							showTabletDebugger
						}
					}
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}

		private IIpcClient<IDriverDaemon> DriverDaemon { set; get; }

		private async Task ResetSettings()
		{
			var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
			var tablet = await DriverDaemon.InvokeAsync(d => d.GetTablet());
			var settings = Settings.Defaults;
			settings.DisplayWidth = virtualScreen.Width;
            settings.DisplayHeight = virtualScreen.Height;
            settings.DisplayX = virtualScreen.Width / 2;
            settings.DisplayY = virtualScreen.Height / 2;
            settings.TabletWidth = tablet?.Width ?? 0;
            settings.TabletHeight = tablet?.Height ?? 0;
            settings.TabletX = tablet?.Width / 2 ?? 0;
            settings.TabletY = tablet?.Height / 2 ?? 0;

			await DriverDaemon.InvokeAsync(d => d.SetSettings(settings));
		}

        private async Task LoadSettingsDialog()
		{
			var fileDialog = new OpenFileDialog
			{
				Title = "Load OpenTabletDriver settings...",
				Filters =
				{
					new FileFilter("OpenTabletDriver Settings (*.json)", ".json")
				}
			};
			switch (fileDialog.ShowDialog(this))
			{
				case DialogResult.Ok:
				case DialogResult.Yes:
					// TODO: Load settings
					var file = new FileInfo(fileDialog.FileName);
					var settings = Settings.Deserialize(file);
					await DriverDaemon.InvokeAsync(d => d.SetSettings(settings));
					break;
			}
		}

		private async Task SaveSettingsDialog()
		{
			var fileDialog = new SaveFileDialog
			{
				Title = "Save OpenTabletDriver settings...",
				Filters = 
				{
					new FileFilter("OpenTabletDriver Settings (*.json)", ".json")
				}
			};
			switch (fileDialog.ShowDialog(this))
			{
				case DialogResult.Ok:
				case DialogResult.Yes:
					var file = new FileInfo(fileDialog.FileName);
					var settings = await DriverDaemon.InvokeAsync(d => d.GetSettings());
					settings?.Serialize(file);
					break;
			}
		}

		private async Task SaveSettings()
		{
			var settings = await DriverDaemon.InvokeAsync(d => d.GetSettings());
			settings.Serialize(AppInfo.SettingsFile);
		}

		private async Task DetectAllTablets()
		{
			foreach (var path in Directory.GetFiles(AppInfo.ConfigurationDirectory.FullName, "*.json", SearchOption.AllDirectories))
			{
				var file = new FileInfo(path);
				var tablet = TabletProperties.Read(file);
				if (await DriverDaemon.InvokeAsync(d => d.SetTablet(tablet)))
				{
					var settings = await DriverDaemon.InvokeAsync(d => d.GetSettings());
					if (settings != null)
					{
						await DriverDaemon.InvokeAsync(d=> d.SetInputHook(settings.AutoHook));
					}
					break;
				}
			}
		}
	}
}
