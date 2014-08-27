using PunchIn.Models;
using PunchIn.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

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
                if (e.PropertyName == "Errors")
                {
                    Errors = ((SharePointService)s).Errors;
                }
                if (e.PropertyName == "HasSharePointList")
                {
                    HasSharePointList = ((SharePointService)s).HasSharePointList;
                }
            };
            LoadItems();
        }
        private async void LoadItems()
        {
            var listItems = await this.service.GetListItems();
            SharePointList = new ObservableCollection<SPExportItem>(listItems);
        }
        
        private bool hasError;
        public bool HasError
        {
            get { return this.hasError; }
            set
            {
                if (this.hasError != value)
                {
                    this.hasError = value;
                    OnPropertyChanged("HasError");
                }
            }
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
                    HasError = !string.IsNullOrWhiteSpace(this.errors);
                }
            }
        }
        private bool hasSharePointList = true;
        public bool HasSharePointList
        {
            get { return this.hasSharePointList; }
            set
            {
                if (this.hasSharePointList != value)
                {
                    this.hasSharePointList = value;
                    OnPropertyChanged("HasSharePointList");
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

        public ICommand AddListCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => !HasSharePointList,
                    CommandAction = (o) =>
                    {
                        this.service.AddTimeTrackList();
                    }
                };
            }
        }
    }
}
