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
            "DefaultDatePickerFormatString", "DefaultDateTimePickerFormatString"
        };
        public SettingsViewModel()
        {
            this.sharePointSiteUri = Properties.Settings.Default.SharePointSiteUri;
            this.sharePointListName = Properties.Settings.Default.SharePointListName;
            this.defaultDatePickerFormatString = Properties.Settings.Default.DefaultDatePickerFormatString;
            this.defaultDateTimePickerFormatString = Properties.Settings.Default.DefaultDateTimePickerFormatString;
            this.PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (updatablePropertyNames.Contains(e.PropertyName))
                Properties.Settings.Default.Save();
        }

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

        #region View properties
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
        
        private string defaultUserShortcutFolder;
        public string DefaultUserShortcutFolder
        {
            get { return this.defaultUserShortcutFolder; }
            set
            {
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
