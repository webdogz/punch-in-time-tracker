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
            this.service = new SharePointService();
            this.service.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "Errors")
                {
                    Errors = ((SharePointService)s).Errors;
                }
            };
            LoadItems();
        }
        private async void LoadItems()
        {
            var listItems = await this.service.GetListItems();
            SharePointList = new ObservableCollection<SPExportItem>(listItems);
        }
        
        private string errors;
        public string Errors
        {
            get { return this.errors; }
            set
            {
                if (this.errors != value)
                {
                    this.errors = value;
                    OnPropertyChanged("Errors");
                }
            }
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
