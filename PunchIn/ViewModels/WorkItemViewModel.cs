using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchIn.ViewModels
{
    public class WorkItemViewModel : DbViewModelBase, IGuidPK
    {
        public WorkItemViewModel()
        {
            this.Entries = new List<TimeEntryViewModel>();
        }

        #region Properties
        public Guid Id { get; set; }
        public int? TfsId
        {
            get { return this.tfsId; }
            set
            {
                if (this.tfsId != value)
                {
                    this.tfsId = value;
                    OnPropertyChanged("TfsId");
                }
            }
        }
        private int? tfsId;

        public int? ServiceCall
        {
            get { return this.serviceCall; }
            set
            {
                if (this.serviceCall != value)
                {
                    this.serviceCall = value;
                    OnPropertyChanged("ServiceCall");
                }
            }
        }
        private int? serviceCall;

        public int? Change
        {
            get { return this.change; }
            set
            {
                if (this.change != value)
                {
                    this.change = value;
                    OnPropertyChanged("Change");
                }
            }
        }
        private int? change;

        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        private string title;

        public double Effort
        {
            get { return this.effort; }
            set
            {
                if (this.effort != value)
                {
                    this.effort = value;
                    OnPropertyChanged("Effort");
                }
            }
        }
        private double effort;

        public States Status
        {
            get { return this.status; }
            set
            {
                if (this.status != value)
                {
                    this.status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        private States status;

        public WorkTypes WorkType
        {
            get { return this.workType; }
            set
            {
                if (this.workType != value)
                {
                    this.workType = value;
                    OnPropertyChanged("WorkType");
                }
            }
        }
        private WorkTypes workType;
        #endregion

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
        #endregion

        public List<TimeEntryViewModel> Entries { get; set; }
        public ObservableElementCollection<TimeEntryViewModel> TimeEntries { get; set; }

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
            return ToViewModel<WorkItem, WorkItemViewModel>(workItem, EmitMapper.ObjectMapperManager.DefaultInstance
                        .GetMapper<WorkItem, WorkItemViewModel>(
                            new EmitMapper.MappingConfiguration.DefaultMapConfig().ConvertGeneric(typeof(IList<TimeEntry>), typeof(IList<TimeEntryViewModel>),
                                new EmitMapper.MappingConfiguration.DefaultCustomConverterProvider(typeof(Converters.ModelListToViewModelListConverter<TimeEntry, TimeEntryViewModel>))
                            ).PostProcess<WorkItemViewModel>((vm, state) => 
                            {
                                vm.TimeEntries = new ObservableElementCollection<TimeEntryViewModel>(vm.Entries);
                                vm.TimeEntries.ChildElementPropertyChanged += new ObservableElementCollection<TimeEntryViewModel>.ChildElementPropertyChangedEventHandler(vm.Entries_ChildElementPropertyChanged);
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

        public class TimeEntriesToVMListConverter
        {
            public List<TimeEntryViewModel> Convert(IList<PunchIn.Models.TimeEntry> from, object state)
            {
                if (from == null) return null;
                List<TimeEntryViewModel> list = new List<TimeEntryViewModel>();
                foreach (var item in from)
                {
                    list.Add(TimeEntryViewModel.ConvertFrom(item));
                }
                return list;
            }
        }
        #endregion

        #region Events
        void Entries_ChildElementPropertyChanged(ObservableElementCollection<TimeEntryViewModel>.ChildElementPropertyChangedEventArgs e)
        {
            Console.WriteLine((e.ChildElement as TimeEntryViewModel).Description);
        }
        #endregion
    }
}
