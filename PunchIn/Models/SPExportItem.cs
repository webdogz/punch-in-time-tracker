using System;

namespace PunchIn.Models
{
    /// <summary>
    /// Simple DTO for SharePoint Tracker List
    /// </summary>
    public class SPExportItem
    {
        public Guid ItemGuid { get; set; }
        public int WorkItemId { get; set; }
        public int ServiceCall { get; set; }
        public int Change { get; set; }
        public string Title { get; set; }
        public Double HoursCompleted { get; set; }
        public Double HoursRemaining { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string WorkType { get; set; }
    }
}
