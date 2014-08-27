﻿using PunchIn.Models;
using PunchIn.Services;
using System;
using System.Collections.ObjectModel;
using PunchIn.Extensions;

namespace PunchIn.ViewModels
{
    public class ReportsWeeklyViewModel : ViewModelBase
    {
        private readonly PunchInService service;
        public ReportsWeeklyViewModel()
        {
            this.service = new PunchInService();
            int thisWeek = DateTime.Now.GetWeekOfYear();
            ReportItems = new ObservableCollection<ReportExportItem>(this.service.GetReportExportItems());
        }
        public string MyIcon { get { return "backlogitem";} }
        private int weekOfYear;
        public int WeekOfYear
        {
            get { return this.weekOfYear; }
            set
            {
                if (this.weekOfYear != value)
                {
                    this.weekOfYear = value;
                    OnPropertyChanged("WeekOfYear");
                }
            }
        }
        private ObservableCollection<ReportExportItem> reportItems;
        public ObservableCollection<ReportExportItem> ReportItems
        {
            get { return this.reportItems; }
            set
            {
                if (this.reportItems != value)
                {
                    this.reportItems = value;
                    OnPropertyChanged("ReportItems");
                }
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
    }
}
