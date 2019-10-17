using System.Collections.Generic;
using Avalonia.Controls;

namespace OpenTabletDriverGUI
{
    internal static class FileDialogs
    {
        public static SaveFileDialog CreateSaveFileDialog(string title, string type, string extension)
        {
            return new SaveFileDialog()
            {
                Title = title,
                Filters = CreateFilterList(type, extension)
            };
        }

        public static OpenFileDialog CreateOpenFileDialog(string title, string type, string extension)
        {
            return new OpenFileDialog()
            {
                Title = title,
                Filters = CreateFilterList(type, extension)
            };
        }

        public static List<FileDialogFilter> CreateFilterList(string type, string extension)
        {
            return new List<FileDialogFilter>()
            {
                CreateFilter($"{type} (.{extension})", extension),
                CreateFilter("All files", "*")
            };
        }

        public static FileDialogFilter CreateFilter(string name, string extension)
        {
            return new FileDialogFilter
            {
                Name = $"{name}",
                Extensions = new List<string>
                {
                    extension
                }
            };
        }
    }
}