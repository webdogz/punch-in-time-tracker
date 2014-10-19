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
        internal PunchInService Client { get { return this.client; } }
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
            if (CurrentWorkItem == null)
                CurrentWorkItem = workItems.Where(w => w.Entries.Any(e => e.EndDate == null)).LastOrDefault() ?? workItems.LastOrDefault();
        }

        public virtual WorkItem CurrentWorkItem
        {
            get { return currentWorkItem; }
            set
            {
                if (this.currentWorkItem != value)
                {
                    this.currentWorkItem = value;
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
                if (this.currentEntry != value)
                {
                    this.currentEntry = value;
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
            return this.client.GetItemById(id);
        }
        public void SelectWorkItemById(Guid id)
        {
            CurrentWorkItem = WorkItems.FirstOrDefault(w => w.Id == id);
        }
        #endregion

        #region Commands
        private ICommand refreshWorkItemsCommand;
        public ICommand RefreshWorkItemsCommand
        {
            get
            {
                if (this.refreshWorkItemsCommand == null)
                    this.refreshWorkItemsCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsDirty,
                        CommandAction = (o) =>
                        {
                            this.Refresh();
                        }
                    };
                return this.refreshWorkItemsCommand;
            }
        }

        private ICommand saveWorkItemCommand;
        public ICommand SaveWorkItemCommand
        {
            get
            {
                if (this.saveWorkItemCommand == null)
                    this.saveWorkItemCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => this.CurrentWorkItem != null,
                        CommandAction = (o) =>
                        {
                            client.SaveWorkItem(CurrentWorkItem);
                            IsDirty = false;
                            if (o is Action)
                                (o as Action).Invoke();
                        }
                    };
                return this.saveWorkItemCommand;
            }
        }

        private ICommand addWorkItemCommand;
        public ICommand AddWorkItemCommand
        {
            get
            {
                if (this.addWorkItemCommand == null)
                    this.addWorkItemCommand = new DelegateCommand
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
                return this.addWorkItemCommand;
            }
        }

        private ICommand addEntryCommand;
        public ICommand AddEntryCommand
        {
            get
            {
                if (this.addEntryCommand == null)
                    this.addEntryCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => this.CurrentWorkItem != null,
                        CommandAction = (o) =>
                        {
                            TimeEntry entry;
                            if (null != o && o is TimeEntry)
                                entry = o as TimeEntry;
                            else
                                entry = new TimeEntry { StartDate = DateTime.Now };
                            this.CurrentWorkItem.Entries.Add(entry);
                            this.CurrentEntry = entry;
                        }
                    };
                return this.addEntryCommand;
            }
        }
        #endregion
    }
}
