using System;

namespace PunchIn.ViewModels.Converters
{
    public class DisableCurrentWorkItemConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (NotifyIconViewModel.Current.CurrentWorkItem != null)
            {
                Guid workItemId = (Guid)value;
                return workItemId != NotifyIconViewModel.Current.CurrentWorkItem.Id;
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
