using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Webdogz.UI.Controls;

namespace PunchIn.Controls
{
    public class TrayDialog : ModernDialog
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the IsTrayWindow dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTrayWindowProperty = DependencyProperty.Register("IsTrayWindow", typeof(bool), typeof(TrayDialog), new PropertyMetadata(false));
        public bool IsTrayWindow
        {
            get { return (bool)GetValue(IsTrayWindowProperty); }
            set { SetValue(IsTrayWindowProperty, value); }
        }
        #endregion

        public TrayDialog() : base()
        {
            this.Loaded += ModernDialog_Loaded;
            this.SizeChanged += TrayDialog_SizeChanged;
        }

        void TrayDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsTrayWindow)
            {
                SetTrayPosition();
            }
        }

        void ModernDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsTrayWindow)
            {
                Topmost = true;
                WindowStartupLocation = WindowStartupLocation.Manual;
                SetTrayPosition();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                this.Loaded -= ModernDialog_Loaded;
            }
            catch { }
        }
        /// <summary>
        /// Sets the windows position to the bottom right corner of the primary screen
        /// </summary>
        public void SetTrayPosition()
        {
            try
            {
                this.Top = SystemParameters.WorkArea.Height - this.ActualHeight;
                this.Left = SystemParameters.WorkArea.Width - this.ActualWidth;
            }
            catch { }
        }
    }
}
