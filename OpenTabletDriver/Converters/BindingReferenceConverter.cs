using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using OpenTabletDriver.Plugins;

namespace OpenTabletDriver.Converters
{
    public class BindingReferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                    return null;
                else
                    return new BindingReference(str);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindingReference binding)
            {
                return binding.Path + ", " + binding.Value;
            }
            else
            {
                return null;
            }
        }
    }
}