using Eto.Forms;
using OpenTabletDriver.UX.ViewModels;
using OpenTabletDriver.UX.Views.Windows;

namespace OpenTabletDriver.UX
{
    public class App : Application
    {
        public App(string platform) : base(platform)
        {
            Name = "OpenTabletDriver";
        }

        public void Start()
        {
            var mainForm = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };

            Run(mainForm);
        }

        protected override void OnInitialized(EventArgs e)
        {
            // TODO: set styles here

            base.OnInitialized(e);
        }
    }
}
