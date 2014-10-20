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
                CurrentEntry = new TimeEntry { StartDate = DateTime.Now, Status = States.Analysis };
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
            CurrentEntry.Status = States.Done;
            parent.SaveWorkItemCommand.Execute(new Action(() => this.parent.CurrentEntry = null));
        }
        #endregion

        #region Properties
        public List<WorkItem> WorkItems
        {
            get { return this.parent.WorkItems; }
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

        public WorkItem SelectedWorkItem
        {
            get { return this.currentWorkItem; }
            set
            {
                if (this.currentWorkItem != value)
                {
                    // set the global selection as well
                    // and let CurrentWorkItem do notifications
                    this.parent.CurrentWorkItem =
                    CurrentWorkItem = value;
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
