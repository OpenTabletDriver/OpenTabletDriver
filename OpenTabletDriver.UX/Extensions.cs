using System;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Dialogs;
using StreamJsonRpc.Protocol;

namespace OpenTabletDriver.UX
{
    public static class Extensions
    {
        private static bool MessageBoxActive = false;

        public static void ShowMessageBox(this Exception exception)
        {
            if (MessageBoxActive)
                return;
            Application.Instance.Invoke(() =>
            {
                exception = exception.GetBaseException();

                var dialog = new ExceptionDialog(exception);
                MessageBoxActive = true;
                dialog.ShowModal(Application.Instance.MainForm);
                MessageBoxActive = false;
            });
        }

        public static void ShowMessageBox(this CommonErrorData errorData)
        {
            if (MessageBoxActive)
                return;
            string message = errorData.Message + Environment.NewLine + errorData.StackTrace;
            Log.Write(
                errorData.TypeName,
                message,
                LogLevel.Error
            );
            MessageBoxActive = true;
            MessageBox.Show(
                message,
                errorData.TypeName,
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
            MessageBoxActive = false;
        }

        public static BindableBinding<TControl, bool> GetEnabledBinding<TControl>(this TControl control) where TControl : Control
        {
            return new BindableBinding<TControl, bool>(
                control,
                (c) => c.Enabled,
                (c, v) => c.Enabled = v,
                (c, e) => c.EnabledChanged += e,
                (c, e) => c.EnabledChanged -= e
            );
        }

        public static async Task<TabletReference> GetTabletReference(this Profile profile)
        {
            var tablets = await App.Driver.Instance.GetTablets();
            return tablets.FirstOrDefault(t => t.Properties.Name == profile.Tablet);
        }
    }
}
