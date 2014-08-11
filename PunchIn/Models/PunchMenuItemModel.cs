using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PunchIn.Models
{
    public class PunchMenuItemModel
    {
        public PunchMenuItemModel()
        {
            this.Children = new PunchMenuItemModelCollection();
        }

        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }
        public PunchMenuItemModelCollection Children { get; set; }
    }
    /// <summary>
    /// Empty impl for design time data usage
    /// </summary>
    public class PunchMenuItemModelCollection : List<PunchMenuItemModel>
    {
        public PunchMenuItemModelCollection() { }
    }
}
