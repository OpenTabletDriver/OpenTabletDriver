using Eto.Forms;
using Eto.Drawing;
using OpenTabletDriverUX.Controls;
using System.IO;
using TabletDriverPlugin.Tablet;
using System.Threading.Tasks;
using TabletDriverLib;
using TabletDriverPlugin;

namespace OpenTabletDriverUX
{
    public partial class MainForm : Form, IViewModelRoot<MainFormViewModel>
	{
		public MainForm()
		{
			this.DataContext = new MainFormViewModel();
			
			Title = "OpenTabletDriver";
			ClientSize = new Size(960, 730);
			MinimumSize = new Size(960, 730);
			Icon = App.Logo.WithSize(256, 256);

			displayAreaEditor = new AreaEditor();
			ViewModel.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(ViewModel.Settings))
				{
					displayAreaEditor.Bind(c => c.ViewModel.Width, ViewModel.Settings, m => m.DisplayWidth);
					displayAreaEditor.Bind(c => c.ViewModel.Height, ViewModel.Settings, m => m.DisplayHeight);
					displayAreaEditor.Bind(c => c.ViewModel.X, ViewModel.Settings, m => m.DisplayX);
					displayAreaEditor.Bind(c => c.ViewModel.Y, ViewModel.Settings, m => m.DisplayY);
				}
			};
			
			var displayAreaGroup = new GroupBox
			{
				Text = "Display Area",
				Padding = new Padding(5),
				Content = displayAreaEditor
			};

			tabletAreaEditor = new AreaEditor();
			ViewModel.PropertyChanged += (sender, e) => 
			{
				if (e.PropertyName == nameof(ViewModel.Settings))
				{
					tabletAreaEditor.Bind(c => c.ViewModel.Width, ViewModel.Settings, m => m.TabletWidth);
					tabletAreaEditor.Bind(c => c.ViewModel.Height, ViewModel.Settings, m => m.TabletHeight);
					tabletAreaEditor.Bind(c => c.ViewModel.X, ViewModel.Settings, m => m.TabletX);
					tabletAreaEditor.Bind(c => c.ViewModel.Y, ViewModel.Settings, m => m.TabletY);
					tabletAreaEditor.Bind(c => c.ViewModel.Rotation, ViewModel.Settings, m => m.TabletRotation);
				}
			};

			var tabletAreaGroup = new GroupBox
			{
				Text = "Tablet Area",
				Padding = new Padding(5),
				Content = tabletAreaEditor
			};
			
			var outputConfigContent = new TableLayout
			{
				Padding = new Padding(5),
				Rows = 
				{
					new TableRow(new TableCell(displayAreaGroup, true))
					{
						ScaleHeight = true
					},
					new TableRow(new TableCell(tabletAreaGroup, true))
					{
						ScaleHeight = true
					}
				}
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

			var aboutCommand = new Command { MenuText = "About...", Shortcut = Keys.F1 };
			aboutCommand.Executed += (sender, e) => App.AboutDialog.ShowDialog(this);

			var resetSettings = new Command { MenuText = "Reset to defaults" };
			resetSettings.Executed += async (sender, e) => await ResetSettings();

			var loadSettings = new Command { MenuText = "Load settings...", Shortcut = Application.Instance.CommonModifier | Keys.O };
			loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

			var saveSettingsAs = new Command { MenuText = "Save settings as...", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S };
			saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

			var saveSettings = new Command { MenuText = "Save settings", Shortcut = Application.Instance.CommonModifier | Keys.S };
			saveSettings.Executed += async (sender, e) => await SaveSettings();

			var applySettings = new Command { MenuText = "Apply settings", Shortcut = Application.Instance.CommonModifier | Keys.Enter };
			applySettings.Executed += async (sender, e) => await ApplySettings();

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
							resetSettings,
							applySettings
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
			if (await App.DriverDaemon.InvokeAsync(d => d.GetTablet()) is TabletProperties tablet)
			{
				SetTabletAreaDimensions(tablet);
			}
			else
			{
				await DetectAllTablets();
			}

			if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
			{
				ViewModel.Settings = settings;
			}
			else if (AppInfo.SettingsFile.Exists)
			{
				ViewModel.Settings = Settings.Deserialize(AppInfo.SettingsFile);
				await App.DriverDaemon.InvokeAsync(d => d.SetSettings(ViewModel.Settings));
			}
			else
			{
				await ResetSettings();
			}

			var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
			displayAreaEditor.ViewModel.MaxWidth = virtualScreen.Width;
			displayAreaEditor.ViewModel.MaxHeight = virtualScreen.Height;
		}

		private AreaEditor displayAreaEditor, tabletAreaEditor;

		public MainFormViewModel ViewModel
		{
			set => this.DataContext = value;
			get => (MainFormViewModel)this.DataContext;
		}

		private async Task ResetSettings()
		{
			var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
			var tablet = await App.DriverDaemon.InvokeAsync(d => d.GetTablet());
			ViewModel.Settings = TabletDriverLib.Settings.Defaults;
			ViewModel.Settings.DisplayWidth = virtualScreen.Width;
            ViewModel.Settings.DisplayHeight = virtualScreen.Height;
            ViewModel.Settings.DisplayX = virtualScreen.Width / 2;
            ViewModel.Settings.DisplayY = virtualScreen.Height / 2;
            ViewModel.Settings.TabletWidth = tablet?.Width ?? 0;
            ViewModel.Settings.TabletHeight = tablet?.Height ?? 0;
            ViewModel.Settings.TabletX = tablet?.Width / 2 ?? 0;
            ViewModel.Settings.TabletY = tablet?.Height / 2 ?? 0;

			await App.DriverDaemon.InvokeAsync(d => d.SetSettings(ViewModel.Settings));
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
					ViewModel.Settings = Settings.Deserialize(file);
					await App.DriverDaemon.InvokeAsync(d => d.SetSettings(ViewModel.Settings));
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

		private async Task ApplySettings()
		{
			if (ViewModel.Settings is Settings settings)
				await App.DriverDaemon.InvokeAsync(d => d.SetSettings(settings));
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
							await App.DriverDaemon.InvokeAsync(d => d.SetInputHook(settings.AutoHook));
						}
						SetTabletAreaDimensions(tablet);
						break;
					}
				}
			}
			else
			{
				Log.Write("Detect", $"Configuration directory '{AppInfo.ConfigurationDirectory.FullName}' does not exist.");
			}
		}

		private void SetTabletAreaDimensions(TabletProperties tablet)
		{
			tabletAreaEditor.ViewModel.MaxWidth = tablet.Width;
			tabletAreaEditor.ViewModel.MaxHeight = tablet.Height;
		}
	}
}
