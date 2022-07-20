using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A utility for existing instance detection via <see cref="Mutex"/>.
    /// </summary>
    [PublicAPI]
    public sealed class Instance : IDisposable
    {
        private readonly Mutex _mutex;

        public Instance(string name)
        {
            _mutex = new Mutex(true, $"{MUTEX_PREFIX}{name}", out bool createdNew);
            Name = name;
            AlreadyExists = !createdNew;
            if (createdNew)
                OwnedInstances.Add(this);
        }

        private const string MUTEX_PREFIX = @"Global\";

        private static readonly List<Instance> OwnedInstances = new List<Instance>();

        public string Name { get; }
        public bool AlreadyExists { private set; get; }

        /// <summary>
        /// Checks if an instance already exists for this name.
        /// </summary>
        /// <param name="name">The instance name.</param>
        /// <returns>True if an instance already exists, otherwise false.</returns>
        public static bool Exists(string name)
        {
            var result = Mutex.TryOpenExisting($"{MUTEX_PREFIX}{name}", out var mutex);
            using (mutex)
            {
                mutex?.Close();
                return result;
            }
        }

        /// <summary>
        /// Checks if an instance is owned by the current application.
        /// </summary>
        /// <param name="name">The instance name.</param>
        /// <returns>True if the current application owns the instance, otherwise false.</returns>
        public static bool IsOwnerOf(string name)
        {
            return OwnedInstances.Any(i => i.Name == name);
        }

        public void Dispose()
        {
            _mutex.Close();
            _mutex.Dispose();
        }
    }
}
