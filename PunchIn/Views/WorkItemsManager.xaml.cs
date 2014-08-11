using PunchIn.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PunchIn.Views
{
    /// <summary>
    /// Interaction logic for WorkItemsManager.xaml
    /// </summary>
    public partial class WorkItemsManager : Window
    {
        private readonly TimeTrackViewModel viewModel;
        public WorkItemsManager(TimeTrackViewModel vm)
        {
            viewModel = vm;
            InitializeComponent();
            //this.Loaded += (s, e) => { this.DataContext = this.viewModel; };
            this.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
        }
        #region View

        //public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(ContentView), typeof(WorkItemsManager), new UIPropertyMetadata(null, OnViewChanged));
        //public ContentView View
        //{
        //    get
        //    {
        //        return (ContentView)GetValue(ViewProperty);
        //    }
        //    set
        //    {
        //        SetValue(ViewProperty, value);
        //    }
        //}

        //private static void OnViewChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    WorkItemsManager window = o as WorkItemsManager;
        //    if (window != null)
        //        window.OnViewChanged((ContentView)e.OldValue, (ContentView)e.NewValue);
        //}

        //protected virtual void OnViewChanged(ContentView oldValue, ContentView newValue)
        //{
        //    this.InitView();
        //}

        #endregion //View

        #region Event Handler

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitView();
        }

        private void OnTreeViewSelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            this.UpdateSelectedView(e.NewValue as TreeViewItem);
        }

        private void UpdateSelectedView(TreeViewItem treeViewItem)
        {
            if (treeViewItem != null)
            {
                Type type = treeViewItem.Tag.GetType().BaseType;
                if (type != null)
                {
                    //this.View = (ContentView)Activator.CreateInstance(type);
                }
            }
        }


        #endregion //EventHandler

        #region Methods

        private void InitView()
        {
            //todo: impl
        }

        #endregion

    }
}
