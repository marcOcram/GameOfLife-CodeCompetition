using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// Contains a position on a life board
    /// </summary>
    [DebuggerDisplay("({X}, {Y})")]
    public struct Position
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Position"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Position(int x, int y)
        {
            Y = y;
            X = x;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the x-coordinate.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public int Y { get; }

        #endregion Public Properties

        #region Public Methods

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        #endregion Public Methods
    }
}