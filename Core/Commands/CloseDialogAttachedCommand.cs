using System;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Core.Commands
{
    public class CloseDialogAttachedCommand : ICommand
    {
        private Action _execution;
        private Predicate<object> _canExecute;

        public CloseDialogAttachedCommand(Predicate<object> canExecute, Action execution)
        {
            _canExecute = canExecute;
            _execution = execution;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            _execution.Invoke();
            var dialogHost = (DialogHost) parameter;
            dialogHost.IsOpen = false;
        }
        

        public event EventHandler CanExecuteChanged;
    }
}