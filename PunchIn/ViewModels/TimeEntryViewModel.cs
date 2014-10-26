using System;
using PunchIn.Models;
using EmitMapper;
using EmitMapper.MappingConfiguration;

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
            get { return this.description; }
            set
            { 
                if (this.description != value)
                {
                    this.description = value;
                    EnsurePropertyChanged(value, "Description");
                }
            }
        }

        private DateTime startDate;
        public DateTime StartDate
        { get { return this.startDate; }
            set
            {
                if (this.startDate != value)
                {
                    this.startDate = value;
                    EnsurePropertyChanged(value, "StartDate");
                }
            }
        }

        private Nullable<DateTime> endDate;
        public Nullable<DateTime> EndDate {
            get { return this.endDate; }
            set
            {
                if(this.endDate != value)
                {
                    this.endDate = value;
                    EnsurePropertyChanged(value, "EndDate");
                }
            }
        }

        private States status;
        public States Status
        {
            get { return this.status; }
            set
            {
                if(this.status != value)
                {
                    this.status = value;
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
            get { return this.isDirty; }
            set
            {
                this.isDirty = value;
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
            return ToViewModel<TimeEntry, TimeEntryViewModel>(timeEntry, EmitMapper.ObjectMapperManager.DefaultInstance
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
