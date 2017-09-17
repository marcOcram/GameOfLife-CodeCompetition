using GameOfLife;
using GameOfLifeWPF.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace GameOfLifeWPF.ViewModel
{
    internal class FieldViewModel : PropertyChangedBase
    {
        #region Private Fields

        private readonly SimpleCommand _requestLifeStateChangeCommand;
        private bool? _isAlive;

        #endregion Private Fields

        #region Public Constructors

        public FieldViewModel(uint x, uint y, LifeState initialLifeState)
        {
            X = x;
            Y = y;

            switch (initialLifeState) {
                case LifeState.Dead:
                    _isAlive = false;
                    break;

                case LifeState.Alive:
                    _isAlive = true;
                    break;
            }

            _requestLifeStateChangeCommand = new SimpleCommand(OnLifeStateChangeRequested);
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler LifeStateChangeRequested;

        #endregion Public Events

        #region Public Properties

        public bool? IsAlive {
            get { return _isAlive; }
            set {
                if (IsAlive.HasValue) {
                    SetField(ref _isAlive, value);
                }
            }
        }

        public ICommand RequestLifeStateChangeCommand => _requestLifeStateChangeCommand;
        public uint X { get; }

        public uint Y { get; }

        #endregion Public Properties

        #region Protected Methods

        protected virtual void OnLifeStateChangeRequested()
        {
            LifeStateChangeRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion Protected Methods
    }
}