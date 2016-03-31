using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using LiteDB;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using PunchIn.Models;

namespace UnitTestPunchIn
{
    [TestClass]
    public class TimeTrackQueriesUnitTest
    {
        private readonly ConcurrentDictionary<string, string> _dbNamesCache = new ConcurrentDictionary<string, string>();
        private readonly System.Globalization.CultureInfo cultureInfo;
        private System.Globalization.Calendar calendar;
        public TimeTrackQueriesUnitTest()
        {
            cultureInfo = new System.Globalization.CultureInfo("en-AU");
            calendar = cultureInfo.Calendar;
        }
        private int GetWeekOfYear(DateTime date)
        {
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
        [TestMethod]
        public void WeeklyReportQuery()
        {
            var now = DateTime.Now;
            var rangeStart = now.AddDays(-((now.DayOfWeek - System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7));
            var rangeEnded = rangeStart.AddDays(7);
            Predicate<TimeEntry> predicate = new Predicate<TimeEntry>(e => e.StartDate > rangeStart && e.StartDate < rangeEnded);
            using (var db = new LiteDatabase(Helpers.GetDbName()))
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
                var reps = db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().AsQueryable<WorkItem>()
                    .Where(w => w.Entries.Any(e => predicate(e)))
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
        public void ReportAllItemsGroupedByWeekFragmented()
        {
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var items = db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().AsQueryable<WorkItem>();

                var reportItems = new List<ReportByWeekItem>();
                foreach (var item in items)
                {
                    reportItems.AddRange(item.Entries.Select(e => new ReportByWeekItem
                    {
                        ItemGuid = item.Id,
                        TfsId = item.TfsId,
                        ServiceCall = item.ServiceCall,
                        Change = item.Change,
                        Title = item.Title,
                        Description = e.Description,
                        Effort = item.Effort,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        State = item.Status,
                        Status = e.Status,
                        WorkType = item.WorkType
                    }).ToList());
                }

                var reportResults = reportItems.GroupBy(r => new { WeekOfYear = GetWeekOfYear(r.StartDate), Title = r.Title}).Select(g => new ReportByWeekGroup
                {
                    WeekOfYear = g.Key.WeekOfYear,
                    Title = g.Key.Title,
                    MinDate = g.Min(e => e.StartDate),
                    MaxDate = g.Max(e => e.EndDate ?? DateTime.Now),
                    ReportItems = g.Select(e => e).ToList()

                });
                foreach (var ritem in reportResults.OrderBy(r => r.WeekOfYear).ThenBy(r => r.Title))
                {
                    Debug.WriteLine("===========");
                    Debug.WriteLine("Week:{0}\t{1}", ritem.WeekOfYear, ritem.Title);
                    Debug.WriteLine("\tRange: {0} - {1}", ritem.MinDate, ritem.MaxDate);
                    Debug.WriteLine("\tCount: {0}", ritem.ReportItems.Count);
                    foreach (var time in ritem.ReportItems.OrderBy(t => t.StartDate))
                    {
                        Debug.WriteLine("\t\t{0}\t{1}\tHours:{2}\tDesc:{3}:{4}",
                            time.StartDate.ToString("ddd, MM/dd"),
                            (time.EndDate ?? DateTime.Now).ToString("ddd, MM/dd"),
                            ((time.EndDate ?? DateTime.Now) - time.StartDate).TotalHours,
                            time.TfsId,
                            time.Description);
                    }
                }
            }
        }

        [TestMethod]
        public void ReportAllItemsGroupedByWeek()
        {
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var reportItems = db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().AsQueryable<WorkItem>()
                    .SelectMany(item => item.Entries.Select(e => new ReportByWeekItem
                    {
                        ItemGuid = item.Id,
                        TfsId = item.TfsId,
                        ServiceCall = item.ServiceCall,
                        Change = item.Change,
                        Title = item.Title,
                        Description = e.Description,
                        Effort = item.Effort,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        State = item.Status,
                        Status = e.Status,
                        WorkType = item.WorkType
                    })).GroupBy(r => new
                    {
                        WeekOfYear = GetWeekOfYear(r.StartDate),
                        Title = r.Title,
                        Effort = r.Effort
                    }).Select(g => new ReportByWeekGroup
                    {
                        WeekOfYear = g.Key.WeekOfYear,
                        Title = g.Key.Title,
                        Effort = g.Key.Effort,
                        MinDate = g.Min(e => e.StartDate),
                        MaxDate = g.Max(e => e.EndDate ?? DateTime.Now),
                        ReportItems = g.Select(e => e).ToList()
                    }).OrderBy(r => r.WeekOfYear).ThenBy(r => r.Title);
                var exportItems = new List<ReportExportItem>();
                foreach (var item in reportItems)
                {
                    Debug.WriteLine("===========");
                    Debug.WriteLine("Week:{0}\t{1}", item.WeekOfYear, item.Title);
                    Debug.WriteLine("\tRange: {0} - {1}", item.MinDate, item.MaxDate);
                    Debug.WriteLine("\tCount: {0}", item.ReportItems.Count);
                    double effort = item.Effort * 8;
                    foreach (var time in item.ReportItems.OrderBy(t => t.StartDate))
                    {
                        TimeSpan completed = ((time.EndDate ?? DateTime.Now) - time.StartDate);
                        double hoursRemain = effort -= completed.TotalHours;
                        exportItems.Add(new ReportExportItem
                        {
                            ItemGuid = time.ItemGuid,
                            TfsId = time.TfsId,
                            ServiceCall = time.ServiceCall,
                            Change = time.Change,
                            Title = item.Title,
                            Description = time.Description,
                            HoursCompleted = completed.TotalHours,
                            HoursRemaining = (hoursRemain > 0 ? hoursRemain : 0),
                            StartDate = time.StartDate,
                            EndDate = time.EndDate,
                            State = time.State,
                            Status = time.Status,
                            WorkType = time.WorkType,
                            WeekOfYear = item.WeekOfYear
                        });

                        Debug.WriteLine("\t\t{0}\t{1}\tHours:{2}\tDesc:{3}:{4}",
                            time.StartDate.ToString("ddd, MM/dd"),
                            (time.EndDate ?? DateTime.Now).ToString("ddd, MM/dd"),
                            completed.TotalHours,
                            time.TfsId,
                            time.Description);
                    }
                }
                Debug.WriteLine("********************");
                Debug.WriteLine("*** Export Items ***");
                Debug.WriteLine("********************");
                foreach (var exportItem in exportItems)
                {
                    Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                        exportItem.TfsId, exportItem.ServiceCall, exportItem.Change,
                        exportItem.Title, exportItem.HoursCompleted, exportItem.HoursRemaining,
                        exportItem.State, exportItem.Status, exportItem.WorkType, exportItem.WeekOfYear);
                }
            }
        }

