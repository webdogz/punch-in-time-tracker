using PunchIn.Core.Contracts;
using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchIn.ViewModels
{
    public class WorkItemViewModel : DbViewModelBase, IGuidPK, ICanDirty
    {
        private bool _initialised = false;
        public WorkItemViewModel()
        {
            Entries = new List<TimeEntryViewModel>();
        }
        public void Init()
        {
            _initialised = true;
        }

        #region Properties
        public Guid Id { get; set; }
        public int? TfsId
        {
            get { return tfsId; }
            set
            {
                if (tfsId != value)
                {
                    tfsId = value;
                    EnsurePropertyChanged(value, "TfsId");
                }
            }
        }
        private int? tfsId;

        public int? ServiceCall
        {
            get { return serviceCall; }
            set
            {
                if (serviceCall != value)
                {
                    serviceCall = value;
                    EnsurePropertyChanged(value, "ServiceCall");
                }
            }
        }
        private int? serviceCall;

        public int? Change
        {
            get { return change; }
            set
            {
                if (change != value)
                {
                    change = value;
                    EnsurePropertyChanged(value, "Change");
                }
            }
        }
        private int? change;

        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    EnsurePropertyChanged(value, "Title");
                }
            }
        }
        private string title;

        public double Effort
        {
            get { return effort; }
            set
            {
                if (effort != value)
                {
                    effort = value;
                    EnsurePropertyChanged(value, "Effort");
                }
            }
        }
        private double effort;

        public Status Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    EnsurePropertyChanged(value, "Status");
                }
            }
        }
        private Status status;

        public WorkType WorkType
        {
            get { return workType; }
            set
            {
                if (workType != value)
                {
                    workType = value;
                    EnsurePropertyChanged(value, "WorkType");
                }
            }
        }
        private WorkType workType;

        public List<TimeEntryViewModel> Entries { get; set; }

        #region Bubble Properties
        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                ForceIsDirty(value, true);
            }
        }
        private bool isDirty = false;

        protected void EnsurePropertyChanged(object obj, params string[] propertyNames)
        {
            OnPropertyChanged(propertyNames);
            if (_initialised && obj != null)
                IsDirty = true;
        }
        #endregion

        #endregion

        #region Enum Lists
        public IEnumerable<Status> StatesList
        {
            get { return Enum.GetValues(typeof(Status)).Cast<Status>(); }
        }

        public IEnumerable<WorkType> WorkTypesList
        {
            get { return Enum.GetValues(typeof(WorkType)).Cast<WorkType>(); }
        }
        #endregion

        #region Methods
        public void ForceIsDirty(bool dirty, bool notify)
        {
            isDirty = dirty;
            if (notify)
                OnPropertyChanged("IsDirty");
        }
        /// <summary>
        /// New up a WorkItemViewModel based on the workItem
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns>New WorkItemViewModel mapped from WorkItem</returns>
        public static WorkItemViewModel ConvertFrom(WorkItem workItem)
        {
            return ToViewModel(workItem, EmitMapper.ObjectMapperManager.DefaultInstance
                        .GetMapper<WorkItem, WorkItemViewModel>(
                            new EmitMapper.MappingConfiguration.DefaultMapConfig().ConvertGeneric(typeof(IList<TimeEntry>), typeof(IList<TimeEntryViewModel>),
                                new EmitMapper.MappingConfiguration.DefaultCustomConverterProvider(typeof(Converters.ModelListToViewModelListConverter<TimeEntry, TimeEntryViewModel>))
                            ).PostProcess<WorkItemViewModel>((vm, state) => 
                            {
                                vm.Init();
                                return vm;
                            })
                        ));
        }
        /// <summary>
        /// Gets the WorkItem associated with this WorkItemViewModel instance
        /// </summary>
        public WorkItem WorkItem
        {
            get
            {
                return ToModel<WorkItemViewModel, WorkItem>(this);
            }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            WorkItemViewModel o = (obj as WorkItemViewModel);
            if (o != null) return o.Id.Equals(Id);
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            string ids = "";
            if (TfsId.HasValue)
                ids += string.Format("TfsId: {0}\n", TfsId);
            if (ServiceCall.HasValue)
                ids += string.Format("SC: {0}\n", ServiceCall);
            if (Change.HasValue)
                ids += string.Format("CH: {0}\n", Change);
            return string.Format("{0}{1}", ids, Title);
        }
        #endregion
    }
}
