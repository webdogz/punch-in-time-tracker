using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchIn.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        string[] updatablePropertyNames = new string[] { 
            "SharePointListName", "SharePointSiteUri",
            "DefaultDatePickerFormatString", "DefaultDateTimePickerFormatString",
            "SharePointSiteUri","SharePointListName",
            "DefaultUserShortcutFolder", "DefaultUserDatabaseFolderLocation"
        };
        public SettingsViewModel()
        {
            this.sharePointSiteUri = Properties.Settings.Default.SharePointSiteUri;
            this.sharePointListName = Properties.Settings.Default.SharePointListName;
            this.defaultDatePickerFormatString = Properties.Settings.Default.DefaultDatePickerFormatString;
            this.defaultDateTimePickerFormatString = Properties.Settings.Default.DefaultDateTimePickerFormatString;
            this.defaultUserShortcutFolder = Properties.Settings.Default.DefaultUserShortcutFolderLocation;
            this.defaultUserDatabaseFolder = Properties.Settings.Default.DefaultUserDatabaseFolderLocation;
            this.PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (updatablePropertyNames.Contains(e.PropertyName))
                Properties.Settings.Default.Save();
        }

        #region > Static Methods
        internal static void InitialiseAndSyncSettings()
        {
            const string AppName = "Punch Time Tracker";
            // User Database location
            string defaultSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), AppName);
            string userSettingsPath = Properties.Settings.Default.DefaultUserDatabaseFolderLocation;
            if (string.IsNullOrWhiteSpace(userSettingsPath))
            {
                userSettingsPath = defaultSettingsPath;
            }
            if (!System.IO.Directory.Exists(userSettingsPath))
                System.IO.Directory.CreateDirectory(userSettingsPath);
            Properties.Settings.Default.DefaultUserDatabaseFolderLocation = userSettingsPath;
            // Shortcuts folder
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultUserShortcutFolderLocation))
            {
                Properties.Settings.Default.DefaultUserShortcutFolderLocation =
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Favorites), AppName); ;
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
                Properties.Settings.Default.SharePointListName = AppName;
            // Default Date Format for DatePicker
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultDatePickerFormatString))
                Properties.Settings.Default.DefaultDatePickerFormatString = "dd/MM/yyyy";
            // Default Date Format for DateTimePicker
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultDatePickerFormatString))
                Properties.Settings.Default.DefaultDateTimePickerFormatString = "yyyy.MM.dd HH:mm";
            Properties.Settings.Default.Save();
        }
        #endregion // Static Methods

        #region View properties
        private string GetDateFormatted(string format)
        {
            try
            {
                return DateTime.Now.ToString(format);
            }
            catch
            {
                return "Invalid Format";
            }
        }

        public string PreviewDefaultDateFormat
        {
            get { return GetDateFormatted(this.defaultDatePickerFormatString); }
        }
        public string PreviewDefaultDateTimeFormat
        {
            get { return GetDateFormatted(this.defaultDateTimePickerFormatString); }
        }
        #endregion

        #region Properties
        
        private string defaultUserDatabaseFolder;
        public string DefaultUserDatabaseFolder
        {
            get { return this.defaultUserDatabaseFolder; }
            set
            {
                // TODO: add ValidationError
                if (this.defaultUserDatabaseFolder != value &&
                    System.IO.Directory.Exists(value))
                {
                    this.defaultUserDatabaseFolder = value;
                    OnPropertyChanged("DefaultUserDatabaseFolder");
                }
            }
        }
        private string defaultUserShortcutFolder;
        public string DefaultUserShortcutFolder
        {
            get { return this.defaultUserShortcutFolder; }
            set
            {
                // TODO: add ValidationError
                if (this.defaultUserShortcutFolder != value &&
                    System.IO.Directory.Exists(value))
                {
                    this.defaultUserShortcutFolder = value;
                    Properties.Settings.Default.DefaultUserShortcutFolderLocation = value;
                    OnPropertyChanged("DefaultUserShortcutFolder");
                }
            }
        }
        private string defaultDatePickerFormatString;
        public string DefaultDatePickerFormatString
        {
            get { return this.defaultDatePickerFormatString; }
            set
            {
                if (this.defaultDatePickerFormatString != value)
                {
                    this.defaultDatePickerFormatString = value;
                    Properties.Settings.Default.DefaultDatePickerFormatString = value;
                    OnPropertyChanged("DefaultDatePickerFormatString");
                }
            }
        }
        private string defaultDateTimePickerFormatString;
        public string DefaultDateTimePickerFormatString
        {
            get { return this.defaultDateTimePickerFormatString; }
            set
            {
                if (this.defaultDateTimePickerFormatString != value)
                {
                    this.defaultDateTimePickerFormatString = value;
                    Properties.Settings.Default.DefaultDateTimePickerFormatString = value;
                    OnPropertyChanged("DefaultDateTimePickerFormatString");
                }
            }
        }
        private string sharePointListName;
        public string SharePointListName
        {
            get { return this.sharePointListName; }
            set
            {
                if (this.sharePointListName != value)
                {
                    this.sharePointListName = value;
                    Properties.Settings.Default.SharePointListName = value;
                    OnPropertyChanged("SharePointListName");
                }
            }
        }
        private Uri sharePointSiteUri;
        public Uri SharePointSiteUri
        {
            get { return this.sharePointSiteUri; }
            set
            {
                if (this.sharePointSiteUri != value)
                {
                    this.sharePointSiteUri = value;
                    Properties.Settings.Default.SharePointSiteUri = value;
                    OnPropertyChanged("SharePointSiteUri");
                }
            }
        }
        #endregion
    }
}
