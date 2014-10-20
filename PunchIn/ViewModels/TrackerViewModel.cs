using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using PunchIn.Services;

namespace PunchIn.ViewModels
{
    public class TrackerViewModel : TimeTrackViewModel, ICleanUp
    {
        public TrackerViewModel() : base()
        {
            this.dirtyWorkItems = new List<WorkItem>();
            NotifyIconViewModel.Current.PropertyChanged += NotifyIcon_PropertyChanged;
            NotifyIconViewModel.Current.Manager = this;
        }

        #region TimeTracker Overrides
        public override List<WorkItem> WorkItems
        {
            get
            {
                return base.WorkItems;
            }
            set
            {
                base.WorkItems = value;
                if (base.WorkItems != null)
                {
                    SetObservableWorkItems();
                }
            }
        }

        public override WorkItem CurrentWorkItem
        {
            get
            {
                return base.CurrentWorkItem;
            }
            set
            {
                if (base.CurrentWorkItem != value)
                {
                    base.CurrentWorkItem = value;
                    if (value != null)
                        SelectedWorkItemViewModel = WorkItemViewModel.ConvertFrom(value);
                }
            }
        }
        #endregion

        #region Properties
        private ObservableCollection<WorkItemViewModel> observableWorkItems;
        public ObservableCollection<WorkItemViewModel> ObservableWorkItems
        {
            get { return this.observableWorkItems; }
            set
            {
                if (this.observableWorkItems != value)
                {
                    this.observableWorkItems = value;
                    OnPropertyChanged("ObservableWorkItems");
                }
            }
        }
        
        private WorkItemViewModel selectedWorkItemViewModel;
        public WorkItemViewModel SelectedWorkItemViewModel
        {
            get
            {
                return this.selectedWorkItemViewModel;
            }
            set
            {
                if (this.selectedWorkItemViewModel != value)
                {
                    if (this.selectedWorkItemViewModel != null)
                        this.selectedWorkItemViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                    this.selectedWorkItemViewModel = value;
                    if (this.selectedWorkItemViewModel != null)
                        this.selectedWorkItemViewModel.PropertyChanged += SelectedWorkItemViewModel_PropertyChanged;
                    OnPropertyChanged("SelectedWorkItemViewModel", "IsSelectedWorkItemNotSelected", "WorkItemSummaryHoursCompleted");
                }
            }
        }

        private TimeEntryViewModel selectedEntryViewModel;
        public TimeEntryViewModel SelectedEntryViewModel
        {
            get { return this.selectedEntryViewModel; }
            set
            {
                if (this.selectedEntryViewModel != value)
                {
                    if (this.selectedEntryViewModel != null)
                        this.selectedEntryViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                    this.selectedEntryViewModel = value;
                    if (this.selectedEntryViewModel != null)
                    {
                        this.selectedEntryViewModel.Init();
                        this.selectedEntryViewModel.PropertyChanged += SelectedWorkItemViewModel_PropertyChanged;
                    }
                    OnPropertyChanged("SelectedEntryViewModel");
                    base.CurrentEntry = value == null ? null : this.selectedEntryViewModel.TimeEntry;
                }
            }
        }

        public double WorkItemSummaryHoursCompleted
        {
            get 
            {
                if (this.selectedWorkItemViewModel == null) return 0;
                TimeSpan span = new TimeSpan();
                this.selectedWorkItemViewModel.Entries.ForEach(new Action<TimeEntryViewModel>(e =>
                {
                    DateTime end = e.EndDate ?? e.StartDate;
                    span = span.Add(end - e.StartDate);
                }));
                return span.TotalHours;
            }
        }

        public bool IsSelectedWorkItemNotSelected
        {
            get
            {
                if (this.selectedWorkItemViewModel != null)
                {
                    return (NotifyIconViewModel.Current.CurrentWorkItem != null &&
                            NotifyIconViewModel.Current.CurrentWorkItem.Id != this.selectedWorkItemViewModel.Id);
                }
                return true;
            }
        }

        private List<WorkItem> dirtyWorkItems;
        public List<WorkItem> DirtyWorkItems
        {
            get { return this.dirtyWorkItems; }
        }
        #endregion

        #region Enum Lists
        public IEnumerable<States> StatesList
        {
            get { return Enum.GetValues(typeof(States)).Cast<States>(); }
        }

        public IEnumerable<WorkTypes> WorkTypesList
        {
            get { return Enum.GetValues(typeof(WorkTypes)).Cast<WorkTypes>(); }
        }
        #endregion

        #region Methods
        private void UpdateWorkItemsWithCurrentWorkItem()
        {
            UpdateWorkItemCollection(NotifyIconViewModel.Current.CurrentWorkItem, true);
        }

        private void UpdateWorkItemCollection(WorkItemViewModel viewModel, bool updateObservables)
        {
            int index = WorkItems.FindIndex(w => w.Id == viewModel.Id);
            if (index > -1)
                WorkItems[index] = viewModel.WorkItem;
            else
                WorkItems.Add(viewModel.WorkItem);
            if (updateObservables)
                SetObservableWorkItems();
        }

