using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenTabletDriver
{
    public class Instance : IDisposable
    {
        public Instance(string name)
        {
            this.mutex = new Mutex(true, $"{MUTEX_PREFIX}{name}", out bool createdNew);
            this.Name = name;
            AlreadyExists = !createdNew;
            if (createdNew)
                ownedInstances.Add(this);
        }

        public static bool Exists(string name)
        {
            var result = Mutex.TryOpenExisting($"{MUTEX_PREFIX}{name}", out var mutex);
            using (mutex)
            {
                mutex?.Close();
                return result;
            }
        }

        public static bool IsOwnerOf(string name)
        {
            return ownedInstances.Any(i => i.Name == name);
        }

        private const string MUTEX_PREFIX = @"Global\";
        private readonly static List<Instance> ownedInstances = new List<Instance>();

        private Mutex mutex;

        public string Name { get; }
        public bool AlreadyExists { protected set; get; }

        public void Dispose()
        {
            mutex.Close();
            mutex.Dispose();
        }
    }
}
