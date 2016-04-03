using PunchIn.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Webdogz.UI.TaskbarNotification;

namespace PunchIn
{
    public sealed class GlobalHotkeyManager : NativeWindow, IDisposable
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly int hotkeyWorkItemId;
        private readonly int hotkeyTimeEntryId;
        private readonly TaskbarIcon notifyIcon;

        public GlobalHotkeyManager(TaskbarIcon notifyIcon)
        {
            this.notifyIcon = notifyIcon;
            CreateHandle(new CreateParams());

            hotkeyWorkItemId = typeof(WorkItemViewModel).GetHashCode();
            hotkeyTimeEntryId = typeof(TimeEntryViewModel).GetHashCode();
            //todo:add settings page for user defined hotkeys
            RegisterHotKey(Handle, hotkeyWorkItemId, 
                HotKeyConstants.CTRL + HotKeyConstants.ALT, 
                (int)Keys.N);
            RegisterHotKey(Handle, hotkeyTimeEntryId,
                HotKeyConstants.CTRL + HotKeyConstants.ALT,
                (int)Keys.P);

            string msg = "Registering HotKeys. Ctrl+Alt+P to Punch In/Out. Ctrl+Alt+N for new Work Item.";
            this.notifyIcon.ShowBalloonTip("Punch Time Tracker", msg, BalloonIcon.Info);
        }

        private void DoWorkItemHotkey()
        {
            if (NotifyIconViewModel.Current.NewWorkItemCommand.CanExecute(null))
                NotifyIconViewModel.Current.NewWorkItemCommand.Execute(null);
        }

        private void DoTimeEntryHotkey()
        {
            if (NotifyIconViewModel.Current.PunchInCommand.CanExecute(null))
            {
                string msg = string.Empty;
                if (NotifyIconViewModel.Current.CurrentTimeEntry != null)
                {
                    TimeSpan span = DateTime.Now - NotifyIconViewModel.Current.CurrentTimeEntry.CurrentEntry.StartDate;
                    msg = string.Format("'{0}' was punched out time spent is {1:F} hours",
                        NotifyIconViewModel.Current.CurrentTimeEntry.CurrentEntry.Description,
                        span.TotalHours);
                }
                NotifyIconViewModel.Current.PunchInCommand.Execute(null);
                if (NotifyIconViewModel.Current.CurrentTimeEntry == null && !string.IsNullOrWhiteSpace(msg))
                    notifyIcon.ShowBalloonTip("Punched Out", msg, BalloonIcon.Info);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyConstants.WM_HOTKEY_MSG_ID)
            {
                if (m.WParam.ToInt32() == hotkeyWorkItemId)
                {
                    DoWorkItemHotkey();
                }
                if (m.WParam.ToInt32() == hotkeyTimeEntryId)
                {
                    if (NotifyIconViewModel.Current.CurrentWorkItem == null)
                    {
                        // we actually have nothing so lets start out by creating
                        // a work item and then let is flow down to creating the
                        // time entry.
                        DoWorkItemHotkey();
                    }
                    DoTimeEntryHotkey();
                }
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, hotkeyWorkItemId);
            UnregisterHotKey(Handle, hotkeyTimeEntryId);
            DestroyHandle();
        }
    }

    /// <summary>
    /// Virtual Key codes from: http://msdn.microsoft.com/en-us/library/ms927178.aspx
    /// </summary>
    public class HotKeyConstants
    {
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;
        public const int CAPS_LOCK = 0x14;

        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }
}
