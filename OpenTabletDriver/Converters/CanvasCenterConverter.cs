using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpenTabletDriver.Converters
{
    public class CanvasCenterConverter : IMultiValueConverter
    {
        public CanvasCenterConverter()
        {
        }

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is float x && values[1] is float size)
                return x - (size / 2);
            else
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }
}