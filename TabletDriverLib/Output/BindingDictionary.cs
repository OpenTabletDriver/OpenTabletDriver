using System.Collections;
using System.Collections.Generic;
using TabletDriverLib.Interop.Cursor;

namespace TabletDriverLib.Output
{
    public class BindingDictionary : Dictionary<int, MouseButton>
    {
        public new MouseButton this[int key]
        { 
            get
            {
                if (base.TryGetValue(key, out var value))
                    return value;
                else
                    return 0;
            }
            set
            {
                if (base.ContainsKey(key))
                    base[key] = value;
                else
                    base.Add(key, value);
            }
        }
    }
}