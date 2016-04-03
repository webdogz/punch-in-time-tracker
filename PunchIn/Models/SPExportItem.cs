using System;

namespace PunchIn.Models
{
    /// <summary>
    /// Simple DTO for SharePoint Tracker List
    /// </summary>
    public class SPExportItem
    {
        public Guid TimeEntryGuid { get; set; }
        public int TfsId { get; set; }
        public int ServiceCall { get; set; }
        public int Change { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double HoursCompleted { get; set; }
        public double HoursRemaining { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string WorkType { get; set; }
        public DateTime WeekStarting { get; set; }
        public int WeekOfYear { get; set; }
    }
}
