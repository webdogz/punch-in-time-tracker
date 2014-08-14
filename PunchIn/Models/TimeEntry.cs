using System;

namespace PunchIn.Models
{
    public class TimeEntry
    {
        public TimeEntry()
        {
            this.Id = Guid.NewGuid();
            this.StartDate = DateTime.Now;
        }
        public Guid Id { get; private set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public States Status { get; set; }
    }
}
