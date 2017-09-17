using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace GameOfLifeWPF.Mvvm
{
    internal class SimpleCommand : ICommand
    {
        #region Private Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        #endregion Private Fields

        #region Public Constructors

        public SimpleCommand(Action execute) :
            this(execute, null)
        {
        }

        public SimpleCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler CanExecuteChanged;

        #endregion Public Events

        #region Public Methods

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Public Methods
    }
}