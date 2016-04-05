using LiteDB;
using PunchIn.Models;

namespace PunchIn.Data
{
    public class PunchDatabase : LiteDatabase
    {
        public PunchDatabase() : base(GlobalConfig.ConnectionString) { }
        public LiteCollection<WorkItem> WorkItems { get { return GetCollection<WorkItem>(CollectionNames.WorkItems); } }
        public LiteCollection<TimeEntry> Times { get { return GetCollection<TimeEntry>(CollectionNames.TimeEntries); } }

        protected override void OnModelCreating(BsonMapper mapper)
        {
            mapper.Entity<WorkItem>()
                .DbRef(x => x.Entries, CollectionNames.TimeEntries);
        }

        protected override void OnVersionUpdate(int newVersion)
        {
            switch (newVersion)
            {
                case 1:
                    System.Diagnostics.Debug.WriteLine("New Version = {0}, Filename = ", newVersion, GlobalConfig.DatabaseLocation);
                    break;
            }
        }

        #region Helpers
        class CollectionNames
        {
            public const string WorkItems = "workitems";
            public const string TimeEntries = "times";
        }
        #endregion
    }
}
