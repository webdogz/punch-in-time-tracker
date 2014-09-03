using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using PunchIn.Services;

namespace PunchIn.ViewModels
{
    public class TrackerViewModel : TimeTrackViewModel
    {
        public TrackerViewModel() : base()
        {
            this.dirtyWorkItems = new List<WorkItem>();
        }

        #region TimeTracker Overrides
        public override WorkItem CurrentWorkItem
        {
            get
            {
                if (base.CurrentWorkItem != null)
                {
                    if (NotifyIconViewModel.Current.CurrentWorkItem != null &&
                        NotifyIconViewModel.Current.CurrentWorkItem.Id == base.CurrentWorkItem.Id)
                        return null;
                    return base.CurrentWorkItem;
                }
                return null;
            }
            set
            {
                if (base.CurrentWorkItem != value)
                {
                    if (NotifyIconViewModel.Current.CurrentWorkItem != null && value != null &&
                        NotifyIconViewModel.Current.CurrentWorkItem.Id == ((WorkItem)value).Id)
                        return;
                    base.CurrentWorkItem = value;
                }
            }
        }
        #endregion

        #region Properties
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
                            foreach (WorkItem item in DirtyWorkItems)
                                this.Client.SaveWorkItem(item);
                            IsDirty = false;
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
                                this.Client.DeleteWorkItem(CurrentWorkItem.Id);
                                this.WorkItems.Remove(CurrentWorkItem);
                                OnPropertyChanged("WorkItems");
                                CurrentWorkItem = null;
                                SetCurrentWorkItem();
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
    }
}
