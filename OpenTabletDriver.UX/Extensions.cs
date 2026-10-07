using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Eto.Drawing;
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
        private static bool MessageBoxActive;

        public static void ShowMessageBox(this Exception exception)
        {
            if (MessageBoxActive)
                return;
            Application.Instance.Invoke(() =>
            {
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
                errorData.TypeName ?? "<unknown>",
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

        public static async Task<TabletReference?> GetTabletReference(this Profile profile)
        {
            Debug.Assert(App.Driver.IsConnected, "User shouldn't be able to ask for a tablet reference without a connected daemon");
            var tablets = await App.Driver.Instance.GetTablets();
            return tablets.FirstOrDefault(t => t.Properties.Name == profile.Tablet);
        }

        [Obsolete("Please use method specifying an initialFileName. 'null' is an acceptable value")]
        public static T BuildFileDialog<T>(string? title, string? directory, IEnumerable<FileFilter>? filters, bool? multiSelect = null)
            where T : FileDialog, new() => BuildFileDialog<T>(title, directory, filters, null, multiSelect);

        public static T BuildFileDialog<T>(string? title, string? directory, IEnumerable<FileFilter>? filters, string? initialFileName, bool? multiSelect = null)
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
                fileDialog.AddRangeToFilters(filters);

            if (!string.IsNullOrEmpty(directory))
                fileDialog.Directory = new Uri(directory);

            if (fileDialog is OpenFileDialog openFileDialog && multiSelect.HasValue)
                openFileDialog.MultiSelect = multiSelect.Value;
            else if (multiSelect.HasValue)
                Debug.Fail("Multiselect set without compatible file dialog type");

            if (!string.IsNullOrEmpty(initialFileName))
                fileDialog.FileName = initialFileName;

            return fileDialog;
        }

        public static OpenFileDialog OpenFileDialog(string? title, string? directory, IEnumerable<FileFilter>? filters, bool? multiSelect = null) =>
            BuildFileDialog<OpenFileDialog>(title, directory, filters, null, multiSelect);

        [Obsolete("Please use method specifying an initialFileName. 'null' is an acceptable value")]
        public static SaveFileDialog SaveFileDialog(string? title, string? directory, IEnumerable<FileFilter>? filters) =>
            BuildFileDialog<SaveFileDialog>(title, directory, filters);

        public static SaveFileDialog SaveFileDialog(string? title, string? directory, IEnumerable<FileFilter>? filters, string? initialFilename) =>
            BuildFileDialog<SaveFileDialog>(title, directory, filters, initialFilename);

        public static void AddRangeToFilters(this FileDialog fileDialog, IEnumerable<FileFilter> filters)
        {
            foreach (var filter in filters)
                fileDialog.Filters.Add(filter);
        }

        [Pure]
        public static SizeF Measure(this Font font, string text, int repeats = 1) =>
            font.MeasureString(
                repeats > 1
                    ? string.Concat(Enumerable.Repeat(text, repeats))
                    : text);

        /// <summary>
        /// Returns the corners of a rotated <see cref="RectangleF"/>. Corner names are assigned before the rotation transform and may not be accurate in absolute terms.
        /// </summary>
        [Pure]
        public static Corners GetAreaCorners(this RectangleF area, float rotation)
        {
            var origin = new Vector2(area.X, area.Y);
            var matrix = Matrix3x2.CreateTranslation(-origin);
            matrix *= Matrix3x2.CreateRotation((float)(rotation * Math.PI / 180));
            matrix *= Matrix3x2.CreateTranslation(origin);

            float halfWidth = area.Width / 2;
            float halfHeight = area.Height / 2;

            return new Corners
            {
                TopLeft = Vector2.Transform(new Vector2(area.X - halfWidth, area.Y + halfHeight), matrix),
                TopRight = Vector2.Transform(new Vector2(area.X + halfWidth, area.Y + halfHeight), matrix),
                BottomLeft = Vector2.Transform(new Vector2(area.X - halfWidth, area.Y - halfHeight), matrix),
                BottomRight = Vector2.Transform(new Vector2(area.X + halfWidth, area.Y - halfHeight), matrix),
            };
        }

        [Pure]
        public static RectangleF GetRectangleF(this AreaSettings areaSettings) =>
            new(areaSettings.X, areaSettings.Y, areaSettings.Width, areaSettings.Height);


        public record struct Corners(Vector2 TopLeft, Vector2 TopRight, Vector2 BottomLeft, Vector2 BottomRight);

        [Pure]
        public static Vector2[] ToArray(this Corners corners)
        {
            return [corners.TopLeft, corners.TopRight, corners.BottomLeft, corners.BottomRight];
        }
    }
}
