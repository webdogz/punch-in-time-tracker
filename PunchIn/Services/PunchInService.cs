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
    public class PunchInService
    {
        private readonly ConcurrentDictionary<string, string> _dbNamesCache = new ConcurrentDictionary<string, string>();

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
