using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

#nullable enable

namespace OpenTabletDriver.Desktop
{
    public static class FileUtilities
    {
        /// <summary>
        /// Evaluates all environment variables and returns the first rooted path, otherwise null.
        /// </summary>
        /// <returns>A valid path in which may or may not currently exist.</returns>
        public static string? GetPath(params string[] paths)
        {
            foreach (var dir in paths.Select(InjectEnvironmentVariables))
                if (Path.IsPathRooted(dir))
                    return dir;

            return null;
        }

        /// <summary>
        /// Evaluates all environment variables and returns the first existing path or null.
        /// </summary>
        /// <returns>An existing path or null.</returns>
        public static string? GetExistingPath(params string[] paths)
        {
            foreach (var dir in paths.Select(InjectEnvironmentVariables))
                if (Directory.Exists(dir))
                    return dir;

            return null;
        }

        /// <summary>
        /// Evaluates all environment variables and returns the first existing path.
        /// If no path exists, the last one in the list is returned.
        /// </summary>
        /// <returns>A path in which to be created.</returns>
        public static string GetExistingPathOrLast(params string[] paths)
        {
            return GetExistingPath(paths) ?? InjectEnvironmentVariables(paths.Last());
        }

        public static string InjectEnvironmentVariables(string str)
        {
            StringBuilder sb = new StringBuilder(str);
            sb.Replace("~", Environment.GetEnvironmentVariable("HOME"));

            foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
            {
                string? key = envVar.Key as string;
                string? value = envVar.Value as string;
                sb.Replace($"${key}", value); // $KEY
                sb.Replace($"${{{key}}}", value); // ${KEY}
            }

            return sb.ToString();
        }
    }
}
