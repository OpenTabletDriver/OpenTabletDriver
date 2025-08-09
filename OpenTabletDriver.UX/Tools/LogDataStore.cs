using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.UX.Tools
{
    public class LogDataStore : INotifyCollectionChanged, IEnumerable<LogMessage>, IList<LogMessage>
    {
        public LogDataStore(IEnumerable<LogMessage> currentMessages)
        {
            messages = new Queue<LogMessage>(currentMessages.TakeLast(MAX_NUM_MESSAGES));
            filteredMessages = new Queue<LogMessage>(GetFilteredMessages());
        }

        private const int MAX_NUM_MESSAGES = 250;

        private readonly Queue<LogMessage> messages;
        private Queue<LogMessage> filteredMessages;

        private LogLevel filter = LogLevel.Info;
        public LogLevel Filter
        {
            set
            {
                this.filter = value;
                filteredMessages = new Queue<LogMessage>(GetFilteredMessages());
                OnCollectionChanged();
            }
            get => this.filter;
        }

        public int Count => filteredMessages.Count;

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(LogMessage message)
        {
            this.messages.Enqueue(message);
            while (messages.Count > MAX_NUM_MESSAGES)
                messages.TryDequeue(out _);

            if (message.Level >= this.Filter)
            {
                this.filteredMessages.Enqueue(message);
                while (this.filteredMessages.Count > MAX_NUM_MESSAGES)
                    this.filteredMessages.TryDequeue(out _);

                OnCollectionChanged(NotifyCollectionChangedAction.Add, message);
            }
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

        private IEnumerable<LogMessage> GetFilteredMessages()
        {
            return from message in this.messages
                   where message.Level >= Filter
                   select message;
        }

        IEnumerator<LogMessage> IEnumerable<LogMessage>.GetEnumerator()
        {
            return this.filteredMessages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.filteredMessages as IEnumerable).GetEnumerator();
        }

        void ICollection<LogMessage>.Clear()
        {
            this.messages.Clear();
            this.filteredMessages.Clear();
        }

        bool ICollection<LogMessage>.Contains(LogMessage item)
        {
            return this.messages.Contains(item);
        }

        void ICollection<LogMessage>.CopyTo(LogMessage[] array, int arrayIndex)
        {
            messages.CopyTo(array, arrayIndex);
        }

        int IList<LogMessage>.IndexOf(LogMessage item)
        {
            return (filteredMessages as IList<LogMessage>).IndexOf(item);
        }

        void IList<LogMessage>.Insert(int index, LogMessage item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<LogMessage>.Remove(LogMessage item)
        {
            throw new NotSupportedException();
        }

        void IList<LogMessage>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        LogMessage IList<LogMessage>.this[int index]
        {
            get => (filteredMessages as IList<LogMessage>)[index];
            set => (filteredMessages as IList<LogMessage>)[index] = value;
        }
    }
}
