using System;
using System.Windows;
using System.Windows.Data;

namespace Webdogz.UI.Converters
{
    public class StringToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            bool convertToLower = (parameter as string) == "tolower";
            string s = (value as string) ?? value.ToString();
            string val = convertToLower ? s.ToLowerInvariant() : s;
            return Application.Current.FindResource(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
