using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;

namespace OpenTabletDriverGUI
{
    internal static class FileDialogs
    {
        public static SaveFileDialog CreateSaveFileDialog(string title, string type, string extension, DirectoryInfo directory = null)
        {
            return new SaveFileDialog()
            {
                Title = title,
                Filters = CreateFilterList(type, extension),
                Directory = directory != null ? directory.FullName : Directory.GetCurrentDirectory()
            };
        }

        public static OpenFileDialog CreateOpenFileDialog(string title, string type, string extension, DirectoryInfo directory = null)
        {
            return new OpenFileDialog()
            {
                Title = title,
                Filters = CreateFilterList(type, extension),
                Directory = directory != null ? directory.FullName : Directory.GetCurrentDirectory()
            };
        }

        public static OpenFolderDialog CreateOpenFolderDialog(string title, DirectoryInfo directory = null)
        {
            return new OpenFolderDialog()
            {
                Title = title,
                Directory = directory != null ? directory.FullName : Directory.GetCurrentDirectory()
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