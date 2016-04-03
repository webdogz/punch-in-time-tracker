using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using PunchIn.Core.Contracts;

namespace PunchIn.ViewModels
{
    public class TrackerViewModel : TimeTrackViewModel, ICleanUp
    {
        public TrackerViewModel() : base(NotifyIconViewModel.Current.ViewModel.WorkItems)
        {
            dirtyWorkItems = new List<WorkItem>();
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
            get { return observableWorkItems; }
            set
            {
                if (observableWorkItems != value)
                {
                    observableWorkItems = value;
                    OnPropertyChanged("ObservableWorkItems");
                }
            }
        }
        
        private WorkItemViewModel selectedWorkItemViewModel;
        public WorkItemViewModel SelectedWorkItemViewModel
        {
            get
            {
                return selectedWorkItemViewModel;
            }
            set
            {
                if (selectedWorkItemViewModel != value)
                {
                    if (selectedWorkItemViewModel != null)
                        selectedWorkItemViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                    selectedWorkItemViewModel = value;
                    if (selectedWorkItemViewModel != null)
                        selectedWorkItemViewModel.PropertyChanged += SelectedWorkItemViewModel_PropertyChanged;
                    OnPropertyChanged("SelectedWorkItemViewModel", "IsSelectedWorkItemNotSelected", "WorkItemSummaryHoursCompleted");
                }
            }
        }

        private TimeEntryViewModel selectedEntryViewModel;
        public TimeEntryViewModel SelectedEntryViewModel
        {
            get { return selectedEntryViewModel; }
            set
            {
                if (selectedEntryViewModel != value)
                {
                    if (selectedEntryViewModel != null)
                        selectedEntryViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                    selectedEntryViewModel = value;
                    if (selectedEntryViewModel != null)
                    {
                        selectedEntryViewModel.Init();
                        selectedEntryViewModel.PropertyChanged += SelectedWorkItemViewModel_PropertyChanged;
                    }
                    OnPropertyChanged("SelectedEntryViewModel");
                    base.CurrentEntry = value == null ? null : selectedEntryViewModel.TimeEntry;
                }
            }
        }

        public double WorkItemSummaryHoursCompleted
        {
            get 
            {
                if (selectedWorkItemViewModel == null)
                {
                    WorkItemSummaryHoursRemaining = 0;
                    WorkItemSummaryEffort = 0;
                    return 0;
                }
                TimeSpan span = new TimeSpan();
                selectedWorkItemViewModel.Entries.ForEach(new Action<TimeEntryViewModel>(e =>
                {
                    DateTime end = e.EndDate ?? e.StartDate;
                    span = span.Add(end - e.StartDate);
                }));
                WorkItemSummaryEffort = selectedWorkItemViewModel.Effort * 8;
                WorkItemSummaryHoursRemaining = WorkItemSummaryEffort - span.TotalHours;
                return span.TotalHours;
            }
        }
        
        private double workItemSummaryEffort;
        public double WorkItemSummaryEffort
        {
            get { return workItemSummaryEffort; }
            set
            {
                if (workItemSummaryEffort != value)
                {
                    workItemSummaryEffort = value;
                    OnPropertyChanged("WorkItemSummaryEffort");
                }
            }
        }
        private double workItemSummaryHoursRemaining;
        public double WorkItemSummaryHoursRemaining
        {
            get { return workItemSummaryHoursRemaining; }
            set
            {
                if (workItemSummaryHoursRemaining != value)
                {
                    workItemSummaryHoursRemaining = value;
                    OnPropertyChanged("WorkItemSummaryHoursRemaining");
                }
            }
        }

        public bool IsSelectedWorkItemNotSelected
        {
            get
            {
                if (selectedWorkItemViewModel != null)
                {
                    return (!Guid.Empty.Equals(NotifyIconViewModel.Current.CurrentWorkItemId) &&
                            NotifyIconViewModel.Current.CurrentWorkItemId != selectedWorkItemViewModel.Id);
                }
                return true;
            }
        }

        private List<WorkItem> dirtyWorkItems;
        public List<WorkItem> DirtyWorkItems
        {
            get { return dirtyWorkItems; }
        }
        #endregion

        #region Enum Lists
        public IEnumerable<Status> StatesList
        {
            get { return Enum.GetValues(typeof(Status)).Cast<Status>(); }
        }

