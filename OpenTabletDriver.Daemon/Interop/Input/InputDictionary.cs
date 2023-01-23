using System.Collections;
using System.Collections.Generic;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop.Input
{
    public class InputDictionary : IDictionary<MouseButton, bool>
    {
        private readonly Dictionary<MouseButton, bool> _mouse = new Dictionary<MouseButton, bool>();

        public bool this[MouseButton key]
        {
            set => _mouse[key] = value;
            get => _mouse[key];
        }

        public ICollection<MouseButton> Keys => _mouse.Keys;

        public ICollection<bool> Values => _mouse.Values;

        public int Count => _mouse.Count;

        public bool IsReadOnly => false;

        public void Add(MouseButton key, bool value)
        {
            _mouse.Add(key, value);
        }

        public void Add(KeyValuePair<MouseButton, bool> item)
        {
            ((IList)_mouse).Add(item);
        }

        public void Clear()
        {
            _mouse.Clear();
        }

        public bool Contains(KeyValuePair<MouseButton, bool> item)
        {
            return ((IList)_mouse).Contains(item);
        }

        public bool ContainsKey(MouseButton key)
        {
            return _mouse.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<MouseButton, bool>[] array, int arrayIndex)
        {
            ((IList)_mouse).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<MouseButton, bool>> GetEnumerator()
        {
            return _mouse.GetEnumerator();
        }

        public bool Remove(MouseButton key)
        {
            return _mouse.Remove(key);
        }

        public bool Remove(KeyValuePair<MouseButton, bool> item)
        {
            return _mouse.Remove(item.Key);
        }

        public bool TryGetValue(MouseButton key, out bool value)
        {
            return _mouse.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _mouse.GetEnumerator();
        }

        public void UpdateState(MouseButton button, bool isPressed)
        {
            if (_mouse.ContainsKey(button))
                _mouse[button] = isPressed;
            else
                _mouse.Add(button, isPressed);
        }
    }
}
