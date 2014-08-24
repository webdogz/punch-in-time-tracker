using PunchIn.Models;
using PunchIn.Services;
using System.Collections.ObjectModel;

namespace PunchIn.ViewModels
{
    public class SharePointExportViewModel : ViewModelBase
    {
        private readonly SharePointService service;
        public SharePointExportViewModel()
        {
            service = new SharePointService();
            SharePointList = new ObservableCollection<SPExportItem>(service.GetListItems());
        }

        private ObservableCollection<SPExportItem> sharePointList;
        public ObservableCollection<SPExportItem> SharePointList
        {
            get { return this.sharePointList; }
            set
            {
                if (this.sharePointList != value)
                {
                    this.sharePointList = value;
                    OnPropertyChanged("SharePointList");
                }
            }
        }
    }
}
