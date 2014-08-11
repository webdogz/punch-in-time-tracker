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
            this.Loaded += delegate
            {
                mainTree.DataContext = (DataContext as NotifyIconViewModel).WorkItemMenus;
            };
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NotifyIconViewModel currentVM = DataContext as NotifyIconViewModel;
            TreeView tw = sender as TreeView;
            PunchMenuItemViewModel twVM = tw.DataContext as PunchMenuItemViewModel;
            Console.WriteLine(twVM.Text);
        }
    }
}
