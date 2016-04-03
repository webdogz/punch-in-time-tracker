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
            "DefaultUserShortcutFolder", "DefaultUserDatabaseFolderLocation",
            "PunchCardMode"
        };
        public SettingsViewModel()
        {
            sharePointSiteUri = Properties.Settings.Default.SharePointSiteUri;
            sharePointListName = Properties.Settings.Default.SharePointListName;
            defaultDatePickerFormatString = Properties.Settings.Default.DefaultDatePickerFormatString;
            defaultDateTimePickerFormatString = Properties.Settings.Default.DefaultDateTimePickerFormatString;
            defaultUserShortcutFolder = Properties.Settings.Default.DefaultUserShortcutFolderLocation;
            defaultUserDatabaseFolder = Properties.Settings.Default.DefaultUserDatabaseFolderLocation;
            punchCardMode = Properties.Settings.Default.PunchCardMode;
            PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (updatablePropertyNames.Contains(e.PropertyName))
                Properties.Settings.Default.Save();
        }

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
            get { return GetDateFormatted(defaultDatePickerFormatString); }
        }
        public string PreviewDefaultDateTimeFormat
        {
            get { return GetDateFormatted(defaultDateTimePickerFormatString); }
        }

        public IEnumerable<Core.PunchCardMode> PunchCardModes
        {
            get { return Enum.GetValues(typeof(Core.PunchCardMode)).Cast<Core.PunchCardMode>(); }
        }
        #endregion

        #region Properties

        private Core.PunchCardMode punchCardMode;
        public Core.PunchCardMode PunchCardMode
        {
            get { return punchCardMode; }
            set
            {
                // TODO: add ValidationError
                if (punchCardMode != value)
                {
                    punchCardMode = value;
                    Properties.Settings.Default.PunchCardMode = value;
                    OnPropertyChanged("PunchCardMode");
                }
            }
        }

        private string defaultUserDatabaseFolder;
        public string DefaultUserDatabaseFolder
        {
            get { return defaultUserDatabaseFolder; }
            set
            {
                // TODO: add ValidationError
                if (defaultUserDatabaseFolder != value &&
                    System.IO.Directory.Exists(value))
                {
                    defaultUserDatabaseFolder = value;
                    OnPropertyChanged("DefaultUserDatabaseFolder");
                }
            }
        }
        private string defaultUserShortcutFolder;
        public string DefaultUserShortcutFolder
        {
            get { return defaultUserShortcutFolder; }
            set
            {
                // TODO: add ValidationError
                if (defaultUserShortcutFolder != value &&
                    System.IO.Directory.Exists(value))
                {
                    defaultUserShortcutFolder = value;
                    Properties.Settings.Default.DefaultUserShortcutFolderLocation = value;
                    OnPropertyChanged("DefaultUserShortcutFolder");
                }
            }
        }
        private string defaultDatePickerFormatString;
        public string DefaultDatePickerFormatString
        {
            get { return defaultDatePickerFormatString; }
            set
            {
                if (defaultDatePickerFormatString != value)
                {
                    defaultDatePickerFormatString = value;
                    Properties.Settings.Default.DefaultDatePickerFormatString = value;
                    OnPropertyChanged("DefaultDatePickerFormatString");
                }
            }
        }
        private string defaultDateTimePickerFormatString;
        public string DefaultDateTimePickerFormatString
        {
            get { return defaultDateTimePickerFormatString; }
            set
            {
                if (defaultDateTimePickerFormatString != value)
                {
                    defaultDateTimePickerFormatString = value;
                    Properties.Settings.Default.DefaultDateTimePickerFormatString = value;
                    OnPropertyChanged("DefaultDateTimePickerFormatString");
                }
            }
        }
        private string sharePointListName;
        public string SharePointListName
        {
            get { return sharePointListName; }
            set
            {
                if (sharePointListName != value)
                {
                    sharePointListName = value;
                    Properties.Settings.Default.SharePointListName = value;
                    OnPropertyChanged("SharePointListName");
                }
            }
        }
        private Uri sharePointSiteUri;
        public Uri SharePointSiteUri
        {
            get { return sharePointSiteUri; }
            set
            {
                if (sharePointSiteUri != value)
                {
                    sharePointSiteUri = value;
                    Properties.Settings.Default.SharePointSiteUri = value;
                    OnPropertyChanged("SharePointSiteUri");
                }
            }
        }
        #endregion
    }
}
