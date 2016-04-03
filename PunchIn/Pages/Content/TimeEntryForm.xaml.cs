using System.Windows.Controls;
using System.Windows.Input;

namespace PunchIn.Pages.Content
{
    /// <summary>
    /// Interaction logic for TimeEntryUserControl.xaml
    /// </summary>
    public partial class TimeEntryForm : UserControl
    {
        public TimeEntryForm()
        {
            InitializeComponent();
            Loaded += delegate
            {
                titleTextBox.Focusable = true;
                Keyboard.Focus(titleTextBox);
            };
        }
    }
}
