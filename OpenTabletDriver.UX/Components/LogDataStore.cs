using System.Collections;
using System.Collections.Specialized;
using OpenTabletDriver.Logging;
using OpenTabletDriver.UX.Collections;

namespace OpenTabletDriver.UX.Components
{
    public sealed class LogDataStore : INotifyCollectionChanged, IList<LogMessage>, IList
    {
        public LogDataStore(IEnumerable<LogMessage> currentMessages)
        {
            _messages = new RandomAccessQueue<LogMessage>(currentMessages.TakeLast(MAX_NUM_MESSAGES));
            _filteredMessages = new RandomAccessQueue<LogMessage>(GetFilteredMessages());
        }

        private const int MAX_NUM_MESSAGES = 250;

        private readonly RandomAccessQueue<LogMessage> _messages;
        private RandomAccessQueue<LogMessage> _filteredMessages;
        private LogLevel _filter = LogLevel.Info;

        public LogLevel Filter
        {
            set
            {
                _filter = value;
                _filteredMessages = new RandomAccessQueue<LogMessage>(GetFilteredMessages());
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
            return _messages.Where(m => m?.Level >= Filter);
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
            return _filteredMessages.Contains(item);
        }

        void ICollection<LogMessage>.CopyTo(LogMessage[] array, int arrayIndex)
        {
            _filteredMessages.CopyTo(array, arrayIndex);
        }

        int IList<LogMessage>.IndexOf(LogMessage item)
        {
            return _filteredMessages.IndexOf(item);
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

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        object? IList.this[int index] { get => _filteredMessages[index]; set => _filteredMessages[index] = (LogMessage)value!; }

        int IList.Add(object? value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            ((ICollection<LogMessage>)this).Clear();
        }

        bool IList.Contains(object? value)
        {
            return _filteredMessages.Contains(value);
        }

        int IList.IndexOf(object? value)
        {
            if (value is LogMessage message)
            {
                return _filteredMessages.IndexOf(message);
            }
            return -1;
        }

        void IList.Insert(int index, object? value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object? value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_filteredMessages).CopyTo(array, index);
        }

        LogMessage IList<LogMessage>.this[int index]
        {
            get => _filteredMessages[index];
            set => _filteredMessages[index] = value;
        }
    }
}
