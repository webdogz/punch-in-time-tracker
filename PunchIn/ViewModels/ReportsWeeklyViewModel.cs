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
            this.service = new PunchInService();
            reportList = this.service.GetReportExportItems();
            try
            {
                int week = DateTime.Now.GetWeekOfYear();
                WeekOfYearFilter = WeekOfYearList.LastOrDefault().WeekOfYear;
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
            }
        }
        #region Filter Propterties
        private ObservableCollection<WorkTypesListCheckbox> workTypesFilter;
        public ObservableCollection<WorkTypesListCheckbox> WorkTypesFilter
        {
            get 
            {
                if (this.workTypesFilter == null)
                    this.workTypesFilter = new ObservableCollection<WorkTypesListCheckbox>(
                        Enum.GetValues(typeof(WorkTypes)).Cast<WorkTypes>()
                        .Select(wt => new WorkTypesListCheckbox
                        {
                            WorkType = wt,
                            Text = wt.ToString(),
                            IsSelected = true,
                            Command = CheckedCommand
                        }));
                return this.workTypesFilter; 
            }
            set
            {
                if (this.workTypesFilter != value)
                {
                    this.workTypesFilter = value;
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
            get { return this.weekOfYearFilter; }
            set
            {
                if (this.weekOfYearFilter != value)
                {
                    this.weekOfYearFilter = value;
                    OnPropertyChanged("WeekOfYearFilter");
                    OnPropertyChanged("ReportItems");
                }
            }
        }
        #endregion

        #region List Properties
        private readonly List<ReportExportItem> reportList;
        private ObservableCollection<ReportExportItem> reportItems;
        public ObservableCollection<ReportExportItem> ReportItems
        {
            get 
            {
                WorkTypes[] workTypes = this.workTypesFilter.Where(w => w.IsSelected == true).Select(w => w.WorkType).ToArray();
                List<ReportExportItem> list = this.reportList.Where(r => workTypes.Contains(r.WorkType) &&
                    r.WeekOfYear == this.weekOfYearFilter).ToList();
                this.reportItems = new ObservableCollection<ReportExportItem>(list);
                return this.reportItems;
            }
            set
            {
                if (this.reportItems != value)
                {
                    this.reportItems = value;
                    OnPropertyChanged("ReportItems");
                }
            }
        }
        private ICommand checkedCommand;
        public ICommand CheckedCommand
        {
            get
            {
                if (this.checkedCommand == null)
                    this.checkedCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) =>
                            {
                                OnPropertyChanged("ReportItems");
                            }
                    };
                return this.checkedCommand;
            }
        }
        
        private ReportExportItem selectedItem;
        public ReportExportItem SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                if (this.selectedItem != value)
                {
                    this.selectedItem = value;
                    OnPropertyChanged("SelectedItem");
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
                }
            }
        }
        #endregion
    }

    public class WorkTypesListCheckbox : ViewModelBase
    {
        public WorkTypes WorkType { get; set; }
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
