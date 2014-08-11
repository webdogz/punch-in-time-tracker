using System.Windows.Controls;

namespace PunchIn.Controls
{
    /// <summary>
    /// Interaction logic for WorkItemUserControl.xaml
    /// </summary>
    public partial class WorkItemUserControl : UserControl
    {
        public WorkItemUserControl()
        {
            InitializeComponent();
            this.Loaded += delegate
            {
                this.titleTextBox.Focus();
            };
        }
    }
}
