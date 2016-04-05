using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using PunchIn.Models;
using PunchIn.Services;
using System.ComponentModel;
using System.Linq;

namespace PunchIn.ViewModels
{
    public class TimeTrackViewModel : ViewModelBase
    {
        private readonly PunchInService service;
        internal PunchInService Service { get { return service; } }
        #region ctor and init
        public TimeTrackViewModel()
        {
            service = new PunchInService();
            Refresh();
        }
        public TimeTrackViewModel(List<WorkItem> items)
        {
            service = new PunchInService();
            WorkItems = items;
        }
        private void Refresh()
        {
            Task.Run(async () =>
            {
                WorkItems = await service.GetWorkItemsAsync();
                IsDirty = false;
            });
        }
        #endregion

        #region Fields and Properties
        public virtual List<WorkItem> WorkItems
        {
            get { return workItems; }
            set
            {
                workItems = value;
                OnPropertyChanged("WorkItems");
                SetCurrentWorkItem();
            }
        }
        private List<WorkItem> workItems;
        protected void SetCurrentWorkItem()
        {
            if (CurrentWorkItem == null && (workItems != null && workItems.Count > 0))
                CurrentWorkItem = workItems.Where(w => w.Entries.Any(e => e.EndDate == null)).LastOrDefault() ?? workItems.LastOrDefault();
        }

        public virtual WorkItem CurrentWorkItem
        {
            get { return currentWorkItem; }
            set
            {
                if (currentWorkItem != value)
                {
                    currentWorkItem = value;
                    OnPropertyChanged("CurrentWorkItem");

                    if (CurrentWorkItem != null)
                        CurrentEntry = CurrentWorkItem.Entries.LastOrDefault(e => e.EndDate == null);
                    else
                        CurrentEntry = null;
                }
            }
        }
        private WorkItem currentWorkItem;

        public virtual TimeEntry CurrentEntry
        {
            get { return currentEntry; }
            set
            {
                if (currentEntry != value)
                {
                    currentEntry = value;
                    OnPropertyChanged("CurrentEntry");
                    OnPropertyChanged("CanModifyEntry");
                }
            }
        }
        private TimeEntry currentEntry;

        public bool CanModifyEntry
        {
            get { return CurrentEntry != null; }
        }

        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                OnPropertyChanged("IsDirty");
            }
        }
        private bool isDirty = false;
        #endregion

        #region Public Methods
        public WorkItem GetWorkItemById(Guid id)
        {
            return service.GetItemById(id);
        }
        public void SelectWorkItemById(Guid id)
        {
            CurrentWorkItem = WorkItems.FirstOrDefault(w => w.Id == id);
        }
        public void PunchIn(TimeEntry timeEntry)
        {
            if (!CanModifyEntry)
                AddEntryCommand.Execute(timeEntry);
            //todo: to save or not to save?
            SaveWorkItemCommand.Execute(null);
        }
        public void PunchOut(TimeEntry timeEntry)
        {
            if (!timeEntry.EndDate.HasValue)
                timeEntry.EndDate = DateTime.Now;
            timeEntry.Status = Status.Done;
            SaveWorkItemCommand.Execute(new Action(() => CurrentEntry = null));
        }
        #endregion

        #region Commands
        private ICommand refreshWorkItemsCommand;
        public ICommand RefreshWorkItemsCommand
        {
            get
            {
                if (refreshWorkItemsCommand == null)
                    refreshWorkItemsCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsDirty,
                        CommandAction = (o) =>
                        {
                            Refresh();
                        }
                    };
                return refreshWorkItemsCommand;
            }
        }

        private ICommand saveWorkItemCommand;
        public ICommand SaveWorkItemCommand
        {
            get
            {
                if (saveWorkItemCommand == null)
                    saveWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => CurrentWorkItem != null,
                        CommandAction = (o) =>
                        {
                            service.SaveWorkItem(CurrentWorkItem);
                            IsDirty = false;
                            if (o is Action)
                                (o as Action).Invoke();
                        }
                    };
                return saveWorkItemCommand;
            }
        }

        private ICommand addWorkItemCommand;
        public ICommand AddWorkItemCommand
        {
            get
            {
                if (addWorkItemCommand == null)
                    addWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) =>
                        {
                            if (null != o && o is WorkItem)
                                CurrentWorkItem = o as WorkItem;
                            else
                                CurrentWorkItem = new WorkItem();
                            if (WorkItems.FirstOrDefault(w => w.Id == CurrentWorkItem.Id) == default(WorkItem))
                                WorkItems.Add(CurrentWorkItem);
                            OnPropertyChanged("WorkItems");
                            IsDirty = true;
                        }
                    };
                return addWorkItemCommand;
            }
        }

        private ICommand addEntryCommand;
        public ICommand AddEntryCommand
        {
            get
            {
                if (addEntryCommand == null)
                    addEntryCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => CurrentWorkItem != null,
                        CommandAction = (o) =>
                        {
                            TimeEntry entry;
                            if (null != o && o is TimeEntry)
                                entry = o as TimeEntry;
                            else
                                entry = new TimeEntry { StartDate = DateTime.Now };
                            CurrentWorkItem.Entries.Add(entry);
                            CurrentEntry = entry;
                        }
                    };
                return addEntryCommand;
            }
        }
        #endregion
    }
}
