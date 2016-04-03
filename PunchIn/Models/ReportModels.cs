using System;
using System.Collections.Generic;

namespace PunchIn.Models
{
    public class ReportByWeekGroup
    {
        public ReportByWeekGroup()
        {
            this.ReportItems = new List<ReportByWeekItem>();
        }
        public int WeekOfYear { get; set; }
        public string Title { get; set; }
        public double Effort { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public List<ReportByWeekItem> ReportItems { get; set; }
    }
    public class ReportByWeekItem : TimeEntry
    {
        public Guid ItemGuid { get; set; }
        public int? TfsId { get; set; }
        public int? ServiceCall { get; set; }
        public int? Change { get; set; }
        public string Title { get; set; }
        public double Effort { get; set; }
        public Status State { get; set; }
        public WorkType WorkType { get; set; }
    }
    public class ReportExportItem : ReportByWeekItem
    {
        public double HoursCompleted { get; set; }
        public double HoursRemaining { get; set; }
        public int WeekOfYear { get; set; }
        public DateTime WeekStarting { get; set; }
        public string Icon
        {
            get
            {
                return this.WorkType.ToString().ToLower();
            }
        }
    }
}
