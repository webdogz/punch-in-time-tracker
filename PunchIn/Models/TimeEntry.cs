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
            this.Id = id;
            this.StartDate = DateTime.Now;
        }
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public States Status { get; set; }
    }
}
