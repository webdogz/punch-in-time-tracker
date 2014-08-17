using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;

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

        //TODO: Add commands for WorkItem list...delete, and canexecute only if item is not 
        //      NotifyIconViewModel.Current.CurrentWorkItem
    }
}