        public IEnumerable<WorkType> WorkTypesList
        {
            get { return Enum.GetValues(typeof(WorkType)).Cast<WorkType>(); }
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

        private void RemoveWorkItem(WorkItemViewModel viewModel)
        {
            int index = WorkItems.FindIndex(w => w.Id == viewModel.Id);
            if (index > -1)
            {
                WorkItems.RemoveAt(index);
                OnPropertyChanged("WorkItems");
            }
        }

        private void SetObservableWorkItems()
        {
            var selected = selectedWorkItemViewModel;
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
                    int dirtyIdx = DirtyWorkItems.FindIndex(w => w.Id == item.Id);
                    if (dirtyIdx > -1)
                        DirtyWorkItems[dirtyIdx] = item.WorkItem;
                    else
                        DirtyWorkItems.Add(item.WorkItem);
                }
                UpdateWorkItemCollection(workItem, false);
                OnPropertyChanged("WorkItemSummaryHoursCompleted");
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
                    if (CurrentWorkItem != null && !CurrentWorkItem.Id.Equals(NotifyIconViewModel.Current.CurrentWorkItemId))
                        SetObservableWorkItems();
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
            if (e.PropertyName == "StartDate" || e.PropertyName == "EndDate" || e.PropertyName == "Effort")
            {
                OnPropertyChanged("WorkItemSummaryHoursCompleted");
            }
            if (e.PropertyName == "IsDirty")
            {
                ICanDirty item = (sender as ICanDirty);
                if (item.IsDirty)
                {
                    if (!selectedWorkItemViewModel.IsDirty)
                        selectedWorkItemViewModel.ForceIsDirty(true, false); // no notify
                    IsDirty = true;
                }
                UpdateSelectedWorkItem(selectedWorkItemViewModel);
            }
        }
        #endregion

        #region Commands
        private ICommand _newCommand;
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new DelegateCommand
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
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsDirty,
                        CommandAction = (o) =>
                        {
                            foreach (WorkItem item in DirtyWorkItems)
                                Service.SaveWorkItem(item);
                            DirtyWorkItems.Clear();
                            IsDirty = false;
                            SetObservableWorkItems();
                        }
                    };
                }
                return _saveCommand;
            }
        }
        private ICommand _deleteWorkItemCommand;
        public ICommand DeleteWorkItemCommand
        {
            get
            {
                if (_deleteWorkItemCommand == null)
                {
                    _deleteWorkItemCommand = new DelegateCommand
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
                                    Service.DeleteWorkItem(SelectedWorkItemViewModel.Id);
                                    RemoveWorkItem(SelectedWorkItemViewModel);
                                    if (CurrentWorkItem != null && CurrentWorkItem.Id == SelectedWorkItemViewModel.Id)
                                        CurrentWorkItem = null;
                                    SelectedWorkItemViewModel = null;
                                    NotifyIconViewModel.Current.RefreshWorkItemMenus();
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
                return _deleteWorkItemCommand;
            }
        }
        private ICommand _selectedCurrentWorkItemCommand;
        public ICommand SelectedCurrentWorkItemCommand
        {
            get
            {
                if (_selectedCurrentWorkItemCommand == null)
                {
                    _selectedCurrentWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsSelectedWorkItemNotSelected,
                        CommandAction = (o) =>
                        {
                            WorkItem wi = selectedWorkItemViewModel.WorkItem;
                            if (CurrentWorkItem != wi)
                            {
                                if (NotifyIconViewModel.Current.CurrentTimeEntry != null)
                                    NotifyIconViewModel.Current.PunchInCommand.Execute(null);
                                // set the global selected current work item
                                NotifyIconViewModel.Current.ViewModel.CurrentWorkItem = wi;
                                SetObservableWorkItems();
                            }
                        }
                    };
                }
                return _selectedCurrentWorkItemCommand;
            }
        }
        #endregion

        #region Exit/Cleanup
        public void CleanUp()
        {
            try
            {
                NotifyIconViewModel.Current.PropertyChanged -= NotifyIcon_PropertyChanged;
                if (selectedWorkItemViewModel != null)
                    selectedWorkItemViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                if (selectedEntryViewModel != null)
                    selectedEntryViewModel.PropertyChanged -= SelectedWorkItemViewModel_PropertyChanged;
                if (NotifyIconViewModel.Current.Manager == this)
                    NotifyIconViewModel.Current.Manager = null;
            }
            catch { }
        }
        #endregion
    }
}