        private void SetObservableWorkItems()
        {
            var selected = this.selectedWorkItemViewModel;
            Converters.ModelListToViewModelListConverter<WorkItem, WorkItemViewModel> converter =
                        new Converters.ModelListToViewModelListConverter<WorkItem, WorkItemViewModel>();
            ObservableWorkItems = new ObservableCollection<WorkItemViewModel>(converter.Convert(WorkItems, null));
            if (selected != null)
            {
                SelectedWorkItemViewModel = ObservableWorkItems.FirstOrDefault(w => w.Id == selected.Id);
            }
        }

        private void UpdateSelectedWorkItem(WorkItemViewModel workItem)
        {
            if (IsDirty && workItem.IsDirty)
            {
                foreach (var item in ObservableWorkItems.Where(w => w.IsDirty))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Dirty Item:TfsId:{0}:{1}", item.TfsId, item.Id));
                    int dirtyIdx = DirtyWorkItems.FindIndex(w => w.Id == item.Id);
                    if (dirtyIdx > -1)
                        DirtyWorkItems[dirtyIdx] = item.WorkItem;
                    else
                        DirtyWorkItems.Add(item.WorkItem);
                }
                UpdateWorkItemCollection(workItem, false);
            }
        }
        #endregion

        #region Events Handlers
        /// <summary>
        /// Detect when the currently selected global workitem changes so we can sync it up here.
        /// You should not be able to modify the current global workitem from the Tracker manager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PunchIn":
                case "PunchOut":
                case "SaveWorkItem":
                case "NewWorkItem":
                    UpdateWorkItemsWithCurrentWorkItem();
                    break;
                case "CurrentTimeEntry":
                case "CurrentWorkItem":
                    OnPropertyChanged("IsSelectedWorkItemNotSelected", "WorkItems", "ObservableWorkItems");
                    break;
            }
        }
        /// <summary>
        /// Poormans NotifyPropertyChanged Bubbler. Classes that impl ICanDirty can subscribe to this event to
        /// send IsDirty flag and in turn notify up to singleton NotifyIconViewModel.Current.Manager.IsDirty.
        /// MainWindow will check this flag before closing and prompt user to save/dicard changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedWorkItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDirty")
            {
                ICanDirty item = (sender as ICanDirty);
                if (item.IsDirty)
                    IsDirty = item.IsDirty;
                UpdateSelectedWorkItem(this.selectedWorkItemViewModel);
            }
        }
        #endregion

        #region Commands
        private ICommand _newCommand;
        public ICommand NewCommand
        {
            get
            {
                if (this._newCommand == null)
                {
                    this._newCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => NotifyIconViewModel.Current.NewWorkItemCommand.CanExecute(true),
                        CommandAction = (o) =>
                        {
                            NotifyIconViewModel.Current.NewWorkItemCommand.Execute(true);
                        }
                    };
                }
                return this._newCommand;
            }
        }
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (this._saveCommand == null)
                {
                    this._saveCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsDirty,
                        CommandAction = (o) =>
                        {
                            foreach (WorkItem item in DirtyWorkItems)
                                this.Client.SaveWorkItem(item);
                            DirtyWorkItems.Clear();
                            IsDirty = false;
                            SetObservableWorkItems();
                        }
                    };
                }
                return this._saveCommand;
            }
        }
        private ICommand _deleteWorkItemCommand;
        public ICommand DeleteWorkItemCommand
        {
            get
            {
                if (this._deleteWorkItemCommand == null)
                {
                    this._deleteWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => CurrentWorkItem != null,
                        CommandAction = (o) =>
                        {
                            try
                            {
                                string confirmMsg = string.Format("Delete the following WorkItem?\n{0}", 
                                    SelectedWorkItemViewModel.ToString());
                                if (Webdogz.UI.Controls.ModernDialog.ShowMessage(
                                    confirmMsg, "Are you sure?", 
                                    System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                                {
                                    this.Client.DeleteWorkItem(CurrentWorkItem.Id);
                                    this.WorkItems.Remove(CurrentWorkItem);
                                    NotifyIconViewModel.Current.RefreshWorkItemMenus();
                                    CurrentWorkItem = null;
                                    SetCurrentWorkItem();
                                    SetObservableWorkItems();
                                    OnPropertyChanged("WorkItems", "ObservableWorkItems");
                                }
                            }
                            catch (Exception ex)
                            {
                                Errors = ex.Message;
                            }
                        }
                    };
                }
                return this._deleteWorkItemCommand;
            }
        }
        #endregion

        #region Exit/Cleanup
        public void CleanUp()
        {
            try
            {
                NotifyIconViewModel.Current.PropertyChanged -= NotifyIcon_PropertyChanged;
                if (this.selectedWorkItemViewModel != null)
                    this.selectedWorkItemViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                if (this.selectedEntryViewModel != null)
                    this.selectedEntryViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                if (NotifyIconViewModel.Current.Manager == this)
                    NotifyIconViewModel.Current.Manager = null;
            }
            catch { }
        }
        #endregion
    }
}
