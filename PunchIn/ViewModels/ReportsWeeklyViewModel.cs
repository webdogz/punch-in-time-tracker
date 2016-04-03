using PunchIn.Models;
using PunchIn.Services;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using PunchIn.Extensions;
using System.Collections.Generic;
using System.Windows.Input;

namespace PunchIn.ViewModels
{
    public class ReportsWeeklyViewModel : ViewModelBase
    {
        private readonly PunchInService service;
        public ReportsWeeklyViewModel()
        {
            service = new PunchInService();
            try
            {
                RefreshItems();
                WeekOfYearFilter = WeekOfYearList.LastOrDefault().WeekOfYear;
                IsSummaryReportSelected = true;
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
            }
        }

        private void RefreshItems()
        {
            reportList = service.GetReportExportItems();
            OnPropertyChanged("ReportItems");
        }

        #region Properties
        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }
        private double weeklyHoursCompleted;
        public double WeeklyHoursCompleted
        {
            get { return weeklyHoursCompleted; }
            set
            {
                if (weeklyHoursCompleted != value)
                {
                    weeklyHoursCompleted = value;
                    OnPropertyChanged("WeeklyHoursCompleted");
                }
            }
        }
        #endregion

        #region Filter Propterties
        private ObservableCollection<WorkTypesListCheckbox> workTypesFilter;
        public ObservableCollection<WorkTypesListCheckbox> WorkTypesFilter
        {
            get 
            {
                if (workTypesFilter == null)
                    workTypesFilter = new ObservableCollection<WorkTypesListCheckbox>(
                        Enum.GetValues(typeof(WorkType)).Cast<WorkType>()
                        .Select(wt => new WorkTypesListCheckbox
                        {
                            WorkType = wt,
                            Text = wt.ToString(),
                            IsSelected = true,
                            Command = CheckedCommand
                        }));
                return workTypesFilter; 
            }
            set
            {
                if (workTypesFilter != value)
                {
                    workTypesFilter = value;
                    OnPropertyChanged("WorkTypesFilter");
                    OnPropertyChanged("ReportItems");
                }
            }
        }
        private List<WeekOfYearItem> weekOfYearList;
        public List<WeekOfYearItem> WeekOfYearList
        {
            get
            {
                if (weekOfYearList == null)
                {
                    weekOfYearList = reportList.Select(r => r.WeekOfYear)
                        .Distinct().Select(d => new WeekOfYearItem
                        {
                            WeekOfYear = d,
                            WeekOfYearDate = d.GetWeekOfYearDate().ToString("dd/MM/yyyy")
                        }).ToList();
                }
                return weekOfYearList;
            }
        }
        private int weekOfYearFilter;
        public int WeekOfYearFilter
        {
            get { return weekOfYearFilter; }
            set
            {
                if (weekOfYearFilter != value)
                {
                    weekOfYearFilter = value;
                    OnPropertyChanged("WeekOfYearFilter");
                    OnPropertyChanged("ReportItems");
                }
            }
        }
        
        private bool isSummaryReportSelected;
        public bool IsSummaryReportSelected
        {
            get { return isSummaryReportSelected; }
            set
            {
                if (isSummaryReportSelected != value)
                {
                    isSummaryReportSelected = value;
                    OnPropertyChanged("IsSummaryReportSelected");
                }
            }
        }
        #endregion

        #region List Properties
        private List<ReportExportItem> reportList;
        public ObservableCollection<ReportExportItem> ReportItems
        {
            get 
            {
                WorkType[] workTypes = workTypesFilter.Where(w => w.IsSelected == true).Select(w => w.WorkType).ToArray();
                List<ReportExportItem> list = reportList.Where(r => workTypes.Contains(r.WorkType) &&
                    r.WeekOfYear == weekOfYearFilter).ToList();
                WeeklyHoursCompleted = list.Sum(w => w.HoursCompleted);
                return new ObservableCollection<ReportExportItem>(list);
            }
        }
        private ICommand checkedCommand;
        public ICommand CheckedCommand
        {
            get
            {
                if (checkedCommand == null)
                    checkedCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) =>
                            {
                                OnPropertyChanged("ReportItems");
                            }
                    };
                return checkedCommand;
            }
        }
        
        private ReportExportItem selectedItem;
        public ReportExportItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }
        #endregion

        #region Methods
        private List<ReportExportItem> GetReportExportItems(bool allItems)
        {
            PunchInService dbService = new PunchInService();
            if (allItems)
                return dbService.GetReportExportItems();
            else
                return dbService.GetReportExportItems(WeekOfYearFilter);
        }
        private List<ReportExportItem> GetSummaryReportExportItems(bool allItems)
        {
            PunchInService dbService = new PunchInService();
            if (allItems)
                return dbService.GetSummaryReportExportItems();
            else
                return dbService.GetSummaryReportExportItems(WeekOfYearFilter);
        }
        private string GetSaveAsLocation()
        {
            string exportFilename = string.Format("{0}-{1}-{2}.csv",
                                    "times",
                                    System.Environment.UserName,
                                    System.DateTime.Now.ToString("yyyyMMddHHmm"));
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = exportFilename;
            dlg.InitialDirectory = GlobalConfig.DatabaseFolder;
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
            bool? result = dlg.ShowDialog();
            if (result == true)
                return dlg.FileName;
            else return string.Empty;
        }
        #endregion

        #region Commands
        private ICommand refreshReportItemsCommand;
        /// <summary>
        /// Refresh the view
        /// </summary>
        public ICommand RefreshReportItemsCommand
        {
            get
            {
                if (refreshReportItemsCommand == null)
                {
                    refreshReportItemsCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) =>
                            {
                                RefreshItems();
                            }
                    };
                }
                return refreshReportItemsCommand;
            }
        }
        private ICommand exportToExcelCommand;
        /// <summary>
        /// Export the currently selected weeks items to csv
        /// </summary>
        public ICommand ExportToExcelCommand
        {
            get
            {
                if (exportToExcelCommand == null)
                    exportToExcelCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => !IsBusy,
                        CommandAction = (o) =>
                        {
                            IsBusy = true;
                            try
                            {
                                string exportPath = GetSaveAsLocation();
                                if (!string.IsNullOrWhiteSpace(exportPath))
                                {
                                    CsvExportService csv = new CsvExportService();
                                    List<ReportExportItem> exportItems;
                                    string[] columns;
                                    if (IsSummaryReportSelected)
                                    {
                                        columns = new string[] { "TfsId", "ServiceCall", "Change", "Title", "HoursCompleted", "HoursRemaining", "State", "WorkType", "WeekOfYear", "WeekStarting" };
                                        exportItems = GetSummaryReportExportItems(o == null);
                                    }
                                    else
                                    {
                                        columns = new string[] { "TfsId", "ServiceCall", "Change", "Title", "HoursCompleted", "HoursRemaining", "Description", "State", "Status", "WorkType", "WeekOfYear", "WeekStarting" };
                                        exportItems = GetReportExportItems(o == null);
                                    }
                                    csv.ExportCollection(exportItems, columns, true, exportPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Errors = ex.Message;
                            }
                            IsBusy = false;
                        }
                    };
                return exportToExcelCommand;
            }
        }
        #endregion
    }

    public class WorkTypesListCheckbox : ViewModelBase
    {
        public WorkType WorkType { get; set; }
        public string Text { get; set; }
        public ICommand Command { get; set; }
        public bool IsSelected { get; set; }
    }

    public class WeekOfYearItem
    {
        public int WeekOfYear { get; set; }
        public string WeekOfYearDate {get;set;}
    }
}
