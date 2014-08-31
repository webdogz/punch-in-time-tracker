using System.Windows;
using System.Windows.Controls;

namespace PunchIn.Controls
{
    public class BindableRadioButton : RadioButton
    {
        protected override void OnChecked(RoutedEventArgs e)
        {
            // Do nothing. This will prevent IsChecked from being manually set and overwriting the binding.
        }

        protected override void OnToggle()
        {
            // Do nothing. This will prevent IsChecked from being manually set and overwriting the binding.
        }
    }
}
