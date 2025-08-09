using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows
{
    public class WindowSingleton<T> where T : Window, new()
    {
        private readonly object sync = new();
        private T window;

        public T GetWindow()
        {
            lock (sync)
            {
                if (window == null)
                {
                    window = new T();
                    window.Closed += HandleWindowClosed;
                }

                return window;
            }
        }

        public void Show()
        {
            var window = GetWindow();

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
