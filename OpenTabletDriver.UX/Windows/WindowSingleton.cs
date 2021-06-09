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
                case Form form:
                    form.Show();
                    break;
                case Dialog dialog:
                    dialog.ShowModal();
                    break;
            }

            window.Focus();
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            window?.Dispose();
            window = null;
        }
    }
}