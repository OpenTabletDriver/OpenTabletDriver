using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    static class Utility
    {
        static public T Wrap<T>(IntPtr ptr)
        { 
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }
    }
}
