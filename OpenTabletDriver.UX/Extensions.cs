using System;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using StreamJsonRpc.Protocol;

namespace OpenTabletDriver.UX
{
    public static class Extensions
    {
        public static void ShowMessageBox(this Exception exception)
        {
            Log.Exception(exception);
            MessageBox.Show(
                exception.Message + Environment.NewLine + exception.StackTrace,
                $"Error: {exception.GetType().Name}",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        public static void ShowMessageBox(this CommonErrorData errorData)
        {
            string message = errorData.Message + Environment.NewLine + errorData.StackTrace;
            Log.Write(
                errorData.TypeName,
                message,
                LogLevel.Error
            );
            MessageBox.Show(
                message,
                errorData.TypeName,
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
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
