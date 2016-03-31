using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestPunchIn
{
    class Helpers
    {
        static ConcurrentDictionary<string, string> _dbNamesCache = new ConcurrentDictionary<string, string>();
        public const int MaxWorkItemsToCreate = 20; // number of workitems to create
        public const int MaxWorkItemHoursPerDayRange = 8; // max hours per time entry
        public const int MaxWorkItemDaysRange = 14; // 2 weeks worth of workitems

        #region Helpers
        public static string GetDbName()
        {
            return _dbNamesCache.GetOrAdd(Environment.UserName, ProduceDbName);
        }

        public static string ProduceDbName(string login)
        {
            var dbName = string.Format("{0}_punchin-test.ndb", login);

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Time");

            return Path.Combine(path, dbName);
        }
        
        #endregion
    }
    public class CollectionNames
    {
        public const string WorkItems = "workitems";
        public const string TimeEntries = "times";
    }
}
