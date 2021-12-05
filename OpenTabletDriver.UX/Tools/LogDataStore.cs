using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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

        private IEnumerable<LogMessage> GetFilteredMessages() =>
            from message in this.messages ?? (IEnumerable<LogMessage>)Array.Empty<LogMessage>()
                where message.Level >= Filter
                select message;

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

        public IEnumerator<LogMessage> GetEnumerator()
        {
            return this.filteredMessages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.filteredMessages as IEnumerable).GetEnumerator();
        }

        public void Clear()
        {
            this.messages.Clear();
            this.filteredMessages.Clear();
        }

        public bool Contains(LogMessage item)
        {
            return this.messages.Contains(item);
        }

        public void CopyTo(LogMessage[] array, int arrayIndex)
        {
            messages.CopyTo(array, arrayIndex);
        }

        public int IndexOf(LogMessage item)
        {
            return (filteredMessages as IList<LogMessage>).IndexOf(item);
        }

        public void Insert(int index, LogMessage item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(LogMessage item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public LogMessage this[int index]
        {
            get => (filteredMessages as IList<LogMessage>)[index];
            set => (filteredMessages as IList<LogMessage>)[index] = value;
        }
    }
}