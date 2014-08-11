using System;
using System.Collections;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace PunchIn.Controls
{
    public class TrayWindow : Window
    {
        static TrayWindow() { }
        protected virtual void SetPosition() {
            this.Top = SystemParameters.WorkArea.Height - this.ActualHeight;
            this.Left = SystemParameters.WorkArea.Width - this.ActualWidth;
        }
    }
}
