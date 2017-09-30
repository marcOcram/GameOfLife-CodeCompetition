using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// Contains the positions where the life board has changed.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class LifeBoardChangeEventArgs : EventArgs
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LifeBoardChangeEventArgs"/> class.
        /// </summary>
        /// <param name="changes">The changed positions.</param>
        /// <exception cref="ArgumentNullException">changes</exception>
        public LifeBoardChangeEventArgs(IEnumerable<Position> changes)
        {
            Changes = changes ?? throw new ArgumentNullException(nameof(changes));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the changed positions.
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        public IEnumerable<Position> Changes { get; }

        #endregion Public Properties
    }
}