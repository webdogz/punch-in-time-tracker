using System;
using System.Windows;

namespace PunchIn.ViewModels.Converters
{
    public class HiLiteCurrentWorkItemConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (NotifyIconViewModel.Current.CurrentWorkItem != null)
            {
                Guid workItemId = (Guid)value;
                if (workItemId.Equals(NotifyIconViewModel.Current.CurrentWorkItem.Id))
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
