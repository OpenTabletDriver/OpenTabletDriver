using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows
{
    public class WindowSingleton<T> where T : Form, new()
    {
        public WindowSingleton()
        {
            RefreshInternalForm();
        }

        private T form;
        private bool isClosed;

        public void ShowSingleton()
        {
            if (isClosed)
                RefreshInternalForm();

            form.Show();
            form.Focus();
        }

        private void RefreshInternalForm()
        {
            isClosed = false;
            form = new T();
            form.Closed += (_, _) => isClosed = true;
        }
    }
}