using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpenTabletDriver.Converters
{
    public class ByteArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] data)
            {
                return GetString(data);
            }
            else if (value == null)
            {
                return null;
            }
            else
            {
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                var raw = str.Split(' ');
                byte[] buffer = new byte[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                {
                    if (TryGetHexValue(raw[i], out var val))
                        buffer[i] = val;
                    else
                        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
                }
                Debug.WriteLine(BitConverter.ToString(buffer).Replace("-", " "));
                return buffer;
            }
            else if (value == null)
            {
                return null;
            }
            else
            {
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }
        }

        private static bool TryGetHexValue(string str, out byte value)
        {
            return byte.TryParse(str.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out value);
        }

        private static string GetString(byte[] value)
        {
            return "0x" + BitConverter.ToString(value).Replace("-", " 0x");
        }
    }
}