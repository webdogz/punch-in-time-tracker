using PunchIn.Models;
using PunchIn.Services;
using System;
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
            LoadItems();
        }
        private async void LoadItems()
        {
            IsBusy = true;
            try
            {
                IsSharePointEnabled = true;
                HasSharePointList = true;
                var listItems = await this.service.GetListItems();
                SharePointList = new ObservableCollection<SPExportItem>(listItems);
            }
            catch (System.Net.WebException webEx)
            {
                SetErrorsMessage(webEx);
                IsSharePointEnabled = HasSharePointList = false;
            }
            catch (ArgumentException aex)
            {
                SetErrorsMessage(aex);
                HasSharePointList = false;
            }
            catch (EntryPointNotFoundException listNotFoundEx)
            {
                SetErrorsMessage(listNotFoundEx);
                HasSharePointList = false;
            }
            catch (Exception ex)
            {
                SetErrorsMessage(ex);
                IsSharePointEnabled = HasSharePointList = false;
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void SetErrorsMessage(Exception e)
        {
            Errors = e.Message;
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
        private bool isSharePointEnabled;
        public bool IsSharePointEnabled
        {
            get { return this.isSharePointEnabled; }
            set
            {
                if (this.isSharePointEnabled != value)
                {
                    this.isSharePointEnabled = value;
                    OnPropertyChanged("IsSharePointEnabled");
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
        #region Commands
        private ICommand _refresh;
        public ICommand Refresh
        {
            get
            {
                if (this._refresh == null)
                    this._refresh = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) =>
                        {
                            LoadItems();
                        }
                    };
                return this._refresh;
            }
        }
        private ICommand _addListCommand;
        public ICommand AddListCommand
        {
            get
            {
                if (_addListCommand == null)
                    _addListCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsSharePointEnabled && !HasSharePointList && !IsBusy,
                        CommandAction = (o) =>
                        {
                            IsBusy = true;
                            try
                            {
                                this.service.CreateTimeTrackList();
                            }
                            catch (Exception ex)
                            {
                                SetErrorsMessage(ex);
                                IsSharePointEnabled = false;
                            }
                            IsBusy = false;
                        }
                    };
                return _addListCommand;
            }
        }

        private ICommand _exportToSharePointListCommand;
        public ICommand ExportToSharePointListCommand
        {
            get
            {
                if (_exportToSharePointListCommand == null)
                    _exportToSharePointListCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => IsSharePointEnabled && HasSharePointList && !IsBusy,
                        CommandAction = (o) =>
                        {
                            IsBusy = true;
                            try
                            {
                                PunchInService dbService = new PunchInService();
                                List<SPExportItem> exportList = new List<SPExportItem>();
                                foreach (ReportExportItem item in dbService.GetReportExportItems())
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
                            }
                            catch (Exception ex)
                            {
                                SetErrorsMessage(ex);
                            }
                            IsBusy = false;
                        }
                    };
                return _exportToSharePointListCommand;
            }
        }
        private ICommand _exportToExcelCommand;
        public ICommand ExportToExcelCommand
        {
            get
            {
                if (_exportToExcelCommand == null)
                    _exportToExcelCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => !IsBusy,
                        CommandAction = (o) =>
                        {
                            IsBusy = true;
                            try
                            {
                                string exportFilename = string.Format("{0}-{1}-{2}.csv",
                                    "times",
                                    System.Environment.UserName,
                                    System.DateTime.Now.ToString("yyyyMMddHHmm"));
                                string exportPath = System.IO.Path.Combine(Properties.Settings.Default.DefaultUserDatabaseFolderLocation, exportFilename);
                                PunchInService dbService = new PunchInService();
                                CsvExportService csv = new CsvExportService();
                                csv.ExportCollection(dbService.GetReportExportItems(),
                                    new string[] { "TfsId", "ServiceCall", "Change", "Title", "HoursCompleted", "HoursRemaining", "Description", "State", "Status", "WorkType" },
                                    true, exportPath);
                            }
                            catch (Exception ex)
                            {
                                SetErrorsMessage(ex);
                            }
                            IsBusy = false;
                        }
                    };
                return _exportToExcelCommand;
            }
        }
        #endregion
    }
}
