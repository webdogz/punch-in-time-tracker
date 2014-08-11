using Hardcodet.Wpf.TaskbarNotification;
using PunchIn.Models;
using PunchIn.ViewModels;
using PunchIn.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel : ViewModelBase
    {
        public NotifyIconViewModel()
        {
            this.viewModel = new TimeTrackViewModel();
            this.viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "WorkItems")
                    BuildWorkItemMenusAsync();
            };
            BuildShortcutMenusAsync();
            IsTimerActive = false;
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnTimerTick, Application.Current.Dispatcher);
        }
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
            PunchMenuItemViewModel menu = new PunchMenuItemViewModel() { Text = "Work Items", Icon = "Resources/list.png" };
            menu.Children.Add(new PunchMenuItemViewModel
            {
                Text = "New Work Item...",
                Icon = "Resources/add.png",
                Command = NewWorkItemCommand
            });
            menu.Children.Add(null); // add a seperator
            foreach (WorkItem item in this.viewModel.WorkItems.Where(w => w.Status != States.Done))
            {
                string icon = (item.WorkType.ToString() ?? "task").ToLower();
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
                PunchMenuItemViewModel m = new PunchMenuItemViewModel
                {
                    Text = string.Format("[{0}] {1}", id, item.Title),
                    Icon = string.Format("Resources/{0}.png", icon),
                    Id = item.Id,
                    Command = WorkItemSelectCommand
                };
                menu.Children.Add(m);
            }
            return menu;
        }

        public PunchMenuItemViewModel WorkItemMenus
        {
            get 
            {
                return workItemMenus; 
            }
            set
            {
                workItemMenus = value;
                OnPropertyChanged("WorkItemMenus");
            }
        }
        private PunchMenuItemViewModel workItemMenus;

        public string PunchMenuText
        {
            get { return punchMenuText; }
            set
            {
                punchMenuText = value;
                OnPropertyChanged("PunchMenuText");
            }
        }
        private string punchMenuText;

        public string PunchMenuIcon
        {
            get { return punchMenuIcon; }
            set
            {
                punchMenuIcon = value;
                OnPropertyChanged("PunchMenuIcon");
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
                    PunchMenuIcon = "Resources/punchout.png";
                    PunchMenuText = "Punch Out";
                }
                else
                {
                    PunchMenuIcon = "Resources/punchin.png";
                    PunchMenuText = "Punch In...";
                }
                IsTimerDone = !IsTimerActive;
            }
        }
        private bool isTimerActive;

        public bool IsTimerDone
        {
            get { return isTimeDone; }
            set
            {
                isTimeDone = value;
                OnPropertyChanged("IsTimerDone");
            }
        }
        private bool isTimeDone;

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
        private async void BuildShortcutMenusAsync()
        {
            await Task.Run(() => 
            {
                DirectoryInfo rootPath = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Favorites), "HIN SC2TFS"));
                ShortcutMenus = GetShortcutMenu(rootPath, "Shortcuts");
                Debug.WriteLine("Shortcut menu done!");
            });
        }
        private ShortcutMenuItemViewModel GetShortcutMenu(DirectoryInfo root, string name)
        {
            var menu = new ShortcutMenuItemViewModel() { Text = name ?? root.Name, Icon = "Resources/folder.png" };
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
                    Icon = "Resources/shortcut.png",
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

        private DispatcherTimer timer;
        public TimeTrackViewModel ViewModel { get { return viewModel; } }
        private readonly TimeTrackViewModel viewModel;

        public TimeEntryViewModel CurrentTimeEntry
        {
            get { return currentTimeEntry; }
            set
            {
                currentTimeEntry = value;
                OnPropertyChanged("CurrentTimeEntry");
            }
        }
        private TimeEntryViewModel currentTimeEntry = null;

        

        #region Commands
        public ICommand PunchInCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => ViewModel.CurrentWorkItem != null,
                    CommandAction = (o) =>
                        {
                            if (CurrentTimeEntry == null)
                            {
                                CurrentTimeEntry = new TimeEntryViewModel(ViewModel);
                                TimeEntryDialog dialog = new TimeEntryDialog(CurrentTimeEntry);
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
            }
        }
        public ICommand NewWorkItemCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => ViewModel != null,
                    CommandAction = (o) =>
                    {
                        WorkItemDialog dialog = new WorkItemDialog();
                        dialog.ShowDialog();
                        if (dialog.DialogResult.HasValue &&
                            dialog.DialogResult.Value)
                        {
                            ViewModel.AddWorkItemCommand.Execute(dialog.Model);
                            ViewModel.SaveWorkItemCommand.Execute(null);
                        }
                    }
                };
            }
        }
        public ICommand WorkItemSelectCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => true,
                    CommandAction = (idParam) =>
                        {
                            Guid id = (Guid)idParam;
                            this.ViewModel.SelectWorkItemById(id);
                        }
                };
            }
        }
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShortcutActionCommand
        {
            get
            {
                return new DelegateCommand
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
            }
        }

        public ICommand ShowCurrentTaskPopupCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => true,
                    CommandAction = (o) =>
                    {
                        TaskbarIcon notifyIcon = (TaskbarIcon)Application.Current.FindResource("NotifyIcon");
                        CurrentTaskPopup balloon = new CurrentTaskPopup { DataContext = notifyIcon.DataContext };
                        //show balloon and close it after 10 seconds
                        notifyIcon.ShowCustomBalloon(balloon, System.Windows.Controls.Primitives.PopupAnimation.Slide, 10000);
                    }
                };
            }
        }
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                var self = this;
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => Application.Current.MainWindow == null,
                    CommandAction = (o) =>
                    {
                        //Application.Current.MainWindow = new WorkItemsManager(ViewModel);
                        Application.Current.MainWindow = new TimeTrackWindow { DataContext = self };
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (o) => Application.Current.MainWindow.Close(),
                    CanExecuteFunc = (o) => Application.Current.MainWindow != null
                };
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
    }
}
