using System;

namespace PunchIn.Models
{
    public class TimeEntry
    {
        public TimeEntry() : this(Guid.NewGuid()) { }
        public TimeEntry(Guid id)
        {
            if (id.Equals(Guid.Empty))
                id = Guid.NewGuid();
            Id = id;
            StartDate = DateTime.Now;
        }
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Status Status { get; set; }
    }
}
