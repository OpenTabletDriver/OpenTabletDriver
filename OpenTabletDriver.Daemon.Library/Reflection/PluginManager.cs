using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Compression;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Reflection
{
    public class PluginManager : IPluginManager
    {
        private static readonly ImmutableArray<Assembly> _coreAssemblies = ImmutableArray.Create(
            Assembly.Load("OpenTabletDriver"),
            Assembly.Load("OpenTabletDriver.Daemon.Library"),
            Assembly.Load("OpenTabletDriver.Configurations")
        );

        public const string PLUGIN_ASSEMBLY_NAMESPACE = "OpenTabletDriver";
        public const string REPOSITORY_OWNER = "OpenTabletDriver";
        public const string REPOSITORY_NAME = "Plugin-Repository";

        public static Assembly PluginAssembly => _coreAssemblies[0];

        public PluginManager(AppInfo appInfo)
            : this(appInfo.PluginDirectory, appInfo.TrashDirectory, appInfo.TemporaryDirectory)
        {
        }

        public PluginManager(string pluginDirectory, string trashDirectory, string temporaryDirectory)
        {
            _pluginDirectory = new DirectoryInfo(pluginDirectory);
            _trashDirectory = new DirectoryInfo(trashDirectory);
            _temporaryDirectory = new DirectoryInfo(temporaryDirectory);

            if (!_pluginDirectory.Exists)
                _pluginDirectory.Create();

            Clean();

            var pluginInterfacesLinq = from asm in _coreAssemblies
                from type in asm.ExportedTypes
                where type.IsInterface || type.IsAbstract
                where type.GetCustomAttribute<PluginInterfaceAttribute>() != null
                select type;

            PluginInterfaces = pluginInterfacesLinq.ToImmutableArray();

            var internalPluginTypesLinq = from asm in _coreAssemblies
                from type in asm.ExportedTypes
                where IsLoadablePlugin(type)
                select type;

            InternalPluginTypes = internalPluginTypesLinq.ToImmutableArray();
        }

        private readonly DirectoryInfo _pluginDirectory;
        private readonly DirectoryInfo _trashDirectory;
        private readonly DirectoryInfo _temporaryDirectory;

        private ImmutableArray<PluginContext> _plugins = ImmutableArray<PluginContext>.Empty;

        public event EventHandler<PluginContext>? PluginAdded;
        public event EventHandler<PluginContext>? PluginRemoved;

        public ImmutableArray<Type> PluginInterfaces { get; }
        public ImmutableArray<Type> InternalPluginTypes { get; }
        public ImmutableArray<PluginContext> Plugins => _plugins;

        public void Load()
        {
            foreach (var plugin in Plugins)
                UnloadPlugin(plugin);

            var directories = _pluginDirectory.GetDirectories();
            foreach (var dir in directories)
                LoadPlugin(dir);
        }

        public bool IsLoadablePlugin(Type type)
        {
            if (!type.GetCustomAttribute<SupportedPlatformAttribute>()?.IsCurrentPlatform ?? false)
            {
                Log.Write("Plugin", $"Plugin '{type.FullName}' is not supported on this platform.", LogLevel.Debug);
                return false;
            }

            if (type.GetCustomAttribute<PluginIgnoreAttribute>() != null)
            {
                return false;
            }

            if (type.IsInterface || type.IsAbstract)
            {
                return false;
            }

            // do not use nested LINQ to avoid trashing the GC
            foreach (var libraryType in PluginInterfaces)
            {
                if (libraryType.IsAssignableFrom(type))
                    return true;

                foreach (var interfaceType in type.GetInterfaces())
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == libraryType)
                        return true;
            }

            return false;
        }

        public async Task<IEnumerable<PluginMetadata>> GetInstallablePlugins(
            string owner = REPOSITORY_OWNER,
            string name = REPOSITORY_NAME,
            string gitRef = "master")
        {
            return await MetadataUtilities.GetRemoteMetadatas(owner, name, gitRef, _temporaryDirectory.FullName);
        }

        public async Task<bool> InstallFromRemote(PluginMetadata metadata)
        {
            var sourcePath = Path.Join(_temporaryDirectory.FullName, Guid.NewGuid().ToString());
            var targetPath = Path.Join(_pluginDirectory.FullName, metadata.Name);
            var metadataPath = Path.Join(targetPath, "metadata.json");

            var sourceDir = new DirectoryInfo(sourcePath);
            var targetDir = new DirectoryInfo(targetPath);

            await MetadataUtilities.DownloadPlugin(metadata, sourcePath);

            var context = Plugins.FirstOrDefault(ctx => ctx.Directory.FullName == targetDir.FullName);
            var result = targetDir.Exists ? UpdatePlugin(context!, sourceDir) : InstallPlugin(targetDir, sourceDir);

            await using (var fs = File.Create(metadataPath))
                Serialization.Serialize(fs, metadata);

            if (!_temporaryDirectory.GetFileSystemInfos().Any())
                Directory.Delete(_temporaryDirectory.FullName, true);

            return result;
        }

        public Task<bool> InstallFromLocal(string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
                return Task.FromResult(false);

            var name = Path.GetFileNameWithoutExtension(file.Name);
            var tempDir = new DirectoryInfo(Path.Join(_temporaryDirectory.FullName, name));
            if (!tempDir.Exists)
                tempDir.Create();

            var pluginPath = Path.Join(_pluginDirectory.FullName, name);
            var pluginDir = new DirectoryInfo(pluginPath);
            switch (file.Extension)
            {
                case ".zip":
                {
                    ZipFile.ExtractToDirectory(file.FullName, tempDir.FullName, true);
                    break;
                }
                case ".dll":
                {
                    file.CopyTo(Path.Join(tempDir.FullName, file.Name));
                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported archive type: {file.Extension}");
            }
            var context = Plugins.FirstOrDefault(ctx => ctx.Directory.FullName == pluginDir.FullName);
            var result = pluginDir.Exists ? UpdatePlugin(context!, tempDir) : InstallPlugin(pluginDir, tempDir);

            if (!_temporaryDirectory.GetFileSystemInfos().Any())
                Directory.Delete(_temporaryDirectory.FullName, true);

            if (result)
                LoadPlugin(pluginDir);

            return Task.FromResult(result);
        }

        public bool UninstallPlugin(PluginContext plugin)
        {
            var random = new Random();
            if (!Directory.Exists(_trashDirectory.FullName))
                _trashDirectory.Create();

            Log.Write("Plugin", $"Uninstalling plugin '{plugin.FriendlyName}'");

            var trashPath = Path.Join(_trashDirectory.FullName, $"{plugin.FriendlyName}_{random.Next()}");
            Directory.Move(plugin.Directory.FullName, trashPath);

            return UnloadPlugin(plugin);
        }

        public IEnumerable<Type> GetPlugins<T>()
        {
            return GetPlugins(typeof(T));
        }

        public IEnumerable<Type> GetPlugins(Type pluginInterface)
        {
            Debug.Assert(PluginInterfaces.Contains(pluginInterface), $"Plugin interface {pluginInterface.FullName} is not a registered plugin interface.");

            foreach (var plugin in Plugins)
            {
                foreach (var pluginType in plugin.PluginTypes)
                {
                    if (pluginType.IsAssignableFrom(pluginInterface))
                        yield return pluginInterface;
                }
            }
        }

        public Type? GetPlugin(string path)
        {
            return GetAllPlugins().FirstOrDefault(type => type.FullName == path);
        }

        public IEnumerable<Type> GetImplementedInterfaces(Type pluginType)
        {
            foreach (var pluginInterface in PluginInterfaces)
            {
                if (pluginInterface.IsAssignableFrom(pluginType))
                {
                    yield return pluginInterface;
                    continue;
                }

                foreach (var interfaceType in pluginType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == pluginInterface)
                    {
                        yield return pluginInterface;
                        continue;
                    }
                }
            }
        }

        private IEnumerable<Type> GetAllPlugins()
        {
            foreach (var plugin in InternalPluginTypes)
                yield return plugin;

            foreach (var plugin in Plugins.SelectMany(plugin => plugin.PluginTypes))
                yield return plugin;
        }

        private bool UpdatePlugin(PluginContext plugin, DirectoryInfo source)
        {
            // TODO: Fix update not updating the plugin?
            var targetDir = new DirectoryInfo(plugin.Directory.FullName);
            if (UninstallPlugin(plugin))
                return InstallPlugin(targetDir, source);
            return false;
        }

        private bool InstallPlugin(DirectoryInfo target, DirectoryInfo source)
        {
            Log.Write("Plugin", $"Installing plugin '{target.Name}'");
            CopyDirectory(source, target);
            LoadPlugin(target);
            return true;
        }

        private bool UnloadPlugin(PluginContext context)
        {
            if (ImmutableInterlocked.Update(ref _plugins, plugins => plugins.Remove(context)))
            {
                PluginRemoved?.Invoke(this, context);
                return true;
            }

            return false;
        }

        private void LoadPlugin(DirectoryInfo directory)
        {
            // "Plugins" are directories that contain managed and unmanaged dll
            // These dlls are loaded into a PluginContext per directory
            directory.Refresh();
            if (Plugins.All(p => p.Directory.Name != directory.Name))
            {
                if (directory.Exists)
                {
                    Log.Write("Plugin", $"Loading plugin '{directory.Name}'", LogLevel.Debug);
                    var context = new PluginContext(this, directory);

                    ImmutableInterlocked.Update(ref _plugins, plugins => plugins.Add(context));
                    PluginAdded?.Invoke(this, context);
                }
                else
                {
                    Log.Write("Plugin", $"Tried to load a nonexistent plugin '{directory.Name}'", LogLevel.Warning);
                }
            }
            else
            {
                Log.Write("Plugin", $"Attempted to load the plugin {directory.Name} when it is already loaded.", LogLevel.Debug);
            }
        }

        private static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
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

            foreach (var subDir in source.GetDirectories())
            {
                CopyDirectory(
                    new DirectoryInfo(subDir.FullName),
                    new DirectoryInfo(Path.Combine(destination.FullName, subDir.Name))
                );
            }
        }

        private void Clean()
        {
            try
            {
                if (_pluginDirectory.Exists)
                {
                    foreach (var file in _pluginDirectory.GetFiles())
                    {
                        Log.Write("Plugin", $"Unexpected file found: '{file.FullName}'", LogLevel.Warning);
                    }
                }

                if (_trashDirectory.Exists)
                    Directory.Delete(_trashDirectory.FullName, true);
                if (_temporaryDirectory.Exists)
                    Directory.Delete(_temporaryDirectory.FullName, true);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private static class MetadataUtilities
        {
            public static async Task DownloadPlugin(PluginMetadata metadata, string outputDirectory)
            {
                await using var httpStream = await GetMetadataDownloadStream(metadata);
                await using var stream = new MemoryStream();

                // Download into memory
                await httpStream!.CopyToAsync(stream);
                stream.Position = 0;

                // Verify SHA256 hash
                if (metadata.SHA256 != null && CalculateStreamSHA256(stream) != metadata.SHA256)
                    throw new CryptographicException("The SHA256 cryptographic hashes of the downloaded content and the metadata do not match.");

                stream.Decompress(outputDirectory, metadata.CompressionFormat!);
            }

            public static async Task<ImmutableArray<PluginMetadata>> GetRemoteMetadatas(string owner, string name, string gitRef, string cacheDir)
            {
                string archiveUrl = $"https://api.github.com/repos/{owner}/{name}/tarball/{gitRef}";
                using var client = GetClient();
                using var httpStream = await client.GetStreamAsync(archiveUrl);
                using var memStream = new MemoryStream();

                await httpStream.CopyToAsync(memStream);

                using var gzipStream = new GZipInputStream(memStream);
                using var archive = TarArchive.CreateInputTarArchive(gzipStream, null);
                var hash = CalculateStreamSHA256(memStream);
                var metadataCacheDir = Path.Join(cacheDir, $"{hash}-OpenTabletDriver-PluginMetadata");

                if (Directory.Exists(metadataCacheDir))
                    Directory.Delete(metadataCacheDir, true);
                archive.ExtractContents(metadataCacheDir);

                return EnumeratePluginMetadata(metadataCacheDir).ToImmutableArray();
            }

            private static HttpClient GetClient()
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "OpenTabletDriver");
                return client;
            }

            private static async Task<Stream?> GetMetadataDownloadStream(PluginMetadata metadata)
            {
                using var client = GetClient();
                return string.IsNullOrWhiteSpace(metadata.DownloadUrl) ? null : await client.GetStreamAsync(metadata.DownloadUrl);
            }

            private static string CalculateStreamSHA256(Stream stream)
            {
                using var sha256 = SHA256.Create();

                var hashData = sha256.ComputeHash(stream);
                stream.Position = 0;
                var sb = new StringBuilder();
                foreach (var val in hashData)
                {
                    var hex = val.ToString("x2");
                    sb.Append(hex);
                }
                return sb.ToString();
            }

            private static IEnumerable<PluginMetadata> EnumeratePluginMetadata(string directoryPath)
            {
                foreach (var file in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories))
                    using (var fs = File.OpenRead(file))
                        yield return Serialization.Deserialize<PluginMetadata>(fs)!;
            }
        }
    }
}
