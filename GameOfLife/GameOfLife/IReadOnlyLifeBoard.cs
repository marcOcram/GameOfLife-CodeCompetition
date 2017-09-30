using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// Read-only access to a life board.
    /// </summary>
    public interface IReadOnlyLifeBoard
    {
        #region Public Properties

        /// <summary>
        /// Gets the height of the board.
        /// </summary>
        /// <value>
        /// The total height.
        /// </value>
        int Height { get; }

        /// <summary>
        /// Gets the width of the board.
        /// </summary>
        /// <value>
        /// The total width.
        /// </value>
        int Width { get; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <value>
        /// The <see cref="LifeState"/>.
        /// </value>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        LifeState this[Position position] { get; }

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <value>
        /// The <see cref="LifeState"/>.
        /// </value>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns></returns>
        LifeState this[int x, int y] { get; }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns></returns>
        LifeState GetLifeState(int x, int y);

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <value>
        /// The <see cref="LifeState"/>.
        /// </value>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        LifeState GetLifeState(Position position);

        /// <summary>
        /// Gets the <see cref="LifeState"/>s of the specified positiosn.
        /// </summary>
        /// <value>
        /// The <see cref="LifeState"/>s.
        /// </value>
        /// <param name="positions">The positions.</param>
        /// <returns></returns>
        LifeState[] GetLifeStates(IReadOnlyList<Position> positions);

        /// <summary>
        /// Gets the <see cref="LifeStates"/> of the neighbors.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        LifeState[] GetNeighbors(Position position);

        #endregion Public Methods
    }
}