using System.Collections.ObjectModel;

namespace PunchIn.ViewModels
{
    public class WorkItemManagerViewModel : ViewModelBase
    {
        private readonly TimeTrackViewModel parentViewModel;
        public WorkItemManagerViewModel(TimeTrackViewModel vm)
        {
            parentViewModel = vm;
        }

        public ObservableCollection<PunchMenuItemViewModel> TreeItems
        {
            get { return treeItems; }
            set
            {
                treeItems = value;
                OnPropertyChanged("TreeItems");
            }
        }
        private ObservableCollection<PunchMenuItemViewModel> treeItems;
    }
}
