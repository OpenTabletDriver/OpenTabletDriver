using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenTabletDriver
{
    internal static class Extensions
    {
        public static void CopyPropertiesTo(this object source, object dest)
        {
            var sourceType = source.GetType();
            var destType = source.GetType();
            var sourceProperties = sourceType.GetProperties().Where(x => x.CanRead).ToList();
            var destinationProperties = destType.GetProperties().Where(x => x.CanWrite).ToList();
            foreach (var sourceProp in sourceProperties)
            {
                if (destinationProperties.Any(x => x.Name == sourceProp.Name))
                {
                    var property = destinationProperties.First(prop => prop.Name == sourceProp.Name);
                    if (property.CanWrite)
                        property.SetValue(dest, sourceProp.GetValue(source, null), null);
                }
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this ICollection<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        public static T Convert<T>(this object obj)
        {
            var result = System.Convert.ChangeType(obj, typeof(T));
            if (result != null)
                return (T)result;
            else
                throw new InvalidCastException($"Failed to cast '{obj.GetType().FullName}' to '{typeof(T).FullName}");
        }
    }
}