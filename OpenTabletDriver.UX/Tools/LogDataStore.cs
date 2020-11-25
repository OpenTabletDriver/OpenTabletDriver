using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.UX.Tools
{
    public class LogDataStore : INotifyCollectionChanged, IEnumerable<LogMessage>
    {
        private readonly ConcurrentQueue<LogMessage> messages = new ConcurrentQueue<LogMessage>();

        private IEnumerable<LogMessage> filteredMessages =>
            from message in this.messages
                where message.Level >= Filter
                select message;

        private LogLevel filter = LogLevel.Info;
        public LogLevel Filter
        {
            set
            {
                this.filter = value;
                OnCollectionChanged();
            }
            get => this.filter;
        }

        public int Count => filteredMessages.Count();

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(LogMessage message)
        {
            this.messages.Enqueue(message);
            if (message.Level >= this.Filter)
                OnCollectionChanged(NotifyCollectionChangedAction.Add, message);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset)
        {
            var args = new NotifyCollectionChangedEventArgs(action);
            CollectionChanged?.Invoke(this, args);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object obj)
        {
            var args = new NotifyCollectionChangedEventArgs(action, obj);
            CollectionChanged?.Invoke(this, args);
        }

        public IEnumerator<LogMessage> GetEnumerator()
        {
            return (this.filteredMessages as IEnumerable<LogMessage>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.filteredMessages as IEnumerable).GetEnumerator();
        }
    }
}