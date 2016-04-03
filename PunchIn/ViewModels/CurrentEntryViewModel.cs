using System;
using PunchIn.Models;
using System.Collections.Generic;

namespace PunchIn.ViewModels
{
    public class CurrentEntryViewModel : ViewModelBase
    {
        private readonly TimeTrackViewModel parent;
        public CurrentEntryViewModel(TimeTrackViewModel parent)
        {
            this.parent = parent;
            CurrentWorkItem = this.parent.CurrentWorkItem;
            if (this.parent.CurrentEntry != null)
                CurrentEntry = this.parent.CurrentEntry;
            else
                CurrentEntry = new TimeEntry { StartDate = DateTime.Now, Status = Status.Analysis };
        }

        #region Methods
        public void PunchIn()
        {
            if (!parent.CanModifyEntry)
                parent.AddEntryCommand.Execute(CurrentEntry);
            //todo: to save or not to save?
            parent.SaveWorkItemCommand.Execute(null);
        }
        public void PunchOut()
        {
            CurrentEntry.EndDate = DateTime.Now;
            CurrentEntry.Status = Status.Done;
            parent.SaveWorkItemCommand.Execute(new Action(() => this.parent.CurrentEntry = null));
        }
        #endregion

        #region Properties
        public List<WorkItem> WorkItems
        {
            get { return this.parent.WorkItems; }
        }
        public List<WorkItemViewModel> ObservableWorkItems
        {
            get
            {
                Converters.ModelListToViewModelListConverter<WorkItem, WorkItemViewModel> converter =
                        new Converters.ModelListToViewModelListConverter<WorkItem, WorkItemViewModel>();
                return new List<WorkItemViewModel>(converter.Convert(WorkItems, null));
            }
        }

        public TimeEntry CurrentEntry
        {
            get { return currentEntry; }
            set
            {
                if (this.currentEntry != value)
                {
                    this.currentEntry = value;
                    OnPropertyChanged("CurrentEntry");
                }
            }
        }
        private TimeEntry currentEntry;

        public WorkItem CurrentWorkItem
        {
            get { return currentWorkItem; }
            set
            {
                if (this.currentWorkItem != value)
                {
                    this.currentWorkItem = value;
                    OnPropertyChanged("CurrentWorkItem", "SelectedWorkItem");
                }
            }
        }
        private WorkItem currentWorkItem;

        private WorkItemViewModel selectedWorkItem;
        public WorkItemViewModel SelectedWorkItem
        {
            get 
            {
                if (this.selectedWorkItem == null && this.currentWorkItem != null)
                    this.selectedWorkItem = WorkItemViewModel.ConvertFrom(this.currentWorkItem);
                return this.selectedWorkItem; }
            set
            {
                if (!this.selectedWorkItem.Equals(value))
                {
                    this.selectedWorkItem = value;
                    WorkItem wi = value.WorkItem;
                    // set the global selection as well
                    this.parent.CurrentWorkItem =
                    CurrentWorkItem = wi;
                    OnPropertyChanged("SelectedWorkItem");
                }
            }
        }

        public bool CanModifyEntry
        {
            get { return CurrentEntry != null; }
        }

        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                OnPropertyChanged("DialogResult");
            }
        }
        private bool? dialogResult;
        #endregion
    }
}
