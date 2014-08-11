using System;

namespace PunchIn.ViewModels
{
    public class PunchMenuItemViewModel : MenuItemViewModel
    {
        public PunchMenuItemViewModel() : base() { }
        /// <summary>
        /// Guid id of the backing item
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Is the menu item expanded? Mainly used in a TreeView
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        private bool isExpanded;
        /// <summary>
        /// Is the menu item selected? Mainly used in a TreeView but can be used for checked property
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        private bool isSelected;
    }
}
