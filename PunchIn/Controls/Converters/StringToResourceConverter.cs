﻿using System;
using System.Windows;
using System.Windows.Data;

namespace PunchIn.Controls.Converters
{
    public class StringToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Application.Current.FindResource(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
