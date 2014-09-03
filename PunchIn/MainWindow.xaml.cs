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
                    this.Top = location.Top;
                    this.Left = location.Left;
                    this.Width = location.Width;
                    this.Height = location.Height;
                }
            }
            catch { }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                Properties.Settings.Default.MainWindowLocation =
                    new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
                Properties.Settings.Default.Save();
            }
            finally
            {
                (DataContext as MainWindowViewModel).CleanUp();
                DataContext = null;
            }
        }
    }
}
