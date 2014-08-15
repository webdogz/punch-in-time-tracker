using PunchIn.ViewModels;

namespace PunchIn.Themes
{
    /// <summary>
    /// Provides a base implementation for objects that are displayed in the UI.
    /// </summary>
    public abstract class Displayable
        : ViewModelBase
    {
        private string displayName;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                if (this.displayName != value) {
                    this.displayName = value;
                    OnPropertyChanged("DisplayName");
                }
            }
        }
    }
}
