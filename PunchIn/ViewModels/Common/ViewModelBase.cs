using System.ComponentModel;

namespace PunchIn.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected void OnPropertyChanged(params string[] propertyName)
        {
            if (PropertyChanged != null)
            {
                foreach(string name in propertyName)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Error message to display in UI
        /// </summary>
        public virtual string Errors
        {
            get { return this.errors; }
            set
            {
                if (this.errors != value)
                {
                    this.errors = value;
                    OnPropertyChanged("Errors");
                }
            }
        }
        private string errors;
    }
}
