using System;
using PunchIn.Models;
using EmitMapper;
using EmitMapper.MappingConfiguration;

namespace PunchIn.ViewModels
{
    public class TimeEntryViewModel : DbViewModelBase, IGuidPK
    {
        public TimeEntryViewModel()
        {
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
                    OnPropertyChanged("Description");
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
                    OnPropertyChanged("StartDate");
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
                    OnPropertyChanged("EndDate");
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
                    OnPropertyChanged("Status");
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

        #endregion

        #region Methods
        /// <summary>
        /// New up a TimeEntryViewModel based on the timeEntry
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns>New WorkItemViewModel mapped from WorkItem</returns>
        public static TimeEntryViewModel ConvertFrom(TimeEntry timeEntry)
        {
            return ToViewModel<TimeEntry, TimeEntryViewModel>(timeEntry);
        }
        /// <summary>
        /// Gets the TimeEntry associated with this TimeEntryViewModel instance
        /// </summary>
        public TimeEntry TimeEntry
        {
            get
            {
                //return ToModel<TimeEntryViewModel, TimeEntry>(this, this.Id);
                return ToModel<TimeEntryViewModel, TimeEntry>(this);
            }
        }
        #endregion
    }
}
