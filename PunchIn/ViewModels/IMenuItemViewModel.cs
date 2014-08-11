using System.Windows.Input;

namespace PunchIn.ViewModels
{
    public interface IMenuItemViewModel
    {
        string Text { get; set; }
        string Icon { get; set; }
        ICommand Command { get; set; }
        MenuItemViewModelCollection Children { get; }
    }
}
