using EmitMapper;
using PunchIn.Models;
using PunchIn.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace PunchIn.Controls
{
    /// <summary>
    /// Interaction logic for WorkItemManagerUserControl.xaml
    /// </summary>
    public partial class WorkItemManagerUserControl : UserControl
    {
        public WorkItemManagerUserControl()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ListView)sender;
            var item = (WorkItem)selected.SelectedItem;
            (this.DataContext as NotifyIconViewModel).CurrentWorkItem = ObjectMapperManager.DefaultInstance.GetMapper<WorkItem, WorkItemViewModel>().Map(item);
        }
    }
}
