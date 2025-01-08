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
            string message = errorData.Message + Environment.NewLine + errorData.StackTrace;
            Log.Write(
                errorData.TypeName,
                message,
                LogLevel.Error
            );
            if (MessageBoxActive)
                return;
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

        public static SaveFileDialog SaveFileDialog(string title, string directory, FileFilter[] filters)
        {
            var fileDialog = new SaveFileDialog();

            SetupFileDialog(fileDialog, title, directory, filters, "Save File");

            return fileDialog;
        }

        public static OpenFileDialog OpenFileDialog(string title, string directory, FileFilter[] filters, bool multiSelect = false)
        {
            var fileDialog = new OpenFileDialog();

            SetupFileDialog(fileDialog, title, directory, filters, "Open File");
            fileDialog.MultiSelect = multiSelect;

            return fileDialog;
        }

        private static void SetupFileDialog(FileDialog fileDialog, string title, string directory, FileFilter[] filters, string defaultTitle)
        {
            fileDialog.Title = !string.IsNullOrEmpty(title) ? title : defaultTitle;

            if (filters != null)
                foreach (var filter in filters)
                    fileDialog.Filters.Add(filter);

            if (!string.IsNullOrEmpty(directory))
                fileDialog.Directory = new Uri(directory);
        }
    }
}
