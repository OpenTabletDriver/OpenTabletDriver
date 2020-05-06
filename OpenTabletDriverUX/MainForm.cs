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
using TabletDriverPlugin;

namespace OpenTabletDriverUX
{
    public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "OpenTabletDriver";
			ClientSize = new Size(960, 720);
			MinimumSize = new Size(960, 720);
			Icon = App.Logo.WithSize(256, 256);

			displayAreaEditor = new AreaEditor();

			tabletAreaEditor = new AreaEditor();
			
			var outputConfigContent = new StackLayout
			{
				Orientation = Orientation.Vertical,
				Padding = new Padding(5),
				Items = 
				{
					new StackLayoutItem(new GroupBox
					{
						Text = "Display Area",
						Padding = new Padding(5),
						Content = displayAreaEditor
					}, true),
					new StackLayoutItem(new GroupBox
					{
						Text = "Tablet Area",
						Padding = new Padding(5),
						Content = tabletAreaEditor
					}, true)
				}
			};
			this.SizeChanged += (sender, e) => 
			{
				outputConfigContent.Width = ParentWindow.ClientSize.Width;
				foreach (var child in outputConfigContent.Children)
					child.Width = outputConfigContent.Width - 10;
			};
			
			// Main Content
			Content = new TabControl
			{
				Pages = 
				{
					// Main Tab
					new TabPage
					{
						Text = "Output Configuration",
						// Content = mainTabLayout
						Content = outputConfigContent
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
			aboutCommand.Executed += (sender, e) => App.AboutDialog.ShowDialog(this);

			var resetSettings = new Command { MenuText = "Reset to defaults" };
			resetSettings.Executed += async (sender, e) => await ResetSettings();

			var loadSettings = new Command { MenuText = "Load settings...", Shortcut = Application.Instance.CommonModifier | Keys.O };
			loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

			var saveSettingsAs = new Command { MenuText = "Save settings as..." };
			saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

			var saveSettings = new Command { MenuText = "Save settings", Shortcut = Application.Instance.CommonModifier | Keys.S };
			saveSettings.Executed += async (sender, e) => await SaveSettings();

			var detectTablet = new Command { MenuText = "Detect tablet", Shortcut = Application.Instance.CommonModifier | Keys.D };
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
							saveSettings,
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

			InitializeAsync();
		}

		public async void InitializeAsync()
		{
			if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
			{
				App.Settings = settings;
			}
			else if (App.Settings is Settings appSettings)
			{
				await App.DriverDaemon.InvokeAsync(d => d.SetSettings(appSettings));
			}
			else if (AppInfo.SettingsFile.Exists)
			{
				App.Settings = Settings.Deserialize(AppInfo.SettingsFile);
			}

			if (await App.DriverDaemon.InvokeAsync(d => d.GetTablet()) is TabletProperties tablet)
			{
				if (App.Settings == null)
				{
					await ResetSettings();
				}
			}
			else
			{
				await DetectAllTablets();
				if (App.Settings == null)
				{
					await ResetSettings();
				}
			}
		}

		private AreaEditor displayAreaEditor, tabletAreaEditor;

		private async Task ResetSettings()
		{
			var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
			var tablet = await App.DriverDaemon.InvokeAsync(d => d.GetTablet());
			App.Settings = TabletDriverLib.Settings.Defaults;
			App.Settings.DisplayWidth = virtualScreen.Width;
            App.Settings.DisplayHeight = virtualScreen.Height;
            App.Settings.DisplayX = virtualScreen.Width / 2;
            App.Settings.DisplayY = virtualScreen.Height / 2;
            App.Settings.TabletWidth = tablet?.Width ?? 0;
            App.Settings.TabletHeight = tablet?.Height ?? 0;
            App.Settings.TabletX = tablet?.Width / 2 ?? 0;
            App.Settings.TabletY = tablet?.Height / 2 ?? 0;

			await App.DriverDaemon.InvokeAsync(d => d.SetSettings(App.Settings));
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
					var file = new FileInfo(fileDialog.FileName);
					App.Settings = Settings.Deserialize(file);
					await App.DriverDaemon.InvokeAsync(d => d.SetSettings(App.Settings));
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
					if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
						settings.Serialize(file);
					break;
			}
		}

		private async Task SaveSettings()
		{
			if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
				settings.Serialize(AppInfo.SettingsFile);
		}

		private async Task DetectAllTablets()
		{
			if (AppInfo.ConfigurationDirectory.Exists)
			{
				foreach (var path in Directory.GetFiles(AppInfo.ConfigurationDirectory.FullName, "*.json", SearchOption.AllDirectories))
				{
					var file = new FileInfo(path);
					var tablet = TabletProperties.Read(file);
					if (await App.DriverDaemon.InvokeAsync(d => d.SetTablet(tablet)))
					{
						var settings = await App.DriverDaemon.InvokeAsync(d => d.GetSettings());
						if (settings != null)
						{
							await App.DriverDaemon.InvokeAsync(d=> d.SetInputHook(settings.AutoHook));
						}
						break;
					}
				}
			}
			else
			{
				Log.Write("Detect", $"Configuration directory '{AppInfo.ConfigurationDirectory.FullName}' does not exist.");
			}
		}
	}
}
