using System;
using PunchIn.Models;

namespace PunchIn.ViewModels
{
    public class CurrentEntryViewModel : ViewModelBase
    {
        private readonly TimeTrackViewModel parent;
        public CurrentEntryViewModel(TimeTrackViewModel parent/*, TimeEntry currentEntry = null*/)
        {
            //TODO: Refactor all this if/else/then crap code to something a little more elegant :)
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
        public TimeEntry CurrentEntry
        {
            get { return currentEntry; }
            set
            {
                currentEntry = value;
                OnPropertyChanged("CurrentEntry");
            }
        }
        private TimeEntry currentEntry;

        public WorkItem CurrentWorkItem
        {
            get { return currentWorkItem; }
            set
            {
                currentWorkItem = value;
                OnPropertyChanged("CurrentWorkItem");
            }
        }
        private WorkItem currentWorkItem;

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
