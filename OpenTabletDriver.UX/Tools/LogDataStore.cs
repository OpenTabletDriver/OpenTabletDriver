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
    public class LogDataStore : INotifyCollectionChanged, IEnumerable<LogMessage>
    {
        public LogDataStore(IEnumerable<LogMessage> currentMessages)
        {
            var messageList = currentMessages.ToArray();
            if (messageList.Length > MAX_NUM_MESSAGES)
            {
                var startIndex = messageList.Length - MAX_NUM_MESSAGES;
                messageList = messageList[startIndex..^0];
            }
            messages = new ConcurrentQueue<LogMessage>(messageList);
        }

        private const int MAX_NUM_MESSAGES = 250;

        private readonly ConcurrentQueue<LogMessage> messages;

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

        public int Count => Filter > LogLevel.Debug ? filteredMessages.Count() : messages.Count;

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(LogMessage message)
        {
            this.messages.Enqueue(message);

            if (this.messages.Count > MAX_NUM_MESSAGES)
                this.messages.TryDequeue(out _);

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
            return this.filteredMessages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.filteredMessages as IEnumerable).GetEnumerator();
        }
    }
}