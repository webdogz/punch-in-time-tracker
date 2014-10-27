using System;

namespace PunchIn.ViewModels.Converters
{
    public class DisableCurrentWorkItemConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Guid.Empty.Equals(NotifyIconViewModel.Current.CurrentWorkItemId))
            {
                Guid workItemId = (Guid)value;
                return workItemId != NotifyIconViewModel.Current.CurrentWorkItemId;
            }
            else
            {
                return true; // IsEnabled = true
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
