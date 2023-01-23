using System.Diagnostics;
using Eto.Forms;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.ViewModels;

namespace OpenTabletDriver.UX.Views.Windows
{
    public class MainWindow : Form
    {
        public MainWindow()
        {
            Title = "OpenTabletDriver";

            var textBox = new Label();
            var textBoxBinding = Binding.Property((MainWindowViewModel vm) => vm.IsConnected).Convert(b => b ? "Connected" : "Disconnected");
            textBox.TextBinding.BindDataContext(textBoxBinding);

            Content = new DockingLayout
            {
                Left = new Sidebar()
                {
                    Items =
                    {
                        new SidebarItem
                        {
                            Content = textBox
                        }
                    }
                }
            };
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            Debug.Assert(DataContext is MainWindowViewModel);
            base.OnDataContextChanged(e);
        }
    }
}
