using System.Collections.Generic;
using System.Windows.Input;

namespace PunchIn.Core.Contracts
{
    public interface IMenuItemViewModel
    {
        string Text { get; set; }
        string Icon { get; set; }
        ICommand Command { get; set; }
        IMenuItemViewModelCollection Children { get; }
    }
    public interface IMenuItemViewModelCollection : ICollection<IMenuItemViewModel>
    {
    }
}
