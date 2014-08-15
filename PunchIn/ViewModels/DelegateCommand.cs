using System;
using System.Windows.Input;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public DelegateCommand()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <param name="canExecute">The can execute.</param>
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null) {
                throw new ArgumentNullException("execute");
            }
            if (canExecute == null) {
                // no can execute provided, then always executable
                canExecute = (o) => true;
            }
            this.CommandAction = execute;
            this.CanExecuteFunc = canExecute;
        }

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
