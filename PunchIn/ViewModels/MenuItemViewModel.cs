using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PunchIn.ViewModels
{
    public class MenuItemViewModel : ViewModelBase, IMenuItemViewModel
    {
        public MenuItemViewModel()
        {
            this.Children = new MenuItemViewModelCollection();
        }
        /// <summary>
        /// Text of menu item to display. I.e. Header, Text etc
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }
        private string text;
        /// <summary>
        /// Icon path to resource
        /// </summary>
        public string Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }
        private string icon;
        /// <summary>
        /// The command the current menu item will be executing
        /// </summary>
        public ICommand Command
        {
            get { return command; }
            set
            {
                command = value;
                OnPropertyChanged("Command");
            }
        }
        private ICommand command;

        /// <summary>
        /// Children for hierarchical menus
        /// </summary>
        public MenuItemViewModelCollection Children
        {
            get { return children; }
            private set
            {
                children = value;
                OnPropertyChanged("Children");
            }
        }
        private MenuItemViewModelCollection children;
    }
    public class MenuItemViewModelCollection : ObservableCollection<MenuItemViewModel>
    {
        public MenuItemViewModelCollection() { }
    }
}
