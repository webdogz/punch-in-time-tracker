using System.Windows.Controls;
using System.Windows.Input;

namespace PunchIn.Pages.Content
{
    /// <summary>
    /// Interaction logic for WorkItemUserControl.xaml
    /// </summary>
    public partial class WorkItemForm : UserControl
    {
        public WorkItemForm()
        {
            InitializeComponent();
            Loaded += delegate
            {
                titleTextBox.Focus();
                //titleTextBox.Focusable = true;
                //Keyboard.Focus(titleTextBox);
            };
        }
    }
}
