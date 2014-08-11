using System;
using System.Windows.Input;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action<object> CommandAction { get; set; }
        public Func<object, bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
