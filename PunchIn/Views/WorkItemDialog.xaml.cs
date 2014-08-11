using EmitMapper;
using PunchIn.Models;
using PunchIn.ViewModels;
using System.Windows;

namespace PunchIn.Views
{
    /// <summary>
    /// Interaction logic for WorkItemDialog.xaml
    /// </summary>
    public partial class WorkItemDialog : Window
    {
        public WorkItemDialog()
        {
            InitializeComponent();
        }
        public WorkItemDialog(WorkItem model)
            : this()
        {
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<WorkItem, WorkItemViewModel>();
            WorkItemViewModel vm = mapper.Map(model);
            this.Loaded += (s, e) => { this.DataContext = vm; this.SetTrayPosition(); };
        }
        private void SetTrayPosition()
        {
            this.Top = SystemParameters.WorkArea.Height - this.ActualHeight;
            this.Left = SystemParameters.WorkArea.Width - this.ActualWidth;
        }

        public WorkItemViewModel ViewModel
        {
            get { return this.DataContext as WorkItemViewModel; }
        }
        public WorkItem Model
        {
            get
            {
                return ObjectMapperManager.DefaultInstance.GetMapper<WorkItemViewModel, WorkItem>().Map(ViewModel);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
