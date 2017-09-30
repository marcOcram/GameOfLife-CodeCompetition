using GameOfLife;
using GameOfLifeWPF.Controls;
using GameOfLifeWPF.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace GameOfLifeWPF.ViewModel
{
    /// <summary>
    /// The view model for an <see cref="IField"/>
    /// </summary>
    /// <seealso cref="GameOfLifeWPF.Mvvm.PropertyChangedBase" />
    /// <seealso cref="GameOfLifeWPF.Controls.IField" />
    internal class FieldViewModel : PropertyChangedBase, IField
    {
        #region Private Fields

        private readonly SimpleCommand _requestLifeStateChangeCommand;
        private bool? _isAlive;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldViewModel"/> class.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="initialLifeState">Initial life state of the field.</param>
        public FieldViewModel(int x, int y, LifeState initialLifeState)
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

        /// <summary>
        /// Occurs when a change of the life state is requested.
        /// </summary>
        public event EventHandler LifeStateChangeRequested;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets whether the field is alive, dead or there's no life possible (<see cref="null" />);
        /// </summary>
        /// <value></value>
        public bool? IsAlive {
            get { return _isAlive; }
            set {
                if (IsAlive.HasValue) {
                    SetField(ref _isAlive, value);
                }
            }
        }

        /// <summary>
        /// Gets the command to request a change of the current life state.
        /// </summary>
        /// <value>
        /// The request life state change command.
        /// </value>
        public ICommand RequestLifeStateChangeCommand => _requestLifeStateChangeCommand;

        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        public int Y { get; }

        #endregion Public Properties

        #region Protected Methods

        protected virtual void OnLifeStateChangeRequested()
        {
            LifeStateChangeRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion Protected Methods
    }
}