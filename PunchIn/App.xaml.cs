using PunchIn.ViewModels;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //initialise settings
            GlobalConfig.InitialiseAndSyncSettings();
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            notifyIcon.DataContext = NotifyIconViewModel.Current;
            //add global hotkey support
            hotkeyManager = new GlobalHotkeyManager(notifyIcon);
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
