using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using NDatabase;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using EmitMapper;
using PunchIn.Models;

namespace UnitTestPunchIn
{
    [TestClass]
    public class TimeTrackQueriesUnitTest
    {
        private readonly ConcurrentDictionary<string, string> _dbNamesCache = new ConcurrentDictionary<string, string>();
        
        [TestMethod]
        public void WeeklyReportQuery()
        {
            var now = DateTime.Now;
            var rangeStart = now.AddDays(-((now.DayOfWeek - System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7));
            var rangeEnded = rangeStart.AddDays(7);
            Predicate<TimeEntry> predicate = new Predicate<TimeEntry>(e => e.StartDate > rangeStart && e.StartDate < rangeEnded);
            using (var db = OdbFactory.Open(GetDbName()))
            {
                //var reps = db.AsQueryable<WorkItem>().Where(w => w.Entries.Any(e => predicate(e)))
                //    .Select(w => new WorkItem(w.Id)
                //    {
                //        TfsId = w.TfsId,
                //        ServiceCall = w.ServiceCall,
                //        Change = w.Change,
                //        Effort = w.Effort,
                //        Status = w.Status,
                //        Title = w.Title,
                //        WorkType = w.WorkType,
                //        Entries = w.Entries.Where(e => predicate(e)).ToList()
                //    });
                var reps = db.AsQueryable<WorkItem>().Where(w => w.Entries.Any(e => predicate(e)))
                    .Select(w => new ReportWorkItem(w.Id)
                    {
                        TfsId = w.TfsId,
                        ServiceCall = w.ServiceCall,
                        Change = w.Change,
                        Effort = w.Effort,
                        Status = w.Status,
                        Title = w.Title,
                        WorkType = w.WorkType,
                        Entries = w.Entries.Where(e => predicate(e)).ToList(),
                        MinDate = w.Entries.Min(e => e.StartDate),
                        MaxDate = w.Entries.Max(e => e.EndDate ?? now),
                        Count = w.Entries.Count
                    });
                foreach (var rep in reps)
                {
                    Debug.WriteLine("===========");
                    Debug.WriteLine("WorkItem.Id {0}", rep.Id);
                    Debug.WriteLine("\tRange: {0} - {1}", rep.MinDate, rep.MaxDate);
                    Debug.WriteLine("\tCount: {0}", rep.Count);
                    Debug.WriteLine("\tEntryCount: {0}", rep.Entries.Count);
                }
            }
        }
        public class ReportWorkItem : WorkItem
        {
            public ReportWorkItem(Guid id) : base(id) { }
            public DateTime MinDate { get; set; }
            public DateTime MaxDate { get; set; }
            public int Count { get; set; }
        }

        [TestMethod]
        public void GroupByMonthAndYear()
        {
            var today = DateTime.Now;
            var thisMonth = new DateTime(today.Year, today.Month, 6);
            var lastMonth = thisMonth.AddDays(-2);//.AddMonths(-1);
            Predicate<TimeEntry> predicate = new Predicate<TimeEntry>(e => e.StartDate > lastMonth && e.StartDate < thisMonth);
            using (var db = OdbFactory.Open(GetDbName()))
            {
                var items = db.AsQueryable<WorkItem>().Where(w => w.Entries.Any(e => predicate(e)));

                var reportItems = new List<ReportItem>();
                foreach(var item in items)
                {
                    reportItems.AddRange(item.Entries.Where(entry => predicate(entry)).Select(e => new ReportItem
                    {
                        WorkItemId = item.Id,
                        Title = item.Title,
                        Description = e.Description,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        State = item.Status,
                        Status = e.Status,
                        WorkType = item.WorkType
                    }).ToList());
                }
                //var reportResults = from r in reportItems
                //                    group r by r.Id into g
                //                    select new ReportTime
                //                    {
                //                        Id = g.Key,
                //                        MinDate = g.Min(e => e.StartDate),
                //                        MaxDate = g.Max(e => e.EndDate ?? e.StartDate),
                //                        ReportItems = g.Select(e => e).ToList()
                //                    };
                var reportResults = reportItems.GroupBy(r => r.Id).Select(g => new ReportTime
                    {
                        Id = g.Key,
                        MinDate = g.Min(e => e.StartDate),
                        MaxDate = g.Max(e => e.EndDate ?? DateTime.Now),
                        ReportItems = g.Select(e => e).ToList()

                    });
                foreach(var ritem in reportResults)
                {
                    Debug.WriteLine("===========");
                    Debug.WriteLine("WorkItem.Id {0}", ritem.Id);
                    Debug.WriteLine("\tRange: {0} - {1}", ritem.MinDate, ritem.MaxDate);
                    Debug.WriteLine("\tCount: {0}", ritem.ReportItems.Count);
                }
                
                //var items = from wi in db.AsQueryable<WorkItem>()
                //            group wi by wi.Entries.Select(e => e.StartDate) into g
                //            select new { Date = g.Key };

            }
        }
        
        public class ReportItem : TimeEntry
        {
            public Guid WorkItemId { get; set; }
            public string Title { get; set; }
            public WorkTypes WorkType { get; set; }
            public States State { get; set; }
        }
        public class ReportTime
        {
            public ReportTime()
            {
                this.ReportItems = new List<ReportItem>();
            }
            public Guid Id { get; set; }
            public DateTime MinDate { get; set; }
            public DateTime MaxDate { get; set; }
            public List<ReportItem> ReportItems { get; set; }
        }
        [TestMethod]
        public void QueryWhereTimeEntriesAreInCurrentMonthOnly()
        {
            var today = DateTime.Now;
            var thisMonth = new DateTime(today.Year, today.Month, 6);
            var lastMonth = thisMonth.AddDays(-2);//.AddMonths(-1);
            using (var db = OdbFactory.Open(GetDbName()))
            {
                var items = from wi in db.AsQueryable<WorkItem>()
                            where wi.Entries.Any(e => e.StartDate > lastMonth && e.StartDate < thisMonth)
                            select wi;
                Debug.WriteLine("Entries between {0} and {1}", lastMonth, thisMonth);
                foreach(var item in items)
                {
                    Debug.WriteLine("==========");
                    Debug.WriteLine("Item.Id = {0}", item.Id);
                    Debug.WriteLine(
                        "TFS:{0}, ServiceCall:{1}\nTitle:{2}",
                        item.TfsId, item.ServiceCall, item.Title);
                    TimeSpan span = new TimeSpan();
                    foreach (var entry in item.Entries.Where(e => e.StartDate > lastMonth && e.StartDate < thisMonth))
                    {
                        span = PrintTimeEntryAdnReturnSpan(entry, span);
                    }
                    Debug.WriteLine("\tHours Spent: {0}", span.TotalHours);
                }
                Assert.IsTrue(items.Count() > 0);
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

        #region Helpers
        private string GetDbName()
        {
            return _dbNamesCache.GetOrAdd(Environment.UserName, ProduceDbName);
        }

        private static string ProduceDbName(string login)
        {
            var dbName = string.Format("{0}_punchin.ndb", login);

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Time");

            return Path.Combine(path, dbName);
        }
        #endregion
    }
}
