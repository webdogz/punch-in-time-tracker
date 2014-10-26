using PunchIn.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Hosting;
using System.Windows;
using Webdogz.UI.TaskbarNotification;

namespace PunchIn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private GlobalHotkeyManager hotkeyManager;
        private bool _initialised = false;

        internal delegate void ProcessArgDelegate(String arg);
        internal static ProcessArgDelegate ProcessArg;

        private void SetCurrentArg(string arg)
        {
            GlobalConfig.SetCurrentDatabaseLocation(arg);
        }
        private void ResetApplication(string arg)
        {
            if (_initialised)
            {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }

                NotifyIconViewModel.Current.CleanUp();
                if (notifyIcon != null && notifyIcon.DataContext != null)
                    notifyIcon.DataContext = null;
            }
            SetCurrentArg(arg);
            Initilise();
        }
        private void Initilise()
        {
            if (!_initialised)
            {
                //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
                notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
                //add global hotkey support
                hotkeyManager = new GlobalHotkeyManager(notifyIcon);
            }
            //initialise settings
            GlobalConfig.InitialiseAndSyncSettings();
            notifyIcon.DataContext = NotifyIconViewModel.Current;
            _initialised = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ProcessArg = delegate(string arg)
            {
                ResetApplication(arg);
            };
            WpfSingleInstance.Make("PunchInApplication", this);
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NotifyIconViewModel.Current.CleanUp();
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            hotkeyManager.Dispose();
            base.OnExit(e);
        }
    }
}
