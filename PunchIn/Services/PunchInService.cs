using NDatabase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PunchIn.Models;
using EmitMapper;
using EmitMapper.MappingConfiguration;

namespace PunchIn.Services
{
    internal class PunchInService
    {
        private readonly ConcurrentDictionary<string, string> _dbNamesCache = new ConcurrentDictionary<string, string>();
        private readonly System.Globalization.CultureInfo _cultureInfo;
        private System.Globalization.Calendar _calendar;
        public PunchInService()
        {
            this._cultureInfo = new System.Globalization.CultureInfo("en-AU");
            this._calendar = this._cultureInfo.Calendar;
        }
        public int GetWeekOfYear(DateTime date)
        {
            return this._calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
        
        /// <summary>
        /// Retrieve all WorkItems from the database
        /// </summary>
        /// <returns>List of all WorkItems</returns>
        public List<WorkItem> GetWorkItems()
        {
            using (var db = OdbFactory.Open(this.DbName))
            {
                return db.QueryAndExecute<WorkItem>().ToList();
            }
        }
        
        /// <summary>
        /// Retrieve WorkItem from the database by its Id
        /// </summary>
        /// <param name="id">Guid ID of the WorkItem</param>
        /// <returns>The WorkItem</returns>
        public WorkItem GetItemById(Guid id)
        {
            using (var db = OdbFactory.Open(this.DbName))
            {
                return GetItemById(id, db);
            }
        }
        private WorkItem GetItemById(Guid id, NDatabase.Api.IOdb db)
        {
            return db.QueryAndExecute<WorkItem>().FirstOrDefault(w => w.Id == id);
        }

        /// <summary>
        /// Update or Insert the WorkItem to the database
        /// </summary>
        /// <param name="workItem">WorkItem to be saved</param>
        public void SaveWorkItem(WorkItem workItem)
        {
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<WorkItem, WorkItem>(
                    new DefaultMapConfig().ConstructBy<WorkItem>(() => new WorkItem(workItem.Id))
                );
            using (var db = OdbFactory.Open(this.DbName))
            {
                WorkItem wi = mapper.Map(workItem, GetItemById(workItem.Id, db));
                db.Store<WorkItem>(wi);
            }
        }

        public void DeleteWorkItem(Guid workItemId)
        {
            using (var db = OdbFactory.Open(this.DbName))
            {
                WorkItem item = GetItemById(workItemId, db);
                db.Delete<WorkItem>(item);
            }
        }
        #region Reporting
        public IOrderedQueryable<ReportByWeekGroup> GetItemsGroupedByWeek(NDatabase.Api.IOdb db)
        {
            var reportItems = db.AsQueryable<WorkItem>().SelectMany(item => item.Entries.Select(e => new ReportByWeekItem
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
            return reportItems;
        }
        /// <summary>
        /// Gets a list of all <see cref="ReportExportItem"/>
        /// </summary>
        /// <returns>List of <see cref="ReportExportItem"/></returns>
        public List<ReportExportItem> GetReportExportItems()
        {
            return GetReportExportItems(new Predicate<ReportByWeekGroup>(g => true));
        }
        /// <summary>
        /// Gets a list of <see cref="ReportExportItem"/> for a given WeekOfYear
        /// </summary>
        /// <param name="weekOfYear">Week of year</param>
        /// <returns>List of <see cref="ReportExportItem"/></returns>
        public List<ReportExportItem> GetReportExportItems(int weekOfYear)
        {
            return GetReportExportItems(new Predicate<ReportByWeekGroup>(g => g.WeekOfYear == weekOfYear));
        }
        /// <summary>
        /// Gets a list of <see cref="ReportExportItem"/> within a week of year range
        /// </summary>
        /// <param name="fromWeekOfYear">Beginning of range</param>
        /// <param name="toWeekOfYear">End of range</param>
        /// <returns>List of <see cref="ReportExportItem"/></returns>
        public List<ReportExportItem> GetReportExportItems(int fromWeekOfYear, int toWeekOfYear)
        {
            if (fromWeekOfYear > toWeekOfYear) throw new ArgumentOutOfRangeException();
            return GetReportExportItems(new Predicate<ReportByWeekGroup>(g => g.WeekOfYear >= fromWeekOfYear && g.WeekOfYear <= toWeekOfYear));
        }
        /// <summary>
        /// Gets a list of <see cref="ReportExportItem"/> filtered by <see cref="Predicate<WorkByWeekGroup>"/>
        /// </summary>
        /// <param name="predicate"><see cref="WorkByWeekGroup"/> predicate</param>
        /// <returns>List of <see cref="ReportExportItem"/></returns>
        public List<ReportExportItem> GetReportExportItems(Predicate<ReportByWeekGroup> predicate)
        {
            using (var db = OdbFactory.Open(this.DbName))
            {
                var exportItems = new List<ReportExportItem>();
                foreach (var item in GetItemsGroupedByWeek(db).Where(g => predicate(g)))
                {
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
                    }
                }
                return exportItems;
            }
        }
        #endregion

        #region Async ops
        public async Task<List<WorkItem>> GetWorkItemsAsync()
        {
            List<WorkItem> items = new List<WorkItem>();
            await Task.Run(() =>
            {
                items = GetWorkItems();
            });
            return items;
        }
        public async void SaveWorkItemAsync(WorkItem item)
        {
            await Task.Run(() =>
            {
                SaveWorkItem(item);
            });
        }
        #endregion

        #region Helpers
        string _dbName;
        string DbName
        {
            get
            {
                if (_dbName == null)
                    _dbName = GetDbName(Environment.UserName);
                return _dbName;
            }
        }
        private string GetDbName(string username)
        {
            return _dbNamesCache.GetOrAdd(username, ProduceDbName);
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
