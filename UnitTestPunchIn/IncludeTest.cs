using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PunchIn.Models;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using System.Diagnostics;

namespace UnitTestPunchIn
{
    public class PunchDatabase : LiteDatabase
    {
        public PunchDatabase() : base(Helpers.GetDbName()) { }
        public LiteCollection<WorkItem> WorkItems { get { return GetCollection<WorkItem>(CollectionNames.WorkItems); } }
        public LiteCollection<TimeEntry> Times { get { return GetCollection<TimeEntry>(CollectionNames.TimeEntries); } }

        protected override void OnModelCreating(BsonMapper mapper)
        {
            mapper.Entity<WorkItem>()
                .DbRef(x => x.Entries, CollectionNames.TimeEntries);
        }

        #region Helpers
        class CollectionNames
        {
            public const string WorkItems = "workitems";
            public const string TimeEntries = "times";
        }
        #endregion
    }
    [TestClass]
    public class IncludeTest
    {
        [TestMethod]
        public void CreateWorkItemAndTimeEntries_Save_ThenQuery()
        {
            Guid wiid = Guid.NewGuid();
            Guid teid = Guid.NewGuid();
            List<WorkItem> items;
            WorkItem item = new WorkItem()
            {
                Title = "CreateWorkItemAndTimeEntries_Save_ThenQuery",
                Id = wiid
            };
            TimeEntry entry = new TimeEntry()
            {
                Id = teid,
                Description = "1",
                StartDate = new DateTime(2000, 1, 1),
                EndDate = new DateTime(2000, 1, 2)
            };
            item.Entries.Add(entry);
            using(var db = new PunchDatabase())
            {
                var ecol = db.Times;
                ecol.Insert(entry);
                var col = db.WorkItems;//.Include(e => e.Entries);
                col.Insert(item);
            }
            using (var db = new PunchDatabase())
            {
                var col = db.WorkItems.Include(e => e.Entries);
                items = col.FindAll().ToList();
                PrintWorkItems(items);
                WorkItem item2 = col.FindById(new BsonValue(wiid));
                Assert.AreEqual(1, item2.Entries.Count);
                Assert.AreEqual(new DateTime(2000, 1, 1), item2.Entries.First().StartDate);
            }
        }

        private void PrintWorkItems(List<WorkItem> items)
        {
            foreach (var item in items)
            {
                Debug.WriteLine("==========");
                Debug.WriteLine("Item.Id = {0}", item.Id);
                Debug.WriteLine(
                    "TFS:{0}, ServiceCall:{1}\nTitle:{2}",
                    item.TfsId, item.ServiceCall, item.Title);
                TimeSpan span = new TimeSpan();
                foreach (var entry in item.Entries)
                {
                    span = PrintTimeEntryAdnReturnSpan(entry, span);
                }
                Debug.WriteLine("\tHours Spent: {0}", span.TotalHours);
            }
        }
        private TimeSpan PrintTimeEntryAdnReturnSpan(TimeEntry entry, TimeSpan span)
        {
            Debug.WriteLine("\t{0}\n\tStart:{1}\n\tEnd:{2}",
                        entry.Description, entry.StartDate, entry.EndDate);
            DateTime end = entry.EndDate ?? entry.StartDate;
            return span.Add(end - entry.StartDate);
        }
    }
}
