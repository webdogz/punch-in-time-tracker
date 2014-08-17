using EmitMapper;
using EmitMapper.MappingConfiguration;
using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchIn.ViewModels
{
    public class WorkItemViewModel : ViewModelBase
    {
        public WorkItemViewModel() { }

        #region Properties
        public Guid Id { get; set; }
        public int? TfsId
        {
            get { return tfsId; }
            set
            {
                tfsId = value;
                OnPropertyChanged("TfsId");
            }
        }
        private int? tfsId;

        public int? ServiceCall
        {
            get { return serviceCall; }
            set
            {
                serviceCall = value;
                OnPropertyChanged("ServiceCall");
            }
        }
        private int? serviceCall;

        public int? Change
        {
            get { return change; }
            set
            {
                change = value;
                OnPropertyChanged("Change");
            }
        }
        private int? change;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }
        private string title;

        public double Effort
        {
            get { return effort; }
            set
            {
                effort = value;
                OnPropertyChanged("Effort");
            }
        }
        private double effort;

        public States Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
        private States status;

        public WorkTypes WorkType
        {
            get { return workType; }
            set
            {
                workType = value;
                OnPropertyChanged("WorkType");
            }
        }
        private WorkTypes workType;
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

        #region Methods
        /// <summary>
        /// New up a WorkItemViewModel based on the workItem
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns>New WorkItemViewModel mapped from WorkItem</returns>
        public static WorkItemViewModel ConvertFrom(WorkItem workItem)
        {
            return ObjectMapperManager.DefaultInstance.GetMapper<WorkItem, WorkItemViewModel>().Map(workItem);
        }
        /// <summary>
        /// Gets the WorkItem associated with this WorkItemViewModel instance
        /// </summary>
        public WorkItem WorkItem
        {
            get
            {
                return ObjectMapperManager.DefaultInstance
                    .GetMapper<WorkItemViewModel, WorkItem>(
                        new DefaultMapConfig().ConstructBy<WorkItem>(() => new WorkItem(this.Id))
                    ).Map(this);
            }
        }
        #endregion
    }
}
