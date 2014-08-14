using System;
using System.Collections.Generic;
namespace PunchIn.Models
{
    public class WorkItem
    {
        public WorkItem()
        {
            this.Id = Guid.NewGuid();
            this.Status = States.Analysis;
            this.WorkType = WorkTypes.Task;
            this.Effort = 2.0;
            this.Entries = new List<TimeEntry>();
        }
        public WorkItem(Guid id)
        {
            this.Id = id;
        }
        public Guid Id { get; private set; }
        public Nullable<int> TfsId { get; set; }
        public Nullable<int> ServiceCall { get; set; }
        public Nullable<int> Change { get; set; }
        public WorkTypes WorkType { get; set; }
        public string Title { get; set; }
        public double Effort { get; set; }
        public States Status { get; set; }
        public List<TimeEntry> Entries { get; set; }
    }
}
