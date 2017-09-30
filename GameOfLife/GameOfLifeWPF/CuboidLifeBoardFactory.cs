using GameOfLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLifeWPF
{
    /// <summary>
    /// Factory to create a <see cref="CuboidLifeBoard"/>.
    /// </summary>
    /// <seealso cref="GameOfLifeWPF.ILifeBoardFactory" />
    internal class CuboidLifeBoardFactory : ILifeBoardFactory
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the depth of the cuboid.
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        public int Depth { get; set; } = 10;

        /// <summary>
        /// Gets or sets the height of the cuboid.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; set; } = 10;

        /// <summary>
        /// Gets or sets the width of the cuboid.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; set; } = 10;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the see <see cref="CuboidLifeBoard"/>.
        /// </summary>
        /// <returns></returns>
        public LifeBoard Create()
        {
            return CuboidLifeBoard.Create(Width, Height, Depth, new List<Position>());
        }

        #endregion Public Methods
    }
}