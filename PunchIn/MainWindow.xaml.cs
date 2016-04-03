using PunchIn.ViewModels;
using System.Windows;
using Webdogz.UI.Controls;

namespace PunchIn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = new MainWindowViewModel();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Rect location = Properties.Settings.Default.MainWindowLocation;
                if (SystemParameters.WorkArea.Contains(location))
                {
                    Top = location.Top;
                    Left = location.Left;
                    Width = location.Width;
                    Height = location.Height;
                }
                WindowState = Properties.Settings.Default.MainWindowWindowState;
            }
            catch { }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (NotifyIconViewModel.Current.Manager.IsDirty)
            {
                e.Cancel = ShouldSaveDirtyManger();
            }
            if (!e.Cancel)
                SaveSettingsAndCleanup();
        }

        private bool ShouldSaveDirtyManger()
        {
            string msg = "You have some unsaved work. Would you like to save now? Yes to save, No to discard, Cancel to review.";
            MessageBoxResult result = ModernDialog.ShowMessage(msg, "Save your work?", MessageBoxButton.YesNoCancel, this);
            if (result == MessageBoxResult.Yes)
            {
                NotifyIconViewModel.Current.Manager.SaveCommand.Execute(null);
            }
            return result == MessageBoxResult.Cancel;
        }

        private void SaveSettingsAndCleanup()
        {
            try
            {
                Properties.Settings.Default.MainWindowLocation =
                    new Rect(Left, Top, ActualWidth, ActualHeight);
                Properties.Settings.Default.MainWindowWindowState =
                    (WindowState == WindowState.Minimized) ?
                    WindowState.Normal :
                    WindowState;
                Properties.Settings.Default.Save();
            }
            finally
            {
                (DataContext as MainWindowViewModel).CleanUp();
                DataContext = null;
                if (NotifyIconViewModel.Current.Manager != null)
                    NotifyIconViewModel.Current.Manager.CleanUp();
            }
        }
    }
}
