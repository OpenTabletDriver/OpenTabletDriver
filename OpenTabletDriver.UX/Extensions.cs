using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        #nullable enable

        public static T BuildFileDialog<T>(string? title, string? directory, FileFilter[]? filters, bool? multiSelect = null)
            where T : FileDialog, new()
        {
            var fileDialog = new T();

            var defaultTitle = fileDialog switch
            {
                Eto.Forms.OpenFileDialog => "Open File",
                Eto.Forms.SaveFileDialog => "Save File",
                _ => string.Empty,
            };
            var dialogTitle = !string.IsNullOrEmpty(title) ? title : defaultTitle;
            if (!string.IsNullOrEmpty(dialogTitle))
                fileDialog.Title = dialogTitle;

            if (filters != null)
                fileDialog.AddRangeToFilters(filters);;

            if (!string.IsNullOrEmpty(directory))
                fileDialog.Directory = new Uri(directory);

            if (fileDialog is OpenFileDialog openFileDialog && multiSelect.HasValue)
                openFileDialog.MultiSelect = multiSelect.Value;
            else if (multiSelect.HasValue)
                Debug.Fail("Multiselect set without compatible file dialog type");

            return fileDialog;
        }

        public static OpenFileDialog OpenFileDialog(string? title, string? directory, FileFilter[]? filters, bool? multiSelect = null) =>
            BuildFileDialog<OpenFileDialog>(title, directory, filters, multiSelect);

        public static SaveFileDialog SaveFileDialog(string? title, string? directory, FileFilter[]? filters) =>
            BuildFileDialog<SaveFileDialog>(title, directory, filters);

        public static void AddRangeToFilters(this FileDialog fileDialog, IEnumerable<FileFilter> filters)
        {
            foreach (var filter in filters)
                fileDialog.Filters.Add(filter);
        }
    }
}
