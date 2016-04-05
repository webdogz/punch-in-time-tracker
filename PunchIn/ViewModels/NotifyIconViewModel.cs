using PunchIn.Core.Contracts;
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
using PunchIn.Extensions;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon
    /// </summary>
    public class NotifyIconViewModel : ViewModelBase, ICleanUp
    {
        #region Ctor and Instance Accessor
        private NotifyIconViewModel()
        {
            viewModel = new TimeTrackViewModel();
            viewModel.PropertyChanged += TimeTrackViewModel_PropertyChanged;
            BuildShortcutMenusAsync();
            SyncThemeSettings();
            IsTimerActive = false;
            timer = GetUITimer(TimeSpan.FromSeconds(1), OnTimerTick);
        }

        private static NotifyIconViewModel current;
        /// <summary>
        /// Gets the current <see cref="NotifyIconViewModel"/> instance
        /// </summary>
        public static NotifyIconViewModel Current
        {
            get
            {
                if (current == null) current = new NotifyIconViewModel();
                return current;
            }
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
                PunchMenuItemViewModel menu = new PunchMenuItemViewModel() { Text = "Work Items", Icon = "list" };
                GetWorkItemMenuItems(menu);
                WorkItemMenus = menu;
            });
        }
        private void GetWorkItemMenuItems(PunchMenuItemViewModel menu)
        {
            menu.Children.Clear();
            menu.Children.Add(new PunchMenuItemViewModel
            {
                Text = "New Work Item...",
                Icon = "add",
                Command = NewWorkItemCommand
            });
            menu.Children.Add(null); // add a seperator
            foreach (WorkItem item in viewModel.WorkItems.Where(w => w.Status != Status.Done))
            {
                menu.Children.Add(NewPunchMenuItem(item));
            }
        }

        private PunchMenuItemViewModel NewPunchMenuItem(WorkItem item)
        {
            string icon = (item.WorkType.ToString() ?? WorkType.Task.ToString()).ToLower();
            int id = 0;
            switch (item.WorkType)
            {
                case WorkType.BacklogItem:
                case WorkType.Datafix:
                case WorkType.Bug:
                    id = item.TfsId ?? 0;
                    break;
                case WorkType.Change:
                    id = item.Change ?? 0;
                    break;
                case WorkType.ServiceCall:
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
                if (workItemMenus != value)
                {
                    workItemMenus = value;
                    OnPropertyChanged("WorkItemMenus");
                }
            }
        }
        private PunchMenuItemViewModel workItemMenus;

        public void RefreshWorkItemMenus()
        {
            GetWorkItemMenuItems(WorkItemMenus);
        }

        #region Punch Menu Item
        public string PunchMenuText
        {
            get { return punchMenuText; }
            set
            {
                if (punchMenuText != value)
                {
                    punchMenuText = value;
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
                if (punchMenuIcon != value)
                {
                    punchMenuIcon = value;
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
                isTimerActive = value;
                OnPropertyChanged("IsTimerActive");
                if (isTimerActive)
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
            if (shortcutWatcher != null) return;
            shortcutWatcher = new FileSystemWatcher(rootPath.FullName);
            shortcutWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            shortcutWatcher.IncludeSubdirectories = true;
            shortcutWatcher.Changed += OnShortcutWatcher_Changed;
            shortcutWatcher.Created += OnShortcutWatcher_Changed;
            shortcutWatcher.Deleted += OnShortcutWatcher_Changed;
            shortcutWatcher.Renamed += OnShortcutWatcher_Renamed;
            shortcutWatcher.EnableRaisingEvents = true;
        }
        private void RebuildShortcutMenusHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShortcutMenus = null;
                BuildShortcutMenusAsync();
                shortcutTimer.Tick -= RebuildShortcutMenusHandler;
                shortcutTimer = null;
            }));
        }
        void OnShortcutWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (shortcutTimer != null) return;
            shortcutTimer = GetUITimer(TimeSpan.FromMilliseconds(900), RebuildShortcutMenusHandler);
        }

        void OnShortcutWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (shortcutTimer != null) return;
            shortcutTimer = GetUITimer(TimeSpan.FromMilliseconds(900), RebuildShortcutMenusHandler);
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
        private TimeTrackViewModel viewModel;

        public CurrentEntryViewModel CurrentTimeEntry
        {
            get { return currentTimeEntry; }
            set
            {
                if (currentTimeEntry != value)
                {
                    currentTimeEntry = value;
                    OnPropertyChanged("CurrentTimeEntry");
                }
            }
        }
        private CurrentEntryViewModel currentTimeEntry = null;

        
        public Guid CurrentWorkItemId
        {
            get { return currentWorkItem != null ? currentWorkItem.Id : Guid.Empty; }
        }
        public WorkItemViewModel CurrentWorkItem
        {
            get 
            {
                if (CurrentTimeEntry != null && CurrentTimeEntry.CurrentWorkItem != null)
                    currentWorkItem = WorkItemViewModel.ConvertFrom(CurrentTimeEntry.CurrentWorkItem);
                else
                    currentWorkItem = WorkItemViewModel.ConvertFrom(viewModel.CurrentWorkItem);
                return currentWorkItem;
            }
            set
            {
                if (currentWorkItem != value)
                {
                    currentWorkItem = value;
                    OnPropertyChanged("CurrentWorkItem");
                }
            }
        }
        private WorkItemViewModel currentWorkItem = null;

        private TrackerViewModel manager;
        public TrackerViewModel Manager
        {
            get { return manager; }
            set
            {
                if (manager != value)
                {
                    manager = value;
                    OnPropertyChanged("Manager");
                }
            }
        }
        #endregion

        #region Properties
        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }
        #endregion

        #region Event Handlers
        private void TimeTrackViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "WorkItems":
                    BuildWorkItemMenusAsync();
                    break;
                case "CurrentEntry":
                    if (CurrentTimeEntry == null)
                    {
                        if (viewModel.CurrentEntry != null)
                        {
                            CurrentTimeEntry = new CurrentEntryViewModel(ViewModel);
                            IsTimerActive = true;
                        }
                    }
                    break;
                // bubble notifications - add more when needed
                case "CurrentWorkItem":
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }
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
                if (punchInCommand == null)
                    punchInCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => ViewModel.CurrentWorkItem != null && !IsBusy,
                        CommandAction = (o) =>
                            {
                                IsBusy = true;
                                try
                                {
                                    if (CurrentTimeEntry == null)
                                    {
                                        switch (Properties.Settings.Default.PunchCardMode)
                                        {
                                            case Core.PunchCardMode.Timesheet:
                                                ViewModel.PunchIn(new TimeEntry()
                                                {
                                                    Description = string.Format("Timesheet Entry #{0}", ViewModel.CurrentWorkItem.Entries.Count),
                                                    StartDate = DateTime.Now,
                                                    Status = Status.InProgress
                                                });
                                                break;
                                            case Core.PunchCardMode.Project: // WorkItem mode
                                                var dialog = new TrayDialog
                                                {
                                                    Title = "New Time Entry",
                                                    Content = new TimeEntryForm(),
                                                    IsTrayWindow = (o == null),
                                                    DataContext = new CurrentEntryViewModel(ViewModel)
                                                };
                                                dialog.Buttons = new Button[] { dialog.OkButton, dialog.CancelButton };
                                                dialog.ShowDialog();

                                                if (dialog.DialogResult.HasValue &&
                                                    dialog.DialogResult.Value)
                                                {
                                                    var tmpCurrentTimeEntry = (dialog.DataContext as CurrentEntryViewModel);
                                                    tmpCurrentTimeEntry.PunchIn();
                                                    currentTimeEntry = tmpCurrentTimeEntry;
                                                    IsTimerActive = true;
                                                    OnPropertyChanged("PunchIn");
                                                }
                                                dialog.DataContext = null;
                                                dialog = null;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        CurrentTimeEntry.PunchOut();
                                        IsTimerActive = false;
                                        OnPropertyChanged("PunchOut");
                                        CurrentTimeEntry = null;
                                    }
                                }
                                finally
                                {
                                    IsBusy = false;
                                }
                            }
                    };
                return punchInCommand;
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
                if (newWorkItemCommand == null)
                    newWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => ViewModel != null && !IsBusy,
                        CommandAction = (o) =>
                        {
                            IsBusy = true;
                            try
                            {
                                switch (Properties.Settings.Default.PunchCardMode)
                                {
                                    case Core.PunchCardMode.Timesheet:
                                        if (CurrentTimeEntry != null)
                                            PunchInCommand.Execute(null);
                                        DateTime dt = DateTime.Now;
                                        AddNewWorkItem(new WorkItem()
                                        {
                                            WorkType = WorkType.Timer,
                                            Title = string.Format("Day {0}, week {1}", dt.ToString("dddd - dd-MMM-yy"), dt.GetWeekOfYear())
                                        });
                                        break;
                                    case Core.PunchCardMode.Project:
                                        var dialog = new TrayDialog
                                        {
                                            Title = "New Work Item",
                                            Content = new WorkItemForm(),
                                            IsTrayWindow = (o == null),
                                            SizeToContent = System.Windows.SizeToContent.Height,
                                            DataContext = new WorkItemViewModel()
                                        };
                                        dialog.Buttons = new Button[] { dialog.OkButton, dialog.CancelButton };
                                        dialog.ShowDialog();

                                        if (dialog.DialogResult.HasValue &&
                                            dialog.DialogResult.Value)
                                        {
                                            if (CurrentTimeEntry != null)
                                                PunchInCommand.Execute(null);
                                            WorkItem model = ((WorkItemViewModel)dialog.DataContext).WorkItem;
                                            AddNewWorkItem(model);
                                        }
                                        break;
                                }
                            }
                            finally
                            {
                                IsBusy = false;
                            }
                        }
                    };
                return newWorkItemCommand;
            }
        }

        private void AddNewWorkItem(WorkItem workItem)
        {
            ViewModel.AddWorkItemCommand.Execute(workItem);
            ViewModel.SaveWorkItemCommand.Execute(null);
            RefreshWorkItemMenus();
            OnPropertyChanged("CurrentWorkItem", "NewWorkItem");
        }

        private ICommand selectWorkItemCommand;
        /// <summary>
        /// Select a given WorkItem and make it the CurrentWorkItem
        /// </summary>
        public ICommand SelectWorkItemCommand
        {
            get
            {
                if (selectWorkItemCommand == null)
                    selectWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (idParam) => 
                            {
                                try
                                {
                                    Guid id = (Guid)idParam;
                                    return ViewModel.CurrentWorkItem != null && !id.Equals(ViewModel.CurrentWorkItem.Id);
                                }
                                catch { return true; }
                            },
                        CommandAction = (idParam) =>
                            {
                                Guid id = (Guid)idParam;
                            
                                // are we already punched in?
                                if (CurrentTimeEntry != null)
                                {
                                    // since we have a current time entry, this will punch out and clean up timer
                                    PunchInCommand.Execute(null);
                                }
                                ViewModel.SelectWorkItemById(id);
                                // punch back in with new time entry
                                if (ViewModel.CurrentWorkItem != null && CurrentTimeEntry == null)
                                {
                                    PunchInCommand.Execute(null);
                                }
                            }
                    };
                return selectWorkItemCommand;
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
                if (shortcutActionCommand == null)
                    shortcutActionCommand = new DelegateCommand
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
                return shortcutActionCommand;
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
                if (showWindowCommand == null)
                    showWindowCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) =>
                        {
                            if (Application.Current.MainWindow == null)
                            {
                                Application.Current.MainWindow = new MainWindow();
                                Application.Current.MainWindow.Show();
                            }
                            else
                                Application.Current.MainWindow.Activate();
                        }
                    };
                return showWindowCommand;
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
        public void CleanUp()
        {
            // do clean up here
            try
            {
                if (viewModel != null)
                    viewModel.PropertyChanged -= TimeTrackViewModel_PropertyChanged;
            }
            catch { /* gulp */ }
            finally
            {
                viewModel = null;
            }
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
                if (shortcutWatcher != null)
                {
                    shortcutWatcher.Changed -= OnShortcutWatcher_Changed;
                    shortcutWatcher.Created -= OnShortcutWatcher_Changed;
                    shortcutWatcher.Deleted -= OnShortcutWatcher_Changed;
                    shortcutWatcher.Renamed -= OnShortcutWatcher_Renamed;
                }
                shortcutWatcher.Dispose();
            }
            finally
            {
                shortcutWatcher = null;
            }
            try
            {
                current = null;
            }
            catch { }
        }
        #endregion
    }
}
