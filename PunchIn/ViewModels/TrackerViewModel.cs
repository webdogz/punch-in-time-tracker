using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using PunchIn.Services;

namespace PunchIn.ViewModels
{
    public class TrackerViewModel : ViewModelBase
    {
        public TrackerViewModel()
        {
            this.viewModel = new TimeTrackViewModel();
            this.dirtyWorkItems = new List<WorkItem>();
        }

        public TimeTrackViewModel ViewModel
        {
            get { return this.viewModel; }
        }
        private readonly TimeTrackViewModel viewModel;

        public WorkItem CurrentWorkItem
        {
            get
            {
                if (this.ViewModel.CurrentWorkItem != null &&
                    NotifyIconViewModel.Current.CurrentWorkItem != null &&
                    NotifyIconViewModel.Current.CurrentWorkItem.Id != this.ViewModel.CurrentWorkItem.Id)
                {
                    return this.ViewModel.CurrentWorkItem;
                }
                return null;
            }
            set
            {
                if (this.ViewModel.CurrentWorkItem != value &&
                    NotifyIconViewModel.Current.CurrentWorkItem.Id != ((WorkItem)value).Id)
                {
                    this.ViewModel.CurrentWorkItem = value;
                    OnPropertyChanged("CurrentWorkItem");
                }
            }
        }
        private bool isDirty;
        public bool IsDirty
        {
            get { return this.isDirty; }
            set
            {
                if (this.isDirty != value)
                {
                    this.isDirty = value;
                    OnPropertyChanged("IsDirty");
                    if (!this.isDirty)
                        this.ViewModel.IsDirty = false;
                }
            }
        }
        private List<WorkItem> dirtyWorkItems;
        public List<WorkItem> DirtyWorkItems
        {
            get { return this.dirtyWorkItems; }
        }

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
        //TODO: Add commands for WorkItem list...delete, and canexecute only if item is not 
        //      NotifyIconViewModel.Current.CurrentWorkItem
        #region Commands
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
                            PunchInService service = new PunchInService();
                            foreach (WorkItem item in DirtyWorkItems)
                                service.SaveWorkItem(item);
                            IsDirty = false;
                        }
                    };
                }
                return this._saveCommand;
            }
        }
        #endregion
    }
}
