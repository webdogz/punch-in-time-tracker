using System;
using PunchIn.Models;
using PunchIn.Core.Contracts;

namespace PunchIn.ViewModels
{
    public class TimeEntryViewModel : DbViewModelBase, IGuidPK, ICanDirty
    {
        private bool _initialised = false;
        public TimeEntryViewModel()
        {
        }
        public void Init()
        {
            _initialised = true;
        }

        #region Properties
        public Guid Id { get; set; }
        
        private string description;
        public string Description
        {
            get { return description; }
            set
            { 
                if (description != value)
                {
                    description = value;
                    EnsurePropertyChanged(value, "Description");
                }
            }
        }

        private DateTime startDate;
        public DateTime StartDate
        { get { return startDate; }
            set
            {
                if (startDate != value)
                {
                    startDate = value;
                    EnsurePropertyChanged(value, "StartDate");
                }
            }
        }

        private DateTime? endDate;
        public DateTime? EndDate {
            get { return endDate; }
            set
            {
                if(endDate != value)
                {
                    endDate = value;
                    EnsurePropertyChanged(value, "EndDate");
                }
            }
        }

        private Status status;
        public Status Status
        {
            get { return status; }
            set
            {
                if(status != value)
                {
                    status = value;
                    EnsurePropertyChanged(value, "Status");
                }
            }
        }

        private WorkItem currentWorkItem;
        public WorkItem CurrentWorkItem
        {
            get { return currentWorkItem; }
            set
            {
                currentWorkItem = value;
                OnPropertyChanged("CurrentWorkItem");
            }
        }

        #region Bubble Properties
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

        protected void EnsurePropertyChanged(object obj, string propertyName)
        {
            OnPropertyChanged(propertyName);
            if (_initialised && obj != null)
                IsDirty = true;
        }
        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// New up a TimeEntryViewModel based on the timeEntry
        /// </summary>
        /// <param name="timeEntry"></param>
        /// <returns>New TimeEntryViewModel mapped from TimeEntry</returns>
        public static TimeEntryViewModel ConvertFrom(TimeEntry timeEntry)
        {
            return ToViewModel(timeEntry, EmitMapper.ObjectMapperManager.DefaultInstance
                        .GetMapper<TimeEntry, TimeEntryViewModel>(
                            new EmitMapper.MappingConfiguration.DefaultMapConfig()
                            .PostProcess<TimeEntryViewModel>((vm, state) =>
                            {
                                vm.Init();
                                return vm;
                            })
                        ));
        }
        /// <summary>
        /// Gets the TimeEntry associated with this TimeEntryViewModel instance
        /// </summary>
        public TimeEntry TimeEntry
        {
            get
            {
                return ToModel<TimeEntryViewModel, TimeEntry>(this);
            }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            TimeEntryViewModel o = (obj as TimeEntryViewModel);
            if (o != null) return o.Id.Equals(this.Id);
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            string df = "d-MMM-yyyy HH:mm";
            string end = EndDate.HasValue ? (EndDate??DateTime.Now).ToString(df) : "Not Complete";
            return string.Format("{0} ({1} - {2})", Description, StartDate.ToString(df), end);
        }
        #endregion
    }
}
