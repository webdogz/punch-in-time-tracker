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
    public class NDatabaseTests
    {
        [TestMethod]
        public void CreateWorkItemAndTimeEntries()
        {
            DateTime StartPointDate = DateTime.Now.AddDays(-Helpers.MaxWorkItemDaysRange); // starting point
            string dbPath = Helpers.GetDbName();
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                var rnd = new Random();
                for (int i = 0; i < Helpers.MaxWorkItemsToCreate; i++)
                {
                    WorkItem item = new WorkItem()
                    {
                        TfsId = i + 1,
                        ServiceCall = (i + 1) * 100,
                        Effort = rnd.NextDouble() * 3,
                        Title = string.Format("Work item #{0}", (i + 1)),
                        WorkType = WorkType.BacklogItem,
                        Status = Status.Analysis
                    };
                    foreach (var name in new string[] { "First entry", "Second entry", "Third entry" })
                    {
                        DateTime startDate = StartPointDate
                            .AddDays(rnd.NextDouble() * Helpers.MaxWorkItemDaysRange)
                            .AddHours(rnd.NextDouble() * Helpers.MaxWorkItemHoursPerDayRange);
                        TimeEntry entry = new TimeEntry()
                        {
                            Description = name,
                            Status = Status.InProgress,
                            StartDate = startDate,
                            EndDate = startDate.AddHours(rnd.NextDouble() * 4)
                        };
                        item.Entries.Add(entry);
                    }
                    col.Insert(item);
                }
                
                int cnt = col.Count();
                Assert.AreEqual(Helpers.MaxWorkItemsToCreate, cnt);
            }
        }

        [TestMethod]
        public void CanMapWorkItemToWorkItemWithoutDuplication()
        {
            WorkItem item = null;
            int beforeCount, afterCount;
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                item = db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().FirstOrDefault();
                beforeCount = item.Entries.Count;
            }
            item.Entries.Add(new TimeEntry
            {
                Description = "New Entry for first item",
                StartDate = DateTime.Now,
                Status = Status.InProgress
            });
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                WorkItem wi = col.FindOne(w => w.Id == item.Id);

                Assert.AreEqual(wi.Id, item.Id);
                
                col.Update(item);

                var first = col.FindAll().FirstOrDefault();
                var items = new List<WorkItem>();
                items.Add(item);
                items.Add(wi);
                items.Add(first);
                PrintWorkItems(items);
                afterCount = first.Entries.Count;
            }
            Assert.AreNotEqual(beforeCount, afterCount);
        }

        [TestMethod]
        public void QueryWorkItemAndTimeEntries()
        {
            List<WorkItem> items;
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                items = db.GetCollection<WorkItem>(CollectionNames.WorkItems).FindAll().ToList();
                Assert.AreEqual(Helpers.MaxWorkItemsToCreate, items.Count());
                WorkItem item = items.FirstOrDefault();
                Assert.IsNotNull(item);
                Assert.IsInstanceOfType(item, typeof(WorkItem));
            }
        }

        [TestMethod]
        public void ModifyWorkItemAndTimeEntriesAndSave()
        {
            List<WorkItem> items;
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                items = col.FindAll().ToList();
                PrintWorkItems(items);
                WorkItem item2 = items.FirstOrDefault(w => w.TfsId == 2);
                Assert.AreEqual(200, item2.ServiceCall);
                item2.WorkType = WorkType.Datafix;
                col.Update(item2);
            }
            using(var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                items = col.FindAll()
                    .Where(w => w.TfsId == 2).ToList();
                Assert.IsTrue(items.Count == 1);
                WorkItem wit = items.First();
                Assert.AreEqual(WorkType.Datafix, wit.WorkType);
            }
        }

        [TestMethod]
        public void CanAddWorkItemWithNoEntries()
        {
            
            int count, afterCount;
            using(var db = new LiteDatabase(Helpers.GetDbName()))
            {
                count = db.GetCollection<WorkItem>(CollectionNames.WorkItems).Count();
            }
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                col.Insert(new WorkItem()
                {
                    TfsId = 1000,
                    ServiceCall = 100000,
                    Title = "TEST: Can Add WorkItem With No Entries",
                    WorkType = WorkType.Bug
                });
                afterCount = col.Count();
            }
            Assert.AreNotEqual(count, afterCount);
        }

        [TestMethod]
        public void CanAddEntryWithNoEndDateDirect()
        {
            WorkItem item = null;
            int beforeCount, afterCount;
            using(var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                item = col.FindAll().FirstOrDefault();
                beforeCount = item.Entries.Count;
            }
            item.Entries.Add(new TimeEntry
                {
                    Description = "New Entry for first item",
                    StartDate = DateTime.Now,
                    Status = Status.InProgress
                });
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                col.Update(item);
            }
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                var wi = col.FindAll().FirstOrDefault();
                afterCount = wi.Entries.Count;
            }
            Assert.AreNotEqual(beforeCount, afterCount);
        }

        [TestMethod]
        public void QueryByOidAndAddEntryWithNoEndDate()
        {
            BsonValue oid;
            WorkItem item = null;
            int beforeCount, afterCount;
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                item = col.FindAll().FirstOrDefault();
                oid = new BsonValue(item.Id);
                beforeCount = item.Entries.Count;
            }
            item.Entries.Add(new TimeEntry
            {
                Description = "New Entry for first item",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1.25),
                Status = Status.Analysis
            });
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                col.Update(item);
            }
            using(var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                item = col.FindById(oid);
                afterCount = item.Entries.Count;
            }
            Assert.AreNotEqual(beforeCount, afterCount);
        }

        [TestMethod]
        public void QueryUsingLinq()
        {
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                var items = from wi in col.FindAll().AsQueryable()
                            where wi.WorkType.Equals(WorkType.Datafix)
                            select wi;
                var item = items.First();
                Assert.IsTrue(item.TfsId == 2);
                Assert.IsTrue(items.Count() == 1);

            }
        }
        
        [TestMethod]
        public void QueryByTfsIdUsingLinq()
        {
            using (var db = new LiteDatabase(Helpers.GetDbName()))
            {
                var col = db.GetCollection<WorkItem>(CollectionNames.WorkItems);
                var items = from wi in col.FindAll().AsQueryable()
                            where wi.TfsId.Value == 2
                            select wi;
                var item = items.First();
                Assert.IsTrue(item.WorkType.Equals(WorkType.Datafix));
                Assert.IsTrue(items.Count() == 1);

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
