using PunchIn.Controls;
using PunchIn.Models;
using PunchIn.Pages.Content;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon
    /// </summary>
    public class NotifyIconViewModel : ViewModelBase
    {
        #region Ctor and Instance Accessor
        private NotifyIconViewModel()
        {
            this.viewModel = new TimeTrackViewModel();
            this.viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "WorkItems")
                    BuildWorkItemMenusAsync();
                if (e.PropertyName == "CurrentEntry" && CurrentTimeEntry == null)
                {
                    if (this.viewModel.CurrentEntry != null)
                    {
                        CurrentTimeEntry = new CurrentEntryViewModel(this.ViewModel);
                        IsTimerActive = true;
                    }
                }
            };
            BuildShortcutMenusAsync();
            SyncThemeSettings();
            IsTimerActive = false;
            timer = GetUITimer(TimeSpan.FromSeconds(1), OnTimerTick);
        }
        private static NotifyIconViewModel current = new NotifyIconViewModel();
        /// <summary>
        /// Gets the current <see cref="NotifyIconViewModel"/> instance
        /// </summary>
        public static NotifyIconViewModel Current
        {
            get { return current; }
        }
        #endregion

        #region Misc UI stuff
        private DispatcherTimer GetUITimer(TimeSpan ts, EventHandler eventHandler)
        {
            return new DispatcherTimer(ts, DispatcherPriority.Normal, eventHandler, Application.Current.Dispatcher);
        }

        private void SyncThemeSettings()
        {
            Webdogz.UI.Presentation.AppearanceManager.Current.SyncFromUserSettings
                (
                    Properties.Settings.Default.SelectedAccentColor,
                    Properties.Settings.Default.SelectedThemeSource
                );
        }
        #endregion

        #region WorkItem Menu build
        private async void BuildWorkItemMenusAsync()
        {
            await Task.Run(() =>
            {
                var menu = GetWorkItemMenuItems();
                menu.Text = "Work Items";
                WorkItemMenus = menu;
            });
        }
        private PunchMenuItemViewModel GetWorkItemMenuItems()
        {
            PunchMenuItemViewModel menu = new PunchMenuItemViewModel() { Text = "Work Items", Icon = "list" };
            menu.Children.Add(new PunchMenuItemViewModel
            {
                Text = "New Work Item...",
                Icon = "add",
                Command = NewWorkItemCommand
            });
            menu.Children.Add(null); // add a seperator
            foreach (WorkItem item in this.viewModel.WorkItems.Where(w => w.Status != States.Done))
            {
                menu.Children.Add(NewPunchMenuItem(item));
            }
            return menu;
        }

        private PunchMenuItemViewModel NewPunchMenuItem(WorkItem item)
        {
            string icon = (item.WorkType.ToString() ?? WorkTypes.Task.ToString()).ToLower();
            int id = 0;
            switch (item.WorkType)
            {
                case WorkTypes.BacklogItem:
                case WorkTypes.Datafix:
                case WorkTypes.Bug:
                    id = item.TfsId ?? 0;
                    break;
                case WorkTypes.Change:
                    id = item.Change ?? 0;
                    break;
                case WorkTypes.ServiceCall:
                    id = item.ServiceCall ?? 0;
                    break;
            }
            return new PunchMenuItemViewModel
            {
                Text = string.Format("[{0}] {1}", id, item.Title),
                Icon = icon,
                Id = item.Id,
                Command = SelectWorkItemCommand
            };
        }

        public PunchMenuItemViewModel WorkItemMenus
        {
            get { return workItemMenus; }
            set
            {
                if (this.workItemMenus != value)
                {
                    this.workItemMenus = value;
                    OnPropertyChanged("WorkItemMenus");
                }
            }
        }
        private PunchMenuItemViewModel workItemMenus;

        #region Punch Menu Item
        public string PunchMenuText
        {
            get { return punchMenuText; }
            set
            {
                if (this.punchMenuText != value)
                {
                    this.punchMenuText = value;
                    OnPropertyChanged("PunchMenuText");
                }
            }
        }
        private string punchMenuText;

        public string PunchMenuIcon
        {
            get { return punchMenuIcon; }
            set
            {
                if (this.punchMenuIcon != value)
                {
                    this.punchMenuIcon = value;
                    OnPropertyChanged("PunchMenuIcon");
                }
            }
        }
        private string punchMenuIcon;

        public bool IsTimerActive
        {
            get { return isTimerActive; }
            set
            {
                this.isTimerActive = value;
                OnPropertyChanged("IsTimerActive");
                if (this.isTimerActive)
                {
                    PunchMenuIcon = "punchout";
                    PunchMenuText = "Punch Out";
                }
                else
                {
                    PunchMenuIcon = "punchin";
                    PunchMenuText = "Punch In...";
                }
            }
        }
        private bool isTimerActive;
        #endregion

        #endregion

        #region Taskbar Popup
        private DispatcherTimer timer;
        public string ElapsedTime
        {
            get
            {
                if (IsTimerActive && CurrentTimeEntry.CurrentEntry != null)
                    return (DateTime.Now - CurrentTimeEntry.CurrentEntry.StartDate).ToString(@"dd\.hh\:mm\:ss");
                return "Punch In!";
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            //fire a property change event for the timestamp
            Application.Current.Dispatcher.BeginInvoke(new Action(() => OnPropertyChanged("ElapsedTime")));
        }
        #endregion

        #region Shortcut Menu build
        private string GetDefaultUserShortcutsFolder()
        {
            string folder = Properties.Settings.Default.DefaultUserShortcutFolderLocation;
            return folder;
        }
        private async void BuildShortcutMenusAsync()
        {
            await Task.Run(() => 
            {
                DirectoryInfo rootPath = new DirectoryInfo(GetDefaultUserShortcutsFolder());
                if (!rootPath.Exists) rootPath.Create();
                ShortcutMenus = GetShortcutMenu(rootPath, "Shortcuts");
                InitShortcutWatcher(rootPath);
            });
        }
        #region Shortcut Folder/File Watcher
        private FileSystemWatcher shortcutWatcher;
        private DispatcherTimer shortcutTimer;
        private void InitShortcutWatcher(DirectoryInfo rootPath)
        {
            if (this.shortcutWatcher != null) return;
            this.shortcutWatcher = new FileSystemWatcher(rootPath.FullName);
            this.shortcutWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            this.shortcutWatcher.IncludeSubdirectories = true;
            this.shortcutWatcher.Changed += OnShortcutWatcher_Changed;
            this.shortcutWatcher.Created += OnShortcutWatcher_Changed;
            this.shortcutWatcher.Deleted += OnShortcutWatcher_Changed;
            this.shortcutWatcher.Renamed += OnShortcutWatcher_Renamed;
            this.shortcutWatcher.EnableRaisingEvents = true;
        }
        private void RebuildShortcutMenusHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShortcutMenus = null;
                BuildShortcutMenusAsync();
                this.shortcutTimer.Tick -= RebuildShortcutMenusHandler;
                this.shortcutTimer = null;
            }));
        }
        void OnShortcutWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (this.shortcutTimer != null) return;
            this.shortcutTimer = GetUITimer(TimeSpan.FromMilliseconds(900), RebuildShortcutMenusHandler);
        }

        void OnShortcutWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.shortcutTimer != null) return;
            this.shortcutTimer = GetUITimer(TimeSpan.FromMilliseconds(900), RebuildShortcutMenusHandler);
        }
        #endregion
        private ShortcutMenuItemViewModel GetShortcutMenu(DirectoryInfo root, string name)
        {
            var menu = new ShortcutMenuItemViewModel() { Text = name ?? root.Name, Icon = "folder" };
            foreach (DirectoryInfo dir in root.GetDirectories())
            {
                ShortcutMenuItemViewModel item = GetShortcutMenu(dir, dir.Name);
                menu.Children.Add(item);
            }
            foreach (FileInfo file in root.GetFiles("*.*"))
            {
                menu.Children.Add(new ShortcutMenuItemViewModel()
                {
                    Text = file.Name.Replace(file.Extension, ""),
                    File = file,
                    Icon = "shortcut",
                    Command = ShortcutActionCommand
                });
            }
            return menu;
        }
        public ShortcutMenuItemViewModel ShortcutMenus
        {
            get { return shortcutMenus; }
            set
            {
                shortcutMenus = value;
                OnPropertyChanged("ShortcutMenus");
            }
        }
        private ShortcutMenuItemViewModel shortcutMenus;
        #endregion

        #region Time Tracker ViewModels
        public TimeTrackViewModel ViewModel { get { return viewModel; } }
        private readonly TimeTrackViewModel viewModel;

        public CurrentEntryViewModel CurrentTimeEntry
        {
            get { return currentTimeEntry; }
            set
            {
                if (this.currentTimeEntry != value)
                {
                    this.currentTimeEntry = value;
                    OnPropertyChanged("CurrentTimeEntry");
                }
            }
        }
        private CurrentEntryViewModel currentTimeEntry = null;

        public WorkItemViewModel CurrentWorkItem
        {
            get 
            {
                if (CurrentTimeEntry != null && CurrentTimeEntry.CurrentWorkItem != null)
                    this.currentWorkItem = WorkItemViewModel.ConvertFrom(CurrentTimeEntry.CurrentWorkItem);
                else
                    this.currentWorkItem = WorkItemViewModel.ConvertFrom(viewModel.CurrentWorkItem);
                return this.currentWorkItem;
            }
            set
            {
                if (this.currentWorkItem != value)
                {
                    this.currentWorkItem = value;
                    OnPropertyChanged("CurrentWorkItem");
                }
            }
        }
        private WorkItemViewModel currentWorkItem = null;
        #endregion

        #region Commands
        private ICommand punchInCommand;
        /// <summary>
        /// PunchIn/PunchOut command will create new TimeEntry and start its timer or close the CurrentTimeEntry
        /// </summary>
        public ICommand PunchInCommand
        {
            get
            {
                if (this.punchInCommand == null)
                    this.punchInCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => this.ViewModel.CurrentWorkItem != null,
                        CommandAction = (o) =>
                            {
                                if (CurrentTimeEntry == null)
                                {
                                    CurrentTimeEntry = new CurrentEntryViewModel(ViewModel);

                                    var dialog = new TrayDialog
                                    {
                                        Title = "New Time Entry",
                                        Content = new TimeEntryForm(),
                                        WindowStartupLocation = WindowStartupLocation.Manual,
                                        IsTrayWindow = true,
                                        DataContext = CurrentTimeEntry
                                    };
                                    dialog.Buttons = new Button[] { dialog.OkButton, dialog.CancelButton };
                                    dialog.ShowDialog();

                                    if (dialog.DialogResult.HasValue &&
                                        dialog.DialogResult.Value)
                                    {
                                        CurrentTimeEntry.PunchIn();
                                        IsTimerActive = true;
                                    }
                                    else
                                        CurrentTimeEntry = null;
                                }
                                else
                                {
                                    CurrentTimeEntry.PunchOut();
                                    IsTimerActive = false;
                                    CurrentTimeEntry = null;
                                }
                            }
                    };
                return this.punchInCommand;
            }
        }
        private ICommand newWorkItemCommand;
        /// <summary>
        /// Create a new WorkItem, add it to the list and make it the CurrentWorkItem
        /// </summary>
        public ICommand NewWorkItemCommand
        {
            get
            {
                if (this.newWorkItemCommand == null)
                    this.newWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => ViewModel != null,
                        CommandAction = (o) =>
                        {
                            var dialog = new TrayDialog
                            {
                                Title = "New Work Item",
                                Content = new WorkItemForm(),
                                WindowStartupLocation = WindowStartupLocation.Manual,
                                IsTrayWindow = true,
                                SizeToContent = System.Windows.SizeToContent.Height,
                                DataContext = new WorkItemViewModel()
                            };
                            dialog.Buttons = new Button[] { dialog.OkButton, dialog.CancelButton };
                            dialog.ShowDialog();

                            if (dialog.DialogResult.HasValue &&
                                dialog.DialogResult.Value)
                            {
                                WorkItem model = ((WorkItemViewModel)dialog.DataContext).WorkItem;
                                ViewModel.AddWorkItemCommand.Execute(model);
                                ViewModel.SaveWorkItemCommand.Execute(null);
                                WorkItemMenus.Children.Add(NewPunchMenuItem(model));
                            }
                        }
                    };
                return this.newWorkItemCommand;
            }
        }
        private ICommand selectWorkItemCommand;
        /// <summary>
        /// Select a given WorkItem and make it the CurrentWorkItem
        /// </summary>
        public ICommand SelectWorkItemCommand
        {
            get
            {
                if (this.selectWorkItemCommand == null)
                    this.selectWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (idParam) => 
                            {
                                try
                                {
                                    Guid id = (Guid)idParam;
                                    return this.ViewModel.CurrentWorkItem != null && !id.Equals(this.ViewModel.CurrentWorkItem.Id);
                                }
                                catch { return true; }
                            },
                        CommandAction = (idParam) =>
                            {
                                Guid id = (Guid)idParam;
                            
                                // are we already punched in?
                                if (this.CurrentTimeEntry != null)
                                {
                                    // since we have a current time entry, this will punch out and clean up timer
                                    this.PunchInCommand.Execute(null);
                                }
                                this.ViewModel.SelectWorkItemById(id);
                                // punch back in with new time entry
                                if (this.ViewModel.CurrentWorkItem != null && this.CurrentTimeEntry == null)
                                {
                                    this.PunchInCommand.Execute(null);
                                }
                            }
                    };
                return this.selectWorkItemCommand;
            }
        }
        private ICommand shortcutActionCommand;
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShortcutActionCommand
        {
            get
            {
                if (this.shortcutActionCommand == null)
                    this.shortcutActionCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (filePath) =>
                        {
                            if (filePath != null)
                            {
                                Process.Start(((FileInfo)filePath).FullName);
                            }
                        }
                    };
                return this.shortcutActionCommand;
            }
        }
        private ICommand showWindowCommand;
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                if (this.showWindowCommand == null)
                    this.showWindowCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => Application.Current.MainWindow == null,
                        CommandAction = (o) =>
                        {
                            Application.Current.MainWindow = new MainWindow();
                            Application.Current.MainWindow.Show();
                        }
                    };
                return this.showWindowCommand;
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand {CommandAction = (o) => Application.Current.Shutdown()};
            }
        }
        #endregion

        #region Shutdown/Clean up
        internal void CleanUp()
        {
            // do clean up here
            try
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                    timer.Tick -= OnTimerTick;
                }
            }
            finally
            {
                timer = null;
            }
            try
            {
                if (this.shortcutWatcher != null)
                {
                    this.shortcutWatcher.Changed -= OnShortcutWatcher_Changed;
                    this.shortcutWatcher.Created -= OnShortcutWatcher_Changed;
                    this.shortcutWatcher.Deleted -= OnShortcutWatcher_Changed;
                    this.shortcutWatcher.Renamed -= OnShortcutWatcher_Renamed;
                }
                this.shortcutWatcher.Dispose();
            }
            finally
            {
                this.shortcutWatcher = null;
            }
        }
        #endregion
    }
}
