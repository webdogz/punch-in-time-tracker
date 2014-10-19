using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shell;

namespace PunchIn.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICleanUp
    {
        public MainWindowViewModel()
        {
            NotifyIconViewModel.Current.PropertyChanged += NotifyIcon_PropertyChanged;
        }
        #region Event Handlers
        private void NotifyIcon_PropertyChanged(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentTimeEntry")
            {
                OnPropertyChanged("ProgressState", "TaskbarDescription", "ThumbDescription", "PunchImage");
            }
        }
        #endregion

        #region Properties
        // bad bad bad, I know, but who cares???
        // MVVM nerds...thats who :O
        public TaskbarItemProgressState ProgressState
        {
            get 
            {
                TaskbarItemProgressState state = TaskbarItemProgressState.Paused;
                if (NotifyIconViewModel.Current.CurrentTimeEntry != null)
                    state = TaskbarItemProgressState.Indeterminate;
                return state;
            }
        }
        
        public string TaskbarDescription
        {
            get
            {
                string description = "No current time entry";
                if (NotifyIconViewModel.Current.CurrentTimeEntry != null)
                {
                    description = NotifyIconViewModel.Current.CurrentTimeEntry.CurrentEntry.Description;
                }
                return description;
            }
        }
        public string ThumbDescription
        {
            get
            {
                string description = "Punch In";
                if (NotifyIconViewModel.Current.CurrentTimeEntry != null)
                {
                    description = "Punch Out";
                }
                return description;
            }
        }
        public string PunchImage
        {
            get
            {
                string key = "punchinImage";
                if (NotifyIconViewModel.Current.CurrentTimeEntry != null)
                {
                    key = "punchoutImage";
                }
                return key;
            }
        }

        #endregion

        #region Commands
        private ICommand punchCommand;
        public ICommand PunchCommand
        {
            get
            {
                if (this.punchCommand == null)
                    this.punchCommand = new DelegateCommand
                    {
                        CanExecuteFunc = (o) => true,
                        CommandAction = (o) => NotifyIconViewModel.Current.PunchInCommand.Execute(o)
                    };
                return this.punchCommand;
            }
        }
        #endregion

        #region Methods
        public void CleanUp()
        {
            try
            {
                NotifyIconViewModel.Current.PropertyChanged -= NotifyIcon_PropertyChanged;
            }
            catch { }
        }
        #endregion
    }
}
