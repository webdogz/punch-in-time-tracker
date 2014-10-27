using System;
using System.Windows;

namespace PunchIn.ViewModels.Converters
{
    public class HiLiteCurrentWorkItemConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Guid.Empty.Equals(NotifyIconViewModel.Current.CurrentWorkItemId))
            {
                Guid workItemId = (Guid)value;
                if (workItemId.Equals(NotifyIconViewModel.Current.CurrentWorkItemId))
                    return Application.Current.FindResource("LightAccent");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
