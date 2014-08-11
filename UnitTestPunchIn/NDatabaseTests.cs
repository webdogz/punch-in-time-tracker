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
    public class NDatabaseTests
    {
        private readonly ConcurrentDictionary<string, string> _dbNamesCache = new ConcurrentDictionary<string, string>();
        [TestMethod]
        public void CreateWorkItemAndTimeEntries()
        {
            string dbPath = GetDbName();
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            using (var db = OdbFactory.Open(GetDbName()))
            {
                var rnd = new Random();
                for(int i = 0; i < 5; i++)
                {
                    WorkItem item = new WorkItem()
                    {
                        TfsId = i + 1,
                        ServiceCall = (i + 1) * 100,
                        Effort = 2,
                        Title = string.Format("Work item #{0}", (i + 1)),
                        WorkType = WorkTypes.BacklogItem,
                        Status = States.Analysis
                    };
                    foreach (var name in new string[] { "First entry", "Second entry", "Third entry" })
                    {
                        DateTime startDate = DateTime.Now.AddHours(rnd.NextDouble() * 8);
                        TimeEntry entry = new TimeEntry()
                        {
                            Description = name,
                            Status = States.InProgress,
                            StartDate = startDate,
                            EndDate = startDate.AddHours(rnd.NextDouble() * 4)
                        };
                        item.Entries.Add(entry);
                        db.Store<WorkItem>(item);
                    }
                }
                int cnt = db.QueryAndExecute<WorkItem>().Count;
                Assert.AreEqual(5, cnt);
            }
        }

        [TestMethod]
        public void QueryWorkItemAndTimeEntries()
        {
            List<WorkItem> items;
            using (var db = OdbFactory.Open(GetDbName()))
            {
                items = db.QueryAndExecute<WorkItem>().ToList();
                Assert.AreEqual(5, items.Count());
                WorkItem item = items.FirstOrDefault();
                Assert.AreEqual("Work item #1", item.Title);
                Assert.AreEqual(3, item.Entries.Count);
            }
        }

        [TestMethod]
        public void ModifyWorkItemAndTimeEntriesAndSave()
        {
            List<WorkItem> items;
            using (var db = OdbFactory.Open(GetDbName()))
            {
                items = db.QueryAndExecute<WorkItem>().ToList();
                PrintWorkItems(items);
                WorkItem item2 = items.FirstOrDefault(w => w.TfsId == 2);
                Assert.AreEqual(200, item2.ServiceCall);
                item2.WorkType = WorkTypes.Datafix;
                db.Store<WorkItem>(item2);
            }
            using(var db = OdbFactory.Open(GetDbName()))
            {
                items = db.QueryAndExecute<WorkItem>()
                    .Where(w => w.TfsId == 2).ToList();
                Assert.IsTrue(items.Count == 1);
                WorkItem wit = items.First();
                Assert.AreEqual(WorkTypes.Datafix, wit.WorkType);
            }
        }

        [TestMethod]
        public void CanAddWorkItemWithNoEntries()
        {
            
            int count, afterCount;
            using(var db = OdbFactory.Open(GetDbName()))
            {
                count = db.QueryAndExecute<WorkItem>().Count();
            }
            using (var db = OdbFactory.Open(GetDbName()))
            {
                db.Store<WorkItem>(new WorkItem()
                {
                    TfsId = 1000,
                    ServiceCall = 100000,
                    Title = "TEST: Can Add WorkItem With No Entries",
                    WorkType = WorkTypes.Bug
                });
                afterCount = db.QueryAndExecute<WorkItem>().Count;
            }
            Assert.AreNotEqual(count, afterCount);
        }

        [TestMethod]
        public void CanAddEntryWithNoEndDateDirect()
        {
            WorkItem item = null;
            int beforeCount, afterCount;
            using(var db = OdbFactory.Open(GetDbName()))
            {
                item = db.QueryAndExecute<WorkItem>().FirstOrDefault();
                beforeCount = item.Entries.Count;
            }
            item.Entries.Add(new TimeEntry
                {
                    Description = "New Entry for first item",
                    StartDate = DateTime.Now,
                    Status = States.InProgress
                });
            using (var db = OdbFactory.Open(GetDbName()))
            {
                var mapper = ObjectMapperManager.DefaultInstance.GetMapper<WorkItem, WorkItem>();
                WorkItem wi = mapper.Map(item, db.QueryAndExecute<WorkItem>().FirstOrDefault(w => w.Id == item.Id));
                db.Store<WorkItem>(wi);
                item = db.QueryAndExecute<WorkItem>().FirstOrDefault();
                afterCount = item.Entries.Count;
            }
            Assert.AreNotEqual(beforeCount, afterCount);
        }

        [TestMethod]
        public void QueryByOidAndAddEntryWithNoEndDate()
        {
            NDatabase.Api.OID oid;
            WorkItem item = null;
            int beforeCount, afterCount;
            using (var db = OdbFactory.Open(GetDbName()))
            {
                item = db.QueryAndExecute<WorkItem>().FirstOrDefault();
                oid = db.GetObjectId(item);
                beforeCount = item.Entries.Count;
            }
            item.Entries.Add(new TimeEntry
            {
                Description = "New Entry for first item",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1.25),
                Status = States.Analysis
            });
            using (var db = OdbFactory.Open(GetDbName()))
            {
                var mapper = ObjectMapperManager.DefaultInstance.GetMapper<WorkItem, WorkItem>();
                WorkItem wi = mapper.Map(item, db.QueryAndExecute<WorkItem>().FirstOrDefault(w => w.Id == item.Id));
                db.Store<WorkItem>(wi);
            }
            using(var db = OdbFactory.Open(GetDbName()))
            {
                item = (WorkItem)db.GetObjectFromId(oid);
                //item = db.QueryAndExecute<WorkItem>().FirstOrDefault();
                afterCount = item.Entries.Count;
            }
            Assert.AreNotEqual(beforeCount, afterCount);
        }

        [TestMethod]
        public void QueryUsingLinq()
        {
            using (var db = OdbFactory.Open(GetDbName()))
            {
                var items = from wi in db.AsQueryable<WorkItem>()
                            where wi.WorkType.Equals(WorkTypes.Datafix)
                            select wi;
                var item = items.First();
                Assert.IsTrue(item.TfsId == 2);
                Assert.IsTrue(items.Count() == 1);

            }
        }
        
        [TestMethod]
        public void QueryByTfsIdUsingLinq()
        {
            using (var db = OdbFactory.Open(GetDbName()))
            {
                var items = from wi in db.AsQueryable<WorkItem>()
                            where wi.TfsId.Value == 2
                            select wi;
                var item = items.First();
                Assert.IsTrue(item.WorkType.Equals(WorkTypes.Datafix));
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
