using System.Collections;
using System.Collections.Specialized;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.UX.Components
{
    public sealed class LogDataStore : INotifyCollectionChanged, IList<LogMessage>
    {
        public LogDataStore(IEnumerable<LogMessage> currentMessages)
        {
            _messages = new Queue<LogMessage>(currentMessages.TakeLast(MAX_NUM_MESSAGES));
            _filteredMessages = new Queue<LogMessage>(GetFilteredMessages());
        }

        private const int MAX_NUM_MESSAGES = 250;

        private readonly Queue<LogMessage> _messages;
        private Queue<LogMessage> _filteredMessages;
        private LogLevel _filter = LogLevel.Info;

        public LogLevel Filter
        {
            set
            {
                _filter = value;
                _filteredMessages = new Queue<LogMessage>(GetFilteredMessages());
                OnCollectionChanged();
            }
            get => _filter;
        }

        public int Count => _filteredMessages.Count;

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Add(LogMessage message)
        {
            _messages.Enqueue(message);
            while (_messages.Count > MAX_NUM_MESSAGES)
                _messages.TryDequeue(out _);

            if (message.Level >= Filter)
            {
                _filteredMessages.Enqueue(message);
                while (_filteredMessages.Count > MAX_NUM_MESSAGES)
                    _filteredMessages.TryDequeue(out _);

                OnCollectionChanged(NotifyCollectionChangedAction.Add, message);
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset)
        {
            var args = new NotifyCollectionChangedEventArgs(action);
            CollectionChanged?.Invoke(this, args);
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object obj)
        {
            var args = new NotifyCollectionChangedEventArgs(action, obj);
            CollectionChanged?.Invoke(this, args);
        }

        private IEnumerable<LogMessage> GetFilteredMessages()
        {
            return _messages.Where(m => m.Level >= Filter);
        }

        IEnumerator<LogMessage> IEnumerable<LogMessage>.GetEnumerator()
        {
            return _filteredMessages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_filteredMessages as IEnumerable).GetEnumerator();
        }

        void ICollection<LogMessage>.Clear()
        {
            _messages.Clear();
            _filteredMessages.Clear();
        }

        bool ICollection<LogMessage>.Contains(LogMessage item)
        {
            return _messages.Contains(item);
        }

        void ICollection<LogMessage>.CopyTo(LogMessage[] array, int arrayIndex)
        {
            _messages.CopyTo(array, arrayIndex);
        }

        int IList<LogMessage>.IndexOf(LogMessage item)
        {
            return _filteredMessages.ToList().IndexOf(item);
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
            get => (_filteredMessages as IList<LogMessage>)![index];
            set => (_filteredMessages as IList<LogMessage>)![index] = value;
        }
    }
}
