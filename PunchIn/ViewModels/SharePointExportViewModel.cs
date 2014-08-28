using PunchIn.Models;
using PunchIn.Services;
using System.Collections.Generic;
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
                if (e.PropertyName == "IsBusy")
                {
                    IsBusy = ((SharePointService)s).IsBusy;
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
        private bool isBusy = false;
        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    OnPropertyChanged("IsBusy");
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
                    CanExecuteFunc = (o) => !HasSharePointList && !IsBusy,
                    CommandAction = (o) =>
                    {
                        this.service.AddTimeTrackList();
                    }
                };
            }
        }

        public ICommand ExportToSharePointListCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => HasSharePointList && !IsBusy,
                    CommandAction = (o) =>
                    {
                        IsBusy = true;
                        PunchInService dbService = new PunchInService();

                        List<SPExportItem> exportList = new List<SPExportItem>();
                        foreach(ReportExportItem item in dbService.GetReportExportItems())
                        {
                            exportList.Add(new SPExportItem
                                {
                                    TimeEntryGuid = item.Id,
                                    TfsId = item.TfsId ?? 0,
                                    ServiceCall = item.ServiceCall ?? 0,
                                    Change = item.Change ?? 0,
                                    Title = item.Title,
                                    Description = item.Description,
                                    HoursCompleted = item.HoursCompleted,
                                    HoursRemaining = item.HoursRemaining,
                                    State = item.State.ToString(),
                                    Status = item.Status.ToString(),
                                    WorkType = item.WorkType.ToString(),
                                    WeekStarting = item.WeekStarting,
                                    WeekOfYear = item.WeekOfYear
                                });
                        }

                        this.service.ExportCollectionToSharePointList(exportList);

                        IsBusy = false;
                    }
                };
            }
        }

        public ICommand ExportToExcelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = (o) => !IsBusy,
                    CommandAction = (o) =>
                    {
                        IsBusy = true;
                        string exportFilename = string.Format("{0}-{1}-{2}.csv", "times", System.Environment.UserName, System.DateTime.Now);
                        string exportPath = System.IO.Path.Combine(Properties.Settings.Default.DefaultUserDatabaseFolderLocation, exportFilename);
                        PunchInService dbService = new PunchInService();
                        CsvExportService csv = new CsvExportService();
                        csv.ExportCollection(dbService.GetReportExportItems(), 
                            new string[] { "TfsId", "ServiceCall", "Change", "Title", "HoursCompleted", "HoursRemaining", "Description", "State", "Status", "WorkType" }, 
                            true,  exportPath);
                        IsBusy = false;
                    }
                };
            }
        }
    }
}
