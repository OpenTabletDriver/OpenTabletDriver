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
                window.Closed += (_, _) => window = null;
            }

            switch (window)
            {
                case Form:
                    (window as Form).Show();
                    break;
                case Dialog:
                    (window as Dialog).ShowModal();
                    break;
            }

            window.Focus();
        }
    }
}