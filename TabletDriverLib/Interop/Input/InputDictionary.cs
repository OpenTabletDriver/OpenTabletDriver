using System.Collections;
using System.Collections.Generic;

namespace TabletDriverLib.Interop.Input
{
    public class InputDictionary : IDictionary<MouseButton, bool>, IDictionary<Key, bool>
    {
        #region MouseButton Implementation

        private Dictionary<MouseButton, bool> _mouse = new Dictionary<MouseButton, bool>();
        
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

        #endregion

        #region Key Implementation

        private Dictionary<Key, bool> _key = new Dictionary<Key, bool>();

        public bool this[Key key] 
        {
            get => _key[key];
            set => _key[key] = value;
        }

        ICollection<Key> IDictionary<Key, bool>.Keys => ((IDictionary<Key, bool>)_key).Keys;

        public void Add(Key key, bool value)
        {
            _key.Add(key, value);
        }

        public bool ContainsKey(Key key)
        {
            return _key.ContainsKey(key);
        }

        public bool Remove(Key key)
        {
            return _key.Remove(key);
        }

        public bool TryGetValue(Key key, out bool value)
        {
            return _key.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<Key, bool> item)
        {
            ((IDictionary<Key, bool>)_key).Add(item);
        }

        public bool Contains(KeyValuePair<Key, bool> item)
        {
            return ((IDictionary<Key, bool>)_key).Contains(item);
        }

        public void CopyTo(KeyValuePair<Key, bool>[] array, int arrayIndex)
        {
            ((IDictionary<Key, bool>)_key).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<Key, bool> item)
        {
            return ((IDictionary<Key, bool>)_key).Remove(item);
        }

        IEnumerator<KeyValuePair<Key, bool>> IEnumerable<KeyValuePair<Key, bool>>.GetEnumerator()
        {
            return ((IDictionary<Key, bool>)_key).GetEnumerator();
        }

        public void UpdateState(Key key, bool isPressed)
        {
            if (_key.ContainsKey(key))
                _key[key] = isPressed;
            else
                _key.Add(key, isPressed);
        }

        #endregion
    }
}