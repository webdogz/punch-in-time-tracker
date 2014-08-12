using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchIn.ViewModels
{
    public class WorkItemViewModel : ViewModelBase
    {
        public WorkItemViewModel() { }
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

        public IEnumerable<States> StatesList
        {
            get { return Enum.GetValues(typeof(States)).Cast<States>(); }
        }

        public IEnumerable<WorkTypes> WorkTypesList
        {
            get { return Enum.GetValues(typeof(WorkTypes)).Cast<WorkTypes>(); }
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
    }
}
