using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows
{
    public class WindowSingleton<T> where T : Window, new()
    {
        private T window;

        public void Show()
        {
            if (window == null)
            {
                window = new T();
                window.Closed += HandleWindowClosed;
            }

            switch (window)
            {
                case DesktopForm desktopForm:
                    desktopForm.Show();
                    break;
                case Form form:
                    form.Show();
                    form.Focus();
                    break;
                case Dialog dialog:
                    dialog.ShowModal();
                    break;
            }
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            window = null;
        }
    }
}
