using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System;

namespace PunchIn.ViewModels
{
    public class TrackerViewModel : ViewModelBase
    {
        public TrackerViewModel()
        {
            this.viewModel = new TimeTrackViewModel();
        }

        public TimeTrackViewModel ViewModel
        {
            get { return this.viewModel; }
        }
        private readonly TimeTrackViewModel viewModel;

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
        //TODO: Add commands for WorkItem list...delete, and canexecute only if item is not 
        //      NotifyIconViewModel.Current.CurrentWorkItem
    }
}
