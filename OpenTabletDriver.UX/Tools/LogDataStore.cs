using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using System.Linq;

namespace OpenTabletDriver.UX.Tools
{
    public class LogDataStore : INotifyCollectionChanged, ICollection<LogMessage>, IEnumerable<LogMessage>
    {
        private readonly ConcurrentBag<LogMessage> data = new ConcurrentBag<LogMessage>();
        private readonly ConcurrentBag<LogMessage> filteredData = new ConcurrentBag<LogMessage>();

        private LogLevel activeFilter = LogLevel.Info;
        public LogLevel ActiveFilter
        {
            set
            {
                activeFilter = value;
                filteredData.Clear();
                foreach (var msg in data)
                {
                    if (msg.Level >= value)
                        filteredData.Add(msg);
                }
                var notif = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, notif);
            }
            get => activeFilter;
        }

        public int Count => filteredData.Count;

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(LogMessage message)
        {
            data.Add(message);

            if (message.Level >= ActiveFilter)
            {
                filteredData.Add(message);
                var notif = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, message);
                CollectionChanged?.Invoke(this, notif);
            }
        }

        public void Clear()
        {
            data.Clear();
            filteredData.Clear();

            var notif = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, notif);
        }

        public bool Contains(LogMessage item)
        {
            return filteredData.Contains(item);
        }

        public void CopyTo(LogMessage[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<LogMessage> GetEnumerator()
        {
            return ((IEnumerable<LogMessage>)filteredData).GetEnumerator();
        }

        public bool Remove(LogMessage item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)filteredData).GetEnumerator();
        }
    }
}