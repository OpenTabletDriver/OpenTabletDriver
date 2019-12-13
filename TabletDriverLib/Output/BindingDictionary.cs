using System.Collections;
using System.Collections.Generic;
using TabletDriverLib.Interop.Cursor;

namespace TabletDriverLib.Output
{
    public class BindingDictionary : IEnumerable<(int, MouseButton)>
    {
        public BindingDictionary()
        {
        }

        private Dictionary<int, MouseButton> _dict = new Dictionary<int, MouseButton>();

        public MouseButton this[int key]
        { 
            get
            {
                if (_dict.TryGetValue(key, out var value))
                    return value;
                else
                    return 0;
            }
            set
            {
                if (_dict.ContainsKey(key))
                    _dict[key] = value;
                else
                    _dict.Add(key, value);
            }
        }

        public void Add(int key, MouseButton value) => _dict.Add(key, value);

        public IEnumerator<(int, MouseButton)> GetEnumerator() => (_dict as IEnumerable<(int, MouseButton)>).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
    }
}