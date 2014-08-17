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
using System.Windows.Shapes;
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
            catch { }
        }
    }
}
