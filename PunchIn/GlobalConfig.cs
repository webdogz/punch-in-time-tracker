using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchIn
{
    public class GlobalConfig
    {
        const string ApplicationName = "Punch Time Tracker";
        #region Database Config
        public static string DatabaseName
        {
            get
            {
                return string.Format("{0}_punchin.ndb", Environment.UserName);
            }
        }
        public static string DatabaseFolder
        {
            get
            {
                return Properties.Settings.Default.DefaultUserDatabaseFolderLocation;
            }
        }
        public static string DatabaseLocation
        {
            get
            {
                return Path.Combine(DatabaseFolder, DatabaseName);
            }
        }
        #endregion

        #region > Static Methods
        internal static void InitialiseAndSyncSettings()
        {
            // User Database location
            string userSettingsPath = Properties.Settings.Default.DefaultUserDatabaseFolderLocation;
            if (string.IsNullOrWhiteSpace(userSettingsPath))
            {
                userSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ApplicationName);
            }
            if (!System.IO.Directory.Exists(userSettingsPath))
                System.IO.Directory.CreateDirectory(userSettingsPath);
            Properties.Settings.Default.DefaultUserDatabaseFolderLocation = userSettingsPath;
            // Shortcuts folder
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultUserShortcutFolderLocation))
            {
                Properties.Settings.Default.DefaultUserShortcutFolderLocation =
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Favorites), ApplicationName); ;
            }
            // SharePoint Uri
            if (Properties.Settings.Default.SharePointSiteUri == null ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.SharePointSiteUri.AbsoluteUri))
                Properties.Settings.Default.SharePointSiteUri =
                    new Uri(string.Format("{0}/my/personal/{1}/",
                            Properties.Settings.Default.AppDefaultSharePointHostUri,
                            Environment.UserName));
            // SharePoint List Name
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SharePointListName))
                Properties.Settings.Default.SharePointListName = ApplicationName;
            // Default Date Format for DatePicker
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultDatePickerFormatString))
                Properties.Settings.Default.DefaultDatePickerFormatString = "dd/MM/yyyy";
            // Default Date Format for DateTimePicker
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultDatePickerFormatString))
                Properties.Settings.Default.DefaultDateTimePickerFormatString = "yyyy.MM.dd HH:mm";
            Properties.Settings.Default.Save();
        }
        #endregion // Static Methods
    }
}
