using System.IO;

namespace OpenTabletDriver.Desktop.Reflection
{
    internal static class Extensions
    {
        public static void CopyTo(this DirectoryInfo source, DirectoryInfo destination)
        {
            if (!source.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source.FullName);
            }

            // If the destination directory doesn't exist, create it.
            destination.Create();

            // Get the files in the directory and copy them to the new location.
            foreach (var file in source.GetFiles())
            {
                string tempPath = Path.Combine(destination.FullName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (DirectoryInfo subdir in source.GetDirectories())
            {
                CopyTo(
                    new DirectoryInfo(subdir.FullName),
                    new DirectoryInfo(Path.Combine(destination.FullName, subdir.Name))
                );
            }
        }
    }
}