        [TestMethod]
        public void ReportAllItemsGroupedByMonth()
        {
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var items = db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().AsQueryable<WorkItem>();

                var reportItems = new List<ReportItem>();
                foreach (var item in items)
                {
                    reportItems.AddRange(item.Entries.Select(e => new ReportItem
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
                var reportResults = reportItems.GroupBy(r => new { r.StartDate.Month, r.StartDate.Year }).Select(g => new ReportByMonthGroup
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    MinDate = g.Min(e => e.StartDate),
                    MaxDate = g.Max(e => e.EndDate ?? DateTime.Now),
                    ReportItems = g.Select(e => e).ToList()

                });
                foreach (var ritem in reportResults)
                {
                    Debug.WriteLine("===========");
                    Debug.WriteLine("WorkItem.Group {0}/{1}", ritem.Year, ritem.Month);
                    Debug.WriteLine("\tRange: {0} - {1}", ritem.MinDate, ritem.MaxDate);
                    Debug.WriteLine("\tCount: {0}", ritem.ReportItems.Count);
                }
            }
        }

        public class ReportByMonthGroup
        {
            public ReportByMonthGroup()
            {
                this.ReportItems = new List<ReportItem>();
            }
            public int Month { get; set; }
            public int Year { get; set; }
            public DateTime MinDate { get; set; }
            public DateTime MaxDate { get; set; }
            public List<ReportItem> ReportItems { get; set; }
        }

        [TestMethod]
        public void GroupByMonthAndYear()
        {
            var dayRange = Helpers.MaxWorkItemDaysRange / 2;
            var today = DateTime.Now;
            var endRange = today.AddDays(dayRange);
            var startRange = today.AddDays(-dayRange);
            Predicate<TimeEntry> predicate = new Predicate<TimeEntry>(e => e.StartDate > startRange && e.StartDate < endRange);
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                var items = col.FindAll().AsQueryable().Where(w => w.Entries.Any(e => predicate(e)));

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
            var dayRange = Helpers.MaxWorkItemDaysRange / 2;
            var today = DateTime.Now;
            var endRange = today.AddDays(dayRange);
            var startRange = today.AddDays(-dayRange);
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var items = from wi in db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().AsQueryable<WorkItem>()
                            where wi.Entries.Any(e => e.StartDate > startRange && e.StartDate < endRange)
                            select wi;
                Debug.WriteLine("Entries between {0} and {1}", startRange.ToString(), endRange.ToString());
                foreach(var item in items)
                {
                    Debug.WriteLine("==========");
                    Debug.WriteLine("Item.Id = {0}", item.Id);
                    Debug.WriteLine(
                        "TFS:{0}, ServiceCall:{1}\nTitle:{2}",
                        item.TfsId, item.ServiceCall, item.Title);
                    TimeSpan span = new TimeSpan();
                    foreach (var entry in item.Entries.Where(e => e.StartDate > startRange && e.StartDate < endRange))
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
    }
}
