using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using Eto.Forms;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Dialogs;

namespace OpenTabletDriver.UX
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public abstract class App : NotifyPropertyChanged
    {
        protected App(string platform, string[] args)
        {
            Platform = platform;
            Arguments = args;
        }

        public void Start()
        {
            var serviceCollection = new UXServiceCollection(this);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _app = new Application(Platform)
            {
                Name = "OpenTabletDriver"
            };

            _app.UnhandledException += UnhandledException;
            ApplyStyles();

            var mainForm = _serviceProvider.GetRequiredService<MainForm>();
            mainForm.Closed += (_, _) => Exit();

            var command = new RootCommand("OpenTabletDriver UX")
            {
                Name = "OpenTabletDriver",
                TreatUnmatchedTokensAsErrors = true,
                Handler = CommandHandler.Create<bool>(minimized =>
                {
                    if (minimized)
                    {
                        mainForm.WindowState = WindowState.Minimized;
                    }
                })
            };

            // We're hooking up the log output handler later than when the log output handler expects
            // This may cause missing log messages
            var rpc = _serviceProvider.GetRequiredService<RpcClient<IDriverDaemon>>();
            Log.Output += (_, message) => LogOutputHandler(rpc, message).Run();

            // Force unobserved exceptions to be considered observed, stops hanging on async exceptions.
            TaskScheduler.UnobservedTaskException += (_, args) => args.SetObserved();

            var code = command.Invoke(Arguments);
            if (code != 0)
                Exit(code);

            // Show the tray icon, if the platform supports it as intended.
            if (EnableTray)
                _serviceProvider.GetRequiredService<TrayIcon>().Show();

            _app.Run(mainForm);
        }

        private void UnhandledException(object? sender, Eto.UnhandledExceptionEventArgs args) => Application.Instance.AsyncInvoke(() =>
        {
            try
            {
                var ex = args.ExceptionObject as Exception;
                ex?.Show();
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2);
            }
        });

        // Some fields are suppressed because they're all initialized in Start() rather than the ctor()
        private Application _app = null!;
        private IServiceProvider _serviceProvider = null!;
        private Settings _settings = null!;
        private ObservableCollection<TabletConfiguration> _tablets = new ObservableCollection<TabletConfiguration>();
        private ObservableCollection<IDisplay> _displays = null!;
        private string _mainFormTitle = DefaultTitle;

        private static string DefaultTitle { get; } = $"OpenTabletDriver v{Metadata.FullVersion}";

        /// <summary>
        /// The <see cref="Eto.Platform"/> this app was built with.
        /// </summary>
        public string Platform { get; }

        /// <summary>
        /// The command line arguments passed to the app.
        /// </summary>
        public string[] Arguments { get; }

        /// <summary>
        /// The client plugin manager, used to load clientside plugins.
        /// </summary>
        public IPluginManager PluginManager { set; get; } = null!;

        /// <summary>
        /// The client plugin factory, used to reflect clientside plugins.
        /// </summary>
        public IPluginFactory PluginFactory { set; get; } = null!;

        /// <summary>
        /// The local client instance of Settings.
        /// This is synchronized when <see cref="IDriverDaemon.ApplySettings"/> is invoked.
        /// </summary>
        public Settings Settings
        {
            set => RaiseAndSetIfChanged(ref _settings!, value);
            get => _settings;
        }

        /// <summary>
        /// Collection of all currently connected tablets.
        /// </summary>
        public ObservableCollection<TabletConfiguration> Tablets
        {
            set
            {
                RaiseAndSetIfChanged(ref _tablets!, value);

                var sb = new StringBuilder(DefaultTitle);
                if (_tablets.Any())
                {
                    sb.Append(" - ");
                    sb.AppendJoin(", ", _tablets.Select(t => t.Name).Take(3));
                }

                MainFormTitle = sb.ToString();
            }
            get => _tablets;
        }

        public ObservableCollection<IDisplay> Displays
        {
            set => RaiseAndSetIfChanged(ref _displays!, value);
            get => _displays;
        }

        public string MainFormTitle
        {
            set => RaiseAndSetIfChanged(ref _mainFormTitle!, value);
            get => _mainFormTitle;
        }

        /// <summary>
        /// <inheritdoc cref="Exit()"/> with an optional exit code
        /// </summary>
        /// <param name="code">The application exit code</param>
        public virtual void Exit(int code)
        {
            Environment.Exit(code);
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        public void Exit() => Exit(0x0);

        /// <summary>
        /// Synchronize with the daemon.
        /// This will wipe any local changes.
        /// </summary>
        public async Task Synchronize()
        {
            var daemon = _serviceProvider.GetRequiredService<IDriverDaemon>();
            daemon.TabletsChanged += (_, t) => _app.AsyncInvoke(() => Tablets = new ObservableCollection<TabletConfiguration>(t));
            daemon.SettingsChanged += (_, s) => _app.AsyncInvoke(() => Settings = s);
            daemon.AssembliesChanged += (_, _) =>
            {
                _app.AsyncInvoke(SynchronizePlugins(daemon).Run);
            };

            Tablets = new ObservableCollection<TabletConfiguration>(await daemon.GetTablets());
            Displays = new ObservableCollection<IDisplay>(await daemon.GetDisplays());

            Settings = await daemon.GetSettings();

            var appInfo = await daemon.GetApplicationInfo();
            PluginManager = _serviceProvider.CreateInstance<PluginManager>(appInfo);
            PluginFactory = _serviceProvider.CreateInstance<PluginFactory>(PluginManager);

            await SynchronizePlugins(daemon);
        }

        private async Task SynchronizePlugins(IDriverDaemon daemon)
        {
            PluginManager.Load();
            if (!await daemon.CheckAssemblyHashes(PluginManager.GetStateHash()))
            {
                ShowDialog<FatalErrorDialog>(_app.MainForm, "Client plugin manager is not synchronized to the daemon.");
                Exit(3);
            }
        }

        /// <summary>
        /// Desynchronize from the daemon.
        /// </summary>
        public void Desynchronize()
        {
            Tablets.Clear();
            Settings = new Settings();
        }

        /// <summary>
        /// Loads settings then synchronizes with the daemon.
        /// </summary>
        /// <param name="filePath">The file path to load settings from.</param>
        public async Task LoadSettings(string filePath)
        {
            if (File.Exists(filePath))
            {
                var daemon = GetDriverDaemon();
                if (Settings.TryDeserialize(new FileInfo(filePath), out var settings))
                    await daemon.ApplySettings(settings);
            }
        }

        /// <summary>
        /// Synchronizes client settings with the daemon and saves.
        /// </summary>
        public async Task SaveSettings()
        {
            await GetDriverDaemon().SaveSettings(Settings);
        }

        /// <summary>
        /// Synchronizes client settings with the daemon without saving.
        /// </summary>
        public async Task ApplySettings()
        {
            await GetDriverDaemon().ApplySettings(Settings);
        }

        /// <summary>
        /// Discards client settings changes and resynchronizes settings with the daemon.
        /// </summary>
        public async Task DiscardSettings()
        {
            Settings = await GetDriverDaemon().GetSettings();
        }

        /// <summary>
        /// Discards client and daemon settings, resetting to defaults.
        /// </summary>
        public async Task ResetSettings()
        {
            await GetDriverDaemon().ResetSettings();
        }

        /// <inheritdoc cref="ShowWindow{TWindow}(object[])"/>
        public void ShowWindow<TWindow>() where TWindow : Form => ShowWindow<TWindow>(Array.Empty<object>());

        /// <summary>
        /// Shows an instance of a window.
        /// </summary>
        /// <typeparam name="TWindow">The window type to show.</typeparam>
        public void ShowWindow<TWindow>(params object[] deps) where TWindow : Form
        {
            if (Application.Instance.Windows.FirstOrDefault(w => w is TWindow) is not TWindow window)
            {
                _app.AsyncInvoke(_serviceProvider.GetOrCreateInstance<TWindow>(deps).Show);
            }
            else
            {
                window.BringToFront();
                window.Focus();
            }
        }

        /// <summary>
        /// Shows an instance of a dialog.
        /// </summary>
        /// <param name="parent">The parent window.</param>
        /// <param name="args">Additional arguments to pass to the <typeparamref name="TDialog"/> constructor.</param>
        /// <typeparam name="TDialog">The dialog type to show.</typeparam>
        public TDialog ShowDialog<TDialog>(Window parent, params object[] args) where TDialog : Dialog
        {
            if (Application.Instance.Windows.FirstOrDefault(w => w is TDialog) is not TDialog dialog)
            {
                dialog = _serviceProvider.GetOrCreateInstance<TDialog>(args);
                dialog.ShowModal(parent);
            }
            else
            {
                dialog.BringToFront();
                dialog.Focus();
            }

            return dialog;
        }

        /// <summary>
        /// Opens a uri with intended default application.
        /// </summary>
        /// <param name="uri">The uri to open</param>
        /// <param name="isDirectory">Whether this uri is a filesystem directory</param>
        public virtual void Open(string? uri, bool isDirectory = false)
        {
            if (uri != null)
                OpenInternal(uri, isDirectory);
        }

        /// <summary>
        /// Determines whether to use the tray icon.
        /// This affects the MainForm minimize behavior.
        /// </summary>
        protected abstract bool EnableTray { get; }

        /// <summary>
        /// Apply platform-specific styles.
        /// </summary>
        protected abstract void ApplyStyles();

        /// <inheritdoc cref="Open"/>
        protected abstract void OpenInternal(string uri, bool isDirectory);

        /// <summary>
        /// Starts the OpenTabletDriver daemon, if applicable.
        /// </summary>
        public abstract void StartDaemon();

        /// <summary>
        /// Stops OpenTabletDriver daemon, if running.
        /// </summary>
        public abstract void StopDaemon();

        /// <summary>
        /// The event handler for all client <see cref="Log.Write(OpenTabletDriver.Logging.LogMessage)"/> calls.
        /// </summary>
        private static async Task LogOutputHandler(RpcClient<IDriverDaemon> rpc, LogMessage message)
        {
            try
            {
                if (rpc.Instance != null)
                    await rpc.Instance.WriteMessage(message);
            }
            catch (Exception ex)
            {
                ex.Show();
            }
        }

        private IDriverDaemon GetDriverDaemon()
        {
            return _serviceProvider.GetRequiredService<IDriverDaemon>();
        }
    }
}
