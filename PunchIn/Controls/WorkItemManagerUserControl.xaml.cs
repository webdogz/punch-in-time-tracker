using PunchIn.Models;
using PunchIn.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            PunchMenuItemModel twVM = tw.DataContext as PunchMenuItemModel;
            Console.WriteLine(twVM.Text);
        }
    }
}
