using System;
using System.Linq;

namespace OpenTabletDriver.UX.Tools
{
    public static class EnumTools
    {
        public static string GetName<T>(this T value) where T : Enum
        {
            return Enum.GetName(typeof(T), value);
        }

        public static string[] GetNames<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }

        public static T[] GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().ToArray();
        }
    }
}
