using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace GameOfLifeWPF.Controls
{
    /// <summary>
    /// Interface for adding a field to the <see cref="LifeBoard"/> control.
    /// </summary>
    public interface IField
    {
        #region Public Properties

        /// <summary>
        /// Gets whether the field is alive, dead or there's no life possible (<see cref="null"/>);
        /// </summary>
        /// <value>
        /// The is alive.
        /// </value>
        bool? IsAlive { get; }

        /// <summary>
        /// Gets the command to request a change of the current life state.
        /// </summary>
        /// <value>
        /// The request life state change command.
        /// </value>
        ICommand RequestLifeStateChangeCommand { get; }

        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        int X { get; }

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        int Y { get; }

        #endregion Public Properties
    }
}