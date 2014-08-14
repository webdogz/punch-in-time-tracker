using PunchIn.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PunchIn.ViewModels
{
    public class WorkItemManagerViewModel : ViewModelBase
    {
        private readonly TimeTrackViewModel parentViewModel;
        public WorkItemManagerViewModel(TimeTrackViewModel vm)
        {
            parentViewModel = vm;
        }

        public ObservableCollection<PunchMenuItemViewModel> WorkItemMenus
        {
            get { return workItemMenus; }
            set
            {
                workItemMenus = value;
                OnPropertyChanged("WorkItemMenus");
            }
        }
        private ObservableCollection<PunchMenuItemViewModel> workItemMenus;

        private void BuildWorkItemMenuItems()
        {
            ObservableCollection<PunchMenuItemViewModel> items = new ObservableCollection<PunchMenuItemViewModel>();
            foreach (WorkItem item in this.parentViewModel.WorkItems.Where(w => w.Status != States.Done))
            {
                string icon = (item.WorkType.ToString() ?? WorkTypes.Task.ToString()).ToLower();
                int id = 0;
                switch (item.WorkType)
                {
                    case WorkTypes.BacklogItem:
                    case WorkTypes.Datafix:
                    case WorkTypes.Bug:
                        id = item.TfsId ?? 0;
                        break;
                    case WorkTypes.Change:
                        id = item.Change ?? 0;
                        break;
                    case WorkTypes.ServiceCall:
                        id = item.ServiceCall ?? 0;
                        break;
                }
                PunchMenuItemViewModel m = new PunchMenuItemViewModel
                {
                    Text = string.Format("[{0}] {1}", id, item.Title),
                    Icon = string.Format("Resources/{0}.png", icon),
                    Id = item.Id,
                    Command = WorkItemSelectCommand
                };
                items.Add(m);
            }
            WorkItemMenus = items;
        }

        public ICommand WorkItemSelectCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => true,
                    CommandAction = (o) => { }
                };
            }
        }
    }
}
