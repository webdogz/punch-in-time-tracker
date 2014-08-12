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
        private readonly PunchInService client;
        #region ctor and init
        public TimeTrackViewModel()
        {
            client = new PunchInService();
            this.Refresh();
        }
        private void Refresh()
        {
            Task.Run(async () =>
            {
                WorkItems = await client.GetWorkItemsAsync();
                IsDirty = false;
            });
        }
        #endregion

        #region Fields and Properties
        public IEnumerable<WorkItem> WorkItems
        {
            get { return workItems; }
            set
            {
                workItems = value;
                OnPropertyChanged("WorkItems");
                if (CurrentWorkItem == null)
                    CurrentWorkItem = workItems.LastOrDefault();
            }
        }
        private IEnumerable<WorkItem>  workItems;

        public WorkItem CurrentWorkItem
        {
            get { return currentWorkItem; }
            set
            {
                currentWorkItem = value;
                OnPropertyChanged("CurrentWorkItem");
                CurrentEntry = null;
                Entries = new BindingList<TimeEntry>(currentWorkItem.Entries);
            }
        }
        private WorkItem currentWorkItem;

        public BindingList<TimeEntry> Entries
        {
            get 
            {
                return this.entries;
                //if (this.CurrentWorkItem != null)
                //    return new ObservableCollection<TimeEntry>(this.CurrentWorkItem.Entries);
                //return null;
            }
            set
            {
                this.entries = value;
                OnPropertyChanged("Entries");
            }
        }
        private BindingList<TimeEntry> entries;

        public TimeEntry CurrentEntry
        {
            get { return currentEntry; }
            set
            {
                currentEntry = value;
                OnPropertyChanged("CurrentEntry");
                OnPropertyChanged("CanModifyEntry");
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
                OnPropertyChanged("Entries");
            }
        }
        private bool isDirty = false;
        #endregion

        #region Public Methods
        public WorkItem GetWorkItemById(Guid id)
        {
            return this.client.GetItemById(id);
        }
        public void SelectWorkItemById(Guid id)
        {
            CurrentWorkItem = WorkItems.FirstOrDefault(w => w.Id == id);
        }
        #endregion

        #region Commands
        public ICommand RefreshWorkItemsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => IsDirty,
                    CommandAction = (o) =>
                    {
                        this.Refresh();
                    }
                };
            }
        }

        public ICommand SaveWorkItemCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => this.CurrentWorkItem != null,
                    CommandAction = (o) =>
                    {
                        client.SaveWorkItem(CurrentWorkItem);
                        IsDirty = false;
                    }
                };
            }
        }

        public ICommand AddWorkItemCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => true,
                    CommandAction = (o) =>
                    {
                        if (null != o && o is WorkItem)
                            this.CurrentWorkItem = o as WorkItem;
                        else
                            this.CurrentWorkItem = new WorkItem();
                        IsDirty = true;
                    }
                };
            }
        }

        public ICommand AddEntryCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => this.CurrentWorkItem != null,
                    CommandAction = (o) =>
                    {
                        if (null != o && o is TimeEntry)
                            this.CurrentEntry = o as TimeEntry;
                        else
                            this.CurrentEntry = new TimeEntry { StartDate = DateTime.Now };
                        this.CurrentWorkItem.Entries.Add(this.CurrentEntry);
                        this.Entries = new BindingList<TimeEntry>(this.CurrentWorkItem.Entries);
                    }
                };
            }
        }
        #endregion
    }
}
