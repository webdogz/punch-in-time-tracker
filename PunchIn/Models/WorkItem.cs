using System;
using System.Collections.Generic;
namespace PunchIn.Models
{
    public class WorkItem
    {
        public WorkItem()
        {
            Id = Guid.NewGuid();
            Status = Status.Analysis;
            WorkType = WorkType.Task;
            Effort = 2.0;
            Entries = new List<TimeEntry>();
        }
        public WorkItem(Guid id)
        {
            if (id.Equals(Guid.Empty))
                id = Guid.NewGuid();
            Id = id;
        }
        public Guid Id { get; set; }
        public int? TfsId { get; set; }
        public int? ServiceCall { get; set; }
        public int? Change { get; set; }
        public WorkType WorkType { get; set; }
        public string Title { get; set; }
        public double Effort { get; set; }
        public Status Status { get; set; }
        public List<TimeEntry> Entries { get; set; }
    }
}
