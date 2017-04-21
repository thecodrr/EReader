using System;
using System.Windows.Input;

namespace EReader.MVVM.Commands
{
    public class RelayCommand : ICommand
    {
        #region Fields 
        private bool enabled;
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion

        public bool IsEnabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
            enabled = true;
        }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            enabled = true;
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }
        #endregion // Constructors 

        #region ICommand Members 
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return enabled; //_canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        public void Execute(object parameter) { _execute(parameter); }
        #endregion // ICommand Members 
    }
}
