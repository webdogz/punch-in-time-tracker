using System.IO;

namespace PunchIn.ViewModels
{
    public class ShortcutMenuItemViewModel : MenuItemViewModel
    {
        public ShortcutMenuItemViewModel()
            : base()
        {
        }
        /// <summary>
        /// FileInfo describing the file to open
        /// </summary>
        public FileInfo File
        {
            get { return file; }
            set
            {
                file = value;
                OnPropertyChanged("File");
            }
        }
        private FileInfo file;
    }
}
