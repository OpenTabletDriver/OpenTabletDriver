using Eto.Forms;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Dialogs;
using OpenTabletDriver.UX.Services;

namespace OpenTabletDriver.UX
{
    using static ServiceDescriptor;
    using static DependencyInjectionExtensions;

    public class UXServiceCollection : ClientServiceCollection
    {
        private static readonly IEnumerable<ServiceDescriptor> RequiredServices = new[]
        {
            Transient<IControlBuilder, ControlBuilder>(),
            Transient(p => p.GetRequiredService<App>().PluginManager),
            Transient(p => p.GetRequiredService<App>().PluginFactory),
            Singleton<IKeysProvider, EtoKeysProvider>(),
            Singleton<MainForm>(),
            Singleton<TrayIcon>(),
            Transient<AboutDialog, OfflineAboutDialog>()
        };

        public UXServiceCollection(App app)
        {
            this.AddServices(RequiredServices);
            this.AddSingleton(app);
        }
    }
}
