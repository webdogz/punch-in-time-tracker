using PunchIn.Controls;
using PunchIn.ViewModels;
using System.Windows;

namespace PunchIn.Views
{
    /// <summary>
    /// Interaction logic for TimeEntryDialog.xaml
    /// </summary>
    public partial class TimeEntryDialog : ModernWindow
    {
        private readonly TimeEntryViewModel viewModel;
        public TimeEntryDialog()
        {
            InitializeComponent();
        }
        public TimeEntryDialog(TimeEntryViewModel vm)
            : this()
        {
            this.viewModel = vm;
            this.Loaded += (s, e) => 
            { 
                this.DataContext = this.viewModel;
                this.SetTrayPosition();
            };
        }
        private void SetTrayPosition()
        {
            this.Top = SystemParameters.WorkArea.Height - this.ActualHeight;
            this.Left = SystemParameters.WorkArea.Width - this.ActualWidth;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
